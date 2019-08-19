using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static mapRenderer;


public class County : PointableObject
{

    
    private static Quaternion Angle = new Quaternion(0, 0.6F, 0.8F, 0);
    private static Quaternion FinalAngle = new Quaternion(0, 0, 1, 0);


    private List<GameObject> buildings = new List<GameObject>();



    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        Debug.Log("County Stuff");
        createBuildings();

        
    }

    public override Quaternion getAngle()
    {
        return Angle;
    }

    public override Quaternion getFinalAngle()
    {
        return FinalAngle;
    }


    public override Vector3 getTranslation(float x, float y)
    {
        return new Vector3(x, 0.28F*y, -0.96F*y);
    }


    public void createBuildings()
    {
        string file = "C:\\Users\\Jesse\\Documents\\group3_vr\\group3_vr\\dummycounty.txt";

        StreamReader inp_stm = new StreamReader(file);

        int myTuple;

        float x, y;

        while ((!inp_stm.EndOfStream))
        {
            string inp_ln = inp_stm.ReadLine();

            string[] strArray = inp_ln.Split(',');

            List<string> strData = new List<string>();

            foreach (string str in strArray)
            {
                strData.Add(str);
            }

            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);



            cube.GetComponent<MeshRenderer>().material = Resources.Load("Materials/Glass", typeof(Material)) as Material;
               
            cube.name = "BUILDING";

            (x, y) = mapRenderer.convert(float.Parse(strData[0]), float.Parse(strData[1]));

            //cube.transform.position = new Vector3(x, y, 0);


            cube.transform.position = this.transform.parent.position;



            cube.transform.localScale = new Vector3(0.01f, 0.5f, 0.01f);

            


        }



    }

}
