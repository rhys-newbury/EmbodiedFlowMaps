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

    void Awake()
    {
        VRTK_ControllerEvents controller;
        VRTK_Pointer pointer;

        //Set up event listeners
        pointer = GetComponent<VRTK_Pointer>();
        pointer.DestinationMarkerEnter += Pointer_DestinationMarkerEnter;
        pointer.DestinationMarkerExit += Pointer_DestinationMarkerExit;
        pointer.SelectionButtonPressed += Pointer_SelectionButtonPressed;

        controller = GetComponent<VRTK_ControllerEvents>();
        controller.TouchpadPressed += Controller_TouchpadPressed;

        help_tooltip = gameObject.transform.GetChild(0).GetComponent<VRTK_ControllerTooltips>();
        help_tooltip.ToggleTips(false);

          
        change_text = delegate (string x)
        {
            Debug.Log(x);
            data_tooltip.UpdateText(VRTK_ControllerTooltips.TooltipButtons.TouchpadTooltip, x);
        };


        data_tooltip = gameObject.transform.GetChild(1).GetComponent<VRTK_ControllerTooltips>();
        data_tooltip.ToggleTips(false);

    }


    private void Controller_TouchpadPressed(object sender, ControllerInteractionEventArgs e)
    {
    //If still on an object
        if (currentObject != null)
        {
            //Do something on click
            if (currentObject.onClick())
            {
                currentList.Add(currentObject);
            }
            else
            {
                currentList.Remove(currentObject);
            }
        }
        else
        {
            help_tooltip_state = !help_tooltip_state;
            help_tooltip.ToggleTips(help_tooltip_state);

        }
    }

    private void Pointer_SelectionButtonPressed(object sender, ControllerInteractionEventArgs e)
    {
        draw_object.clear();
        if (currentList.Count > 0)
        {
            draw_object.currentLevel++;
            for (int i = 0; i < currentList.Count; i++)
            {
                GameObject gameObject = new GameObject();
                draw_object main = gameObject.AddComponent(typeof(draw_object)) as draw_object;
                main.draw(currentList[i]);
            }
        currentList.Clear();
    }
}
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
