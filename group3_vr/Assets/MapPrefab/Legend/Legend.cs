using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// A Legend class which uses class seperators to return colours based on chosen scheme
/// </summary>
public class Legend : MonoBehaviour
{


    public float ClassSeperator1;
    public float ClassSeperator2;
    public float ClassSeperator3;
    public float ClassSeperator4;
    public float ClassSeperator5;
    public float ClassSeperator6;

    internal List<Color> scheme;

    public enum ColourScheme { schemeBlues, schemeGreens, schemeGreys, schemeOranges, schemePurples, schemeReds, schemeBuGn, schemeBuPu, schemeGnBu, schemeOrRd, schemePuBuGn, schemeBlYu };
    public ColourScheme LegendColour;


    /// <summary>
    /// Update tooltips of maps and set colour of legend boxes.
    /// </summary>
    /// <returns></returns>
    /// 
    private void Start()
    {
        //Convert chosen colour scheme enumerable, to colour scheme.
        scheme = getColourScheme(LegendColour).Select(stringToColor).ToList();


        var data = Enumerable.Range(0, this.transform.childCount)
                .Select(this.transform.GetChild)
                .Select(x => x.gameObject)
                .OrderBy(o => o.name)
                .Where(x => x.name != "SwitchModes");

        data
            .Zip(scheme, (x, y) => (x, y))
            .ToList()
            .ForEach(v =>
            {
                var meshRenderer = v.x.GetComponent<MeshRenderer>();
                meshRenderer.material = new Material(Shader.Find("Sprites/Default"));
                meshRenderer.material.color = v.y;
            });



        var class_data = new float[] { ClassSeperator1, ClassSeperator2, ClassSeperator3, ClassSeperator4, ClassSeperator5, ClassSeperator6 };

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
               if(tootip)  tootip.displayText = format(x.y.x, x.y.y);
            });
    }

    /// <summary>
    /// Format the two numbers to be shown on the legend.
    /// </summary>
    /// <returns></returns>
    /// 
    private string format(float num1, float num2)
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

    /// <summary>
    /// Convert colour scheme enumarable in to actual colour scheme.
    /// </summary>
    /// <returns>Array of Colour String Hex Codes</returns>
    /// 
    private string[] getColourScheme(ColourScheme ColourSchemeNum)
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
            case (ColourScheme.schemeBlYu):
                return new string[] { "#BDE5BB", "#ABD9BB", "#99CCBB", "#87C0BC", "#75B3BC", "#63A7BC", "#51A4BC" };
            default:
                return new string[] { "#f6eff7", "#d0d1e6", "#a6bddb", "#67a9cf", "#3690c0", "#02818a", "#016450" };
        }
    }
    /// <summary>
    /// Convert colour hex code string in to Color
    /// </summary>
    /// <returns>Color represented by hex code.</returns>
    /// 
    private Color stringToColor(string hexCode)
    {
        Color colour;
        ColorUtility.TryParseHtmlString(hexCode, out colour);
        return colour;
    }

}
