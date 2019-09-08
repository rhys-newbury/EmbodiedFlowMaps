using UnityEngine;
using VRTK;
using System;

public class Buildings : MonoBehaviour
{
    public float volume { get; set; }
    public float data { get; set; }
    public GameObject gameObj { get; private set; } 

    public GameObject capacityCube { get; set; }

    public GameObject volumeCube { get; set; }

    public GameObject go;

    public VRTK_ObjectTooltip tooltip;



    public Buildings()
    {
        gameObj = new GameObject();
        go = Instantiate(Resources.Load("ObjectTooltip")) as GameObject;
        tooltip = go.GetComponent<VRTK_ObjectTooltip>() as VRTK_ObjectTooltip;
        tooltip.alwaysFaceHeadset = true;
        this.go.SetActive(false);
        go.transform.SetPositionAndRotation(gameObj.transform.position, gameObj.transform.rotation);


    }
}