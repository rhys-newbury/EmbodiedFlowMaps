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

    internal override void delete()
    {
        if (children.Count > 0) Debug.Log(children[0]);
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

