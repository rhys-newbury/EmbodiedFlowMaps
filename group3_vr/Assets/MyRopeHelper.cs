using UnityEngine;
using System.Collections;
using Obi;

//[RequireComponent(typeof(ObiRope))]
//[RequireComponent(typeof(ObiCatmullRomCurve))]
public class MyRopeHelper : MonoBehaviour
{

    //public ObiSolver solver;
    //public ObiRopeSection section;
    //public Material material;
    //public Transform start;
    //public Transform end;

    //private ObiRope rope;
    //private ObiCatmullRomCurve path;
    private GameObject cube0;

    public GameObject cube1 { get; private set; }

    void Start()
    {



    }

    public void GenerateRope(GameObject cube0, GameObject cube1, float thickness)
    {

        //// Get all needed components and interconnect them:
        //rope = GetComponent<ObiRope>();
        //path = GetComponent<ObiCatmullRomCurve>();
        ////path = GetComponent< ObiBezierCurve >()
        //rope.Solver = solver;
        //rope.ropePath = path;
        //rope.Section = section;
        //rope.thickness = thickness;
        //rope.ThicknessFromParticles = false;
        //GetComponent<MeshRenderer>().material = material;

        //this.gameObject.AddComponent<VRTK_ObiInteractableActor>();


        //// Calculate rope start/end and direction in local space:
        //Vector3 localStart = transform.InverseTransformPoint(start.position);
        //Vector3 localEnd = transform.InverseTransformPoint(end.position);
        //Vector3 direction = (localEnd - localStart).normalized;

        //// Generate rope path:
        //path.controlPoints.Clear();
        //path.controlPoints.Add(localStart - direction);
        //path.controlPoints.Add(localStart);
        //path.controlPoints.Add(localEnd);
        //path.controlPoints.Add(localEnd + direction);

        //this.cube0 = cube0;
        //this.cube1 = cube1;         


        //// Setup the simulation:
        //StartCoroutine(Setup());
    }

    //IEnumerator Setup()
    //{

    //    // Generate particles and add them to solver:
    //    yield return StartCoroutine(rope.GeneratePhysicRepresentationForMesh());
    //    rope.AddToSolver(null);

    //    // Fix first and last particle in place:
    //    //rope.invMasses[0] = 0;
    //    //rope.invMasses[rope.UsedParticles - 1] = 0;
    //    //Oni.SetParticleInverseMasses(solver.OniSolver, new float[] { 0 }, 1, rope.particleIndices[0]);
    //    //Oni.SetParticleInverseMasses(solver.OniSolver, new float[] { 0 }, 1, rope.particleIndices[rope.UsedParticles - 1]);

    //    ObiPinConstraints constraints = rope.GetComponent<ObiPinConstraints>();
    //    ObiPinConstraintBatch batch = constraints.GetBatches()[0] as ObiPinConstraintBatch;

    //    // remove the constraints from the solver, because we cannot modify the constraints list while the solver is using it.
    //    constraints.RemoveFromSolver(null);

    //    ObiCollider ropeLink = cube0.GetComponent<ObiCollider>();
    //    batch.AddConstraint(0, ropeLink, Vector3.zero, 1.0f);
    //    batch.AddConstraint(1, ropeLink, Vector3.zero, 1.0f);

    //    ObiCollider ropeLink2 = cube1.GetComponent<ObiCollider>();
    //    batch.AddConstraint(rope.UsedParticles - 1, ropeLink2, Vector3.zero, 1.0f);
    //    batch.AddConstraint(rope.UsedParticles - 2, ropeLink2, Vector3.zero, 1.0f);


    //    ObiDistanceConstraints dist_constraints = GetComponent<ObiDistanceConstraints>();
       

    //    // remove the constraints from the solver, because we cannot modify the constraints list while the solver is using it.
    //    dist_constraints.RemoveFromSolver(null);

    //    dist_constraints.stiffness = 0.15F;
    //    dist_constraints.slack = 1;
        
    //    constraints.AddToSolver(null);
    //    dist_constraints.AddToSolver(null);

    //}
}