using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointGeneratorScript : MonoBehaviour {

    public GameObject Screen1;
    public GameObject Screen2;

    public int nbLine;

    List<GameObject> sensorToTakeIntoAccount = new List<GameObject>();
    public List<KeyValuePair<GameObject, GameObject>> pointsList = new List<KeyValuePair<GameObject, GameObject>>();


    public UnbundleFD unbundle;
    //public UnbundleCPU unbundleCpu;

    // Use this for initialization
    void Start () {
		for(int i = 0; i<nbLine; i++)
        {
            //P1 Generation
            float x1 = Random.Range(-0.5f, 0.5f);
            float y1 = Random.Range(-0.5f, 0.5f);

            float sign = Mathf.Sign(y1);
            float x2 = Random.Range(-0.5f, 0.5f);
            float y2 = Random.Range(Mathf.Min(0, 0.5f*sign), Mathf.Max(0, 0.5f * sign));

            Vector3 p1S = new Vector3(x1, y1, 0f);
            Vector3 p1W = Screen1.transform.TransformPoint(p1S);
            Vector3 p2S = new Vector3(x2, y2, 0f);
            Vector3 p2W = Screen2.transform.TransformPoint(p2S);

            GameObject p1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            p1.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            p1.transform.SetParent(Screen1.transform);
            p1.transform.position = p1W;
            p1.transform.forward = Screen1.transform.forward;
            p1.name = "Point+"+i;

            GameObject p2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            p2.transform.SetParent(Screen2.transform);
            p2.transform.position = p2W;
            p2.transform.localScale = new Vector3(0.05f, 0.05f, 5f);
            p2.transform.forward = Screen2.transform.forward;
            p2.name = "Point+"+i+"-V";


            KeyValuePair<GameObject, GameObject> kv = new KeyValuePair<GameObject, GameObject>(p1, p2);
            pointsList.Add(kv);
            sensorToTakeIntoAccount.Add(p1);
        }
        //unbundle.initUnbundling(sensorToTakeIntoAccount);
        unbundle.initUnbundling(pointsList);
        //unbundleCpu.initUnbundling(sensorToTakeIntoAccount);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
