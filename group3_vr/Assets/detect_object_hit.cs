using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using VRTK;
using static draw_object;
using System;

public class detect_object_hit : MonoBehaviour
{

    private PointableObject currentObject;

    private VRTK_Pointer pointer;

    //This list should be static, as this will allow it to work over both controllers
    static readonly int max = 4;
    static bool updateScene = true;
    static readonly List<PointableObject> currentList = new List<PointableObject>();

    private bool currentlyGripped = false;

    private int currentFrameCount = -1;

    private Tuple<int,int> startPos = new Tuple<int, int>(-1, -1);

    private Tuple<int, int> currentPos = new Tuple<int, int>(-1, -1);

    private GameObject plane_behind_map;



    // Start is called before the first frame update
    void Awake()
    {
        // VRTK_Pointer pointer;
        VRTK_ControllerEvents controller;

        pointer = GetComponent<VRTK_Pointer>();
        pointer.DestinationMarkerEnter += Pointer_DestinationMarkerEnter;
        pointer.DestinationMarkerExit += Pointer_DestinationMarkerExit;
        pointer.SelectionButtonPressed += Pointer_SelectionButtonPressed;

        controller = GetComponent<VRTK_ControllerEvents>();
        controller.TouchpadPressed += Controller_TouchpadPressed;
        controller.GripPressed += Controller_GripPressed;
        controller.GripReleased += Controller_GripReleased;
        controller.GripClicked += Controller_GripClicked;

        plane_behind_map = GameObject.Find("plane_behind_map");
        plane_behind_map.SetActive(false);

    }

    private void Controller_GripClicked(object sender, ControllerInteractionEventArgs e)
    {
        return;
        throw new NotImplementedException();
    }

    private void Controller_GripReleased(object sender, ControllerInteractionEventArgs e)
    {
        currentFrameCount = -1;
    }

    private void Controller_GripPressed(object sender, ControllerInteractionEventArgs e)
    {
        currentFrameCount = 0;
        Debug.Log(GameObject.Find("plane_behind_map"));
        GameObject.Find("plane_behind_map").SetActive(true);
    }



    private void Controller_TouchpadPressed(object sender, ControllerInteractionEventArgs e)
    {
        if (currentFrameCount != -1)
        {
            return;
        }
        Debug.Log(currentList.Count());
        //If still on an object
        if (currentObject != null)
        {
            //Do something on click
            if (currentObject.onClick())
            {
                currentList.Add(currentObject);

                //If too many items selected, remove the first one.
                if (currentList.Count > max)
                {

                    var objToRemove = currentList[0];
                    currentList.RemoveAt(0);
                    objToRemove.onClick();

                }
            }
            else
            {
                currentList.Remove(currentObject);

            }
        }
    }

    private void Pointer_SelectionButtonPressed(object sender, ControllerInteractionEventArgs e)
    {
        if (currentFrameCount != -1)
        {
            return;
        }
        if (currentList.Count > 0) {

            //IEnumerable<string> currentNames = currentList.Select(x => x.getName());

            GameObject gameObject = GameObject.Find("object");
            draw_object main = (draw_object)gameObject.GetComponent(typeof(draw_object));
            main.updateScene(currentList);
            currentList.Clear();
            //Enter New Scene

        }
    }
    private void Pointer_DestinationMarkerExit(object sender, DestinationMarkerEventArgs e)
    {
        if (currentFrameCount != -1)
        {
            return;
        }
        currentObject = e.target.GetComponent("PointableObject") as PointableObject;
        currentObject.onPointLeave();
        currentObject = null;

    }

    private void Pointer_DestinationMarkerEnter(object sender, DestinationMarkerEventArgs e)
    {
        if (currentFrameCount != -1)
        {
            return;
        }
        currentObject = e.target.GetComponent("PointableObject") as PointableObject;
        currentObject.onPointEnter();
    

    }


    // Update is called once per frame
    void Update()
    {
        if (currentFrameCount >= 0)
        {
            currentFrameCount++;
            if (currentFrameCount % 5 == 0)
            {
            }
        }
        
    }
}
