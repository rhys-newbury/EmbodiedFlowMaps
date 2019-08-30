using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine.EventSystems;
using VRTK;
using System;
public class mapRenderer
{

    private readonly Regex NAME_REGEX = new Regex(@"(?i)""name"":""(.*?)""");
    private readonly Regex COORDS_REGEX = new Regex(@"(?i),""coordinates"":\[\[(.*?)\]\]");
    private readonly Regex _convert = new Regex(@"(?i)\[(.*?)\],");

    private static int MAPWIDTH = 1000;
    private static int MAPHEIGHT = 50;

    private readonly float FINAL_AREA = 1;

    private static Dictionary<String, Dictionary<String, List<List<String>>>> _buildingData = null;

    private static Dictionary<String, Dictionary<String, List<Buildings>>> buildingData = new Dictionary<String, Dictionary<String, List<Buildings>>>();



    public enum LEVEL
    {
        COUNTRY_LEVEL,
        STATE_LEVEL,
        COUNTY_LEVEL
    }

    public mapRenderer() {
        if (_buildingData == null)
        {
            _buildingData = new Dictionary<String, Dictionary<String, List<List<String>>>>();

            StreamReader inp_stm = new StreamReader("C:\\Users\\newbu\\vr\\group3_vr\\data_processing_scripts\\building_data.csv");
                
            while (!inp_stm.EndOfStream)
            {
                string inp_ln = inp_stm.ReadLine();
                Debug.Log(inp_ln);

                List<String> data = inp_ln.Split(',').ToList();

                

                if (!_buildingData.ContainsKey(data[4])) {
                    _buildingData.Add(data[4], new Dictionary<string, List<List<string>>>());
                }
                Dictionary<String, List<List<String>>> stateData = _buildingData[data[4]];

                if (!stateData.ContainsKey(data[3]))
                {
                    stateData.Add(data[3], new List<List<string>>());
                }

                stateData[data[3]].Add(data);

            }

        }
       
    }
    public void drawSingular(GameObject gameObject, string inp_ln, string parentName, int level, float centerX = 0, float centerY = 0, int number = 0)
    {
        //bool done = false;
        int count = 0;


        gameObject.transform.SetPositionAndRotation(new Vector3(number, 0, 0), new Quaternion(0, 0, 0, 1));

        List<Tuple<Vector2[], float[], string>> drawingData = new List<Tuple<Vector2[], float[], string>>();

        float maxX = -10000000, maxY = -10000000;
        float minX = 10000000, minY = 10000000;

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

            if (level == (int)LEVEL.COUNTY_LEVEL && (_buildingData.ContainsKey(parentName) && _buildingData[parentName].ContainsKey(currentName)))
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

                    newBuilding.gameObj.transform.SetPositionAndRotation(new Vector3(x,y, 0), new Quaternion(0, 0, 0, 1));
                    newBuilding.gameObj.transform.parent = pointableObject.transform;

                    stateData[currentName].Add(newBuilding);


                }
            }

            

        pointableObject.constructor(data.Item1, data.Item3, temp, data.Item2, parentName);
        pointableObject.setParent(gameObject.transform);

        children.Add(pointableObject);
    }


    MeshFilter[] meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
    CombineInstance[] combine = new CombineInstance[meshFilters.Length];

    int i = 0;
    while (i < meshFilters.Length)
    {
        combine[i].mesh = meshFilters[i].sharedMesh;
        combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
        i++;

    }
    MeshFilter mf = gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
    gameObject.GetComponent<MeshFilter>().mesh = new Mesh();
    gameObject.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);

    foreach (var child in children)
    {
        child.SetPositionAndRotation(child.getTranslation(TmpcenterX, TmpcenterY), child.getAngle());
    }

    gameObject.transform.SetPositionAndRotation(new Vector3(0 - centerX, 1 - centerY, -2), children[0].getFinalAngle());

    GameObject go = MonoBehaviour.Instantiate(Resources.Load("ObjectTooltip")) as GameObject;
    go.transform.parent = gameObject.transform;
    go.transform.SetPositionAndRotation(gameObject.transform.position, gameObject.transform.rotation);
    go.transform.position = new Vector3(go.transform.position.x, totalMinY - 0.1F, go.transform.position.z);


    VRTK_ObjectTooltip tooltip = go.GetComponent<VRTK_ObjectTooltip>() as VRTK_ObjectTooltip;
    tooltip.displayText = parentName;



    }
    public void drawMultiple(GameObject gameObject, string dataFile, int level, float centerX=0, float centerY=0, int number=0)
    {
        //bool done = false;
        int count = 0;
        string parentName = dataFile.Split('\\')[dataFile.Split('\\').Count() - 1];
        parentName = parentName.Substring(0, parentName.Length - 5);

        gameObject.transform.SetPositionAndRotation(new Vector3(number, 0, 0), new Quaternion(0, 0, 0, 1));

        List<Tuple<Vector2[], float[], string>> drawingData = new List<Tuple<Vector2[], float[], string>>();

        StreamReader inp_stm = new StreamReader(dataFile);

        while (!inp_stm.EndOfStream)
        {
            float maxX = -10000000, maxY = -10000000;
            float minX = 10000000, minY = 10000000;

            string inp_ln = inp_stm.ReadLine();

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

            pointableObject.constructor(data.Item1, data.Item3, temp, data.Item2, parentName);
            pointableObject.setParent(gameObject.transform);

            children.Add(pointableObject);
        }

   

        var maximumY = -100F;


        foreach (var child in children)
        {
            child.SetPositionAndRotation(child.getTranslation(TmpcenterX, TmpcenterY), child.getAngle());
            maximumY = Mathf.Max(maximumY, child.transform.parent.position.y);

        }


        if (level != (int)LEVEL.COUNTY_LEVEL)
        {
            GameObject objectToolTip = MonoBehaviour.Instantiate(Resources.Load("ObjectTooltip")) as GameObject;
            objectToolTip.transform.parent = gameObject.transform;

            objectToolTip.transform.localPosition = new Vector3(0, maximumY + 0.15F, 0);
        
            VRTK_ObjectTooltip tooltipData = objectToolTip.GetComponent<VRTK_ObjectTooltip>() as VRTK_ObjectTooltip;
            tooltipData.displayText = parentName;
            tooltipData.drawLineFrom = objectToolTip.transform;
            tooltipData.drawLineTo = objectToolTip.transform;
        }

        gameObject.transform.SetPositionAndRotation(new Vector3(0 - centerX, 1 - centerY, -2), children[0].getFinalAngle());

        //GameObject go = MonoBehaviour.Instantiate(Resources.Load("ObjectTooltip")) as GameObject;
        //go.transform.parent = gameObject.transform;
        //go.transform.SetPositionAndRotation(gameObject.transform.position, gameObject.transform.rotation);
        //go.transform.position = new Vector3(go.transform.position.x, totalMinY - 0.1F, go.transform.position.z);


        //VRTK_ObjectTooltip tooltip = go.GetComponent<VRTK_ObjectTooltip>() as VRTK_ObjectTooltip;
        //tooltip.displayText = parentName;



    }


    public static (float, float) convert(float latitude, float longitude)
    {

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
