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
    private string parentName;
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
    /// Remove from objects on stack, when object is destoryed
    /// </summary>
    //public void OnDestroy()
    //{
    //    if (!this.moved)
    //    {
    //        if (this.level == (int)(int)MapRenderer.LEVEL.STATE_LEVEL)
    //        {
    //            this.mapController.StateStack.destroy(this);
    //        }
    //        else
    //        {
    //            this.mapController.CountyStack.destroy(this);

    //        }
    //    }

    //}

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



        anim = gameObject.AddComponent<Animation>();
        AnimationCurve translateX = AnimationCurve.EaseInOut(0.0f, 0.0f, 0.5f, -0.05f);
        animationClip = new AnimationClip();
        animationClip.legacy = true;
        animationClip.SetCurve("", typeof(Transform), "localPosition.z", translateX);
        anim.AddClip(animationClip, "ZMovement");
        Debug.Log(anim.GetClipCount());

        //animZ = gameObject.AddComponent<Animation>();
        AnimationCurve translateY = AnimationCurve.EaseInOut(0.0f, 0.1f, 0.0f, -0.05f);
        animationClipZ = new AnimationClip();
        animationClipZ.legacy = true;
        animationClipZ.SetCurve("", typeof(Transform), "localPosition.y", translateY);
        anim.AddClip(animationClipZ, "YMovement");
        Debug.Log(anim.GetClipCount());


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

        }


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

        foreach (var i in lines)
        {
            Destroy(i);
        }

        foreach (var item in l)
        {
            if (item != this)
            {

                float seperation = (this.transform.position - item.transform.position).magnitude;
                //Debug.Log(seperation);
                if (seperation > 5) {
                    SetLinkStatus(this.parentName, item.parentName, false);
                
                }
                else if (seperation < 0.5 || IsLinked(this.parentName, item.parentName))

                {
                    SetLinkStatus(this.parentName, item.parentName, true);

                    var selected1 = this.GetComponentsInChildren<InteractableMap>().Where(x => x.IsSelected()).ToArray();
                    var selected2 = item.GetComponentsInChildren<InteractableMap>().Where(x => x.IsSelected()).ToArray();

                    if (selected1.Any() && selected2.Any())
                    {
                        foreach (var origin in selected1)
                        {
                            foreach (var destination in selected2)
                            {

                                
                                if (origin.getMapController().getFlowData(origin.name, origin.parentName, destination.name, destination.parentName) != -1)
                                {
                                
                                    Bezier b = new Bezier(this.transform, origin, destination);
                                    lines.Add(b.line);
                                    item.AddLines(b.line);

                                    b.line.material = new Material(Shader.Find("Sprites/Default"));

                                    b.line.startColor = Color.green; //new Color(253, 187, 45, 255);
                                    b.line.endColor = Color.blue; // new Color(34, 193,195, 255);


                                    b.line.startWidth = 0.1F * (0.00000699192F * origin.getMapController().getFlowData(origin.name, origin.parentName, destination.name, destination.parentName) + 0.05F);

                                    b.line.endWidth = b.line.startWidth; //origin.getMapContainer().getFlowColour(origin.getMapContainer().getFlowData(origin.name, origin.parentName, destination.name, destination.parentName));

                                    //b.createCollider();
                                }
    

                            }
                        }
                    }

                }


            }

        }
    }

  

    private bool animWasPlaying = false;
    private int resetCount = 0;

    /// <summary>
    /// On LateUpdate while object is animating, shift the object back to original position.
    /// </summary>
    /// <returns></returns>
    ///
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

