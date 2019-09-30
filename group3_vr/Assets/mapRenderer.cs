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

public class mapRenderer
{

    //Set up some Regex to strip data from the GeoJSON
    private readonly Regex NAME_REGEX = new Regex(@"(?i)""name"":""(.*?)""");
    private readonly Regex COORDS_REGEX = new Regex(@"(?i),""coordinates"":\[\[(.*?)\]\]");
    private readonly Regex _convert = new Regex(@"(?i)\[(.*?)\],");

    //Constnats for the map dimensions
    private static readonly int MAPWIDTH = 1000;
    private static readonly int MAPHEIGHT = 50;


    //The size of the outer bounding box of the map in unity units
    private readonly float FINAL_AREA = 1;

    //List of string data for the buildings. This can be converted in to actual buildings on county creation
    private static Dictionary<String, Dictionary<String, List<List<String>>>> _buildingData = null;

    //List of buildings
    public static Dictionary<String, Dictionary<String, List<Buildings>>> buildingData = new Dictionary<String, Dictionary<String, List<Buildings>>>();


    private Action<bool> report_grabbed;

    public enum LEVEL
    {
        COUNTRY_LEVEL,
        STATE_LEVEL,
        COUNTY_LEVEL
    }

    public static bool checkForBuildings(string County, string State)
    {
        return _buildingData.ContainsKey(State) && _buildingData[State].ContainsKey(County);
    }

    public mapRenderer() {
        //Only do this once.
        if (_buildingData == null) {
            //Create a dictionary.
            _buildingData = new Dictionary<String, Dictionary<String, List<List<String>>>>();

            StreamReader inp_stm = new StreamReader("C:\\Users\\FIT3161\\Desktop\\group3\\group3_vr\\data_processing_scripts\\building_data.csv");

                
            while (!inp_stm.EndOfStream) {

                string inp_ln = inp_stm.ReadLine();
                
                List<String> data = inp_ln.Split(',').ToList();

                //Check for existance otherwise add a new dictionary
                if (!_buildingData.ContainsKey(data[4])) {
                    _buildingData.Add(data[4], new Dictionary<string, List<List<string>>>());
                }
                Dictionary<String, List<List<String>>> stateData = _buildingData[data[4]];

                
                if (!stateData.ContainsKey(data[3])) {
                    stateData.Add(data[3], new List<List<string>>());
                }

                //Add the data to the state
                stateData[data[3]].Add(data);

            }
        }
    }
    public void drawSingular(GameObject gameObject, Action<bool> report_grabbed, string inp_ln, string parentName, int level, PointableObject parent, float centerX = 0, float centerY = 0, int number = 0)
    {

        drawMultipleThing(gameObject, report_grabbed, inp_ln,level, parentName, parent, centerX, centerY, number);

    }

    public void drawMultiple(GameObject gameObject, Action<bool> report_grabbed, string dataFile, int level,
        PointableObject parent = null, float centerX = 0, float centerY = 0, int number = 0)
    {
        string data = File.ReadAllText(dataFile);

        string parentName = dataFile.Split('\\')[dataFile.Split('\\').Count() - 1];
        parentName = parentName.Split('.')[0];


        drawMultipleThing(gameObject, report_grabbed, data,level, parentName, parent, centerX, centerY, number);
    }



    public void drawMultipleThing(GameObject gameObject, Action<bool> report_grabbed, string dataFile, int level, string parentName, PointableObject parent=null, float centerX=0, float centerY=0, int number=0)
    {

        //bool done = false;
        int count = 0;
    
        gameObject.transform.SetPositionAndRotation(new Vector3(number, 0, 0), new Quaternion(0, 0, 0, 1));

        List<Tuple<Vector2[], float[], string>> drawingData = new List<Tuple<Vector2[], float[], string>>();

        //StreamReader inp_stm = new StreamReader(dataFile);

        foreach (string inp_ln in dataFile.Split('\n'))
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

            }

            float[] bounds = new float[] { maxX, maxY, minX, minY };

            drawingData.Add(new Tuple<Vector2[], float[], string>(vertices2D, bounds, currentName));
        }

        var totalMaxX = Mathf.Max(drawingData.Select(x => x.Item2[0]).ToArray());
        var totalMaxY = Mathf.Max(drawingData.Select(x => x.Item2[1]).ToArray());
        var totalMinX = Mathf.Min(drawingData.Select(x => x.Item2[2]).ToArray());
        var totalMinY = Mathf.Min(drawingData.Select(x => x.Item2[3]).ToArray());


        var TmpcenterX = (totalMaxX + totalMinX) / 2;
        var TmpcenterY = (totalMaxY + totalMinY) / 2;


        var area = (totalMaxX - totalMinX) * (totalMaxY - totalMinY);

        var factor = Mathf.Sqrt(FINAL_AREA / area);

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

        var children = new List<PointableObject>();

        foreach (var data in drawingData)
        {

            GameObject temp = new GameObject();
            PointableObject pointableObject = temp.AddComponent(getType(level)) as PointableObject;

            string currentName = data.Item3;

            pointableObject.Constructor(data.Item1, data.Item3, temp, data.Item2, parentName, report_grabbed);
            pointableObject.SetParent(gameObject.transform);
            children.Add(pointableObject);
            if (parent != null)
            {
                parent.AddChild(pointableObject);
                pointableObject.parent = parent;
            }

            if (level == (int) LEVEL.COUNTY_LEVEL &&
                (_buildingData.ContainsKey(parentName) && _buildingData[parentName].ContainsKey(currentName)))
            {
                var buildingList = _buildingData[parentName][currentName];

                if (!buildingData.ContainsKey(parentName))
                {
                    buildingData.Add(parentName, new Dictionary<string, List<Buildings>>());
                }

                Dictionary<String, List<Buildings>> stateData = buildingData[parentName];

                if (!stateData.ContainsKey(currentName))
                {
                    stateData.Add(currentName, new List<Buildings>());
                }

                stateData[currentName].Clear();


                foreach (var b in buildingList)
                {
                    var newBuilding = new Buildings();
                    float x, y;
                    (x, y) = convert(float.Parse(b[0]), float.Parse(b[1]));
                    x *= factor;
                    y *= factor;

                    newBuilding.GameObj.transform.SetPositionAndRotation(new Vector3(x, y, 0),
                        new Quaternion(0, 0, 0, 1));
                    newBuilding.GameObj.transform.parent = pointableObject.transform;
                    newBuilding.Data = float.Parse(b[5]);
                    newBuilding.Volume = float.Parse(b[6]);

                    stateData[currentName].Add(newBuilding);



                }



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


        if (level != (int)LEVEL.COUNTY_LEVEL)
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

        //GameObject go = MonoBehaviour.Instantiate(Resources.Load("ObjectTooltip")) as GameObject;
        //go.transform.parent = gameObject.transform;
        //go.transform.SetPositionAndRotation(gameObject.transform.position, gameObject.transform.rotation);
        //go.transform.position = new Vector3(go.transform.position.x, totalMinY - 0.1F, go.transform.position.z);


        //VRTK_ObjectTooltip tooltip = go.GetComponent<VRTK_ObjectTooltip>() as VRTK_ObjectTooltip;
        //tooltip.displayText = parentName;

    }


    //This function converts a latitude/longitude pair to x,y
    public static (float, float) convert(float latitude, float longitude) {

        float x = (longitude + 180) * (MAPWIDTH / 360);

        float latRad = latitude * Mathf.PI / 180;

        float mercN = Mathf.Log(Mathf.Tan((Mathf.PI / 4) + (latRad / 2)));
        float y = (MAPHEIGHT / 2) - (MAPWIDTH * mercN / (2 * Mathf.PI));

        return (x / 100, -y / 100);

    }

    private System.Type getType(int level)
    {
        if (level == (int)LEVEL.COUNTRY_LEVEL)
        {
            return typeof(Country);
        }
        else if (level == (int)LEVEL.STATE_LEVEL)
        {
            return typeof(State);
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
