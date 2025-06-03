using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ItemModeControllerTests
{
    private GameObject root;
    private ItemModeController controller;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        // 创建 Road Network 模拟结构
        root = new GameObject("Road Network");

        var itemLine1 = new GameObject("ItemLine_1");
        itemLine1.transform.SetParent(root.transform);
        var itemLine2 = new GameObject("ItemLine_2");
        itemLine2.transform.SetParent(root.transform);

        var nonItem = new GameObject("NotAnItemLine");
        nonItem.transform.SetParent(root.transform);

        var controllerObj = new GameObject("ItemModeController");
        controller = controllerObj.AddComponent<ItemModeController>();
        controller.roadNetworkRoot = root;
        controller.itemLineKeyword = "ItemLine";

        yield return null;
    }

    [UnityTest]
    public IEnumerator ItemLineObjects_AreActivated_WhenItemModeTrue()
    {
        ItemModeController.isItemMode = true;

        controller.ToggleItemMode(true);

        foreach (Transform child in root.transform)
        {
            if (child.name.Contains("ItemLine"))
            {
                Assert.IsTrue(child.gameObject.activeSelf);
            }
        }

        yield return null;
    }

    [UnityTest]
    public IEnumerator ItemLineObjects_AreDeactivated_WhenItemModeFalse()
    {
        ItemModeController.isItemMode = false;

        controller.ToggleItemMode(false);

        foreach (Transform child in root.transform)
        {
            if (child.name.Contains("ItemLine"))
            {
                Assert.IsFalse(child.gameObject.activeSelf);
            }
        }

        yield return null;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(root);
        Object.DestroyImmediate(controller.gameObject);
    }
}