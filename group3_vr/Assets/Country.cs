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

    public override int GetLevel()
    {
        return 0;
    }
    private readonly List<GameObject> lines = new List<GameObject>();
    public override void AddLine(GameObject line)
    {
        lines.Add(line);
    }
    public override void RemoveLines()
    {
        lines.ToList().ForEach(Destroy);
        lines.Clear();
    }

    internal override void AddToList(string parentName, string name)
    {

        dataAccessor.addToList(parentName, name);

    }


    public override void GetInternalFlows(PointableObject origin)
    {
        foreach (var state in origin.siblings)
        {
            try {
                if (state.Key != origin.name)
                {
                    var destination = state.Value;

                    if (destination.IsSelected())
                    {


                        Bezier b = new Bezier(this.transform, origin, destination);

                        this.lines.Add(b.obj);
                        destination.AddLine(b.obj);

                    }

                }
            }
            catch { }
        }

    }

    internal override void Delete()
    {
        var p = children.Count > 0 ? this.children[0].transform.parent.transform.parent.gameObject : null;
        foreach (var child in this.children)
        {
            if  (child != null )
            {
                    child.Delete();
            }
        }
        this.children.Clear();
        Destroy(p);
    }

}

