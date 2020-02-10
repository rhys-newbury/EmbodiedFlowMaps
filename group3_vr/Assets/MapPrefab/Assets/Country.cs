using System.Linq;
/// <summary>
/// An InteractableMap, specifically an instance of a Country.
/// </summary>
public class StateInCountry : InteractableMap
{

    /// <summary>
    /// Specify the level of the Map.
    /// </summary>
    /// <returns>The level of the map</returns>
    /// 
    public override int GetLevel()
    {
        return 0;
    }


    /// <summary>
    /// Override delete, such that the no Country Map can be deleted. However, the children can be.
    /// </summary>
    /// 
    internal override void Delete()
    {
        this.children = this.children.Where(x => x != null).ToList();
        var p = children.Count > 0 ? this.children[0].transform.parent.transform.parent.gameObject : null;
        foreach (var child in this.children)
        {
            if  (child != null )
            {
                    child.Delete();
            }
        }
        this.Deselect();
        this.children.Clear();
        Destroy(p);
    }

}

