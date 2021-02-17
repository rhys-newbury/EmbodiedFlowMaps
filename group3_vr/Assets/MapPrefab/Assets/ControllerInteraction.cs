using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using VRTK;
using System;

/// <summary>
/// Main class which deals with inteactions between pointer and InteractableObject. InteractableObject supports 5 events which are delegated by this class.
/// </summary>
public class ControllerInteraction : MonoBehaviour
{
    private bool selectingObject;
    private InteractableObject currentObject;
    
    private VRTK_ControllerTooltips helpTooltip;
    private bool helpTooltipState = false;

    private VRTK_ControllerTooltips dataToolTip;

    private Action<string> changeMapText;
    private Action<string> changeFlowText;


    private VRTK_ControllerEvents controller;
    private VRTK_Pointer pointer;

    private long oldTime = 0;
    private Vector3 oldPos = new Vector3(0,0,0);

    private List<float> velocityBuffer = (new float[] { 0, 0, 0, 0, 0 }).ToList();
    private List<Vector3> velocityBufferLong = (new Vector3[] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero,Vector3.zero,Vector3.zero,Vector3.zero,Vector3.zero,Vector3.zero,Vector3.zero,Vector3.zero,Vector3.zero,Vector3.zero,Vector3.zero,Vector3.zero,Vector3.zero,Vector3.zero,Vector3.zero,Vector3.zero,Vector3.zero,Vector3.zero,Vector3.zero,Vector3.zero,Vector3.zero,Vector3.zero,Vector3.zero,Vector3.zero,Vector3.zero,Vector3.zero,Vector3.zero }).ToList();

    private bool touchpadPressed = false;
    private float touchpadAngle;

    public UnbundleFD UnbundleScript;

    bool triggerPressed = false;

    bool optim3d = false;
    LineRenderer lineRenderer;

    //List<int> selectedTube = new List<int>();

    bool bundle;

    float sizeRay;
    private Transform cp1;
    private Transform cp2;
    private bool reset_parents;
    private MapContainer m1;
    private MapContainer m2;
    private GameObject cobj;
    private bool reset_vel_buffer;

    /// <summary>
    /// Calculations to be performed on every frame.
    /// </summary>
    /// <returns></returns>
    private void Update()
    {

        //lineRenderer.SetPosition(0, this.transform.position);
        //if (triggerPressed)
        //{
        //    //lineRenderer.SetPosition(1, this.transform.position + this.transform.forward * sizeRay);

        //    //Test of the line segments distance;


        //    //RaycastHit hit;
        //    //if (Physics.Raycast(transform.position, this.transform.forward, out hit, 100.0f))
        //    //{
        //    //    Debug.Log("Hit something");
        //    //}
        //}



        //else
        //{
        //    lineRenderer.SetPosition(1, this.transform.position + this.transform.forward * 0.2f);
        //}

        try
        {

            cobj = GetComponent<VRTK_InteractGrab>().GetGrabbedObject();

            if (touchpadPressed) OnUpdateTouchPadPressed(touchpadAngle, pointer.transform.parent.transform, cobj);
            
            if (reset_vel_buffer)
            {
                reset_vel_buffer = false;
                velocityBuffer = (new float[] { 0, 0, 0, 0, 0 }).ToList();
                velocityBufferLong = (new Vector3[] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero }).ToList();

            }

            long time = DateTime.Now.Ticks;
           
            //Time Limit between frames
            if (time-oldTime < 500000) {

                Vector3 newPos = cobj.transform.position;
                Vector3 delta = newPos - oldPos;
                float speed = delta.magnitude / time * (Mathf.Pow(10,20));

                //Add to the velocity buffer
                if (oldPos.magnitude != 0)
                {
                    velocityBuffer.RemoveAt(0);
                    velocityBuffer.Add(speed);

                    velocityBufferLong.RemoveAt(0);
                    velocityBufferLong.Add(newPos - oldPos);

                }
                oldPos = newPos;
                Debug.LogError("sum: " + velocityBuffer.Sum().ToString());
                //Debug.Log(velocityBuffer.Sum());
                //Debug.Log(velocityBuffer.Count((x) => x > 10));
                //Condition for a throw to occur.
                if (velocityBuffer.Sum() > 80 && velocityBuffer.Count((x) => x > 10) > 3) {
                    List<float> angles = new List<float>();
                    foreach (var i in velocityBufferLong)
                    {
                        foreach (var j in velocityBufferLong)
                        {
                            angles.Add(Vector3.Angle(i, j));
                        }
                    }
                    var x = angles.Where((i) => i > 0).Select((i) => Mathf.Abs(i)).Average();
                    Debug.LogError(x);

                    if (x > 60)
                    {
                        Debug.Log("filter");
                        cobj.GetComponentInChildren<MapContainer>()?.Filter();
                    }
                    else
                    {
                        Debug.LogError("throw");
                        //velocityBuffer.Reverse()
                        cobj.GetComponentInChildren<MapContainer>()?.OnThrow(velocityBufferLong.AsEnumerable().Reverse().Take(5).Aggregate(new Vector3(0, 0, 0), (s, v) => s + v) / (float)(5), GetComponent<VRTK_InteractGrab>());
                    }
                    var y = angles[0];


                }


            }
            else {
                //Reset the buffer
                velocityBuffer = (new float[] { 0, 0, 0, 0, 0 }).ToList();
                velocityBufferLong = (new Vector3[] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero }).ToList();

            }
            //Update the time
            oldTime = time;
        }
        catch { }

    }


    /// <summary>
    /// If the touchpad is being pressed, perform the nessecary action based on the angle.
    /// </summary>
    /// <param name="touchpadAngle">Current angle of the touchpad</param>
    /// <param name="transformDirection">Diretion in which to move the object</param>
    /// <param name="obj">The object to move</param>
    /// <returns></returns>
    /// 
    private void OnUpdateTouchPadPressed(float touchpadAngle, Transform transformDirection, GameObject obj)
    {
        //If the first or last section, move towards the user
        if (touchpadAngle < 72 || touchpadAngle > 288)
        {
            obj.transform.position = obj.transform.position + transformDirection.TransformDirection(new Vector3(0, 0, 0.01F));
        }
        //Rotate about vertical axis, in the negative direction
        else if (touchpadAngle < 144)
        {
            obj.transform.Rotate(new Vector3(0, -1, 0), Space.Self);
        }
        //If the bottom section, move away from user.
        else if (touchpadAngle < 216)
        {
            obj.transform.position = obj.transform.position + transformDirection.TransformDirection(new Vector3(0, 0, -0.01F));
        }
        //Rotate about vertical axis, in the positive direction
        else if (touchpadAngle < 288)
        {
            obj.transform.Rotate(new Vector3(0, 1, 0), Space.Self);
        }

    }



    /// <summary>
    /// Set up event listeners from Controller and Pointer
    /// <returns></returns>
    /// 
    void Awake()
    {
        //VRTK_ControllerEvents controller;

        //Set up event listeners
        pointer = GetComponent<VRTK_Pointer>();
        pointer.DestinationMarkerEnter += Pointer_DestinationMarkerEnter;
        pointer.DestinationMarkerExit += Pointer_DestinationMarkerExit;

        controller = GetComponent<VRTK_ControllerEvents>();
        controller.TriggerPressed += Controller_TriggerPressed;
        controller.TriggerReleased += Controller_TriggerReleased;
        controller.TouchpadAxisChanged += Controller_TouchpadAxisChanged;
        controller.TouchpadReleased += Controller_TouchpadReleased;
        controller.TouchpadPressed += Controller_TouchpadPressed;
        controller.GripClicked += Controller_GripClicked;

        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.widthMultiplier = 0.02f;
        lineRenderer.positionCount = 2;
        bundle = true;
        sizeRay = 1;
        lineRenderer.material.color = Color.green;


        helpTooltip = gameObject.transform.GetChild(0).GetComponent<VRTK_ControllerTooltips>();
        helpTooltip.ToggleTips(false);


        dataToolTip = gameObject.transform.GetChild(1).GetComponent<VRTK_ControllerTooltips>();
        dataToolTip.ToggleTips(false);


        //Anonymous function to Update the dataToolTip text.
        changeMapText = delegate (string x) {
            dataToolTip.ToggleTips(true);
            dataToolTip.ToggleTips(false, VRTK_ControllerTooltips.TooltipButtons.TouchpadTooltip);
            dataToolTip.ToggleTips(true, VRTK_ControllerTooltips.TooltipButtons.StartMenuTooltip);
            dataToolTip.UpdateText(VRTK_ControllerTooltips.TooltipButtons.StartMenuTooltip, x);

        };

        changeFlowText = delegate (string x) {
            dataToolTip.ToggleTips(true);
            dataToolTip.ToggleTips(true, VRTK_ControllerTooltips.TooltipButtons.TouchpadTooltip);
            dataToolTip.ToggleTips(false, VRTK_ControllerTooltips.TooltipButtons.StartMenuTooltip);
            dataToolTip.UpdateText(VRTK_ControllerTooltips.TooltipButtons.StartMenuTooltip, x);
        };


    }

    private void Controller_TriggerReleased(object sender, ControllerInteractionEventArgs e)
    {
        triggerPressed = false;
        if (reset_parents)
        {
            m1.transform.parent = cp1;
            m2.transform.parent = cp2;

            reset_parents = false;
        }

        //var obj = GetComponent<VRTK_InteractGrab>().GetGrabbedObject();
    }



    /// <summary>
    /// When Grip is clicked, notify InteractableObject of OnGripPressed and OnPointerLeave
    /// <returns></returns>
    /// 
    private void Controller_GripClicked(object sender, ControllerInteractionEventArgs e) {
        // Report that the current object is being grabbed. This can tell the stack it is being removed

   
        currentObject?.OnGripPressed();
        if (!(currentObject is null))
        {
            reset_vel_buffer = true;
        }
        selectingObject = false;
        currentObject?.OnPointerLeave();
        currentObject = null;
        dataToolTip.ToggleTips(false);
    }


    /// <summary>
    /// When Grip is clicked, enable touchpad pressed flag.
    /// <returns></returns>
    /// 
    private void Controller_TouchpadPressed(object sender, ControllerInteractionEventArgs e) {
        this.touchpadPressed = true;



    }
    /// <summary>
    /// When Grip is clicked, disable touchpad pressed flag.
    /// <returns></returns>
    /// 
    private void Controller_TouchpadReleased(object sender, ControllerInteractionEventArgs e) {
        this.touchpadPressed = false;
    }
    /// <summary>
    /// When Grip is clicked, disable update current touchpad angle.
    /// <returns></returns>
    /// 
    private void Controller_TouchpadAxisChanged(object sender, ControllerInteractionEventArgs e) {
        //Set the touchpad angle to the current angle
        this.touchpadAngle = e.touchpadAngle;
    }
    /// <summary>
    /// When TriggerPressed, two options for behaviour. If user is currently pointing at object, notify object on event. Otherwise, show tooltip.
    /// <returns></returns>
    /// 
    private void Controller_TriggerPressed(object sender, ControllerInteractionEventArgs e) {

        //If still on an object
        triggerPressed = true;

        TubeRenderer closestTube = pickingTube(this.transform.position, this.transform.position + this.transform.forward * sizeRay);
        if (closestTube != null)
        {
            bool SHOW_TOOLTIP = true;
            if (SHOW_TOOLTIP)
            {
                if (dataToolTip.overallState)
                {
                    dataToolTip.ToggleTips(false);

                    GameObject um = GameObject.Find("UnbundleManager");
                    um.GetComponent<UnbundleFD>().reset_colours = true;

                }
                else
                {
                    dataToolTip.ToggleTips(true);

                    InteractableMap p1 = closestTube.p1;
                    InteractableMap p2 = closestTube.p2;

                    var tm = closestTube.GetComponent<Renderer>().materials;

                    tm[1] = new Material(closestTube.GetComponent<Renderer>().sharedMaterial);
                    tm[1].shader = Shader.Find("VRTK/OutlineBasic");
                    tm[1].SetColor("_OutlineColor", new Color(255, 255, 0, 0));
                    closestTube.GetComponent<Renderer>().materials = tm;

                    

                    var f1 = p1.getMapController().getFlowData(p1.name, p1.parentName, p2.name, p2.parentName);
                    var f2 = p1.getMapController().getFlowData(p2.name, p2.parentName, p1.name, p1.parentName);
                    if (f1 > f2)
                    {
                        changeFlowText("Flow from " + p1.name + " to " + p2.name + " is " + f1.ToString("N0"));

                    }
                    else
                    {
                        changeFlowText("Flow from " + p2.name + " to " + p1.name + " is " + f2.ToString("N0"));
                    }



                }


                //var inc_flow = Mathf.Max(0, this.getMapController().getData(this.name, this.parentName, true));

                //var out_flow = Mathf.Max(0, this.getMapController().getData(this.name, this.parentName, false));

                //var text = this.name + ": \n" + inc_flow.ToString("N0") + "Moved In \n" + out_flow.ToString("N0") + " Moved Out";

            }
            else {
                //closestTube.GetComponent<Renderer>().material.color = lineRenderer.material.color;
                var p1 = closestTube.p1;
                var p2 = closestTube.p2;

                m1 = p1.draw_map(pointer.transform.parent.transform);
                m2 = p2.draw_map(pointer.transform.parent.transform);

                //m1.transform.position = p1.transform            //m1.transform.position = p1.transform


                m1.link_two_maps(m2);

                cp1 = m1.transform.parent;
                cp2 = m2.transform.parent;

                m1.transform.parent = pointer.transform;
                m2.transform.parent = pointer.transform;

                reset_parents = true;
            }


        }
        

        else if (currentObject != null)
        {
            //Do something on click
            GameObject go = currentObject.OnTriggerPressed(pointer.transform.parent.transform);
            if (go !=null) {
                
                GetComponent<VRTK_InteractGrab>().AttemptGrabObject(go);
            }
         }
        else
        {
            helpTooltipState = !helpTooltipState;
            helpTooltip.ToggleTips(helpTooltipState);

        }
    }
    /// <summary>
    /// On PointerExit, notify current object, if not null. Disable the tooltip
    /// <returns></returns>
    /// 
    private void Pointer_DestinationMarkerExit(object sender, DestinationMarkerEventArgs e) {

        try {
            //Not selecting an object
            if (!selectingObject) {
                return;
            }
            selectingObject = false;

            //Tell the objet that the pointer has left
            currentObject = e.raycastHit.collider.gameObject.GetComponent<InteractableObject>();
            currentObject?.OnPointerLeave();

            //Reset tooltips
            currentObject = null;
            dataToolTip.ToggleTips(false);

        }
        catch {
            return;
        }

    }


    private void Pointer_DestinationMarkerEnter(object sender, DestinationMarkerEventArgs e) {
        //If the object being his is a InteractableMap -> Activate the tooltips, and tell the object that the pointer has entered
        try {
            currentObject = e.raycastHit.collider.gameObject.GetComponent<InteractableObject>();
            selectingObject = true;
            currentObject?.OnPointerEnter(changeMapText);
            //dataToolTip.ToggleTips(true);
            helpTooltip.ToggleTips(false);
            helpTooltipState = false;
        }

        catch { }


    }



    TubeRenderer pickingTube(Vector3 P1S1, Vector3 P1S2)
    {
        Dictionary<int, TubeRenderer> tubeList = UnbundleScript.tubeList;

        float minDist = 1000;
        TubeRenderer tubeClosest = null;
        int idClosestTube = -1;
        //bool found = false;

        foreach (KeyValuePair<int, TubeRenderer> entry in tubeList)
        {
                float locMidDist = 1000;

                Vector3[] points = entry.Value.points;
                for (int i = 1; i < points.Length; i++)
                {
                    float dist = SegmentUtils.Dist_Segments(points[i - 1], points[i], P1S1, P1S2);
                    if (dist < locMidDist)
                    {
                        locMidDist = dist;
                    }
                }

                if (locMidDist < minDist)
                {
                    minDist = locMidDist;
                    idClosestTube = entry.Key;
                    tubeClosest = entry.Value;
                }
            

        }

        if (minDist < 0.03f)
        {
            return tubeClosest;
        }
        return null;
    }


}
