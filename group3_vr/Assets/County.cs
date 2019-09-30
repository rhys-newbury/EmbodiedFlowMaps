using System;
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
using static mapRenderer;


public class County : PointableObject
{

    
    private static readonly Quaternion Angle = new Quaternion(0, 0.6F, 0.8F, 0);
    private static readonly Quaternion FinalAngle = new Quaternion(0, 0, 1, 0);




    public List<Buildings> buildings = new List<Buildings>();

    public float speed = 0.05f;


    GameObject cube;
    GameObject dataCube;
    GameObject dummyScaler;


    // Variables for applying scale transformations

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


        // Rhys fixed this with his eyes

        // Creates tooltips for the buildings. Displays volume and capacity information for each facility
        
        foreach(var building in buildings)
        {
            VRTK_ObjectTooltip tooltipData = building.tooltip;
            tooltipData.displayText = "Capacity: " + building.volume.ToString() + "\nVolume: " + building.data.ToString();
            Text[] backend = tooltipData.GetComponentsInChildren<Text>() as Text[];
            backend.ToList().ForEach(x => x.text = "Capacity: " + building.volume.ToString() + "\nVolume: " + building.data.ToString());

        }
    }

    public override int GetLevel()

    {
        return 2;
    }



    // Animates the buildings to scale upwards once county is created

    public void Update()
    {

        var l = buildingData[this.parentName]?[this.name];

        foreach (var building in l)
        {

            journeyLength = Vector3.Distance(building.CapacityCube.transform.localScale, new Vector3(0.01f, 0.5f, 0.01f));


            float distCovered = (Time.time - startTime) * speed;

            float fracJourney = distCovered / journeyLength;

            building.CapacityCube.transform.localScale = Vector3.Lerp(building.CapacityCube.transform.localScale, new Vector3(0.01f, 0.5f + building.Volume / 1000, 0.01f), fracJourney);

            building.VolumeCube.transform.localScale = Vector3.Lerp(building.CapacityCube.transform.localScale, new Vector3(0.01f, 0.2f + building.Data / 1000, 0.01f), 0.5f);

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

    public override void onPointLeave()
    {
        this.go.SetActive(false);
    }

    public override void onPointEnter(Action<string> change_text)
    {
        change_text(this.getName());
        this.go.SetActive(true);

    }


    public void CreateBuildings()

    {

        startTime += 0.1f;

        var l = buildingData[this.parentName][this.name];

            foreach (var building in l)
            {


                building.CapacityCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                building.CapacityCube.GetComponent<MeshRenderer>().material = Resources.Load("Materials/Glass", typeof(Material)) as Material;
                building.CapacityCube.transform.position = building.GameObj.transform.position + new Vector3(0f, 0.2f, 0f);
                building.CapacityCube.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                building.CapacityCube.transform.SetParent(this.transform);


            }
            
        }


    public void CreateDataBuildings()

    {


        var l = buildingData[this.parentName][this.name];

        foreach (var building in l)
        {

            
            building.VolumeCube = GameObject.CreatePrimitive(PrimitiveType.Cube);


            building.VolumeCube.transform.position = building.GameObj.transform.position + new Vector3(0f, 0.28f, 0f);


            // building.volumeCube.transform.position += new Vector3(0f, -0.1f, 0f);

            building.VolumeCube.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

            building.VolumeCube.transform.SetParent(this.transform);

            Vector3 pos = building.VolumeCube.transform.position;



            building.VolumeCube.GetComponent<MeshRenderer>().material = Resources.Load("Materials/GlowingGreen", typeof(Material)) as Material;

           
            buildings.Add(building);

            //dummyScaler.transform.position = pos += new Vector3(0f, -0.16f, 0f);

            //dummyScaler.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

            //building.volumeCube.transform.SetParent(dummyScaler.transform);
        }

    }




}


    




