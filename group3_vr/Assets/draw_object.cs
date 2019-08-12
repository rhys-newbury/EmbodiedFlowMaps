using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine.EventSystems;
using VRTK;
using System;

public class draw_object : MonoBehaviour
{

    public static int currentLevel = 0;
    private static bool startUp = true;
    private static List<draw_object> currentList = new List<draw_object>();

    private void Start()
    {      

        VRTK_InteractableObject interactObject = gameObject.AddComponent(typeof(VRTK_InteractableObject)) as VRTK_InteractableObject;
        VRTK_InteractHaptics interactHaptics = gameObject.AddComponent(typeof(VRTK_InteractHaptics)) as VRTK_InteractHaptics;
        VRTK.GrabAttachMechanics.VRTK_ChildOfControllerGrabAttach grabAttach = gameObject.AddComponent(typeof(VRTK.GrabAttachMechanics.VRTK_ChildOfControllerGrabAttach)) as VRTK.GrabAttachMechanics.VRTK_ChildOfControllerGrabAttach;
        VRTK.SecondaryControllerGrabActions.VRTK_SwapControllerGrabAction grabAction = gameObject.AddComponent(typeof(VRTK.SecondaryControllerGrabActions.VRTK_SwapControllerGrabAction)) as VRTK.SecondaryControllerGrabActions.VRTK_SwapControllerGrabAction;
        Rigidbody rigidBody = gameObject.AddComponent(typeof(Rigidbody)) as Rigidbody;

        interactObject.isGrabbable = true;
        interactObject.holdButtonToGrab = false;
        interactObject.grabAttachMechanicScript = grabAttach;
        interactObject.secondaryGrabActionScript = grabAction;

        rigidBody.useGravity = false;
        rigidBody.isKinematic = true;

        Debug.Log("Creating Game Object");

        if (startUp)
        {
            string file = "C:\\Users\\FIT3161\\Desktop\\group3\\group3_vr\\mapGeoJSON\\American_map.txt";
            mapRenderer map = new mapRenderer();
            map.drawSingular(this.gameObject, file,0);

            currentList.Add(this);
            startUp = false;
        }


    }
    public static void clear()
    {
        foreach (draw_object drawObject in currentList) {
            int childs = drawObject.gameObject.transform.childCount;
            for (var i = childs - 1; i >= 0; i--)
            {
                Destroy(drawObject.gameObject.transform.GetChild(i).gameObject);
            }
        }
        currentList.Clear();
    }



    internal void draw(PointableObject pointableObject)
    {
        string file = "C:\\Users\\FIT3161\\Desktop\\group3\\group3_vr\\mapGeoJSON\\data3.txt";
        mapRenderer map = new mapRenderer();

        map.drawSingular(this.gameObject, file,currentLevel);

        currentList.Add(this);
      

    }
}


