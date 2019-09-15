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

    private Vector3 prevPos = new Vector3();

    private List<LineRenderer> lines = new List<LineRenderer>();

    private static Dictionary<String, Dictionary<string, bool>> currently_joined = new Dictionary<String, Dictionary<string, bool>>();
    private string parentName;

    private void Start()
    {      

        VRTK_InteractableObject interactObject = gameObject.AddComponent(typeof(VRTK_InteractableObject)) as VRTK_InteractableObject;
        VRTK_InteractHaptics interactHaptics = gameObject.AddComponent(typeof(VRTK_InteractHaptics)) as VRTK_InteractHaptics;
        VRTK.GrabAttachMechanics.VRTK_ChildOfControllerGrabAttach grabAttach = gameObject.AddComponent(typeof(VRTK.GrabAttachMechanics.VRTK_ChildOfControllerGrabAttach)) as VRTK.GrabAttachMechanics.VRTK_ChildOfControllerGrabAttach;
        VRTK.SecondaryControllerGrabActions.VRTK_SwapControllerGrabAction grabAction = gameObject.AddComponent(typeof(VRTK.SecondaryControllerGrabActions.VRTK_SwapControllerGrabAction)) as VRTK.SecondaryControllerGrabActions.VRTK_SwapControllerGrabAction;
        VRTK.SecondaryControllerGrabActions.VRTK_AxisScaleGrabAction scaleAction = gameObject.AddComponent(typeof(VRTK.SecondaryControllerGrabActions.VRTK_AxisScaleGrabAction)) as VRTK.SecondaryControllerGrabActions.VRTK_AxisScaleGrabAction;
        //VRTK.VRTK_SlideObjectControlAction slide = gameObject.AddComponent(typeof(VRTK.VRTK_SlideObjectControlAction)) as VRTK.VRTK_SlideObjectControlAction;
        //VRTK.VRTK_TouchpadControl control = gameObject.AddComponent(typeof(VRTK.VRTK_TouchpadControl)) as VRTK.VRTK_TouchpadControl;
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

           // string file = "C:\\Users\\Jesse\\Documents\\group3\\group3_vr\\mapGeoJSON\\America.txt";

            dataAccessor.load();

           string file = "C:\\Users\\FIT3162\\Desktop\\group3_vr\\mapGeoJSON\\America.txt";
            this.parentName = "America";

            mapRenderer map = new mapRenderer();
            map.drawMultiple(this.gameObject, file,0);

            currentList.Add(this);
            startUp = false;

        }


    }

    void addLines(LineRenderer l)
    {
        this.lines.Add(l);
    }
    static void setLinkStatus(string i1, string i2, bool val)
    {
        isLinked(i1, i2);
        currently_joined[i1][i2] = val;
        currently_joined[i2][i1] = val;
    }
    static bool isLinked(string i1, string i2) 
    {
        string access1 = i1;
        string access2 = i2;


       if (!(currently_joined.ContainsKey(access1)))
       {
            currently_joined[access1] = new Dictionary<string, bool>();
            currently_joined[access1][access2] = false;
        }
       else if (!(currently_joined[access1].ContainsKey(access2)))
        {
            currently_joined[access1][access2] = false;
        }

        if (!(currently_joined.ContainsKey(access2)))
        {
            currently_joined[access2] = new Dictionary<string, bool>();
            currently_joined[access2][access1] = false;
        }
        else if (!(currently_joined[access2].ContainsKey(access1)))
        {
            currently_joined[access2][access1] = false;
        }

        return currently_joined[access1][access2];
    }

    // Update is called once per frame
    void Update()
    {
        if (prevPos == this.transform.position)
        {
            return;
        }
        prevPos = this.transform.position;

        draw_object[] l = GameObject.FindObjectsOfType(typeof(draw_object)) as draw_object[];

        foreach (var i in lines)
        {
            GameObject.Destroy(i);
        }

        foreach (var item in l)
        {
            if (item != this)
            {

                float seperation = (this.transform.position - item.transform.position).magnitude;
                Debug.Log(seperation);
                if (seperation > 3) {
                    setLinkStatus(this.parentName, item.parentName, false);
                }
                else if (seperation < 0.5 || isLinked(this.parentName, item.parentName))

                {
                    setLinkStatus(this.parentName, item.parentName, true);

                    var selected1 = this.GetComponentsInChildren<PointableObject>().Where(x => x.isSelected()).ToArray();
                    var selected2 = item.GetComponentsInChildren<PointableObject>().Where(x => x.isSelected()).ToArray();

                    if (selected1.Count() > 0 && selected2.Count() > 0)
                    {
                        foreach (var origin in selected1)
                        {
                            foreach (var destination in selected2)
                            {

                                var newObj = new GameObject();

                                var line = newObj.AddComponent(typeof(LineRenderer)) as LineRenderer;
                                line.transform.SetParent(this.transform);
                                line.useWorldSpace = true;
                                line.startWidth = 0.005F;
                                line.endWidth = 0.005F;

                                Vector3 p0 = origin.transform.parent.transform.position - origin.transform.parent.transform.TransformVector(new Vector3(0, 0, 0.05F));
                                Vector3 p3 = destination.transform.parent.transform.position - origin.transform.parent.transform.TransformVector(new Vector3(0, 0, 0.05F));

                                float dist = (p0 - p3).magnitude;

                                Vector3 p1 = p0 + origin.transform.parent.transform.TransformVector(new Vector3(0, 0, dist));

                                Vector3 p2 = p3 + destination.transform.parent.transform.TransformVector(new Vector3(0, 0, dist));

                                Bezier test = new Bezier(p0, p1, p2, p3, 50);

                                line.positionCount = test.points.Length;
                                line.SetPositions(test.points);

                                lines.Add(line);
                                item.addLines(line);


                            }
                        }
                    }

                }


            }

        }
    }


    internal void draw(PointableObject pointableObject, int level)
    {
        
        string file;
        mapRenderer map = new mapRenderer();
        this.parentName = pointableObject.name;


        if (level == (int)mapRenderer.LEVEL.STATE_LEVEL)
        {
             file = "C:\\Users\\FIT3162\\Desktop\\group3_vr\\mapGeoJSON\\state_map\\" + pointableObject.name + ".json";


            map.drawMultiple(this.gameObject, file, level, pointableObject);


        }
        else
        {
            file = "C:\\Users\\FIT3162\\Desktop\\group3_vr\\mapGeoJSON\\state_map\\" + pointableObject.parentName + ".json";
            foreach (var line in System.IO.File.ReadAllLines(file)) {
                if (line.Contains(pointableObject.name))
                {
                    map.drawSingular(this.gameObject, line, pointableObject.parentName, level, pointableObject);
                    break;
                }
            }

        }



        currentList.Add(this);
      

    }
}

