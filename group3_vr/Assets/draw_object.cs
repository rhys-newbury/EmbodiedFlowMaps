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
    private static bool _startUp = true;
    private static List<draw_object> currentList = new List<draw_object>();

    private Vector3 prevPos = new Vector3(0,0,0);

    private List<LineRenderer> lines = new List<LineRenderer>();

    private static Dictionary<String, Dictionary<string, bool>> currently_joined = new Dictionary<String, Dictionary<string, bool>>();
    private string parentName;
    public static bool update = false;


    public static List<draw_object> positonStack = new List<draw_object>();
    private bool moved = false;

    private Action<bool> reportGrabbed;

    
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



        reportGrabbed = delegate (bool x)
        {
            if (!(this.moved))
            {
                this.stack_remove(this);
                this.moved = true;
            }
        };







        if (_startUp)
        {

           // string file = "C:\\Users\\Jesse\\Documents\\group3\\group3_vr\\mapGeoJSON\\America.txt";

            dataAccessor.load();

           string file = "D:\\vr\\group3_vr\\mapGeoJSON\\America.txt";
            this.parentName = "America";

            mapRenderer map = new mapRenderer();
            map.drawMultiple(this.gameObject, reportGrabbed, file,0);

            currentList.Add(this);
            _startUp = false;

        }


    }

    void AddLines(LineRenderer l)
    {
        this.lines.Add(l);
    }
    static void SetLinkStatus(string i1, string i2, bool val)
    {
        isLinked(i1, i2);
        currently_joined[i1][i2] = val;
        currently_joined[i2][i1] = val;
    }

    internal static void DeselectState()
    {
        update = true;

    }

    internal static float getYPosition(int i)
    {
        return i * 0.2F;
    }

    static bool isLinked(string i1, string i2) 
    {
        string access1 = i1;
        string access2 = i2;


       if (!(currently_joined.ContainsKey(access1))) {
            currently_joined[access1] = new Dictionary<string, bool>
            {
                [access2] = false
            };
        }
       else if (!(currently_joined[access1].ContainsKey(access2)))
        {
            currently_joined[access1][access2] = false;
        }

        if (!(currently_joined.ContainsKey(access2)))
        {
            currently_joined[access2] = new Dictionary<string, bool>
            {
                [access1] = false
            };
        }
        else if (!(currently_joined[access2].ContainsKey(access1)))
        {
            currently_joined[access2][access1] = false;
        }

        bool output = currently_joined[access1][access2] && currently_joined[access2][access1];
        currently_joined[access1][access2] = output;
        currently_joined[access2][access1] = output;
        return output;

    }

    // Update is called once per frame
    void Update()
    {
        if (prevPos == this.transform.position && !update)
        {
            return;
        }
        
        update = false;
        prevPos = this.transform.position;

        draw_object[] l = FindObjectsOfType(typeof(draw_object)) as draw_object[];

        foreach (var i in lines)
        {
            Destroy(i);
        }

        foreach (var item in l)
        {
            if (item != this)
            {

                float seperation = (this.transform.position - item.transform.position).magnitude;
                Debug.Log(seperation);
                if (seperation > 3) {
                    SetLinkStatus(this.parentName, item.parentName, false);
                }
                else if (seperation < 0.5 || isLinked(this.parentName, item.parentName))

                {
                    SetLinkStatus(this.parentName, item.parentName, true);

                    var selected1 = this.GetComponentsInChildren<PointableObject>().Where(x => x.isSelected()).ToArray();
                    var selected2 = item.GetComponentsInChildren<PointableObject>().Where(x => x.isSelected()).ToArray();

                    if (selected1.Any() && selected2.Any())
                    {
                        foreach (var origin in selected1)
                        {
                            foreach (var destination in selected2)
                            {


                                Bezier b = new Bezier(this.transform, origin, destination);
                                lines.Add(b.line);
                                item.AddLines(b.line);


                            }
                        }
                    }

                }


            }

        }
    }

    private void stack_remove(draw_object draw_object)
    {
        if (positonStack.Contains(draw_object))
        {
            var index = positonStack.IndexOf(draw_object);
            
            for (int i = index+1; i<positonStack.Count; i++)
            {
                positonStack[i].transform.position -= new Vector3(0, 0, 0.2F);
            }

            positonStack.Remove(draw_object);
        }
    }

    internal void draw(PointableObject pointableObject, int level)
    {
        
        string file;
        mapRenderer map = new mapRenderer();
        this.parentName = pointableObject.name;

        reportGrabbed = delegate (bool x)
        {
            if (!(this.moved))
            {
                this.stack_remove(this);
                this.moved = true;
            }
        };


        if (level == (int)mapRenderer.LEVEL.STATE_LEVEL)
        {
             file = "D:\\vr\\group3_vr\\mapGeoJSON\\state_map\\" + pointableObject.name + ".json";


            map.drawMultiple(this.gameObject, reportGrabbed, file, level, pointableObject);

            this.gameObject.transform.position += new Vector3(0, 0, getYPosition(positonStack.Count + 1));

            positonStack.Add(this);

        }
        else
        {
            file = "D:\\vr\\group3_vr\\mapGeoJSON\\state_map\\" + pointableObject.parentName + ".json";
            foreach (var line in File.ReadAllLines(file)) {
                if (line.Contains(pointableObject.name))
                {
                    map.drawSingular(this.gameObject, reportGrabbed, line, pointableObject.parentName, level, pointableObject);
                    break;
                }
            }

        }





        currentList.Add(this);
      

    }
}

