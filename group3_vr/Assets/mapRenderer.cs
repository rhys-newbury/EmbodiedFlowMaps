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

    private static readonly Regex _name = new Regex(@"(?i)""name"":""(.*?)""");
    private static readonly Regex _coordinates = new Regex(@"(?i),""coordinates"":\[\[(.*?)\]\]");
    private static readonly Regex _convert = new Regex(@"(?i)\[(.*?)\],");

    private static int mapWidth = 1000;
    private static int mapHeight = 50;

    private static readonly float SQRT = 1 / Mathf.Sqrt(2);

    private static readonly float FINAL_AREA = 1;


    public mapRenderer() { }

    public void drawSingular(GameObject gameObject, string dataFile, float centerX=0, float centerY=0, int number=0)
    {
        //bool done = false;
        int count = 0;
        var parent = GameObject.Find("object" + number.ToString());

        parent.transform.SetPositionAndRotation(new Vector3(number, 0, 0), new Quaternion(0, 0, 0, 1));



        List<Tuple<Vector2[], float[], string>> drawingData = new List<Tuple<Vector2[], float[],string>>();

        StreamReader inp_stm = new StreamReader(dataFile);
        while (!inp_stm.EndOfStream)
        {
            float maxX = -10000000, maxY = -10000000;
            float minX = 10000000, minY = 10000000;

            string inp_ln = inp_stm.ReadLine();

            string StateName = _name.Match(inp_ln).Groups[1].ToString();
         
            string coordinates = _coordinates.Match(inp_ln).Groups[1].ToString();
            

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

            drawingData.Add(new Tuple<Vector2[], float[], string>(vertices2D, bounds, StateName));


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


        //< VRTK_InteractableObject >(new VRTK_InteractableObject());

        var children = new List<PointableObject>();

        foreach (var data in drawingData) {

            GameObject temp = new GameObject();
            PointableObject pointableObject = temp.AddComponent(typeof(PointableObject)) as PointableObject;
            //Doesnt work
            pointableObject.constructor(data.Item1, data.Item3, temp, data.Item2);
            pointableObject.setParent(parent.transform);

            children.Add(pointableObject);
        }

        MeshFilter[] meshFilters = parent.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        int i = 0;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            i++;

        }
        parent.transform.GetComponent<MeshFilter>().mesh = new Mesh();
        parent.transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);

        Debug.Log(TmpcenterX);
        Debug.Log(TmpcenterY);

        foreach (var child in children)
        {
            child.transform.SetPositionAndRotation(new Vector3(TmpcenterX, -TmpcenterY, 0), new Quaternion(0, 1, 0, 0));
        }    
        
        parent.transform.SetPositionAndRotation(new Vector3(0-centerX, 1-centerY, -2), new Quaternion(0, 0, 0, 1));



    }

    internal void drawMultiple(GameObject gameObject, List<PointableObject> current)
    {
        decideGridPos(current);

        drawSingular(gameObject, "C:\\Users\\FIT3161\\Desktop\\group3\\group3_vr\\mapGeoJSON\\data3.txt", 0.15F, 0.05F,0);
        drawSingular(gameObject, "C:\\Users\\FIT3161\\Desktop\\group3\\group3_vr\\mapGeoJSON\\data3.txt", 0.15F, 0.35F,1);
        drawSingular(gameObject, "C:\\Users\\FIT3161\\Desktop\\group3\\group3_vr\\mapGeoJSON\\data3.txt", 0.55F, 0.05F,2);
        drawSingular(gameObject, "C:\\Users\\FIT3161\\Desktop\\group3\\group3_vr\\mapGeoJSON\\data3.txt", 0.55F, 0.35F,3);


    }

    private void decideGridPos(List<PointableObject> currentObjects)
    {
        //Modifies the original pointable objects to fill in their grid positions

        float width = Mathf.Ceil(Mathf.Sqrt(currentObjects.Count));
        float height = Mathf.Ceil(currentObjects.Count / width);

        var XSorted = currentObjects.OrderBy(x => x.getMinX()).ToList();
        var YSorted = currentObjects.OrderBy(x => x.getMinY()).ToList();

        for (int i = 0; i < currentObjects.Count; i++)
        {
            XSorted[i].gridX = (int)Mathf.Floor(i / width);
            YSorted[i].gridY = (int)Mathf.Floor(i / height);
        }

    }

    private (float, float) convert(float latitude, float longitude)
    {

        float x = (longitude + 180) * (mapWidth / 360);

        float latRad = latitude * Mathf.PI / 180;

        float mercN = Mathf.Log(Mathf.Tan((Mathf.PI / 4) + (latRad / 2)));
        float y = (mapHeight / 2) - (mapWidth * mercN / (2 * Mathf.PI));

        return (-x / 100, -y / 100);

    }
}
