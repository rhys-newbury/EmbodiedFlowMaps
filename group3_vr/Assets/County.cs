using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static mapRenderer;


public class County : PointableObject
{

    
    private static Quaternion Angle = new Quaternion(0, 0.6F, 0.8F, 0);
    private static Quaternion FinalAngle = new Quaternion(0, 0, 1, 0);

    private List<GameObject> buildings = new List<GameObject>();

    public float speed = 0.05f;

    GameObject cube;
    GameObject dataCube;
    GameObject dummyScaler;


    private float startTime;

    private float journeyLength;

    private float journeyLengthData;






    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        Debug.Log("County Stuff");
        cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        dataCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        dummyScaler = GameObject.CreatePrimitive(PrimitiveType.Cube);
        dummyScaler.GetComponent<Renderer>().enabled = false;

        startTime = Time.time;
        
        createBuildings();

        createDataBuildings();

        journeyLength = Vector3.Distance(cube.transform.localScale, new Vector3(0.01f, 0.5f, 0.01f));

        journeyLengthData = Vector3.Distance(dataCube.transform.localScale, new Vector3(0.01f, 0.2f, 0.01f));






    }


    public void Update()
    {
        float distCovered = (Time.time - startTime) * speed;

        float distCoveredData = (Time.time - startTime) * speed;

        float fracJourney = distCovered / journeyLength;

        float fracJourneyData = distCoveredData / journeyLengthData;

        cube.transform.localScale = Vector3.Lerp(cube.transform.localScale, new Vector3(0.01f, 0.2f, 0.01f), fracJourney);

        //dataCube.transform.localScale = Vector3.Lerp(cube.transform.localScale, new Vector3(0.01f, 0.2f, 0.01f), 0.5f);

        //dummyScaler.transform.localScale = Vector3.Lerp(dummyScaler.transform.localScale, new Vector3(0f, 0.1f, 0f), fracJourney);




        Vector3 temp = dataCube.transform.localScale;
        Vector3 temp2 = dataCube.transform.localPosition;
        if(fracJourney > 0.09f)
        {
            if (temp.y < 2f)
            {
                temp.y += 0.1f;
                temp2.y += 0.05f;
                

                dataCube.transform.localScale = temp;
                dataCube.transform.localPosition = temp2;

            }
        }
        

        

    }

    public override Quaternion getAngle()
    {
        return Angle;
    }

    public override Quaternion getFinalAngle()
    {
        return FinalAngle;
    }


    public override Vector3 getTranslation(float x, float y)
    {
        return new Vector3(x, 0.28F*y, -0.96F*y);
    }


    public void createBuildings()
    {


        startTime += 0.1f;

        //string file = "C:\\Users\\Jesse\\Documents\\group3_vr\\group3_vr\\dummycounty.txt";

        string file = "C:\\Users\\FIT3161\\Desktop\\group3\\group3_vr\\dummycounty.txt";

     StreamReader inp_stm = new StreamReader(file);

        int myTuple;

        float x, y;

        while ((!inp_stm.EndOfStream))
        {
            string inp_ln = inp_stm.ReadLine();

            string[] strArray = inp_ln.Split(',');

            List<string> strData = new List<string>();

            foreach (string str in strArray)
            {
                strData.Add(str);
            }


            //journeyLength = Vector3.Distance(cube.transform.localScale, new Vector3(0.01f, 0.5f, 0.01f));




            cube.GetComponent<MeshRenderer>().material = Resources.Load("Materials/Glass", typeof(Material)) as Material;

            //dataCube.GetComponent<MeshRenderer>().material = Resources.Load("Materials/GlowingGreen", typeof(Material)) as Material;

            cube.name = "CONTAINER";
            dataCube.name = "DATA";
            dummyScaler.name = "SCALER";

            (x, y) = mapRenderer.convert(float.Parse(strData[0]), float.Parse(strData[1]));

            //cube.transform.position = new Vector3(x, y, 0);


            cube.transform.position = this.transform.parent.position;

            cube.transform.position += new Vector3(0f, 0.05f, 0f);

            cube.transform.SetParent(this.transform);

            dummyScaler.transform.SetParent(this.transform);

            //dataCube.transform.position = this.transform.parent.position;


            //cube.transform.localScale = new Vector3(0.01f, 0.5f, 0.01f);


            


        }



    }

    public void createDataBuildings()
    {
        dataCube.transform.position = this.transform.parent.position;

        dataCube.transform.position += new Vector3(0f, -0.1f, 0f);

        dataCube.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        Vector3 pos  = dataCube.transform.position;



        dataCube.GetComponent<MeshRenderer>().material = Resources.Load("Materials/GlowingGreen", typeof(Material)) as Material;

        dummyScaler.transform.position = pos += new Vector3(0f, -0.16f, 0f);

        dummyScaler.transform.localScale = new Vector3(0.01f, 0.1f, 0.01f);

        dataCube.transform.SetParent(dummyScaler.transform);


    }

}

