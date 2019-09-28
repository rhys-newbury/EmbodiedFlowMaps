using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class State : PointableObject
{

    public override int GetLevel()
    {
        return 1;
    }

    internal override void AddToList(string parentName, string name)
    {

        dataAccessor.addToList(parentName, name);

    }


    public override void GetInternalFlows(PointableObject origin)
    {
        foreach (var state in origin.siblings)
        {
            try
            {
                if (state.Key != origin.name)
                {
                    var destination = state.Value;

                    if (destination.IsSelected())
                    {


                        Bezier b = new Bezier(this.transform, origin, destination);

                        this.lines.Add(b.obj);
                        destination.AddLine(b.obj);

                    }

                }
            }
            catch
            {
                
            }
        }

    }
    private List<GameObject> lines = new List<GameObject>();

    public override void AddLine(GameObject line)
    {
        lines.Add(line);
    }
    public override void RemoveLines()
    {
        lines.ToList().ForEach(Destroy);
        lines.Clear();
    }


}
