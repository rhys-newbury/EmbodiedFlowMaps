using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class County : PointableObject
{

    private static Quaternion Angle = new Quaternion(0, 0.6F, 0.8F, 0);
    private static Quaternion FinalAngle = new Quaternion(0, 0, 1, 0);
    
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

    public override Quaternion getFinalAngle()
    {
        return FinalAngle;
    }


    public override Vector3 getTranslation(float x, float y)
    {
        return new Vector3(x, 0.28F*y, -0.96F*y);
    }
}
