using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using VRTK;
using System;



public class MapContainer : MonoBehaviour
{

    private static bool _startUp = true;

    private Vector3 prevPos = new Vector3(0,0,0);

    private List<LineRenderer> lines = new List<LineRenderer>();

    private static Dictionary<String, Dictionary<string, bool>> currently_joined = new Dictionary<String, Dictionary<string, bool>>();
    private string parentName;
    public static bool update;


    public static List<MapContainer> positonStack = new List<MapContainer>();
    private bool moved;

    private Action<bool> reportGrabbed;

    public Animation anim;
    AnimationClip animationClip;

    public bool animationStarted = false;

    public bool animationFinished = true;

    public Vector3 startPos;


    
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

        anim = gameObject.AddComponent<Animation>();
        AnimationCurve translateX = AnimationCurve.EaseInOut(0.0f, 0.0f, 0.5f, -0.05f);
        animationClip = new AnimationClip();
        animationClip.legacy = true;
        animationClip.SetCurve("", typeof(Transform), "localPosition.z", translateX);
        anim.AddClip(animationClip, "test");
        Debug.Log(anim.GetClipCount());


        this.reportGrabbed = delegate (bool x)
        {
            if (!(this.moved))
            {
                this.transform.localScale = new Vector3(1F, 1F, 1F);

                this.stack_remove(this);
                this.moved = true;
            }
        };


        if (_startUp)
        {
            Debug.Log(this.transform.root);
            string file = this.transform.root.GetComponent<MapController>().mainMap;
            this.parentName = "America";

            mapRenderer map = new mapRenderer();
            map.drawMultiple(this.gameObject, reportGrabbed, file,0);

            _startUp = false;

        }


    }

    void AddLines(LineRenderer l)
    {
        this.lines.Add(l);
    }
    static void SetLinkStatus(string i1, string i2, bool val)
    {
        IsLinked(i1, i2);
        currently_joined[i1][i2] = val;
        currently_joined[i2][i1] = val;
    }

    internal static void DeselectState()
    {
        update = true;
    }

    internal static float GetYPosition(int i)
    {
        return i * 0.05F;
    }

    static bool IsLinked(string i1, string i2) 
    {
        string access1 = i1;
        string access2 = i2;


       if (!(currently_joined.ContainsKey(access1))) {
            currently_joined[access1] = new Dictionary<string, bool>
            {
                [access2] = false
            };
        }
       else if (!(currently_joined[access1].ContainsKey(access2))) {
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

        MapContainer[] l = FindObjectsOfType(typeof(MapContainer)) as MapContainer[];

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
                else if (seperation < 0.5 || IsLinked(this.parentName, item.parentName))

                {
                    SetLinkStatus(this.parentName, item.parentName, true);

                    var selected1 = this.GetComponentsInChildren<PointableObject>().Where(x => x.IsSelected()).ToArray();
                    var selected2 = item.GetComponentsInChildren<PointableObject>().Where(x => x.IsSelected()).ToArray();

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

    private void stack_remove(MapContainer drawObject)
    {
        if (positonStack.Contains(drawObject))
        {
            var index = positonStack.IndexOf(drawObject);
            
            for (int i = index+1; i<positonStack.Count; i++)
            {
                if (positonStack.Count < 10)
                {
                    positonStack[i].animationStarted = true;
                    positonStack[i].startPos = positonStack[i].transform.localPosition;
                    positonStack[i].anim.Play("test");
                }
                else
                {
                    positonStack[i].transform.position -= new Vector3(0, 0, 0.05F);
                }
            }

            positonStack.Remove(drawObject);
        }
    }

    private bool animWasPlaying = false;
    private int resetCount = 0;
    private void LateUpdate()
    {

        if (anim.isPlaying || animWasPlaying)
        {
            animWasPlaying = true;
            transform.localPosition += startPos;
            if (anim.isPlaying)
            {
                resetCount = 0;
            }
            else
            {
                resetCount += 1;
            }

        }


        if (resetCount >= 1)
        {
            animWasPlaying = false;
        }
    }


    internal void Draw(PointableObject pointableObject, int level)
    {

        this.reportGrabbed = delegate (bool x)
        {
            if (!(this.moved))
            {
                this.transform.localScale = new Vector3(1F, 1F, 1F);

                this.stack_remove(this);
                this.moved = true;
            }
        };

        string file;
        mapRenderer map = new mapRenderer();
        this.parentName = pointableObject.name;

        if (level == (int)mapRenderer.LEVEL.STATE_LEVEL)
        {
             file = this.transform.root.GetComponent<MapController>().pathToStates + pointableObject.name + ".json";


            map.drawMultiple(this.gameObject, reportGrabbed, file, level, pointableObject);
            this.gameObject.transform.position += new Vector3(0, 0, GetYPosition(positonStack.Count + 1));
            positonStack.Add(this);

        }
        else
        {
            file = this.transform.root.GetComponent<MapController>().pathToStates + pointableObject.parentName + ".json";
            if (this.transform.root.GetComponent<MapController>().checkForBuildings(pointableObject.name, pointableObject.parentName))
            {

                foreach (var line in File.ReadAllLines(file))
                {
                    if (line.Contains(pointableObject.name))
                    {
                        map.drawSingular(this.gameObject, reportGrabbed, line, pointableObject.parentName, level, pointableObject);
                        break;
                    }
                }
            }
        }

        this.transform.localScale = new Vector3(0.25F, 0.25F, 0.25F);




      

    }
}

