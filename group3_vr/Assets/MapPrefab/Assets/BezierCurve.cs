using UnityEngine;
using VRTK;

[System.Serializable]

/// <summary>
/// Generic class which is used to draw Bezier curves between an origin and Destinations.
/// This code draws a cubic bezier curve, starting at the origin and finishing at the destination
/// The height of the line is based on the distance between the lines.
/// Code based on: https://gist.github.com/valryon/b782bb76f543354f11f7
/// </summary>
public class Bezier : System.Object
{

    /// <summary>
    /// Store for the first control point.
    /// </summary>
    private Vector3 p0;
    /// <summary>
    /// Store for the second control point.
    /// </summary>
    private Vector3 p1;
    /// <summary>
    /// Store for the third control point.
    /// </summary>
    private Vector3 p2;
    /// <summary>
    /// Store for the fourth control point.
    /// </summary>
    private Vector3 p3;

    /// <summary>
    /// List of points on the line
    /// </summary>
    private Vector3[] points;
    /// <summary>
    /// GameObject which holds the line
    /// </summary>
    internal GameObject gameObject;
    /// <summary>
    /// The line renderer instance
    /// </summary>
    internal LineRenderer line;
    private VRTK_InteractableObject interactObject;

    /// <summary>
    /// Constrctor for the class, which enables drawing between two points, with a specfied parent
    /// </summary>
    /// <param name="parent">The parent of the line</param>
    /// <param name="origin">Origin of the curve</param>
    /// <param name="destination">Destination of the curve</param>
    /// <returns></returns>
    public Bezier(Transform parent, InteractableMap origin, InteractableMap destination, float lineWidth)
    {
        //gameObject = new GameObject();

        //line = gameObject.AddComponent(typeof(LineRenderer)) as LineRenderer;
        //line.transform.SetParent(parent);
        //line.useWorldSpace = true;
        //line.startWidth = 0.005F;
        //line.endWidth = 0.005F;

        //interactObject = gameObject.AddComponent(typeof(VRTK_InteractableObject)) as VRTK_InteractableObject;
        //VRTK_InteractHaptics interactHaptics = gameObject.AddComponent(typeof(VRTK_InteractHaptics)) as VRTK_InteractHaptics;
        //VRTK.GrabAttachMechanics.VRTK_ChildOfControllerGrabAttach grabAttach = gameObject.AddComponent(typeof(VRTK.GrabAttachMechanics.VRTK_ChildOfControllerGrabAttach)) as VRTK.GrabAttachMechanics.VRTK_ChildOfControllerGrabAttach;
        //VRTK.SecondaryControllerGrabActions.VRTK_SwapControllerGrabAction grabAction = gameObject.AddComponent(typeof(VRTK.SecondaryControllerGrabActions.VRTK_SwapControllerGrabAction)) as VRTK.SecondaryControllerGrabActions.VRTK_SwapControllerGrabAction;
        //Rigidbody rigidBody = gameObject.AddComponent(typeof(Rigidbody)) as Rigidbody;

        //interactObject.isGrabbable = true;
        //interactObject.holdButtonToGrab = false;
        //interactObject.grabAttachMechanicScript = grabAttach;




        UnbundleFD unbundle = GameObject.Find("UnbundleManager").GetComponent<UnbundleFD>();

        ////interactObject.secondaryGrabActionScript = scaleAction;

        ////grabAttach.precisionGrab = true;

        //rigidBody.useGravity = false;
        //rigidBody.isKinematic = true;

        Vector3 p1W = origin.transform.parent.transform.position; //- origin.transform.parent.transform.TransformVector(new Vector3(0, 0, 0.07F));
        Vector3 p2W = destination.transform.parent.transform.position; //- destination.transform.parent.transform.TransformVector(new Vector3(0, 0, 0.07F));

        //GameObject p1 = GameObject.CreaCreatePrimitive(PrimitiveType.Sphere);
        GameObject p1 = new GameObject();
        p1.transform.SetParent(parent.transform);
        p1.transform.position = p1W;
        p1.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        p1.transform.forward = origin.transform.parent.transform.forward;
        p1.name = "Point+";

        GameObject p2 = new GameObject();
        p2.transform.SetParent(parent.transform);
        p2.transform.position = p2W;
        p2.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        p2.transform.forward = destination.transform.parent.transform.forward;
        p2.name = "Point+" + "-V";
        unbundle.addLine(p1, p2, lineWidth);

        //float dist = (p0 - p3).magnitude;

        //Vector3 p1 = p0 + origin.transform.parent.transform.TransformVector(new Vector3(0, 0, dist));

        //Vector3 p2 = p3 + destination.transform.parent.transform.TransformVector(new Vector3(0, 0, dist));

        //CreateBezier(p0, p1, p2, p3, 50);

        //line.positionCount = this.points.Length;
        //line.SetPositions(this.points);
        //line.useWorldSpace = false;

    }

    //public void createCollider()
    //{
    //    MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();

    //    Mesh mesh = new Mesh();
    //    line.BakeMesh(mesh, true);
    //    meshCollider.sharedMesh = mesh;
    //}


    /// <summary>
    /// Create bezier based on given control points.
    /// </summary>
    /// <param name="v0">Source</param>
    /// <param name="v1">Control point 1</param>
    /// <param name="v2">Control point 1</param>
    /// <param name="v3">Destination</param>
    /// <returns></returns>
    private void CreateBezier(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, int num)
    {
        this.p0 = v0;
        this.p1 = v1;
        this.p2 = v2;
        this.p3 = v3;

        points = new Vector3[num + 1];

        for (int p = 0; p <= num; p++)
        {
            Vector3 newPoint = GetPointAtTime((float)p / num); //get next point
            points[p] = newPoint;

        }
     }

    /// <summary>
    /// Calculate point at specified point, t, along the line
    /// Assume 0 &le; t &le; 1
    /// </summary>
    /// <param name="parent">The parent of the line</param>
    /// <param name="origin">Origin of the curve</param>
    /// <param name="destination">Destination of the curve</param>
    /// <returns></returns>    private Vector3 GetPointAtTime(float t)
    private Vector3 GetPointAtTime(float t)
    {

    Debug.Assert(t <= 1 && t >= 0, "invalid control point");

    float u = 1f - t;
    float tt = t * t;
    float uu = u * u;
    float uuu = uu * u;
    float ttt = tt * t;

    Vector3 p = uuu * p0; //first term
    p += 3 * uu* t * p1; //second term
    p += 3 * u* tt * p2; //third term
    p += ttt* p3; //fourth term

    return p;

    }

}

