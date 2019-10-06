using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Legend : MonoBehaviour
{
    public Color Colour1;
    public Color Colour2;
    public Color Colour3;
    public Color Colour4;

    public int ClassSeperator1;
    public int ClassSeperator2;
    public int ClassSeperator3;

    // Start is called before the first frame update
    void Start()
    {
        var data = Enumerable.Range(0, this.transform.childCount)
            .Select(this.transform.GetChild)
            .Select(x => x.gameObject)
            .OrderBy(o => o.name);

            data
            .Zip(new Color[] { Colour1, Colour2, Colour3, Colour4 }, (x, y) => (x, y))
            .ToList()
            .ForEach(v =>
            {
                var meshRenderer = v.x.GetComponent<MeshRenderer>();
                meshRenderer.material = new Material(Shader.Find("Sprites/Default"));
                meshRenderer.material.color = v.y;
            });

        var l = new (int,int)[] { (0, ClassSeperator1), (ClassSeperator1, ClassSeperator2), (ClassSeperator2, ClassSeperator3), (ClassSeperator3, -1) };


        data
            .Select(x => x.GetComponentInChildren<VRTK.VRTK_ObjectTooltip>())
            .Zip(l, (x,y) => (x,y))
            .ToList()
            .ForEach(x => x.x.displayText = format(x.y.Item1, x.y.Item2));
            
            
    }
    string format(int num1, int num2)
    {
        if (num2 == -1)
        {
            return num1.ToString() + "+";
        }
        else
        {
            return num1.ToString() + " - " + num2.ToString(); 
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
