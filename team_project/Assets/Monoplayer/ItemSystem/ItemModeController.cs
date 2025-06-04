using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A Unity script that controls the visibility of ItemLine objects in a racing game's road network based on item mode.
/// <para>
/// This script enables or disables GameObjects containing a specific keyword in their name (e.g., "ItemLine") within the road network hierarchy,
/// depending on whether item mode is active. It is designed to manage item-related features in a racing game.
/// </para>
/// </summary>
public class ItemModeController : MonoBehaviour
{
    /// <summary>
    /// Determines whether item mode is active, controlling the visibility of ItemLine objects.
    /// </summary>
    [Tooltip("是否为道具模式，控制 ItemLine 是否显示")]
    public static bool isItemMode = true;

    /// <summary>
    /// The keyword used to identify ItemLine objects in the road network hierarchy.
    /// </summary>
    [Tooltip("ItemLine 对象的名称关键词")]
    public string itemLineKeyword = "ItemLine";

    /// <summary>
    /// The root GameObject of the road network containing ItemLine objects.
    /// </summary>
    [Tooltip("Road Network 根物体")]
    public GameObject roadNetworkRoot;

    /// <summary>
    /// Initializes the script by locating the road network and applying item mode settings.
    /// </summary>
    void Start()
    {
        if (roadNetworkRoot == null)
        {
            roadNetworkRoot = GameObject.Find("Road Network");
        }

        if (roadNetworkRoot != null)
        {
            ApplyItemModeRecursive(roadNetworkRoot.transform);
        }
        else
        {
            Debug.LogWarning("未找到名为 'Road Network' 的物体！");
        }
    }

    /// <summary>
    /// Recursively enables or disables ItemLine objects in the road network hierarchy based on item mode.
    /// </summary>
    /// <param name="parent">The parent Transform to search for ItemLine objects.</param>
    public void ApplyItemModeRecursive(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (child.name.Contains(itemLineKeyword))
            {
                child.gameObject.SetActive(isItemMode);
            }

            // 递归调用子物体
            ApplyItemModeRecursive(child);
        }
    }

    /// <summary>
    /// Toggles item mode and updates the visibility of ItemLine objects.
    /// </summary>
    /// <param name="enabled">True to enable item mode, false to disable it.</param>
    public void ToggleItemMode(bool enabled)
    {
        isItemMode = enabled;
        if (roadNetworkRoot != null)
        {
            ApplyItemModeRecursive(roadNetworkRoot.transform);
        }
    }
}