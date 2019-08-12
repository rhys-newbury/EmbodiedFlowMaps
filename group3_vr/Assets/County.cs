using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class County : PointableObject
{

    private static Quaternion Angle = new Quaternion(1, 0, 0, 0);
    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        Debug.Log("County Stuff");

    }

    public override Quaternion getAngle()
    {
        return Angle;
    }
}
