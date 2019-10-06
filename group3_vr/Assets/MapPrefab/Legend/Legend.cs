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
        Enumerable.Range(0, this.transform.childCount)
            .Select(this.transform.GetChild)
            .Select(x => x.gameObject)
            .OrderBy(o => o.name)
            .Zip(new Color[] { Colour1, Colour2, Colour3, Colour4 }, (x, y) => (x, y))
            .ToList()
            .ForEach(v =>
            {
                var meshRenderer = v.x.GetComponent<MeshRenderer>();
                meshRenderer.material = new Material(Shader.Find("Sprites/Default"));
                meshRenderer.material.color = v.y;
            });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
