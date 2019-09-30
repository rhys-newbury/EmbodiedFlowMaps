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

        gameObj = new GameObject();

        capacityCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        volumeCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        GameObject objectToolTip = MonoBehaviour.Instantiate(Resources.Load("ObjectTooltip")) as GameObject;


        tooltip = objectToolTip.GetComponent<VRTK_ObjectTooltip>() as VRTK_ObjectTooltip;
        tooltip.transform.SetParent((gameObj.transform));

        tooltip.transform.SetPositionAndRotation(capacityCube.transform.position, capacityCube.transform.rotation);


        tooltip.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        tooltip.transform.localPosition -= new Vector3(0f, 0f, 0.2f);

        tooltip.transform.localEulerAngles += new Vector3(-70, 0, 0); 

        //tooltipData.alwaysFaceHeadset = true;
        tooltip.drawLineFrom = capacityCube.transform;
        tooltip.drawLineTo = capacityCube.transform;



        tooltip.displayText = "I'm a building";





    }





}