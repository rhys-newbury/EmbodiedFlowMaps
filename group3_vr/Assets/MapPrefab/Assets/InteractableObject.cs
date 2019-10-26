using System;
using UnityEngine;
/// <summary>
/// Defined events for InteractableObjects.
/// </summary>
public abstract class InteractableObject : MonoBehaviour
{
    public virtual void OnUpdateTouchPadPressed(float touchPadAngle, Transform transformDirection) { }
    public virtual void OnPointerEnter(Action<string> changeText) { }
    public virtual void OnPointerLeave() { }

    public virtual void OnTriggerPressed(){ }
    public virtual void OnGripPressed() { }

}
