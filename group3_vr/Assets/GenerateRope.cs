using UnityEngine;

public class GenerateRope : MonoBehaviour
{

    public Material RopeMaterial;  //set in Editor
    public Obi.ObiSolver RopeSolver;  //set in Editor
    public Obi.ObiRopeSection RopeSection;  //set in Editor

    private Vector3? _startPoint;
    static int _genCounter = 0;

    private void Start()
    {
        MakeRope();
    }

    //void Update()
    //{
    //    if (Input.GetMouseButtonDown(0))
    //    {
    //        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    //        var point = ray.GetPoint(2);

    //        if (_startPoint.HasValue)
    //        {
    //            Vector3 startPoint = _startPoint.Value;
    //            Vector3 endPoint = point;
    //            _startPoint = null;

    //            MakeRope(startPoint, endPoint);
    //        }
    //        else
    //        {
    //            _startPoint = point;
    //        }
    //    }
    //}

    private void MakeRope()//Vector3 startPoint, Vector3 endPoint)
    {
        var gon = new GameObject("New Rope (" + (++_genCounter).ToString() + ")");
        var go = GameObject.Find("Cube (4)");
        //go.transform.position = startPoint;
        var goEnd = GameObject.Find("Cube (2)");
        //goEnd.transform.parent = go.transform;
        //goEnd.transform.position = endPoint;
        gon.transform.position = (go.transform.position + go.transform.position) / 2.0F;
        var ropeHelper = gon.AddComponent<MyRopeHelper>();
        //ropeHelper.section = RopeSection;
        //ropeHelper.material = RopeMaterial;
        //ropeHelper.solver = RopeSolver;
        //ropeHelper.start = go.transform;
        //ropeHelper.end = goEnd.transform;
        float thickness = 0.03F;
        ropeHelper.GenerateRope(go, goEnd, thickness);
    }
}