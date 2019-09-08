using UnityEngine;

[System.Serializable]
public class Bezier : System.Object
{

    public Vector3 p0;
    public Vector3 p1;
    public Vector3 p2;
    public Vector3 p3;

    public float length = 0;

    public Vector3[] points;

    // Init function v0 = 1st point, v1 = handle of the 1st point , v2 = handle of the 2nd point, v3 = 2nd point
    // handle1 = v0 + v1
    // handle2 = v3 + v2
    public Bezier(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, int _calculatePoints = 0)
    {
        this.p0 = v0;
        this.p1 = v1;
        this.p2 = v2;
        this.p3 = v3;

        if (_calculatePoints > 0) CalculatePoints(_calculatePoints);
    }

    // 0.0 >= t <= 1.0 her be magic and dragons
    public Vector3 GetPointAtTime(float t)
    {
        float u = 1f - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 p = uuu * p0; //first term
        p += 3 * uu * t * p1; //second term
        p += 3 * u * tt * p2; //third term
        p += ttt * p3; //fourth term

        return p;

    }

    //where _num is the desired output of points and _precision is how good we want matching to be
    public void CalculatePoints(int _num)
    {
        points = new Vector3[_num];

        for (int p = 0; p < _num; p++)
        {
            Vector3 newPoint = GetPointAtTime((float)p / _num); //get next point
            points[p] = newPoint;

        }

    }

}