using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ItemModeController : MonoBehaviour
{
    [Tooltip("�Ƿ�Ϊ����ģʽ������ ItemLine �Ƿ���ʾ")]
    public bool isItemMode = true;

    [Tooltip("ItemLine ��������ƹؼ���")]
    public string itemLineKeyword = "ItemLine";

    [Tooltip("Road Network ������")]
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
            Debug.LogWarning("δ�ҵ���Ϊ 'Road Network' �����壡");
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

            // �ݹ����������
            ApplyItemModeRecursive(child);
        }
    }

    // ��ѡ���ⲿ���ýӿڣ��л�ģʽʱ����
    public void ToggleItemMode(bool enabled)
    {
        isItemMode = enabled;
        if (roadNetworkRoot != null)
        {
            ApplyItemModeRecursive(roadNetworkRoot.transform);
        }
    }
}
