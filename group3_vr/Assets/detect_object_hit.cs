using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using VRTK;
using static draw_object;
using System;

public class detect_object_hit : MonoBehaviour
{

    private VRTK_Pointer pointer;
    private PointableObject currentObject;

    //This list should be static, as this will allow it to work over both controllers
    static readonly int max = 4;
    static bool updateScene = true;
    static readonly List<PointableObject> currentList = new List<PointableObject>();


    // Start is called before the first frame update
    void Awake()
    {
        pointer = GetComponent<VRTK_Pointer>();
        pointer.DestinationMarkerEnter += Pointer_DestinationMarkerEnter;
        pointer.DestinationMarkerExit += Pointer_DestinationMarkerExit;
        pointer.ActivationButtonPressed += Pointer_ActivationButtonPressed;
        pointer.SelectionButtonPressed += Pointer_SelectionButtonPressed;  

    }

    private void Pointer_SelectionButtonPressed(object sender, ControllerInteractionEventArgs e)
    {
        if (currentList.Count > 0) {

            //IEnumerable<string> currentNames = currentList.Select(x => x.getName());

            GameObject gameObject = GameObject.Find("object");
            draw_object main = (draw_object)gameObject.GetComponent(typeof(draw_object));
            main.updateScene(currentList);
            currentList.Clear();
            //Enter New Scene

        }
    }

    private void Pointer_ActivationButtonPressed(object sender, ControllerInteractionEventArgs e)
    {
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
        }
    }

    private void Pointer_DestinationMarkerExit(object sender, DestinationMarkerEventArgs e)
    {
        currentObject = e.target.GetComponent("PointableObject") as PointableObject;
        currentObject.onPointLeave();
        currentObject = null;

    }

    private void Pointer_DestinationMarkerEnter(object sender, DestinationMarkerEventArgs e)
    {

        currentObject = e.target.GetComponent("PointableObject") as PointableObject;
        currentObject.onPointEnter();
    

    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
