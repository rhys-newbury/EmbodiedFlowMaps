using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SegmentUtils{

    public static float SMALL_NUM = 0.00000001f;
    //Adapted from http://geomalgorithms.com/a07-_distance.html
    public static float Dist_Segments(Vector3 S1P0, Vector3 S1P1, Vector3 S2P0, Vector3 S2P1)
    {

        Vector3 u = S1P1 - S1P0;
        Vector3 v = S2P1 - S2P0;
        Vector3 w = S1P0 - S2P0;


        float a = Vector3.Dot(u, u);
        float b = Vector3.Dot(u, v);
        float c = Vector3.Dot(v, v);
        float d = Vector3.Dot(u, w);
        float e = Vector3.Dot(v, w);

        float D = a * c - b * b;

        float sc, sN, sD = D;
        float tc, tN, tD = D;

        if (D < SMALL_NUM)
        {
            sN = 0f;
            sD = 1f;
            tN = e;
            tD = c;
        }
        else
        {
            sN = (b * e - c * d);
            tN = (a * e - b * d);
            if (sN < 0f)
            {
                sN = 0f;
                tN = e;
                tD = c;
            }
            else if(sN > sD)
            {
                sN = sD;
                tN = e + b;
                tD = c;
            }
        }

        if (tN < 0f)
        {
            tN = 0f;
            if (-d < 0f)
            {
                sN = 0;
            }
            else if(-d > a)
            {
                sN = sD;
            }
            else
            {
                sN = -d;
                sD = a;
            }
        }
        else if(tN > tD)
        {
            tN = tD;
            if((-d + b) < 0f)
            {
                sN = 0f;
            }
            else if((-d + b) > a)
            {
                sN = sD;
            }
            else
            {
                sN = (-d + b);
                sD = a;
            }
        }

        sc = (Mathf.Abs(sN) < SMALL_NUM ? 0f : sN / sD);
        tc = (Mathf.Abs(tN) < SMALL_NUM ? 0f : tN / tD);

        Vector3 dP = w + (sc * u) - (tc * v);

        return Vector3.Magnitude(dP);


        //return 0f;
    }
}
