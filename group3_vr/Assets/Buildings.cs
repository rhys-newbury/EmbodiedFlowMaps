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