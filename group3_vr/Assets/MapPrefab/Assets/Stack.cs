using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stack : MonoBehaviour
{

    public enum Sepration { HorizontalSeperation, VerticalSeperation};
    public Sepration seperationMethod;

    public float seperationDistance;

    public List<MapContainer> positonStack = new List<MapContainer>();
    //public List<MapContainer> countyStack = new List<MapContainer>();

    internal Vector3 prevPos;

    internal float GetYPosition(int i)
    {
        return i * seperationDistance;
    }
    // Start is called before the first frame update
    void Start()
    {

        prevPos = this.gameObject.transform.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        var diff = this.gameObject.transform.position - prevPos;
        prevPos = this.gameObject.transform.position;
        if (diff.magnitude == 0) return;

        foreach (var item in positonStack)
        {
            item.transform.position += diff;
        }

    }

    internal void stack_remove(MapContainer drawObject)
    {

        if (positonStack.Contains(drawObject))
        {
            var index = positonStack.IndexOf(drawObject);

            for (int i = index + 1; i < positonStack.Count; i++)
            {
                if (positonStack.Count < 10 && seperationMethod == Sepration.HorizontalSeperation)
                {
                    positonStack[i].animationStarted = true;
                    positonStack[i].startPos = positonStack[i].transform.localPosition;
                    positonStack[i].anim.Play("ZMovement");
                }
                else
                {
                    if (seperationMethod == Sepration.VerticalSeperation)
                    {
                        positonStack[i].transform.position -= new Vector3(0, seperationDistance, 0);
                    }
                    else
                    {
                        positonStack[i].transform.position -= new Vector3(0, 0, seperationDistance);

                    }
                }
            }

            positonStack.Remove(drawObject);
        }
        
    }

    internal void destroy(MapContainer mapContainer)
    {
        stack_remove(mapContainer);
    }

    internal void addMap(GameObject gameObject, MapContainer container)
    {
        gameObject.transform.position = this.gameObject.transform.position;
        gameObject.transform.position += new Vector3(0, 0, GetYPosition(positonStack.Count + 1));
        positonStack.Add(container);

    }
}
