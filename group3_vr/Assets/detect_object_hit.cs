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
            //var pointable = obj.GetComponent<PointableObject>();


            if (touchpadPressed)
            {
                //var t = VRTK.VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.Headset);
                obj.transform.position = obj.transform.position + pointer.transform.parent.transform.TransformDirection(new Vector3(0, 0, (touchpadAngle < 90 || touchpadAngle > 270) ? 0.01F : -0.01F));
            }

            Rigidbody rb = obj.GetComponent<Rigidbody>();
            Vector3 v3Velocity = rb.velocity;


            var Time = System.DateTime.Now.Ticks;
           // Debug.Log(Time - oldTime);

            if (Time-oldTime < 300000)
            {

                var newPos = obj.transform.position;
                var delta = newPos - oldPos;

                var speed = delta.magnitude / Time * (Mathf.Pow(10,20));
                if (oldPos.magnitude != 0)
                {
                    //Debug.Log(speed);

                    velocityBuffer.RemoveAt(0);
                    velocityBuffer.Add(speed);
                }
                oldPos = newPos;


                if (velocityBuffer.Sum() > 150 && velocityBuffer.Where((x) => x > 20).Count() > 3)
                {
                    foreach (var i in velocityBuffer)
                    {
                        Debug.Log("Buffer: " + i.ToString());
                    }
                    int level = -1;
                    //pointable.delete();

                    foreach (var item in obj.GetComponentsInChildren<PointableObject>())
                    {
                        item.delete();
                        item.parent.deselect();
                        level = level == -1 ? item.getLevel() : level;
                    }
                    
                    if (level > 0) GameObject.Destroy(obj);

                }

            }
            else
            {
                velocityBuffer = (new float[] { 0, 0, 0, 0, 0 }).ToList();
            }

            oldTime = Time;

            //Debug.Log(v3Velocity.magnitude);
        }
        catch { }

    }
    void Awake()
    {
        //VRTK_ControllerEvents controller;

        //Set up event listeners
        pointer = GetComponent<VRTK_Pointer>();
        pointer.DestinationMarkerEnter += Pointer_DestinationMarkerEnter;
        pointer.DestinationMarkerExit += Pointer_DestinationMarkerExit;

        //pointer.SelectionButtonPressed += Pointer_SelectionButtonPressed;

        controller = GetComponent<VRTK_ControllerEvents>();
        controller.TriggerPressed += Controller_TriggerPressed;

        controller.TouchpadAxisChanged += Controller_TouchpadAxisChanged;
        controller.TouchpadReleased += Controller_TouchpadReleased;
        controller.TouchpadPressed += Controller_TouchpadPressed;

        help_tooltip = gameObject.transform.GetChild(0).GetComponent<VRTK_ControllerTooltips>();
        help_tooltip.ToggleTips(false);

          
        change_text = delegate (string x)
        {
            data_tooltip.UpdateText(VRTK_ControllerTooltips.TooltipButtons.TouchpadTooltip, x);
        };


        data_tooltip = gameObject.transform.GetChild(1).GetComponent<VRTK_ControllerTooltips>();
        data_tooltip.ToggleTips(false);

    }

    private void Controller_TouchpadPressed(object sender, ControllerInteractionEventArgs e)
    {
        this.touchpadPressed = true;
    }

    private void Controller_TouchpadReleased(object sender, ControllerInteractionEventArgs e)
    {
        this.touchpadPressed = false;
    }

    private void Controller_TouchpadAxisChanged(object sender, ControllerInteractionEventArgs e)
    {
        this.touchpadAngle = e.touchpadAngle;
    }

    private void Controller_TriggerPressed(object sender, ControllerInteractionEventArgs e)
    {
    //If still on an object
        if (currentObject != null)
        {
            //Do something on click
            if (currentObject.onClick())
            {
                currentList.Add(currentObject);
                GameObject gameObject = new GameObject();
                draw_object main = gameObject.AddComponent(typeof(draw_object)) as draw_object;
                main.draw(currentObject, currentObject.getLevel()+1);
            }
            else
            {
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

    //private void Pointer_SelectionButtonPressed(object sender, ControllerInteractionEventArgs e)
    //{
    //   // draw_object.clear();
    //    if (currentList.Count > 0)
    //    {
    //        draw_object.currentLevel++;
    //        for (int i = 0; i < currentList.Count; i++)
    //        {

    //            main.draw(currentList[i]);
    //        }
    //   // currentList.Clear();
    //}
    //}
    private void Pointer_DestinationMarkerExit(object sender, DestinationMarkerEventArgs e)
    {
        try
        {
            if (!selectingObject)
            {
                return;
            }
            selectingObject = false;
            currentObject = e.raycastHit.collider.gameObject.GetComponent("PointableObject") as PointableObject;
            currentObject.onPointLeave();

            currentObject = null;
            data_tooltip.ToggleTips(false);

        }
        catch
        {
            return;
        }

    }

    private void Pointer_DestinationMarkerEnter(object sender, DestinationMarkerEventArgs e)
    {

        currentObject = e.raycastHit.collider.gameObject.GetComponent("PointableObject") as PointableObject;
        selectingObject = true;
        currentObject.onPointEnter(change_text);
        data_tooltip.ToggleTips(true);
        help_tooltip.ToggleTips(false);
        help_tooltip_state = false;
    }

}
