using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// An Interactable Object, specifically a toggle button. Toggles the data between incoming and outgoing.
/// </summary>
public class DataToggle : InteractableObject
{
    /// <summary>
    /// Handle the button, by listening to trigger pressed.
    /// </summary>
    /// <returns></returns>
    public override GameObject OnTriggerPressed(Transform direction)
    {

        //change bool in mapController
        MapController.isIncoming = !MapController.isIncoming;
        //change cylinder scale
        if (this.gameObject.transform.localScale.y == 0.28F) {
            this.gameObject.transform.localScale = new Vector3(0.4F, 0.94F, 0.4F);
        }
        else {
            this.gameObject.transform.localScale = new Vector3(0.4F, 0.28F, 0.4F);
        }

        //Change every InteractableMap colour to be based on updated data.
        var l = GameObject.FindObjectsOfType<InteractableMap>();
        foreach (InteractableMap item in l)
        {
            item.updateColour();
        }

        var tooltip =  this.transform.parent.GetComponentInChildren<VRTK.VRTK_ObjectTooltip>();
        if (tooltip.displayText == "Outgoing Flow")
        {

            tooltip.displayText = "Incoming Flow";
            Text[] backend = tooltip.GetComponentsInChildren<Text>() as Text[];
            backend.ToList().ForEach(x => x.text = "Incoming Flow");

        }
        else
        {
            tooltip.displayText = "Outgoing Flow";
            Text[] backend = tooltip.GetComponentsInChildren<Text>() as Text[];
            backend.ToList().ForEach(x => x.text = "Outgoing Flow");

        }

        return null;


    }
  
}
