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
        VRTK.SecondaryControllerGrabActions.VRTK_AxisScaleGrabAction scaleAction = gameObject.AddComponent(typeof(VRTK.SecondaryControllerGrabActions.VRTK_AxisScaleGrabAction)) as VRTK.SecondaryControllerGrabActions.VRTK_AxisScaleGrabAction;
        Rigidbody rigidBody = gameObject.AddComponent(typeof(Rigidbody)) as Rigidbody;

        interactObject.isGrabbable = true;
        interactObject.holdButtonToGrab = false;
        interactObject.grabAttachMechanicScript = grabAttach;
        interactObject.secondaryGrabActionScript = scaleAction;

        grabAttach.precisionGrab = true;

        rigidBody.useGravity = false;
        rigidBody.isKinematic = true;

        scaleAction.lockAxis = new Vector3State(false, false, true);
        scaleAction.uniformScaling = true;
        

        if (startUp)
        {
            dataAccessor.load();

            string file = "C:\\Users\\newbu\\vr\\group3_vr\\mapGeoJSON\\America.txt";

            mapRenderer map = new mapRenderer();
            map.drawMultiple(this.gameObject, file,0);

            currentList.Add(this);
            startUp = false;
        }


    }
    public static void clear()
    {
        //foreach (draw_object drawObject in currentList) {
        //    //int childs = drawObject.gameObject.transform.childCount;
        //    //for (var i = childs - 1; i >= 0; i--)
        //    //{
        //    //    Destroy(drawObject.gameObject.transform.GetChild(i).gameObject);
        //    //}
        //    //Destroy()
        //    Destroy(drawObject.gameObject);
        //}
       
      //      currentList.Clear();
    }



    internal void draw(PointableObject pointableObject, int level)
    {
        Debug.Log(pointableObject.name);
        string file;
        mapRenderer map = new mapRenderer();


        if (level == (int)mapRenderer.LEVEL.STATE_LEVEL)
        {
             file = "C:\\Users\\newbu\\vr\\group3_vr\\mapGeoJSON\\state_map\\" + pointableObject.name + ".json";


            map.drawMultiple(this.gameObject, file, level);

        }
        else
        {
            file = "C:\\Users\\newbu\\vr\\group3_vr\\mapGeoJSON\\state_map\\" + pointableObject.parentName + ".json";
            foreach (var line in System.IO.File.ReadAllLines(file)) {
                if (line.Contains(pointableObject.name))
                {
                    map.drawSingular(this.gameObject, line, pointableObject.parentName, level);
                    break;
                }
            }

        }



        currentList.Add(this);
      

    }
}


