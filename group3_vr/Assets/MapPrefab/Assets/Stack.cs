using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// A stack class which creates an animated stack of maps
/// </summary>
public class Stack : InteractableObject
{

    public enum Sepration { HorizontalSeperation, VerticalSeperation};
    public Sepration seperationMethod;

    public float seperationDistance;

    public List<MapContainer> positonStack = new List<MapContainer>();
    //public List<MapContainer> countyStack = new List<MapContainer>();

    internal Vector3 prevPos;
    private Quaternion prevRot;
    /// <summary>
    /// Find position of which the object should be placed
    /// </summary>
    /// <returns>float, position along the stack.</returns>
    /// 
    internal float GetYPosition(int i)
    {
        return i * seperationDistance;
    }
    /// <summary>
    /// Save the starting position of the map. 
    /// </summary>
    /// <returns></returns>
    /// 
    void Start()
    {

        prevPos = this.gameObject.transform.position;

    }


    /// <summary>
    /// On update, move the stack objects along with the handle.
    /// </summary>
    /// 
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
    /// <summary>
    /// On removal on an object from the stack.
    /// </summary>
    /// <returns></returns>
    /// 
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
                    }
                    else
                    {
                        var translation = this.transform.TransformDirection(new Vector3(0, seperationDistance, 0));

                        positonStack[i].transform.position -= translation;
               
                    }
                }
            }

            positonStack.Remove(drawObject);
        }
        
    }
    /// <summary>
    /// Remove the destoyed object from the stack.
    /// </summary>
    /// <returns></returns>
    /// 
    internal void destroy(MapContainer mapContainer)
    {
        stack_remove(mapContainer);
    }
    /// <summary>
    /// Add a map object to the stack.
    /// </summary>
    /// <returns></returns>
    /// 
    internal void addMap(GameObject gameObject, MapContainer container)
    {
        gameObject.transform.position = this.gameObject.transform.position;
        
        if (seperationMethod == Sepration.VerticalSeperation)
        {
            gameObject.transform.position += new Vector3(0, GetYPosition(positonStack.Count + 1), 0);
        }
        else
        {
            var translation = this.transform.TransformDirection(new Vector3(0, GetYPosition(positonStack.Count + 1), 0));

            gameObject.transform.position += translation;

        }
        positonStack.Add(container);

    }
}
