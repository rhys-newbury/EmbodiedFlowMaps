using UnityEngine;
using Obi;
using VRTK;

//[RequireComponent(typeof(ObiPinConstraintsBatch))]
public class VRTK_ObiInteractableActor : MonoBehaviour
{
    #region Fields

    public bool isGrabbable = true;

    private GameObject sphere;

    private ObiPinConstraintsBatch pinConstraints;

    #endregion


    #region Mono Events


    private void Reset()
    {
        isGrabbable = true;
    }

    #endregion


    #region Actor Interaction

    public System.Collections.IEnumerator GrabActor(InteractingInfo interactingInfo)
    {
        if (!isGrabbable) yield return -1;

        var controller = interactingInfo.collider.attachedRigidbody.gameObject;
        var cube = GameObject.Find(controller.name.Split(new char[] { ' ' })[0] + "Cube");

        sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.GetComponent<MeshRenderer>().material = Resources.Load<Material>("invisible");

        ObiRope rope = this.GetComponent<ObiRope>();


        sphere.transform.position = rope.GetParticlePosition(interactingInfo.touchingParticle);
        sphere.transform.rotation = rope.GetParticleOrientation(interactingInfo.touchingParticle);
        
        var interactObject = sphere.AddComponent(typeof(VRTK_InteractableObject)) as VRTK_InteractableObject;
        VRTK_InteractHaptics interactHaptics = sphere.AddComponent(typeof(VRTK_InteractHaptics)) as VRTK_InteractHaptics;
        VRTK.GrabAttachMechanics.VRTK_ChildOfControllerGrabAttach grabAttach = sphere.AddComponent(typeof(VRTK.GrabAttachMechanics.VRTK_ChildOfControllerGrabAttach)) as VRTK.GrabAttachMechanics.VRTK_ChildOfControllerGrabAttach;
        VRTK.SecondaryControllerGrabActions.VRTK_SwapControllerGrabAction grabAction = sphere.AddComponent(typeof(VRTK.SecondaryControllerGrabActions.VRTK_SwapControllerGrabAction)) as VRTK.SecondaryControllerGrabActions.VRTK_SwapControllerGrabAction;
        VRTK.SecondaryControllerGrabActions.VRTK_AxisScaleGrabAction scaleAction = sphere.AddComponent(typeof(VRTK.SecondaryControllerGrabActions.VRTK_AxisScaleGrabAction)) as VRTK.SecondaryControllerGrabActions.VRTK_AxisScaleGrabAction;
        Rigidbody rigidBody = sphere.AddComponent(typeof(Rigidbody)) as Rigidbody;

        interactObject.isGrabbable = true;
        interactObject.holdButtonToGrab = false;
        interactObject.grabAttachMechanicScript = grabAttach;
        interactObject.secondaryGrabActionScript = scaleAction;

        grabAttach.precisionGrab = true;

        rigidBody.useGravity = false;
        rigidBody.isKinematic = true;

        sphere.transform.localScale = new Vector3(0.1F, 0.1F, 0.1F);

        interactingInfo.interactGrab.AttemptGrabObject(sphere);
        foreach (var att in GetComponents<ObiParticleAttachment>())
        {
         
       att.target.transform.parent = sphere.transform;
         
        }


        interactingInfo.grabbedParticle = interactingInfo.touchingParticle;

    }

    public void ReleaseActor(InteractingInfo interactingInfo)
    {
        sphere.transform.DetachChildren();
        GameObject.Destroy(sphere);


    }

    #endregion

}