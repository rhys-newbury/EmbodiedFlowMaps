using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Obi
{
    [AddComponentMenu("Physics/Obi/Obi Rope", 880)]
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public class ObiRope : ObiRopeBase
    {
        [SerializeField] protected ObiRopeBlueprint m_RopeBlueprint;

        // rope has a list of structural elements.
        // each structural element is equivalent to 1 distance constraint and 2 bend constraints (with previous, and following element).
        // a structural element has force and rest length.
        // a function re-generates constraints from structural elements when needed, placing them in the appropiate batches.
        // 

        public bool tearingEnabled = false;
        public float tearResistanceMultiplier = 1000;                   /**< Factor that controls how much a structural cloth spring can stretch before breaking.*/
        public int tearRate = 1;

        // distance constraints:
        [SerializeField] protected bool _distanceConstraintsEnabled = true;
        [SerializeField] protected float _stretchCompliance = 0;
        [SerializeField] [Range(0, 1)] protected float _maxCompression = 0;

        // bend constraints:
        [SerializeField] protected bool _bendConstraintsEnabled = true;
        [SerializeField] protected float _bendCompliance = 0;
        [SerializeField] [Range(0, 0.5f)] protected float _maxBending = 0;

        public bool selfCollisions
        {
            get { return m_SelfCollisions; }
            set { if (value != m_SelfCollisions) { m_SelfCollisions = value; SetSelfCollisions(selfCollisions); } }
        }

        public bool distanceConstraintsEnabled
        {
            get { return _distanceConstraintsEnabled; }
            set { if (value != _distanceConstraintsEnabled) { _distanceConstraintsEnabled = value; PushDistanceConstraints(_distanceConstraintsEnabled, _stretchCompliance, _maxCompression); ; } }
        }

        public float stretchCompliance
        {
            get { return _stretchCompliance; }
            set { _stretchCompliance = value; PushDistanceConstraints(_distanceConstraintsEnabled, _stretchCompliance, _maxCompression); }
        }

        public float maxCompression
        {
            get { return _maxCompression; }
            set { _maxCompression = value; PushDistanceConstraints(_distanceConstraintsEnabled, _stretchCompliance, _maxCompression); }
        }

        public bool bendConstraintsEnabled
        {
            get { return _bendConstraintsEnabled; }
            set { if (value != _bendConstraintsEnabled) { _bendConstraintsEnabled = value; PushBendConstraints(_bendConstraintsEnabled, _bendCompliance, _maxBending); } }
        }

        public float bendCompliance
        {
            get { return _bendCompliance; }
            set { _bendCompliance = value; PushBendConstraints(_bendConstraintsEnabled, _bendCompliance, _maxBending); }
        }

        public float maxBending
        {
            get { return _maxBending; }
            set { _maxBending = value; PushBendConstraints(_bendConstraintsEnabled, _bendCompliance, _maxBending); }
        }

        public override ObiActorBlueprint blueprint
        {
            get { return m_RopeBlueprint; }
        }

        public ObiRopeBlueprint ropeBlueprint
        {
            get { return m_RopeBlueprint; }
            set
            {
                if (m_RopeBlueprint != value)
                {
                    RemoveFromSolver();
                    ClearState();
                    m_RopeBlueprint = value;
                    AddToSolver();
                }
            }
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            SetupRuntimeConstraints();
        }

        public override void LoadBlueprint(ObiSolver solver)
        {
            base.LoadBlueprint(solver);
            RebuildElementsFromConstraints();
            SetupRuntimeConstraints();
        }

        private void SetupRuntimeConstraints()
        {
            PushDistanceConstraints(_distanceConstraintsEnabled, _stretchCompliance, _maxCompression);
            PushBendConstraints(_bendConstraintsEnabled, _bendCompliance, _maxBending);
            SetSelfCollisions(selfCollisions);
            RecalculateRestLength();
        }

        public override void EndStep()
        {
            base.EndStep();

            if (isActiveAndEnabled)
                ApplyTearing();
        }

        protected void ApplyTearing()
        {

            if (!tearingEnabled)
                return;

            List<ObiStructuralElement> tornElements = new List<ObiStructuralElement>();

            var dc = GetConstraintsByType(Oni.ConstraintType.Distance) as ObiConstraints<ObiDistanceConstraintsBatch>;
            if (dc == null)
                return;

            // get constraint forces for each batch:
            for (int j = 0; j < 2; ++j)
            {
                var batch = dc.GetBatchInterfaces()[j] as IStructuralConstraintBatch;

                float[] forces = new float[(batch as IObiConstraintsBatch).activeConstraintCount];
                Oni.GetBatchConstraintForces((batch as IObiConstraintsBatch).oniBatch, forces, forces.Length, 0);

                for (int i = 0; i < forces.Length; i++)
                {
                    int elementIndex = j + 2 * i;
                    elements[elementIndex].constraintForce = forces[i];

                    if (-forces[i] > /*resistance * */tearResistanceMultiplier)
                    {
                        tornElements.Add(elements[elementIndex]);
                    }
                }
            }

            if (tornElements.Count > 0)
            {

                // sort edges by force:
                tornElements.Sort(delegate (ObiStructuralElement x, ObiStructuralElement y)
                {
                    return x.constraintForce.CompareTo(y.constraintForce);
                });

                int tornCount = 0;
                for (int i = 0; i < tornElements.Count; i++)
                {
                    if (Tear(tornElements[i]))
                        tornCount++;
                    if (tornCount >= tearRate)
                        break;
                }

                if (tornCount > 0)
                    RebuildConstraintsFromElements();
            }

        }

        private int SplitParticle(int splitIndex)
        {
            // halve the original particle's mass:
            m_Solver.invMasses[splitIndex] *= 2;

            CopyParticle(solver.particleToActor[splitIndex].indexInActor, activeParticleCount);
            ActivateParticle(activeParticleCount);

            return solverIndices[activeParticleCount - 1];
        }

        public bool Tear(ObiStructuralElement element)
        {
            // don't allow splitting if there are no free particles left in the pool.
            if (activeParticleCount >= m_RopeBlueprint.particleCount)
                return false;

            // Cannot split fixed particles:
            if (m_Solver.invMasses[element.particle1] == 0)
                return false;

            // Or particles that have been already split.
            int index = elements.IndexOf(element);
            if (index > 0 && elements[index - 1].particle2 != element.particle1)
                return false;

            element.particle1 = SplitParticle(element.particle1);

            return true;
        }

        protected override void RebuildElementsFromConstraintsInternal()
        {
            var dc = GetConstraintsByType(Oni.ConstraintType.Distance) as ObiConstraints<ObiDistanceConstraintsBatch>;
            if (dc == null)
                return;

            int constraintCount = dc.batches[0].activeConstraintCount + dc.batches[1].activeConstraintCount;

            elements = new List<ObiStructuralElement>(constraintCount);

            for (int i = 0; i < constraintCount; ++i)
            {
                var batch = dc.batches[i % 2] as ObiDistanceConstraintsBatch;
                int constraintIndex = i / 2;

                var e = new ObiStructuralElement();
                e.particle1 = batch.particleIndices[constraintIndex * 2];
                e.particle2 = batch.particleIndices[constraintIndex * 2 + 1];
                e.restLength = batch.restLengths[constraintIndex];
                e.tearResistance = 1;
                elements.Add(e);
            }

            // loop-closing element:
            if (dc.batches.Count > 2)
            {
                var batch = dc.batches[2];
                var e = new ObiStructuralElement();
                e.particle1 = batch.particleIndices[0];
                e.particle2 = batch.particleIndices[1];
                e.restLength = batch.restLengths[0];
                e.tearResistance = 1;
                elements.Add(e);
            }
        }

        public override void RebuildConstraintsFromElements()
        {
            // regenerate constraints from elements:
            var dc = GetConstraintsByType(Oni.ConstraintType.Distance) as ObiConstraints<ObiDistanceConstraintsBatch>;
            var bc = GetConstraintsByType(Oni.ConstraintType.Bending) as ObiConstraints<ObiBendConstraintsBatch>;

            dc.DeactivateAllConstraints();
            bc.DeactivateAllConstraints();

            int elementsCount = elements.Count - (ropeBlueprint.path.Closed ? 1 : 0);
            for (int i = 0; i < elementsCount; ++i)
            {
                var db = dc.batches[i % 2] as ObiDistanceConstraintsBatch;
                int constraint = db.activeConstraintCount;

                db.particleIndices[constraint * 2] = elements[i].particle1;
                db.particleIndices[constraint * 2 + 1] = elements[i].particle2;
                db.restLengths[constraint] = elements[i].restLength;
                db.stiffnesses[constraint] = new Vector2(_stretchCompliance, _maxCompression * db.restLengths[constraint]);
                db.ActivateConstraint(constraint);

                if (i < elementsCount - 1)
                {
                    var bb = bc.batches[i % 3] as ObiBendConstraintsBatch;

                    // create bend constraint only if there's continuity between elements:
                    if (elements[i].particle2 == elements[i + 1].particle1)
                    {
                        constraint = bb.activeConstraintCount;

                        bb.particleIndices[constraint * 3] = elements[i].particle1;
                        bb.particleIndices[constraint * 3 + 1] = elements[i + 1].particle2;
                        bb.particleIndices[constraint * 3 + 2] = elements[i].particle2;
                        bb.restBends[constraint] = 0;
                        bb.bendingStiffnesses[constraint] = new Vector2(_maxBending, _bendCompliance);
                        bb.ActivateConstraint(constraint);
                    }
                }
            }

            // loop-closing constraints:
            if (dc.batches.Count > 2)
            {
                var loopClosingBatch = dc.batches[2];
                var lastElement = elements[elements.Count - 1];
                loopClosingBatch.particleIndices[0] = lastElement.particle1;
                loopClosingBatch.particleIndices[1] = lastElement.particle2;
                loopClosingBatch.restLengths[0] = lastElement.restLength;
                loopClosingBatch.stiffnesses[0] = new Vector2(_stretchCompliance, _maxCompression * loopClosingBatch.restLengths[0]);
                loopClosingBatch.ActivateConstraint(0);
            }

            if (bc.batches.Count > 4 && elements.Count > 2)
            {
                var loopClosingBatch = bc.batches[3];
                var lastElement = elements[elements.Count - 2];
                loopClosingBatch.particleIndices[0] = lastElement.particle1;
                loopClosingBatch.particleIndices[1] = elements[0].particle1;
                loopClosingBatch.particleIndices[2] = lastElement.particle2;
                loopClosingBatch.restBends[0] = 0;
                loopClosingBatch.bendingStiffnesses[0] = new Vector2(_maxBending, _bendCompliance);
                loopClosingBatch.ActivateConstraint(0);

                loopClosingBatch = bc.batches[4];
                loopClosingBatch.particleIndices[0] = lastElement.particle2;
                loopClosingBatch.particleIndices[1] = elements[0].particle2;
                loopClosingBatch.particleIndices[2] = elements[0].particle1;
                loopClosingBatch.restBends[0] = 0;
                loopClosingBatch.bendingStiffnesses[0] = new Vector2(_maxBending, _bendCompliance);
                loopClosingBatch.ActivateConstraint(0);
            }
        }
    }
}
