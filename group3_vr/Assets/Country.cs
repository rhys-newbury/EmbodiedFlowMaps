using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine.EventSystems;
using VRTK;
using System;
using UnityEngine.UI;
public class Country : PointableObject
{

    public override int getLevel()
    {
        return 0;
    }
    private List<GameObject> lines = new List<GameObject>();

    public override void addLine(GameObject line)
    {
        lines.Add(line);
    }
    public override void removeLines()
    {
        lines.ToList().ForEach(x => GameObject.Destroy(x));
        lines.Clear();
    }

    internal override void addToList(string parentName, string name)
    {

        dataAccessor.addToList(parentName, name);

    }


    public override void getInternalFlows(PointableObject origin)
    {
        foreach (var state in origin.siblings)
        {
            try {
                if (state.Key != origin.name)
                {
                    var destination = state.Value;

                    if (destination.isSelected())
                    {


                        var newObj = new GameObject();
                        newObj.transform.parent = this.gameObject.transform;

                        var line = newObj.AddComponent(typeof(LineRenderer)) as LineRenderer;
                        line.transform.SetParent(this.transform);
                        line.useWorldSpace = true;
                        line.startWidth = 0.005F;
                        line.endWidth = 0.005F;

                        Vector3 p0 = origin.transform.parent.transform.position - origin.transform.parent.transform.TransformVector(new Vector3(0, 0, 0.05F));
                        Vector3 p3 = destination.transform.parent.transform.position - origin.transform.parent.transform.TransformVector(new Vector3(0, 0, 0.05F));

                        float dist = (p0 - p3).magnitude;

                        Vector3 p1 = p0 + new Vector3(0, 0, dist);

                        Vector3 p2 = p3 + new Vector3(0, 0, dist);

                        Bezier test = new Bezier(p0, p1, p2, p3, 50);

                        line.positionCount = test.points.Length;
                        line.SetPositions(test.points);
                        line.useWorldSpace = false;
                        this.lines.Add(newObj);
                        destination.addLine(newObj);
                    }

                }
            }
            catch
            {
                ;
            }
            }

    }

    internal override void delete()
    {
        var p = children.Count > 0 ? this.children[0].transform.parent.transform.parent.gameObject : null;
        foreach (var child in this.children)
        {
            if  (child != null )
            {
                    child.delete();
            }
        }
        this.children.Clear();
        GameObject.Destroy(p);
    }

}

