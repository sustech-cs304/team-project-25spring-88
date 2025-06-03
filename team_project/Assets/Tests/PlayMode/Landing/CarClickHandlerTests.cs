using NUnit.Framework;
using UnityEngine;

public class LandingUIControllerTests_t
{
    private GameObject landingObj;
    private LandingUIController uiController;

    [SetUp]
    public void SetUp()
    {
        landingObj = new GameObject("LandingUIControllerObj");
        uiController = landingObj.AddComponent<LandingUIController>();
        ItemModeController.isItemMode = false; // 确保起始状态
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(landingObj);
    }

    [Test]
    public void EnableItemMode_SetsIsItemModeTrue()
    {
        uiController.EnableItemMode();
        Assert.IsTrue(ItemModeController.isItemMode, "EnableItemMode 应该将 isItemMode 设为 true");
    }

    [Test]
    public void DisableItemMode_SetsIsItemModeFalse()
    {
        ItemModeController.isItemMode = true;
        uiController.DisableItemMode();
        Assert.IsFalse(ItemModeController.isItemMode, "DisableItemMode 应该将 isItemMode 设为 false");
    }

    [Test]
    public void ToggleItemMode_TogglesCorrectly()
    {
        ItemModeController.isItemMode = false;
        uiController.ToggleItemMode();
        Assert.IsTrue(ItemModeController.isItemMode, "ToggleItemMode 应该将 isItemMode 从 false 变为 true");

        uiController.ToggleItemMode();
        Assert.IsFalse(ItemModeController.isItemMode, "再次调用 ToggleItemMode 应该将 isItemMode 从 true 变为 false");
    }
}