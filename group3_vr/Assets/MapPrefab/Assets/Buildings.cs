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

        CapacityCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        VolumeCube = GameObject.CreatePrimitive(PrimitiveType.Cube);

        GameObject objectToolTip = MonoBehaviour.Instantiate(Resources.Load("CountyTooltip")) as GameObject;
        objectToolTip.transform.parent = (CapacityCube.transform);
        objectToolTip.transform.SetPositionAndRotation(CapacityCube.transform.position, CapacityCube.transform.rotation);
        //objectToolTip.transform.position = new Vector3(objectToolTip.transform.position.x, 0.1f, objectToolTip.transform.position.z);


        tooltip = objectToolTip.GetComponent<VRTK_ObjectTooltip>() as VRTK_ObjectTooltip;
        tooltip.transform.SetParent((GameObj.transform));

        tooltip.transform.SetPositionAndRotation(CapacityCube.transform.position, CapacityCube.transform.rotation);


        tooltip.transform.localScale = new Vector3(-0.5f, 0.5f, 0.5f);

        tooltip.transform.localPosition -= new Vector3(0f, 0f, 0.2f);

        tooltip.transform.localEulerAngles += new Vector3(-70, 0, 0); 

        //tooltipData.alwaysFaceHeadset = true;
        tooltip.drawLineFrom = CapacityCube.transform;
        tooltip.drawLineTo = CapacityCube.transform;

    }





}