using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine.EventSystems;
using VRTK;
using System;

public class draw_object : MonoBehaviour
{

    private int currentLevel = 0;

    private void Start()
    {
        string file = "C:\\Users\\FIT3161\\Desktop\\group3\\group3_vr\\mapGeoJSON\\data_test.txt";
        mapRenderer map = new mapRenderer();
        map.drawSingular(this.gameObject, file);

        

    }

    internal void updateScene(List<PointableObject> current)
    {
        Debug.Log("Upating");
        int childs = transform.childCount;
        for (var i = childs - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        mapRenderer map = new mapRenderer();
        map.drawMultiple(this.gameObject, current);
     }
}


