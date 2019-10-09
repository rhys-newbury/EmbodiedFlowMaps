using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Legend : MonoBehaviour
{
    //public Color Colour1;
    //public Color Colour2;
    //public Color Colour3;
    //public Color Colour4;
    //public Color Colour5;
    //public Color Colour6;
    //public Color Colour7;

    //Based on D3 colour scheme
    public enum ColourScheme { schemeBlues, schemeGreens, schemeGreys, schemeOranges, schemePurples, schemeReds, schemeBuGn, schemeBuPu, schemeGnBu, schemeOrRd, schemePuBuGn };
    public ColourScheme LegendColour;



    public int ClassSeperator1;
    public int ClassSeperator2;
    public int ClassSeperator3;
    public int ClassSeperator4;
    public int ClassSeperator5;
    public int ClassSeperator6;

    internal List<Color> scheme;

    // Start is called before the first frame update
    void Start()
    {
        scheme = getColourScheme(LegendColour).Select(stringToColor).ToList();

        var data = Enumerable.Range(0, this.transform.childCount)
            .Select(this.transform.GetChild)
            .Select(x => x.gameObject)
            .OrderBy(o => o.name)
            .Where(x => x.name != "SwitchModes");

        if (this.name == "Legend")
        {
            data
                .Zip(scheme, (x, y) => (x, y))
                .ToList()
                .ForEach(v =>
                {
                    var meshRenderer = v.x.GetComponent<MeshRenderer>();
                    meshRenderer.material = new Material(Shader.Find("Sprites/Default"));
                    meshRenderer.material.color = v.y;
                });
        }
        else
        {
            data
                .Zip(scheme, (x, y) => (x, y))
                .ToList()
                .ForEach(v =>
                {
                    var meshRenderer = v.x.GetComponent<MeshRenderer>();
                    meshRenderer.material.SetColor("_EmissionColor", v.y);
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

    public string[] getColourScheme(ColourScheme ColourSchemeNum)
    {
        switch (ColourSchemeNum)
        {
            case (ColourScheme.schemeBlues):
                return new string[] { "#eff3ff", "#c6dbef", "#9ecae1", "#6baed6", "#4292c6", "#2171b5", "#084594" };
            case (ColourScheme.schemeGreens):
                return new string[] { "#edf8e9", "#c7e9c0", "#a1d99b", "#74c476", "#41ab5d", "#238b45", "#005a32" };
            case (ColourScheme.schemeGreys):
                return new string[] { "#f7f7f7", "#d9d9d9", "#bdbdbd", "#969696", "#737373", "#525252", "#252525" };
            case (ColourScheme.schemeOranges):
                return new string[] { "#feedde", "#fdd0a2", "#fdae6b", "#fd8d3c", "#f16913", "#d94801", "#8c2d04" };
            case (ColourScheme.schemePurples):
                return new string[] { "#f2f0f7", "#dadaeb", "#bcbddc", "#9e9ac8", "#807dba", "#6a51a3", "#4a1486" };
            case (ColourScheme.schemeReds):
                return new string[] { "#fee5d9", "#fcbba1", "#fc9272", "#fb6a4a", "#ef3b2c", "#cb181d", "#99000d" };
            case (ColourScheme.schemeBuPu):
                return new string[] { "#edf8fb", "#bfd3e6", "#9ebcda", "#8c96c6", "#8c6bb1", "#88419d", "#6e016b" };
            case (ColourScheme.schemeGnBu):
                return new string[] { "#f0f9e8", "#ccebc5", "#a8ddb5", "#7bccc4", "#4eb3d3", "#2b8cbe", "#08589e" };
            case (ColourScheme.schemeOrRd):
                return new string[] { "#fef0d9", "#fdd49e", "#fdbb84", "#fc8d59", "#ef6548", "#d7301f", "#990000" };
            default:
                return new string[] { "#f6eff7", "#d0d1e6", "#a6bddb", "#67a9cf", "#3690c0", "#02818a", "#016450" };
        }
    }

    public Color stringToColor(string hexCode)
    {
        Color colour;
        ColorUtility.TryParseHtmlString(hexCode, out colour);
        return colour;
    }

// Update is called once per frame
void Update()
    {
        
    }
}
