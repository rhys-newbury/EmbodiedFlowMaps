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
    private readonly List<GameObject> lines = new List<GameObject>();
    public override void addLine(GameObject line)
    {
        lines.Add(line);
    }
    public override void removeLines()
    {
        lines.ToList().ForEach(Destroy);
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


                        Bezier b = new Bezier(this.transform, origin, destination);

                        this.lines.Add(b.obj);
                        destination.addLine(b.obj);

                    }

                }
            }
            catch { }
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
        Destroy(p);
    }

}

