using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerEventListener : MonoBehaviour {

    private SteamVR_TrackedController _controller;
    public UnbundleFD UnbundleScript;

    bool triggerPressed = false;

    bool optim3d = false;
    LineRenderer lineRenderer;

    List<int> selectedTube = new List<int>();

    bool bundle;

    float sizeRay;

    // Use this for initialization
    void Start () {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.widthMultiplier = 0.02f;
        lineRenderer.positionCount = 2;
        bundle = true;
        sizeRay = 1;
        lineRenderer.material.color = Color.green;
        //lineRenderer.isVisible = false;
    }

    private void OnEnable()
    {
        _controller = GetComponent<SteamVR_TrackedController>();
        _controller.TriggerClicked += _controller_TriggerClicked;
        _controller.TriggerUnclicked += controller_TriggerUnClicked;
        _controller.PadClicked += _controller_PadClicked;
        _controller.Gripped += _controller_Gripped;
    }

    private void _controller_Gripped(object sender, ClickedEventArgs e)
    {

        if (bundle)
        {
            UnbundleScript.sendBundle(selectedTube);
        }
        else
        {
            UnbundleScript.sendUnbundle(selectedTube);
        }
        foreach(int t in selectedTube)
        {
            UnbundleScript.tubeList[t].GetComponent<Renderer>().material.color = Color.white;
        }

        selectedTube.Clear();
    }

    private void controller_TriggerUnClicked(object sender, ClickedEventArgs e)
    {
        triggerPressed = false;
    }

    private void _controller_PadClicked(object sender, ClickedEventArgs e)
    {
        if (e.padX >= 0.5f)
        {

            Debug.Log("X clicked on the pad");
            changeMode();
        }
        if (e.padX <= -0.5f)
        {
            Debug.Log("X clicked on the pad");
            changeMode();
        }
        if (e.padY >= 0.5f)
        {
            sizeRay += 0.1f;
        }
        if (e.padY <= -0.5f)
        {
            sizeRay -= 0.1f;
        }
    }

    private void changeMode()
    {
        bundle = !bundle;
        if (bundle)
        {
            lineRenderer.material.color = Color.green;
        }
        else
        {
            lineRenderer.material.color = Color.red;
        }
    }

    private void _controller_TriggerClicked(object sender, ClickedEventArgs e)
    {
        triggerPressed = true;
        //UnbundleScript.setOptim3DDone(optim3d);
    }

    // Update is called once per frame
    void Update () {
        lineRenderer.SetPosition(0, this.transform.position);
        if (triggerPressed)
        {
            lineRenderer.SetPosition(1, this.transform.position + this.transform.forward * sizeRay);
            TubeRenderer closestTube = pickingTube(this.transform.position, this.transform.position + this.transform.forward * sizeRay);
            if(closestTube != null)
            {
                closestTube.GetComponent<Renderer>().material.color = lineRenderer.material.color;
            }
            //Test of the line segments distance;


            //RaycastHit hit;
            //if (Physics.Raycast(transform.position, this.transform.forward, out hit, 100.0f))
            //{
            //    Debug.Log("Hit something");
            //}
        }



        else
        {
            lineRenderer.SetPosition(1, this.transform.position + this.transform.forward * 0.2f);
        }
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
            if (!selectedTube.Contains(entry.Key)) {
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

        }

        if(minDist < 0.01f)
        {
            selectedTube.Add(idClosestTube);
            return tubeClosest;
        }
        return null;
    }




}
