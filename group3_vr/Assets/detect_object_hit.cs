using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using VRTK;
using static draw_object;
using System;

//This class is in charge of the laser pointers and interacting with them

public class detect_object_hit : MonoBehaviour
{
    private bool selectingObject;
    private Pointable currentObject;
        

    
    //The box currently being drawn
    private LineRenderer line;
    private VRTK_ControllerTooltips helpTooltip;
    private bool helpTooltipState = false;
    private VRTK_ControllerTooltips dataToolTip;
    private Action<string> changeText;
    private VRTK_ControllerEvents controller;
    private long oldTime = 0;
    private Vector3 oldPos = new Vector3(0,0,0);
    private VRTK_Pointer pointer;


    private List<float> velocityBuffer = (new float[] { 0, 0, 0, 0, 0 }).ToList();
    private bool touchpadPressed = false;
    private float touchpadAngle;

    private void Update()
    {
        try
        {

            var obj = GetComponent<VRTK_InteractGrab>().GetGrabbedObject();

            if (touchpadPressed) TouchPadMove(obj);

            long time = DateTime.Now.Ticks;
           
            //Time Limit between frames
            if (time-oldTime < 300000) {

                Vector3 newPos = obj.transform.position;
                Vector3 delta = newPos - oldPos;
                float speed = delta.magnitude / time * (Mathf.Pow(10,20));

                //Add to the velocity buffee
                if (oldPos.magnitude != 0)
                {
                    velocityBuffer.RemoveAt(0);
                    velocityBuffer.Add(speed);
                }
                oldPos = newPos;
                //Condition for a throw to occur.
                if (velocityBuffer.Sum() > 150 && velocityBuffer.Count((x) => x > 20) > 3) {
                    obj.GetComponent<Pointable>()?.OnThrow();
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


    void TouchPadMove(GameObject obj)
    {
        currentObject = obj.GetComponent("Pointable") as Pointable;
        currentObject?.OnUpdateTouchPadPressed(touchpadAngle, pointer.transform.parent.transform);
    }

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


        controller.GripClicked += Controller_GripClicked;

        help_tooltip = gameObject.transform.GetChild(0).GetComponent<VRTK_ControllerTooltips>();
        help_tooltip.ToggleTips(false);


        dataToolTip = gameObject.transform.GetChild(1).GetComponent<VRTK_ControllerTooltips>();
        dataToolTip.ToggleTips(false);


        //Anonymous function to Update the dataToolTip text.
        changeText = delegate (string x) {
            dataToolTip.UpdateText(VRTK_ControllerTooltips.TooltipButtons.TouchpadTooltip, x);
        };
                      
    }

    private void Controller_GripClicked(object sender, ControllerInteractionEventArgs e) {
        // Report that the current object is being grabbed. This can tell the stack it is being removed
        currentObject?.OnGripPressed();
        selectingObject = false;
        currentObject?.onPointLeave();
        currentObject = null;
        data_tooltip.ToggleTips(false);
    }



    private void Controller_TouchpadPressed(object sender, ControllerInteractionEventArgs e) {
        this.touchpadPressed = true;
    }

    private void Controller_TouchpadReleased(object sender, ControllerInteractionEventArgs e) {
        this.touchpadPressed = false;
    }

    private void Controller_TouchpadAxisChanged(object sender, ControllerInteractionEventArgs e) {
        //Set the touchpad angle to the current angle
        this.touchpadAngle = e.touchpadAngle;
    }

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

    private void Pointer_DestinationMarkerExit(object sender, DestinationMarkerEventArgs e) {

        try {
            //Not selecting an object
            if (!selectingObject) {
                return;
            }
            selectingObject = false;

            //Tell the objet that the pointer has left
            currentObject = e.raycastHit.collider.gameObject.GetComponent<Pointable>();
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
        //If the object being his is a PointableObject -> Activate the tooltips, and tell the object that the pointer has entered
        try {
            currentObject = e.raycastHit.collider.gameObject.GetComponent<Pointable>();
            selectingObject = true;
            currentObject?.OnPointerEnter(changeText);
            dataToolTip.ToggleTips(true);
            helpTooltip.ToggleTips(false);
            helpTooltipState = false;
        }
        catch { }
    }

}
