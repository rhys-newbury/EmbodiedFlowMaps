/// <summary>
/// An InteractableMap, specifically an instance of a State.
/// </summary>
public class State : InteractableMap
{
    /// <summary>
    /// Specify the level of the Map.
    /// </summary>
    /// <returns>The level of the map</returns>
    /// 
    public override int GetLevel()
    {
        return 1;
    }
}
