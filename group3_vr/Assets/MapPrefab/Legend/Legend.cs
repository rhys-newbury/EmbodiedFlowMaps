using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Legend : MonoBehaviour
{
    public Color Colour1;
    public Color Colour2;
    public Color Colour3;
    public Color Colour4;
    public Color Colour5;
    public Color Colour6;
    public Color Colour7;


    public int ClassSeperator1;
    public int ClassSeperator2;
    public int ClassSeperator3;
    public int ClassSeperator4;
    public int ClassSeperator5;
    public int ClassSeperator6;


    // Start is called before the first frame update
    void Start()
    {
        var data = Enumerable.Range(0, this.transform.childCount)
            .Select(this.transform.GetChild)
            .Select(x => x.gameObject)
            .OrderBy(o => o.name)
            .Where(x => x.name != "SwitchModes");
        if (this.name == "Legend")
        {
            data
                .Zip(new Color[] { Colour1, Colour2, Colour3, Colour4, Colour5, Colour6, Colour7 }, (x, y) => (x, y))
                .ToList()
                .ForEach(v =>
                {
                    var meshRenderer = v.x.GetComponent<MeshRenderer>();
                    meshRenderer.material = new Material(Shader.Find("Sprites/Default"));
                    meshRenderer.material.color = v.y;
                });
        }

        var class_data = new int[] { ClassSeperator1, ClassSeperator2, ClassSeperator3, ClassSeperator4, ClassSeperator5, ClassSeperator6 };

        var data1 = class_data.ToList();
        data1.Add(-1);

        var data2 = class_data.ToList();
        data2.Insert(0, 0);

        var ranges = data1.Zip(data2, (x, y) => (x, y));

        data
            .Zip(ranges, (x, y) => (x, y))
            .ToList()
            .ForEach(x =>
           {
               var tootip = x.x.GetComponentInChildren<VRTK.VRTK_ObjectTooltip>();
               tootip.displayText = format(x.y.x, x.y.y);
           });
                     



    }

    string format(int num1, int num2)
    {
        if (num1 == -1)
        {
            return num2.ToString() + "+";
        }
        else
        {
            return num2.ToString() + "-" + num1.ToString();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
