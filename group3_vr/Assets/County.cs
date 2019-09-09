using System;
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

    private float capacity;




    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        Debug.Log("County Stuff");
        //cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //dataCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //dummyScaler = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //dummyScaler.GetComponent<Renderer>().enabled = false;

        startTime = Time.time;
        createBuildings();

        createDataBuildings();

        

    }

    public override int getLevel()
    {
        return 2;
    }


    public void Update()
    {

        var l = mapRenderer.buildingData[this.parentName][this.name];

        

        foreach (var building in l)
        {

            journeyLength = Vector3.Distance(building.capacityCube.transform.localScale, new Vector3(0.01f, 0.5f, 0.01f));




            float distCovered = (Time.time - startTime) * speed;


            float fracJourney = distCovered / journeyLength;


            building.capacityCube.transform.localScale = Vector3.Lerp(building.capacityCube.transform.localScale, new Vector3(0.01f, 0.5f + building.volume / 1000, 0.01f), fracJourney);

            building.volumeCube.transform.localScale = Vector3.Lerp(building.capacityCube.transform.localScale, new Vector3(0.01f, 0.2f + building.data / 1000, 0.01f), 0.5f);

        }



        //float distCovered = (Time.time - startTime) * speed;

        //float distCoveredData = (Time.time - startTime) * speed;

        //float fracJourney = distCovered / journeyLength;

        //float fracJourneyData = distCoveredData / journeyLengthData;

        //cube.transform.localScale = Vector3.Lerp(cube.transform.localScale, new Vector3(0.05f, 0.5f + capacity/1000, 0.05f), fracJourney);

        //dataCube.transform.localScale = Vector3.Lerp(cube.transform.localScale, new Vector3(0.01f, 0.2f, 0.01f), 0.5f);

        //dummyScaler.transform.localScale = Vector3.Lerp(dummyScaler.transform.localScale, new Vector3(0f, 0.1f, 0f), fracJourney);




        //Vector3 temp = dataCube.transform.localScale;
        //Vector3 temp2 = dataCube.transform.localPosition;
        //if(fracJourney > 0.09f)
        //{
        //    if (temp.y < 2f)
        //    {
        //        temp.y += 0.1f;
        //        temp2.y += 0.05f;
                

        //        dataCube.transform.localScale = temp;
        //        dataCube.transform.localPosition = temp2;

        //    }
        //}
       
        

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

    public override void onPointLeave()
    {
        this.go.SetActive(false);
    }

    public override void onPointEnter(Action<string> change_text)
    {
        change_text(this.getName());
        this.go.SetActive(true);

    }


    public void createBuildings()
    {


        startTime += 0.1f;

        var l = mapRenderer.buildingData[this.parentName][this.name];

            foreach (var building in l)
            {
                //building.cube.name = "CONTAINER";
                //building.capacityCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                building.capacityCube.GetComponent<MeshRenderer>().material = Resources.Load("Materials/Glass", typeof(Material)) as Material;
                building.capacityCube.transform.position = building.gameObj.transform.position;
                building.capacityCube.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                building.capacityCube.transform.SetParent(this.transform);


            }

           // cube.GetComponent<MeshRenderer>().material = Resources.Load("Materials/Glass", typeof(Material)) as Material;
            

            //cube.transform.position = this.transform.parent.position;

            //cube.transform.position += new Vector3(0f, 0.2f, 0f);

            //cube.transform.SetParent(this.transform);

            //dummyScaler.transform.SetParent(this.transform);


        }


    public void createDataBuildings()

    {


        var l = mapRenderer.buildingData[this.parentName][this.name];

        foreach (var building in l)
        {

            


            building.volumeCube.transform.position = building.gameObj.transform.position;



           // building.volumeCube.transform.position += new Vector3(0f, -0.1f, 0f);

            building.volumeCube.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

            building.volumeCube.transform.SetParent(this.transform);

            Vector3 pos = building.volumeCube.transform.position;



            building.volumeCube.GetComponent<MeshRenderer>().material = Resources.Load("Materials/GlowingGreen", typeof(Material)) as Material;

            //dummyScaler.transform.position = pos += new Vector3(0f, -0.16f, 0f);

            //dummyScaler.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

            //building.volumeCube.transform.SetParent(dummyScaler.transform);
        }

    }




}


    




