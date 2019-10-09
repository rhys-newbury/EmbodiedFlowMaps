using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stack : Pointable
{

    public enum Sepration { HorizontalSeperation, VerticalSeperation};
    public Sepration seperationMethod;

    public float seperationDistance;

    public List<MapContainer> positonStack = new List<MapContainer>();
    //public List<MapContainer> countyStack = new List<MapContainer>();

    internal Vector3 prevPos;
    private Quaternion prevRot;

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
        if (seperationMethod == Sepration.VerticalSeperation) {
            this.gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
         }
        else
        {
            this.gameObject.transform.rotation = Quaternion.Euler(90, 0, 0);

        }
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
                    //gameObject.transform.localScale -= new Vector3(0, 0, 0.05F);

                }
                else
                {
                    if (seperationMethod == Sepration.VerticalSeperation)
                    {
                        positonStack[i].transform.position -= new Vector3(0, seperationDistance, 0);
                        //gameObject.transform.localScale -= new Vector3(0, 0, 0.05F);

                    }
                    else
                    {
                        var translation = this.transform.TransformDirection(new Vector3(0, seperationDistance, 0));

                        positonStack[i].transform.position -= translation;
                        //gameObject.transform.localScale -= new Vector3(0, 0.05F, 0);


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
        
        if (seperationMethod == Sepration.VerticalSeperation)
        {
            gameObject.transform.position += new Vector3(0, GetYPosition(positonStack.Count + 1), 0);
            //gameObject.transform.localScale += new Vector3(0/*,*/ 0.05F, 0);

        }
        else
        {
            var translation = this.transform.TransformDirection(new Vector3(0, GetYPosition(positonStack.Count + 1), 0));

            gameObject.transform.position += translation;
            //gameObject.transform.localScale += new Vector3(0, 0, 0.05F);


        }
        positonStack.Add(container);

        //container.transform.parent = this.transform;





    }
}
