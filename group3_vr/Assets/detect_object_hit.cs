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
    private PointableObject currentObject;

    //This list should be static, as this will allow it to work over both controllers
    static readonly List<PointableObject> currentList = new List<PointableObject>();
    
    //The box currently being drawn
    private LineRenderer line;
    private VRTK_ControllerTooltips help_tooltip;
    private bool help_tooltip_state = false;
    private VRTK_ControllerTooltips data_tooltip;
    private Action<string> change_text;
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

            if (touchpadPressed) touchPadMove(obj);

            var Time = System.DateTime.Now.Ticks;
           
            //Time Limit between frames
            if (Time-oldTime < 300000) {

                Vector3 newPos = obj.transform.position;
                Vector3 delta = newPos - oldPos;
                float speed = delta.magnitude / Time * (Mathf.Pow(10,20));

                //Add to the velocity buffee
                if (oldPos.magnitude != 0)
                {
                    velocityBuffer.RemoveAt(0);
                    velocityBuffer.Add(speed);
                }
                oldPos = newPos;

                //Condition for a throw to occur.
                if (velocityBuffer.Sum() > 150 && velocityBuffer.Where((x) => x > 20).Count() > 3) {
                    onThrow(obj);
                }

            }
            else {
                //Reset the buffer
                velocityBuffer = (new float[] { 0, 0, 0, 0, 0 }).ToList();
            }
            //Update the time
            oldTime = Time;
        }
        catch { }

    }

    void onThrow(GameObject obj)
    {
        int level = -1;

        foreach (var item in obj.GetComponentsInChildren<PointableObject>())
        {
            item.delete();
            item.parent.deselect();
            level = level == -1 ? item.getLevel() : level;
        }

        if (level > 0) GameObject.Destroy(obj);
    }

    void touchPadMove(GameObject obj)
    {
        //Left/Right -> Rotate
        //Up/Down -> Back and forth in direction of pointer.
        if (touchpadAngle < 72 || touchpadAngle > 288)
        {
            obj.transform.position = obj.transform.position + pointer.transform.parent.transform.TransformDirection(new Vector3(0, 0, 0.01F));
        }
        else if (touchpadAngle < 144)
        {
            obj.transform.Rotate(new Vector3(0, -1, 0), Space.Self);
        }
        else if (touchpadAngle < 216)
        {
            obj.transform.position = obj.transform.position + pointer.transform.parent.transform.TransformDirection(new Vector3(0, 0, -0.01F));
        }
        else if (touchpadAngle < 288)
        {
            obj.transform.Rotate(new Vector3(0, 1, 0), Space.Self);
        }
     

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

        //Get the tooltips
        help_tooltip = gameObject.transform.GetChild(0).GetComponent<VRTK_ControllerTooltips>();
        help_tooltip.ToggleTips(false);

        data_tooltip = gameObject.transform.GetChild(1).GetComponent<VRTK_ControllerTooltips>();
        data_tooltip.ToggleTips(false);


        //Anonymous function to Update the data_tooltip text.
        change_text = delegate (string x) {
            data_tooltip.UpdateText(VRTK_ControllerTooltips.TooltipButtons.TouchpadTooltip, x);
        };


        
    }

    private void Controller_GripClicked(object sender, ControllerInteractionEventArgs e) {
        // Report that the current object is being grabbed. This can tell the stack it is being removed
        if (currentObject != null)
        {
            currentObject.report_grabbed(true);
        }
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
            
            if (currentObject.onClick()) {
                //Add it to the list of current objects
                currentList.Add(currentObject);

                //Create the gameObject for the map and then draw it.
                GameObject gameObject = new GameObject();
                draw_object main = gameObject.AddComponent(typeof(draw_object)) as draw_object;
                main.draw(currentObject, currentObject.getLevel()+1);

            }
            else {
                //On deselect Remove it from the current list and delete the object and its children 
                currentList.Remove(currentObject);
                currentObject.deleteChildren();
            }
        }
        else
        {
            help_tooltip_state = !help_tooltip_state;
            help_tooltip.ToggleTips(help_tooltip_state);

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
            currentObject = e.raycastHit.collider.gameObject.GetComponent("PointableObject") as PointableObject;
            currentObject.onPointLeave();

            //Reset tooltips
            currentObject = null;
            data_tooltip.ToggleTips(false);

        }
        catch {
            return;
        }

    }

    private void Pointer_DestinationMarkerEnter(object sender, DestinationMarkerEventArgs e) {
        //If the object being his is a PointableObject -> Activate the tooltips, and tell the object that the pointer has entered
        try {
            currentObject = e.raycastHit.collider.gameObject.GetComponent("PointableObject") as PointableObject;
            selectingObject = true;
            currentObject.onPointEnter(change_text);
            data_tooltip.ToggleTips(true);
            help_tooltip.ToggleTips(false);
            help_tooltip_state = false;
        }
        catch { }
    }

}
