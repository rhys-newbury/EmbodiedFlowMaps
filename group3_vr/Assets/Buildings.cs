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
        objectToolTip.transform.parent = (capacityCube.transform);
        objectToolTip.transform.SetPositionAndRotation(capacityCube.transform.position, capacityCube.transform.rotation);
        //objectToolTip.transform.position = new Vector3(objectToolTip.transform.position.x, 0.1f, objectToolTip.transform.position.z);

        VRTK_ObjectTooltip tooltipData = objectToolTip.GetComponent<VRTK_ObjectTooltip>() as VRTK_ObjectTooltip;
        tooltipData.drawLineFrom = objectToolTip.transform;
        tooltipData.drawLineTo = objectToolTip.transform;

        tooltipData.displayText = "I'm a building";


    }
}