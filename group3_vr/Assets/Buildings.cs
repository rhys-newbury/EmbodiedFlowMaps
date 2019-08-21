using UnityEngine;

internal class Buildings
{
    public float volume { get; set; }
    public float data { get; set; }
    public GameObject gameObj { get; private set; } 

    public Buildings()
    {
        gameObj = new GameObject();
    }
}