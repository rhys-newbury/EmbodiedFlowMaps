using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class AddSphere : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        var rope = this.GetComponent<Obi.ObiRope>();
        foreach (var i in rope.blueprint.groups)
        {
            foreach (var particle in i.particleIndices)
            {
                var pos = rope.GetParticlePosition(particle);
                var quat = rope.GetParticleOrientation(particle);
                var g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                g.transform.position = pos;
                g.transform.rotation = quat;
                g.transform.localScale = new Vector3(0.1F, 0.1F, 0.1F);
                g.AddComponent<Obi.ObiCollider>();
                var interactObject = g.AddComponent(typeof(VRTK_InteractableObject)) as VRTK_InteractableObject;
                VRTK_InteractHaptics interactHaptics = g.AddComponent(typeof(VRTK_InteractHaptics)) as VRTK_InteractHaptics;
                VRTK.GrabAttachMechanics.VRTK_ChildOfControllerGrabAttach grabAttach = g.AddComponent(typeof(VRTK.GrabAttachMechanics.VRTK_ChildOfControllerGrabAttach)) as VRTK.GrabAttachMechanics.VRTK_ChildOfControllerGrabAttach;
                VRTK.SecondaryControllerGrabActions.VRTK_SwapControllerGrabAction grabAction = g.AddComponent(typeof(VRTK.SecondaryControllerGrabActions.VRTK_SwapControllerGrabAction)) as VRTK.SecondaryControllerGrabActions.VRTK_SwapControllerGrabAction;
                VRTK.SecondaryControllerGrabActions.VRTK_AxisScaleGrabAction scaleAction = g.AddComponent(typeof(VRTK.SecondaryControllerGrabActions.VRTK_AxisScaleGrabAction)) as VRTK.SecondaryControllerGrabActions.VRTK_AxisScaleGrabAction;
                //VRTK.VRTK_SlideObjectControlAction slide = gameObject.AddComponent(typeof(VRTK.VRTK_SlideObjectControlAction)) as VRTK.VRTK_SlideObjectControlAction;
                //VRTK.VRTK_TouchpadControl control = gameObject.AddComponent(typeof(VRTK.VRTK_TouchpadControl)) as VRTK.VRTK_TouchpadControl;
                Rigidbody rigidBody = g.AddComponent(typeof(Rigidbody)) as Rigidbody;

                interactObject.isGrabbable = true;
                interactObject.holdButtonToGrab = false;
                interactObject.grabAttachMechanicScript = grabAttach;
                interactObject.secondaryGrabActionScript = scaleAction;

                grabAttach.precisionGrab = true;

                rigidBody.useGravity = false;
                rigidBody.isKinematic = true;

                var attach = this.gameObject.AddComponent<Obi.ObiParticleAttachment>();
                attach.target = g.transform;
                attach.attachmentType = Obi.ObiParticleAttachment.AttachmentType.Dynamic;
                attach.particleGroup = i;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
