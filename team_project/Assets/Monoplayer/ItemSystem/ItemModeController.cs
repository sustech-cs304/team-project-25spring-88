using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ItemModeController : MonoBehaviour
{
    [Tooltip("是否为道具模式，控制 ItemLine 是否显示")]
    public bool isItemMode = true;

    [Tooltip("ItemLine 对象的名称关键词")]
    public string itemLineKeyword = "ItemLine";

    [Tooltip("Road Network 根物体")]
    public GameObject roadNetworkRoot;

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

    // 可选：外部调用接口，切换模式时调用
    public void ToggleItemMode(bool enabled)
    {
        isItemMode = enabled;
        if (roadNetworkRoot != null)
        {
            ApplyItemModeRecursive(roadNetworkRoot.transform);
        }
    }
}
