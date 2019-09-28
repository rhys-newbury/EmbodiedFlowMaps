using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class State : PointableObject
{

    public override int getLevel()
    {
        return 1;
    }

    internal override void addToList(string parentName, string name)
    {

        dataAccessor.addToList(parentName, name);

    }


    public override void getInternalFlows(PointableObject origin)
    {
        foreach (var state in origin.siblings)
        {
            try
            {
                if (state.Key != origin.name)
                {
                    var destination = state.Value;

                    if (destination.isSelected())
                    {


                        Bezier b = new Bezier(this.transform, origin, destination);

                        this.lines.Add(b.obj);
                        destination.addLine(b.obj);

                    }

                }
            }
            catch
            {
                
            }
        }

    }
    private List<GameObject> lines = new List<GameObject>();

    public override void addLine(GameObject line)
    {
        lines.Add(line);
    }
    public override void removeLines()
    {
        lines.ToList().ForEach(Destroy);
        lines.Clear();
    }


}
