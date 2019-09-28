using UnityEngine;
using VRTK;
using System;

public class Buildings : MonoBehaviour
{
    public float Volume { get; set; }
    public float Data { get; set; }
    public GameObject GameObj { get; } 

    public GameObject CapacityCube { get; set; }

    public GameObject VolumeCube { get; set; }

    public GameObject go;

    public VRTK_ObjectTooltip tooltip;



    public Buildings()
    {
        GameObj = new GameObject();
        go = Instantiate(Resources.Load("ObjectTooltip")) as GameObject;
        tooltip = go.GetComponent<VRTK_ObjectTooltip>() as VRTK_ObjectTooltip;
        tooltip.alwaysFaceHeadset = true;
        this.go.SetActive(false);
        go.transform.SetPositionAndRotation(GameObj.transform.position, GameObj.transform.rotation);


    }
}