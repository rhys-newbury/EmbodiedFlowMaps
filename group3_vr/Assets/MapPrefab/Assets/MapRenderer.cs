using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using VRTK;
using System;
using UnityEngine.UI;
/// <summary>
/// A script used to parse geoJSON and convert in to a series of x,y points for the triangulator.
/// </summary>
public class MapRenderer
{

    //Set up some Regex to strip data from the GeoJSON
    private readonly Regex NAME_REGEX = new Regex(@"(?i)""name"":""(.*?)""");
    private readonly Regex COORDS_REGEX = new Regex(@"(?i),""coordinates"":\[\[(.*?)\]\]");
    private readonly Regex _convert = new Regex(@"(?i)\[(.*?)\],");

    //Constnats for the map dimensions
    private static readonly int MAPWIDTH = 1500;
    private static readonly int MAPHEIGHT = 1577;


    //The size of the outer bounding box of the map in unity units
    private readonly float FINAL_AREA = 1;

  

    private Action<bool> report_grabbed;

    public enum LEVEL
    {
        COUNTRY_LEVEL,
        STATE_LEVEL,
        COUNTY_LEVEL
    }

    /// <summary>
    /// Draw a singular map, such as a county
    /// </summary>
    /// <returns></returns>
    /// 
    public void drawSingular(GameObject gameObject, Action<bool> report_grabbed, string inp_ln, string parentName, int level, InteractableMap parent, float centerX = 0, float centerY = 0, int number = 0)
    {

        drawMultipleMaps(gameObject, report_grabbed, inp_ln,level, parentName, new Vector3(1,1,1), parent, true, centerX, centerY, number);

    }
    /// <summary>
    /// Draw a series of maps. 
    /// </summary>
    /// <returns></returns>
    /// 
    public void drawMultiple(GameObject gameObject, Action<bool> report_grabbed, string dataFile, int level, bool haveTooltip, Vector3 scale, string parentName="",
        InteractableMap parent = null, float centerX = 0, float centerY = 0, int number = 0)
    {
        string data = File.ReadAllText(dataFile);

        if (parentName == "")
        {
            parentName = dataFile.Split('\\')[dataFile.Split('\\').Count() - 1];
            parentName = parentName.Split('.')[0];
        }


        drawMultipleMaps(gameObject, report_grabbed, data,level, parentName,scale, parent, haveTooltip, centerX, centerY, number);
    }


    /// <summary>
    /// Draw a series of maps, parsing the geoJSON file.
    /// </summary>
    /// <returns></returns>
    /// 
    public void drawMultipleMaps(GameObject gameObject, Action<bool> report_grabbed, string dataFile, int level, string parentName, Vector3 scale,InteractableMap parent=null, bool haveTooltip=true
        ,float centerX=0, float centerY=0, int number=0)
    {

        //bool done = false;
        int count = 0;
    
        gameObject.transform.SetPositionAndRotation(new Vector3(number, 0, 0), new Quaternion(0, 0, 0, 1));

        List<Tuple<Vector2[], float[], string>> drawingData = new List<Tuple<Vector2[], float[], string>>();

        //StreamReader inp_stm = new StreamReader(dataFile);

        foreach (string inp_ln in dataFile.Split('\n'))
        {
            try
            {
                float maxX = -10000000, maxY = -10000000;
                float minX = 10000000, minY = 10000000;

                //string inp_ln = inp_stm.ReadLine();

                string currentName = NAME_REGEX.Match(inp_ln).Groups[1].ToString();

                string coordinates = COORDS_REGEX.Match(inp_ln).Groups[1].ToString();




                MatchCollection matches = _convert.Matches(coordinates);
                Vector2[] vertices2D = new Vector2[matches.Count];
                int indices = 0;

                foreach (Match match in matches)
                {

                    float x, y;

                    string[] data = match.Groups[1].ToString().Split(',');

                    (x, y) = convert(float.Parse(data[1]), float.Parse(data[0]));

                    vertices2D[indices] = new Vector2(x, y);

                    maxX = Mathf.Max(x, maxX);
                    maxY = Mathf.Max(y, maxY);
                    minX = Mathf.Min(x, minX);
                    minY = Mathf.Min(y, minY);

                    indices++;
                    count++;
                    //Debug.Log(x.ToString() + "," + y.ToString());

                }

                float[] bounds = new float[] { maxX, maxY, minX, minY };

                drawingData.Add(new Tuple<Vector2[], float[], string>(vertices2D, bounds, currentName));
            }
            catch
            {
    
            }
        }

        var totalMaxX = Mathf.Max(drawingData.Select(x => x.Item2[0]).ToArray());
        var totalMaxY = Mathf.Max(drawingData.Select(x => x.Item2[1]).ToArray());
        var totalMinX = Mathf.Min(drawingData.Select(x => x.Item2[2]).ToArray());
        var totalMinY = Mathf.Min(drawingData.Select(x => x.Item2[3]).ToArray());


        var TmpcenterX = (totalMaxX + totalMinX) / 2;
        var TmpcenterY = (totalMaxY + totalMinY) / 2;


        var area = (totalMaxX - totalMinX) * (totalMaxY - totalMinY);

        var factor = Mathf.Sqrt(FINAL_AREA / area) * (parentName == "America" ? 5 : 1);

        drawingData = drawingData.Select(x => new Tuple<Vector2[], float[], string>(x.Item1.Select(y => new Vector2(y.x * factor, y.y * factor)).ToArray(),
            new float[] { x.Item2[0] * factor, x.Item2[1] * factor, x.Item2[2] * factor, x.Item2[3] * factor },
            x.Item3)).ToList();


        totalMaxX = Mathf.Max(drawingData.Select(x => x.Item2[0]).ToArray());
        totalMaxY = Mathf.Max(drawingData.Select(x => x.Item2[1]).ToArray());
        totalMinX = Mathf.Min(drawingData.Select(x => x.Item2[2]).ToArray());
        totalMinY = Mathf.Min(drawingData.Select(x => x.Item2[3]).ToArray());


        TmpcenterX = (totalMaxX + totalMinX) / 2;
        TmpcenterY = (totalMaxY + totalMinY) / 2;

        area = (totalMaxX - totalMinX) * (totalMaxY - totalMinY);

        var children = new List<InteractableMap>();

        foreach (var data in drawingData)
        {

            GameObject temp = new GameObject();
            temp.transform.parent = gameObject.transform;

            InteractableMap pointableObject = temp.AddComponent(getType(level)) as InteractableMap;

            string currentName = data.Item3;
            pointableObject.SetParent(gameObject.transform);

            pointableObject.Constructor(data.Item1, data.Item3, temp, data.Item2, parentName, report_grabbed);
            children.Add(pointableObject);

    

            if (parent != null)
            {
                parent.AddChild(pointableObject);
                pointableObject.parent = parent;
            }
          

        }



        float maximumY = 0;
        foreach (var child in children)
        {
            child.SetPositionAndRotation(child.GetTranslation(TmpcenterX, TmpcenterY), child.GetAngle());
            child.SetSiblings(children);
            maximumY = Mathf.Max(maximumY, child.transform.parent.position.y);
        }
        


        gameObject.transform.SetPositionAndRotation(new Vector3(0-centerX, 1-centerY, -2), children[0].GetFinalAngle());


        if (level != (int)LEVEL.COUNTY_LEVEL && haveTooltip)
        {
            GameObject objectToolTip = UnityEngine.Object.Instantiate(Resources.Load("ObjectTooltip")) as GameObject;
            objectToolTip.transform.parent = gameObject.transform;

            objectToolTip.transform.localPosition = new Vector3(0, maximumY + 0.15F, 0);
        
            VRTK_ObjectTooltip tooltipData = objectToolTip.GetComponent<VRTK_ObjectTooltip>() as VRTK_ObjectTooltip;
            tooltipData.displayText = parentName;
            Text[] backend = tooltipData.GetComponentsInChildren<Text>() as Text[];
            backend.ToList().ForEach(x => x.text = parentName);
            tooltipData.drawLineFrom = objectToolTip.transform;
            tooltipData.drawLineTo = objectToolTip.transform;
        }

        gameObject.transform.SetPositionAndRotation(new Vector3(0 - centerX, 1 - centerY, -2), children[0].GetFinalAngle());

        gameObject.transform.localPosition = new Vector3(0, 0, 0);
        gameObject.transform.localScale = scale;
    }


    /// <summary>
    /// Convert latitude and longitude data to (x,y) pairs.
    /// </summary>
    /// <returns>(float, float) containing x and y respectively</returns>
    /// 
    public static (float, float) convert(float latitude, float longitude, bool mercator=false) {

        if (mercator)
        {

            float x = (longitude + 180) * (MAPWIDTH / 360);

            float latRad = latitude * Mathf.PI / 180;

            float mercN = Mathf.Log(Mathf.Tan((Mathf.PI / 4) + (latRad / 2)));
            float y = (MAPHEIGHT / 2) - (MAPWIDTH * mercN / (2 * Mathf.PI));

            return (x / 100, -y / 100);
        }
        else
        {

            float lat0 = -96F * (Mathf.PI / 180);  // Latitude_Of_Origin
            float lng0 = 37.5F * (Mathf.PI / 180);  // Central_Meridian
            float phi1 = 29.5F * (Mathf.PI / 180);   // Standard_Parallel_1
            float phi2 = 45.5F * (Mathf.PI / 180);  // Standard_Parallel_2


            float n = 0.5F * (Mathf.Sin(phi1) + Mathf.Sin(phi2));
            float c = Mathf.Cos(phi1);
            float C = c * c + 2 * n * Mathf.Sin(phi1);
            float p0 = Mathf.Sqrt(C - 2 * n * Mathf.Sin(lat0)) / n;
            float theta = n * (longitude * Mathf.PI / 180 - lng0);
            float p = Mathf.Sqrt(C - 2 * n * Mathf.Sin(latitude * Mathf.PI / 180)) / n;

            float x = p * Mathf.Sin(theta);
            float y = p0 - p * Mathf.Cos(theta);

            return (-y, x);
        }

    }
    /// <summary>
    /// Convert level integer in to the correct type of which to create the map instance
    /// </summary>
    /// <returns>type, which the map instance belongs too./returns>
    /// 
    private System.Type getType(int level)
    {
        if (level == (int)LEVEL.COUNTRY_LEVEL)
        {
            return typeof(StateInCountry);
        }
        else if (level == (int)LEVEL.STATE_LEVEL)
        {
            return typeof(CountyInState);
        }
        else if (level == (int)LEVEL.COUNTY_LEVEL)
        {
            return typeof(County);
        }
        else
        {
            return null;
        }
    }

    
}
