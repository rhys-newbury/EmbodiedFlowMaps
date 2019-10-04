using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pointable : MonoBehaviour
{
    public virtual void OnUpdateTouchPadPressed(float touchPadAngle, Transform transformDirection) { }
    public virtual void OnPointerEnter(Action<string> changeText) { }
    public virtual void OnPointerLeave() { }

    public virtual void OnTriggerPressed(){ }
    public virtual void OnGripPressed() { }
    public virtual void OnThrow() { }

}
