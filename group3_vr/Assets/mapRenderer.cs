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

    private static int mapWidth = 2000;
    private static int mapHeight = 1000;

    private static readonly float SQRT = 1 / Mathf.Sqrt(2);


    public mapRenderer() { }

    public void drawSingular(GameObject gameObject, string dataFile, float centerX=0, float centerY=0)
    {
        //bool done = false;
        int count = 0;


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

        var area = (totalMaxX - totalMinX) * (totalMaxY - totalMinY);

        //Doesnt work yet
        //Stephen to fix.
        var ZShift = area * -0.5F + 7F;

        foreach (var data in drawingData) {

            var vertices2D = data.Item1.Select(x => new Vector2(x.x, x.y)).ToArray();
            GameObject temp = new GameObject();
            PointableObject pointableObject = temp.AddComponent(typeof(PointableObject)) as PointableObject;
            pointableObject.setOrigin(totalMaxX - centerX, totalMaxY - centerY, -ZShift);
            pointableObject.constructor(vertices2D, data.Item3, temp, data.Item2);
            pointableObject.setParent(gameObject.transform);
        }

    }

    internal void drawMultiple(GameObject gameObject, List<PointableObject> current)
    {
        decideGridPos(current);

        mapHeight = 2000*10;
        mapWidth = 2000*10;


        drawSingular(gameObject, "C:\\Users\\FIT3161\\Desktop\\group3\\group3_vr\\mapGeoJSON\\data3.txt", -1.5F, 1.5F);


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
