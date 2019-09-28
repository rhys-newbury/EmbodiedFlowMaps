using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static mapRenderer;


public class County : PointableObject
{

    
    private static readonly Quaternion Angle = new Quaternion(0, 0.6F, 0.8F, 0);
    private static readonly Quaternion FinalAngle = new Quaternion(0, 0, 1, 0);

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
        
        //cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //dataCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //dummyScaler = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //dummyScaler.GetComponent<Renderer>().enabled = false;

        startTime = Time.time;
        CreateBuildings();

        CreateDataBuildings();

        

    }

    public override int GetLevel()
    {
        return 2;
    }


    public void Update()
    {

        var l = buildingData[this.parentName][this.name];

        

        foreach (var building in l)
        {

            journeyLength = Vector3.Distance(building.CapacityCube.transform.localScale, new Vector3(0.01f, 0.5f, 0.01f));




            float distCovered = (Time.time - startTime) * speed;


            float fracJourney = distCovered / journeyLength;


            building.CapacityCube.transform.localScale = Vector3.Lerp(building.CapacityCube.transform.localScale, new Vector3(0.01f, 0.5f + building.Volume / 1000, 0.01f), fracJourney);

            building.VolumeCube.transform.localScale = Vector3.Lerp(building.CapacityCube.transform.localScale, new Vector3(0.01f, 0.2f + building.Data / 1000, 0.01f), 0.5f);

            building.tooltip.displayText = this.name;
        }




    }

    public override Quaternion GetAngle()
    {
        return Angle;
    }

    public override Quaternion GetFinalAngle()
    {
        return FinalAngle;
    }


    public override Vector3 GetTranslation(float x, float y)
    {
        return new Vector3(x, 0.28F*y, -0.96F*y);
    }


    public void CreateBuildings()
    {


        startTime += 0.1f;

        var l = buildingData[this.parentName][this.name];

            foreach (var building in l)
            {
                //building.cube.name = "CONTAINER";
                building.CapacityCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                building.CapacityCube.GetComponent<MeshRenderer>().material = Resources.Load("Materials/Glass", typeof(Material)) as Material;
                building.CapacityCube.transform.position = building.GameObj.transform.position;
                building.CapacityCube.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                building.CapacityCube.transform.SetParent(this.transform);


            }

           // cube.GetComponent<MeshRenderer>().material = Resources.Load("Materials/Glass", typeof(Material)) as Material;
            

            //cube.transform.position = this.transform.parent.position;

            //cube.transform.position += new Vector3(0f, 0.2f, 0f);

            //cube.transform.SetParent(this.transform);

            //dummyScaler.transform.SetParent(this.transform);


        }


    public void CreateDataBuildings()

    {


        var l = buildingData[this.parentName][this.name];

        foreach (var building in l)
        {

            building.VolumeCube = GameObject.CreatePrimitive(PrimitiveType.Cube);


            building.VolumeCube.transform.position = building.GameObj.transform.position;



           // building.volumeCube.transform.position += new Vector3(0f, -0.1f, 0f);

            building.VolumeCube.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

            building.VolumeCube.transform.SetParent(this.transform);

            Vector3 pos = building.VolumeCube.transform.position;



            building.VolumeCube.GetComponent<MeshRenderer>().material = Resources.Load("Materials/GlowingGreen", typeof(Material)) as Material;

            //dummyScaler.transform.position = pos += new Vector3(0f, -0.16f, 0f);

            //dummyScaler.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

            //building.volumeCube.transform.SetParent(dummyScaler.transform);
        }

    }




}


    




