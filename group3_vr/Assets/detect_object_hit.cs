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
    static readonly int max = 4;
    static readonly List<PointableObject> currentList = new List<PointableObject>();
    
    private bool isGripping = false;

    //The box currently being drawn
    private LineRenderer line;
    private Vector3 currentPos;
    private static GameObject plane_behind_map = null;

    void Awake()
    {
        VRTK_ControllerEvents controller;
        VRTK_Pointer pointer;

        //Set up event listeners
        pointer = GetComponent<VRTK_Pointer>();
        pointer.DestinationMarkerEnter += Pointer_DestinationMarkerEnter;
        pointer.DestinationMarkerExit += Pointer_DestinationMarkerExit;
        pointer.SelectionButtonPressed += Pointer_SelectionButtonPressed;
        pointer.DestinationMarkerHover += Pointer_DestinationMarkerHover;

        controller = GetComponent<VRTK_ControllerEvents>();
        controller.TouchpadPressed += Controller_TouchpadPressed;

        //Hide the plane
        if (plane_behind_map == null)
        {
            plane_behind_map = GameObject.Find("plane_behind_map");
            plane_behind_map.SetActive(false);
        }
    }

    private void Pointer_DestinationMarkerHover(object sender, DestinationMarkerEventArgs e)
    {
        
        currentPos = e.destinationPosition;

        if (isGripping)
        {
            var pos1 = line.GetPosition(0);

            var diff = line.GetPosition(2) - new Vector3(currentPos.x, currentPos.y, pos1.z);

            //Ignore if very small differences, remove shakiness
            if (diff.magnitude > 0.01F)
            {
                //Update the box based on current location
                var vertex1 = pos1;
                var vertex2 = new Vector3(currentPos.x, pos1.y, pos1.z);
                var vertex3 = new Vector3(currentPos.x, currentPos.y, pos1.z);
                var vertex4 = new Vector3(pos1.x, currentPos.y, pos1.z);

                Vector3[] list = { vertex1, vertex2, vertex3, vertex4, vertex1 };

                line.SetPositions(list);
            }

        }

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
        if (currentList.Count > 0) {

            //IEnumerable<string> currentNames = currentList.Select(x => x.getName());
            for (int i = 0; i < 4; i++)
            {
                GameObject gameObject = GameObject.Find("object" + i.ToString());
                draw_object main = (draw_object)gameObject.GetComponent(typeof(draw_object));
                main.updateScene(currentList);
            }
            currentList.Clear();
            //Enter New Scene

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

            currentObject.onPointEnter();
               

    }


    // Update is called once per frame
    void Update()
    {

        
        
    }
}
