using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    private LineRenderer line;
    private GameObject parent1;
    private GameObject parent2;

    Vector3 p0;
    Vector3 p3;

    // Start is called before the first frame update
    void Start()
    {
        this.line = this.gameObject.AddComponent<LineRenderer>() as LineRenderer;
        line.useWorldSpace = true;
        line.startWidth = 0.05F;
        line.endWidth = 0.05F;



        this.parent1 = GameObject.Find("Cube");
        this.parent2 = GameObject.Find("Cube2");
    }

    // Update is called once per frame
    void Update()
    {
        if (p0 != this.parent1.transform.position || p3 != this.parent2.transform.position)
        {
            p0 = this.parent1.transform.position;
            p3 = this.parent2.transform.position;


            float dist = (p0 - p3).magnitude;

            Vector3 p1 = p0 + new Vector3(0, dist, 0);

            Vector3 p2 = p3 + new Vector3(0, dist, 0);

            Bezier test = new Bezier(p0, p1, p2, p3, 50);

            line.positionCount = test.points.Length;
            line.SetPositions(test.points);

        }

    }
}
