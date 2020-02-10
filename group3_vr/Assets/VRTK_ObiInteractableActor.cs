using UnityEngine;
using Obi;

//[RequireComponent(typeof(ObiPinConstraints))]
public class VRTK_ObiInteractableActor : MonoBehaviour
{
    #region Fields

    public bool isGrabbable = true;

    //private ObiPinConstraints pinConstraints;

    #endregion


    #region Mono Events

    private void Awake()
    {
        //pinConstraints = GetComponent<ObiPinConstraints>();
    }

    private void Reset()
    {
        //isGrabbable = true;
    }

    #endregion


    #region Actor Interaction

    public void GrabActor(InteractingInfo interactingInfo)
    {
        //if (!isGrabbable) return;

        //ObiPinConstraintBatch batch = pinConstraints.GetBatches()[0] as ObiPinConstraintBatch;
        

        //pinConstraints.RemoveFromSolver(null);

        //interactingInfo.pinIndex = batch.ConstraintCount;
        //interactingInfo.grabbedParticle = interactingInfo.touchingParticle;
        //batch.AddConstraint(interactingInfo.grabbedParticle.Value, interactingInfo.obiCollider, interactingInfo.pinOffset, interactingInfo.stiffness);

        //pinConstraints.AddToSolver(null);

        //pinConstraints.PushDataToSolver();
    }

    public void ReleaseActor(InteractingInfo interactingInfo)
    {
        //if (!isGrabbable) return;

        //ObiPinConstraintBatch batch = pinConstraints.GetBatches()[0] as ObiPinConstraintBatch;

        //pinConstraints.RemoveFromSolver(null);

        //if (interactingInfo.pinIndex.HasValue)
        //    batch.RemoveConstraint(interactingInfo.pinIndex.Value);

        //pinConstraints.AddToSolver(null);

        //pinConstraints.PushDataToSolver();
    }

    #endregion

}