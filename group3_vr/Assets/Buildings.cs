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

        GameObject objectToolTip = MonoBehaviour.Instantiate(Resources.Load("ObjectTooltip")) as GameObject;
        objectToolTip.transform.parent = (CapacityCube.transform);
        objectToolTip.transform.SetPositionAndRotation(CapacityCube.transform.position, CapacityCube.transform.rotation);
        //objectToolTip.transform.position = new Vector3(objectToolTip.transform.position.x, 0.1f, objectToolTip.transform.position.z);

        VRTK_ObjectTooltip tooltipData = objectToolTip.GetComponent<VRTK_ObjectTooltip>() as VRTK_ObjectTooltip;
        tooltipData.drawLineFrom = objectToolTip.transform;
        tooltipData.drawLineTo = objectToolTip.transform;

        tooltipData.displayText = "I'm a building";



    }
}