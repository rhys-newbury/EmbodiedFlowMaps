using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using VRTK;
using UnityEngine.UI;

/// <summary>
/// An InteractableMap, specifically an instance of a County.
/// </summary>
public class County : InteractableMap
{

    
    private static readonly Quaternion Angle = new Quaternion(0, 0.6F, 0.8F, 0);
    private static readonly Quaternion FinalAngle = new Quaternion(0, 0, 1, 0);

    public List<Buildings> buildings = new List<Buildings>();

    public float speed = 0.05f;

    GameObject cube;
    GameObject dataCube;

    // Variables for applying scale transformations

    private float startTime;
    private float journeyLength;
    private float journeyLengthData;
    private float capacity;



    /// <summary>
    /// On start create buildings and update the tooltips to be based on the data.
    /// </summary>
    /// 
    new void Start()
    {
       
        startTime = Time.time;
        CreateBuildings();

        // Creates tooltips for the buildings. Displays volume and capacity information for each facility
       
        foreach(var building in buildings)
        {
            VRTK_ObjectTooltip tooltipData = building.tooltip;
            tooltipData.displayText = "Capacity: " + building.Volume.ToString() + "\nVolume: " + building.Data.ToString();
            Text[] backend = tooltipData.GetComponentsInChildren<Text>() as Text[];
            backend.ToList().ForEach(x => x.text = "Capacity: " + building.Volume.ToString() + "\nVolume: " + building.Data.ToString());

        }
    }

    /// <summary>
    /// Specify the level of the Map.
    /// </summary>
    /// <returns>The level of the map</returns>
    /// 
    public override int GetLevel()
    {
        return 2;
    }



    /// <summary>
    /// On update, animate buildings such that they grow from the floor.
    /// </summary>
    /// 
    public void Update()
    {

        float distCovered = (Time.time - startTime) * speed;

        float fracJourney = distCovered / journeyLength;

        if (fracJourney > 1) return;

        foreach (var building in this.buildings)
        {

            journeyLength = Vector3.Distance(building.CapacityCube.transform.localScale, new Vector3(0.01f, 0.5f, 0.01f));

            building.CapacityCube.transform.localScale = Vector3.Lerp(building.CapacityCube.transform.localScale, new Vector3(0.01f, 0.5f + building.Volume / 1000, 0.01f), fracJourney);

            building.VolumeCube.transform.localScale = Vector3.Lerp(building.CapacityCube.transform.localScale, new Vector3(0.01f, building.Data / 1000, 0.01f), 0.5f);

        }
    }

    /// <summary>
    /// Override default behavaiour, as the County is rotate.
    /// </summary>
    /// 
    public override Quaternion GetAngle()
    {
        return Angle;
    }
    /// <summary>
    /// Override default behavaiour, as the County is rotated.
    /// </summary>
    /// 
    public override Quaternion GetFinalAngle()
    {
        return FinalAngle;
    }

    /// <summary>
    /// Override default behavaiour, as there are currently no Internal Flows
    /// </summary>
    /// 
    public override void GetInternalFlows(InteractableMap origin)
    {
        return;
    }
    /// <summary>
    /// Override default behavaiour, as there are currently no Internal Flows
    /// </summary>
    /// 
    public override void AddLine(GameObject line)
    {
        return;
    }
    /// <summary>
    /// Override default behavaiour, as there are currently no Internal Flows
    /// </summary>
    /// 
    public override void RemoveLines()
    {
        return;
    }
    /// <summary>
    /// Override default behavaiour, as there are currently no Internal Flows
    /// </summary>
    /// 
    //internal override void AddToList(string parentName, string name)
    //{
    //    return;
    //}


    /// <summary>
    /// Get translation of an object, converting from x,y based on rotation.
    /// </summary>
    /// 
    public override Vector3 GetTranslation(float x, float y)
    {
        return new Vector3(x, 0.28F*y, -0.96F*y);
    }

    /// <summary>
    /// Override default behavaiour, just disable the tooltip
    /// </summary>
    /// 
    public override void OnPointerLeave()
    {
        this.go.SetActive(false);
    }
    /// <summary>
    /// Override default behavaiour, just enable the tooltip
    /// </summary>
    /// 
    public override void OnPointerEnter(Action<string> change_text)
    {
        change_text(this.GetName() + "," + this.parentName);
        this.go.SetActive(true);

    }

    /// <summary>
    /// Create the buildings in the correct spot, based on list
    /// </summary>
    /// 
    public void CreateBuildings() {

        startTime += 0.1f;

        foreach (var building in this.buildings)
        {
            createCapacityBuiding(building);
            CreateDataBuilding(building);
        }
            
    }

    private void createCapacityBuiding(Buildings building)
    {
        building.CapacityCube.GetComponent<MeshRenderer>().material = Resources.Load("Materials/Glass", typeof(Material)) as Material;
        building.CapacityCube.transform.position = building.GameObj.transform.position + new Vector3(0f, 0.08f, 0f);
        building.CapacityCube.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        building.CapacityCube.transform.SetParent(this.transform);
    }

    public void CreateDataBuilding(Buildings building)

    {
        building.VolumeCube.transform.position = building.GameObj.transform.position + new Vector3(0f, 0.05f, 0f);

        building.VolumeCube.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        building.VolumeCube.transform.SetParent(this.transform);

        building.VolumeCube.GetComponent<MeshRenderer>().material = Resources.Load("Materials/GlowingGreen", typeof(Material)) as Material;

    }




}


    




