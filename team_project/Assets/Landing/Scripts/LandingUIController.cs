using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandingUIController : MonoBehaviour
{
    public void EnableItemMode()
    {
        ItemModeController.isItemMode = true;
        Debug.Log("Item Mode Enabled");
    }

    public void DisableItemMode()
    {
        ItemModeController.isItemMode = false;
        Debug.Log("Item Mode Disabled");
    }

    public void ToggleItemMode()
    {
        ItemModeController.isItemMode = !ItemModeController.isItemMode;
        Debug.Log("Item Mode Toggled");
    }
}
