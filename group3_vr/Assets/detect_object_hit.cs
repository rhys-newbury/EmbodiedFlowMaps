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
        controller.GripPressed += Controller_GripPressed;
        controller.GripReleased += Controller_GripReleased;
        controller.GripClicked += Controller_GripClicked;

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

    private void Controller_GripClicked(object sender, ControllerInteractionEventArgs e)
        //Draw the line on click
    {
        
        const float dims = 0.05F;
        
        line = plane_behind_map.AddComponent<LineRenderer>();

        line.useWorldSpace = true;

        //Put in front of map
        currentPos.z += 0.25F;

        var vertex1 = new Vector3(currentPos.x, currentPos.y, currentPos.z);
        var vertex2 = new Vector3(currentPos.x+dims, currentPos.y, currentPos.z);
        var vertex3 = new Vector3(currentPos.x + dims, currentPos.y+dims, currentPos.z);
        var vertex4 = new Vector3(currentPos.x, currentPos.y+dims, currentPos.z);

        Vector3[] list = { vertex1, vertex2, vertex3, vertex4, vertex1 };

        line.positionCount = 5;
        line.SetPositions(list);

        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = Color.red;
        line.endColor = Color.yellow;
        line.startWidth = 0.03F;
        line.endWidth = 0.03F;
        line.alignment = LineAlignment.TransformZ;

    }

    private void Controller_GripReleased(object sender, ControllerInteractionEventArgs e)
    {
        //Hide Plane on release
        plane_behind_map.SetActive(false);
        isGripping = false;
        selectingObject = false;


        //TODO : @Stephen
        //Write something to zoom in to box once positioning is correct.
        //var obj = GameObject.Find("object");

        //var pos1 = line.GetPosition(0);
        //var pos2 = line.GetPosition(2);

        //var center = pos1 + pos2;

        //obj.transform.position = new Vector3(center.x / 2, center.y / 2, center.z / 2);

        Destroy(line);


    }

    private void Controller_GripPressed(object sender, ControllerInteractionEventArgs e)
    {
        //On grip press hide plane
        isGripping = true;
        plane_behind_map.SetActive(true);
        selectingObject = false;
    }



    private void Controller_TouchpadPressed(object sender, ControllerInteractionEventArgs e)
    {
        if (isGripping)
        {
            return;
        }

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
        if (isGripping)
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
        if (isGripping || !selectingObject)
        {
            return;
        }
        selectingObject = false;
        currentObject = e.target.GetComponent("PointableObject") as PointableObject;
        currentObject.onPointLeave();
        currentObject = null;

    }

    private void Pointer_DestinationMarkerEnter(object sender, DestinationMarkerEventArgs e)
    {
        if (isGripping)
        {
            return;
        }
        selectingObject = true;
        currentObject = e.target.GetComponent("PointableObject") as PointableObject;
        currentObject.onPointEnter();
    

    }


    // Update is called once per frame
    void Update()
    {

        
        
    }
}
