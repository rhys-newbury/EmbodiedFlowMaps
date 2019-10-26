using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using VRTK;
using System;

/// <summary>
/// Main class which deals with inteactions between pointer and InteractableObject. InteractableObject supports 5 events which are delegated by this class.
/// </summary>
public class ControllerInteraction : MonoBehaviour
{
    private bool selectingObject;
    private InteractableObject currentObject;
    
    private VRTK_ControllerTooltips helpTooltip;
    private bool helpTooltipState = false;

    private VRTK_ControllerTooltips dataToolTip;

    private Action<string> changeText;

    private VRTK_ControllerEvents controller;
    private VRTK_Pointer pointer;

    private long oldTime = 0;
    private Vector3 oldPos = new Vector3(0,0,0);

    private List<float> velocityBuffer = (new float[] { 0, 0, 0, 0, 0 }).ToList();
    private bool touchpadPressed = false;
    private float touchpadAngle;

    /// <summary>
    /// Calculations to be performed on every frame.
    /// </summary>
    /// <returns></returns>
    private void Update()
    {
        try
        {

            var obj = GetComponent<VRTK_InteractGrab>().GetGrabbedObject();

            if (touchpadPressed) OnUpdateTouchPadPressed(touchpadAngle, pointer.transform.parent.transform, obj);

            long time = DateTime.Now.Ticks;
           
            //Time Limit between frames
            if (time-oldTime < 300000) {

                Vector3 newPos = obj.transform.position;
                Vector3 delta = newPos - oldPos;
                float speed = delta.magnitude / time * (Mathf.Pow(10,20));

                //Add to the velocity buffer
                if (oldPos.magnitude != 0)
                {
                    velocityBuffer.RemoveAt(0);
                    velocityBuffer.Add(speed);
                }
                oldPos = newPos;

                Debug.Log(velocityBuffer);
                
                //Condition for a throw to occur.
                if (velocityBuffer.Sum() > 150 && velocityBuffer.Count((x) => x > 20) > 3) {
                    obj.GetComponentInChildren<MapContainer>()?.OnThrow();
                }
                

            }
            else {
                //Reset the buffer
                velocityBuffer = (new float[] { 0, 0, 0, 0, 0 }).ToList();
            }
            //Update the time
            oldTime = time;
        }
        catch { }

    }


    /// <summary>
    /// If the touchpad is being pressed, perform the nessecary action based on the angle.
    /// </summary>
    /// <param name="touchpadAngle">Current angle of the touchpad</param>
    /// <param name="transformDirection">Diretion in which to move the object</param>
    /// <param name="obj">The object to move</param>
    /// <returns></returns>
    /// 
    private void OnUpdateTouchPadPressed(float touchpadAngle, Transform transformDirection, GameObject obj)
    {
        //If the first or last section, move towards the user
        if (touchpadAngle < 72 || touchpadAngle > 288)
        {
            obj.transform.position = obj.transform.position + transformDirection.TransformDirection(new Vector3(0, 0, 0.01F));
        }
        //Rotate about vertical axis, in the negative direction
        else if (touchpadAngle < 144)
        {
            obj.transform.Rotate(new Vector3(0, -1, 0), Space.Self);
        }
        //If the bottom section, move away from user.
        else if (touchpadAngle < 216)
        {
            obj.transform.position = obj.transform.position + transformDirection.TransformDirection(new Vector3(0, 0, -0.01F));
        }
        //Rotate about vertical axis, in the positive direction
        else if (touchpadAngle < 288)
        {
            obj.transform.Rotate(new Vector3(0, 1, 0), Space.Self);
        }

    }



    /// <summary>
    /// Set up event listeners from Controller and Pointer
    /// <returns></returns>
    /// 
    void Awake()
    {
        //VRTK_ControllerEvents controller;

        //Set up event listeners
        pointer = GetComponent<VRTK_Pointer>();
        pointer.DestinationMarkerEnter += Pointer_DestinationMarkerEnter;
        pointer.DestinationMarkerExit += Pointer_DestinationMarkerExit;

        controller = GetComponent<VRTK_ControllerEvents>();
        controller.TriggerPressed += Controller_TriggerPressed;
        controller.TouchpadAxisChanged += Controller_TouchpadAxisChanged;
        controller.TouchpadReleased += Controller_TouchpadReleased;
        controller.TouchpadPressed += Controller_TouchpadPressed;
        controller.GripClicked += Controller_GripClicked;
        
        
        helpTooltip = gameObject.transform.GetChild(0).GetComponent<VRTK_ControllerTooltips>();
        helpTooltip.ToggleTips(false);


        dataToolTip = gameObject.transform.GetChild(1).GetComponent<VRTK_ControllerTooltips>();
        dataToolTip.ToggleTips(false);


        //Anonymous function to Update the dataToolTip text.
        changeText = delegate (string x) {
            dataToolTip.UpdateText(VRTK_ControllerTooltips.TooltipButtons.TouchpadTooltip, x);
        };
                      
    }



    /// <summary>
    /// When Grip is clicked, notify InteractableObject of OnGripPressed and OnPointerLeave
    /// <returns></returns>
    /// 
    private void Controller_GripClicked(object sender, ControllerInteractionEventArgs e) {
        // Report that the current object is being grabbed. This can tell the stack it is being removed
        currentObject?.OnGripPressed();
        selectingObject = false;
        currentObject?.OnPointerLeave();
        currentObject = null;
        dataToolTip.ToggleTips(false);
    }


    /// <summary>
    /// When Grip is clicked, enable touchpad pressed flag.
    /// <returns></returns>
    /// 
    private void Controller_TouchpadPressed(object sender, ControllerInteractionEventArgs e) {
        this.touchpadPressed = true;
    }
    /// <summary>
    /// When Grip is clicked, disable touchpad pressed flag.
    /// <returns></returns>
    /// 
    private void Controller_TouchpadReleased(object sender, ControllerInteractionEventArgs e) {
        this.touchpadPressed = false;
    }
    /// <summary>
    /// When Grip is clicked, disable update current touchpad angle.
    /// <returns></returns>
    /// 
    private void Controller_TouchpadAxisChanged(object sender, ControllerInteractionEventArgs e) {
        //Set the touchpad angle to the current angle
        this.touchpadAngle = e.touchpadAngle;
    }
    /// <summary>
    /// When TriggerPressed, two options for behaviour. If user is currently pointing at object, notify object on event. Otherwise, show tooltip.
    /// <returns></returns>
    /// 
    private void Controller_TriggerPressed(object sender, ControllerInteractionEventArgs e) {

        //If still on an object

        if (currentObject != null)
        {
           //Do something on click
            currentObject.OnTriggerPressed();
         }
        else
        {
            helpTooltipState = !helpTooltipState;
            helpTooltip.ToggleTips(helpTooltipState);

        }
    }
    /// <summary>
    /// On PointerExit, notify current object, if not null. Disable the tooltip
    /// <returns></returns>
    /// 
    private void Pointer_DestinationMarkerExit(object sender, DestinationMarkerEventArgs e) {

        try {
            //Not selecting an object
            if (!selectingObject) {
                return;
            }
            selectingObject = false;

            //Tell the objet that the pointer has left
            currentObject = e.raycastHit.collider.gameObject.GetComponent<InteractableObject>();
            currentObject?.OnPointerLeave();

            //Reset tooltips
            currentObject = null;
            dataToolTip.ToggleTips(false);

        }
        catch {
            return;
        }

    }


    private void Pointer_DestinationMarkerEnter(object sender, DestinationMarkerEventArgs e) {
        //If the object being his is a InteractableMap -> Activate the tooltips, and tell the object that the pointer has entered
        try {
            currentObject = e.raycastHit.collider.gameObject.GetComponent<InteractableObject>();
            selectingObject = true;
            currentObject?.OnPointerEnter(changeText);
            dataToolTip.ToggleTips(true);
            helpTooltip.ToggleTips(false);
            helpTooltipState = false;
        }

        catch { }


    }

}
