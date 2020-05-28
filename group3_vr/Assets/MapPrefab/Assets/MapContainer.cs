using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using VRTK;
using System;


/// <summary>
/// A Collection of InteractableMaps, wrapped in to a map container. For example 'America' would be a collecton of InteractableMaps(eg States).
/// </summary>
public class MapContainer : MonoBehaviour
{

    private static bool _startUp = true;

    private Vector3 prevPos = new Vector3(0,0,0);

    private List<LineRenderer> lines = new List<LineRenderer>();

    private static Dictionary<String, Dictionary<string, bool>> currently_joined = new Dictionary<String, Dictionary<string, bool>>();
    internal string parentName;
    private MapController mapController;
    private int level;
    public static bool update;




    private bool moved;

    private Action<bool> reportGrabbed;

    public VRTK_InteractableObject interactObject;

    public Animation anim;
    AnimationClip animationClip;

    public Animation animZ;
    AnimationClip animationClipZ;

    public bool animationStarted = false;

    public bool animationFinished = true;

    public Vector3 startPos;

    

    /// <summary>
    /// When object is created, set up Animations and VRTK Interactions
    /// </summary>
    /// 
    private void Awake()
    {
        interactObject = gameObject.AddComponent(typeof(VRTK_InteractableObject)) as VRTK_InteractableObject;
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
    }

    private void Start()
    {      


        this.reportGrabbed = delegate (bool x)
        {
            if (!(this.moved) && (this.level > 0))
            {
                //this.transform.localScale = new Vector3(1F, 1F, 1F);
                this.transform.parent = this.transform.root.GetComponent<MapController>().transform;

                //this.stack_remove(this, 0);
                this.moved = true;
            }
        };

        var mc = this.transform.root.GetComponent<MapController>();
        if (mc != null && this.transform.root.GetComponent<MapController>().startUp)
        {
            //Debug.Log(this.transform.root);
            string file = mc.mainMap;
            this.parentName = "America";
            this.level = 0;

            MapRenderer map = new MapRenderer();
            map.drawMultiple(this.gameObject, reportGrabbed, file,0, this.transform.root.GetComponent<MapController>().haveTooltip, this.transform.root.GetComponent<MapController>().mapScale, parentName);

            this.transform.root.GetComponent<MapController>().startUp = false;

            Dictionary<String, InteractableMap> lchild = new Dictionary<string, InteractableMap>();
            foreach (var ma2p in this.GetComponentsInChildren<InteractableMap>())
            {
                lchild[ma2p.name] = ma2p;
            }


            foreach (var pair in mc.flattenedList["America"].Take(20))
            {

                try
                {
                    var origin = lchild[pair.Item1];
                    var destination = lchild[pair.Item2];
                    var flowData = pair.Item3;

                    Bezier b = new Bezier(this.transform, origin, destination, 0.1F * (0.00000399192F * flowData + 0.05F));
                }
                catch { }

                //b.line.startWidth = 0.1F * (0.00000699192F * flowData + 0.05F);
                //Debug.Log("line width = " + b.line.startWidth.ToString());
                //b.line.endWidth = b.line.startWidth;

                //b.line.material = new Material(Shader.Find("Sprites/Default"));

                //b.line.startColor = Color.green; //new Color(253, 187, 45, 255);
                //b.line.endColor = Color.blue; // new Color(34, 193,195, 255);

            }

        }


    }
    private void OnDestroy()
    {
        var g = GameObject.Find("UnbundleManager").GetComponent<UnbundleFD>();
        g.removeLinesFromObject(this.gameObject);
        
    }

    /// <summary>
    /// Delete object and children on throw
    /// </summary>
    /// <returns></returns>
    /// 
    public void OnThrow()
    {

        int level = -1;
        //On Throw Delete each item in children
        //Deselect the parent
        foreach (var item in this.GetComponentsInChildren<InteractableMap>())
        {
            item.Delete();
            item.parent?.Deselect();
            level = level == -1 ? item.GetLevel() : level;
        }
        //Do not destroy country
        
        if (level > 0) Destroy(this.gameObject);


    }
    /// <summary>
    /// Add item to lines list
    /// </summary>
    /// <returns></returns>
    /// 
    void AddLines(LineRenderer l)
    {
        this.lines.Add(l);
    }


    /// <summary>
    /// Set the link status between two objects, set link both ways.
    /// </summary>
    /// <param name="i1">Object One</param>
    /// <param name="i2">Object Two</param>
    /// <param name="val">The value which to update the link too</param>
    /// <returns></returns>
    /// 
    static void SetLinkStatus(string i1, string i2, bool val)
    {
        IsLinked(i1, i2);
        currently_joined[i1][i2] = val;
        currently_joined[i2][i1] = val;



    }
    /// <summary>
    /// Set update flag.
    /// </summary>
    /// <returns></returns>
    ///
    internal static void DeselectState()
    {
        update = true;
    }

    /// <summary>
    /// Check if two objects are linked
    /// </summary>
    /// <returns></returns>
    ///
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

        bool output = currently_joined[access1][access2] || currently_joined[access2][access1];
        
        return output;

    }

    /// <summary>
    /// Check if objects are linked and draw flow if nessecary.
    /// </summary>
    /// <returns></returns>
    ///
    void Update()
    {
        if (prevPos == this.transform.position && !update)
        {
            return;
        }
        
        update = false;
        prevPos = this.transform.position;

        MapContainer[] l = FindObjectsOfType(typeof(MapContainer)) as MapContainer[];


        foreach (var item in l)
        {
            if (item != this)
            {

                float seperation = (this.transform.position - item.transform.position).magnitude;
                //Debug.Log(seperation);
                if (seperation > 5) {
                    SetLinkStatus(this.parentName, item.parentName, false);
                
                }
                else if (seperation < 0.5 && !IsLinked(this.parentName, item.parentName))
                {
                   SetLinkStatus(this.parentName, item.parentName, true);
                   link_two_maps(item);

                    //var f1 = this.mapController.county_flow;

                    //var f1 = this.mapController.county_flow()

                }
            }
         }

    }

    public void link_two_maps(MapContainer item)
    {
        string ordered_state1;
        string ordered_state2;
        if (string.Compare(this.parentName, item.parentName) < 0)
        {
            ordered_state1 = this.parentName;
            ordered_state2 = item.parentName;
        }
        else
        {
            ordered_state1 = item.parentName;
            ordered_state2 = this.parentName;
        }

        var flows = this.mapController.county_flattened[new Tuple<string, string>(ordered_state1, ordered_state2)];

        Dictionary<String, InteractableMap> lchild1 = new Dictionary<string, InteractableMap>();
        foreach (var ma2p in this.GetComponentsInChildren<InteractableMap>())
        {
            lchild1[ma2p.name] = ma2p;
        }

        Dictionary<String, InteractableMap> lchild2 = new Dictionary<string, InteractableMap>();
        foreach (var ma2p in item.GetComponentsInChildren<InteractableMap>())
        {
            lchild2[ma2p.name] = ma2p;
        }

        foreach (var val in flows.Take(20))
        {
            InteractableMap origin;
            InteractableMap destination;
            Transform origin_t;
            Transform dest_t;


            if (val.Item1 == this.parentName)
            {
                origin = lchild1[val.Item2];
                destination = lchild2[val.Item4];
                origin_t = this.transform;
                dest_t = item.transform;
            }
            else
            {
                origin = lchild2[val.Item2];
                destination = lchild1[val.Item4];
                origin_t = item.transform;
                dest_t = this.transform;
            }
            var flowData = val.Item5;

            Bezier b = new Bezier(origin_t, origin, destination, 0.1F * (0.00000399192F * flowData + 0.05F), dest_t);

        }

    }




    //private bool animWasPlaying = false;
    //private int resetCount = 0;

    /// <summary>
    /// On LateUpdate while object is animating, shift the object back to original position.
    /// </summary>
    /// <returns></returns>
    ///
    //private void LateUpdate()
    //{

    //    if (anim.isPlaying || animWasPlaying)
    //    {
    //        animWasPlaying = true;
    //        transform.localPosition += startPos;
    //        if (anim.isPlaying)
    //        {
    //            resetCount = 0;
    //        }
    //        else
    //        {
    //            resetCount += 1;
    //        }

    //    }


    //    if (resetCount >= 1)
    //    {
    //        animWasPlaying = false;
    //    }
    //}

    //internal void stack_remove(MapContainer container, int level)
    //{
    //    if (level == (int)MapRenderer.LEVEL.STATE_LEVEL)
    //    {
    //        this.mapController.StateStack.stack_remove(this);

    //    }
    //    else
    //    {
    //        this.mapController.CountyStack.stack_remove(this);

    //    }
    //}

    /// <summary>
    /// Draw the objects inside the MapContainer
    /// </summary>
    /// <param name="interactableMap">The empty interactable map to fill with data</param>
    /// <param name="level">The current level if the object</param>
    /// <returns></returns>
    /// 
    internal void Draw(InteractableMap interactableMap, int level, Transform start_pos, Transform direction)
    {

        this.mapController = this.transform.root.GetComponent<MapController>();

        this.reportGrabbed = delegate (bool x)
        {
            if (!(this.moved) && this.level > 0)
            {
                //this.transform.localScale = new Vector3(1F, 1F, 1F);
                this.transform.parent = this.mapController.transform;

                //this.stack_remove(this, level);
                this.moved = true;
            }
        };

        string file;
        MapRenderer map = new MapRenderer();
        this.parentName = interactableMap.name;

        this.level = level;

        if (level == (int)MapRenderer.LEVEL.STATE_LEVEL)
        {
             file = this.transform.root.GetComponent<MapController>().pathToStates + interactableMap.name + ".json";


            map.drawMultiple(this.gameObject, reportGrabbed, file, level, true, this.transform.root.GetComponent<MapController>().mapScale, "", interactableMap);
            this.transform.position = start_pos.position;
            this.transform.eulerAngles = start_pos.eulerAngles;
            this.transform.position = this.transform.position + direction.TransformDirection(new Vector3(0, 0, -0.1F));
            //this.transform.root.GetComponent<MapController>().StateStack.addMap(this.gameObject, this);

        }
        
        else
        {
            file = this.transform.root.GetComponent<MapController>().pathToStates + interactableMap.parentName + ".json";
            foreach (var line in File.ReadAllLines(file))
            {
                if (line.Contains(interactableMap.name))
                {
                    map.drawSingular(this.gameObject, reportGrabbed, line, interactableMap.parentName, level, interactableMap);
                    break;
                }
            }

            this.transform.position = start_pos.position;
            this.transform.eulerAngles = start_pos.eulerAngles;
            this.transform.position = this.transform.position + direction.TransformDirection(new Vector3(0, 0, -0.1F));

            //this.gameObject.transform.position += new Vector3(0, GetCountyPosition(countyStack.Count + 1), 0);
            //countyStack.Add(this);
        }

        //this.transform.localScale = new Vector3(0.25F, 0.25F, 0.25F);
        
            



      

    }
}

