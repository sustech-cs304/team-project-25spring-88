using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class KeyImageSwitcherTests
{
    private GameObject obj;
    private KeyImageSwitcher switcher;
    private RawImage image;
    private Texture normalTexture;
    private Texture pressedTexture;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        obj = new GameObject("KeyImageSwitcherObject");
        switcher = obj.AddComponent<KeyImageSwitcher>();

        // 创建 RawImage
        var imageGO = new GameObject("Image");
        image = imageGO.AddComponent<RawImage>();

        // 创建假贴图
        normalTexture = new Texture2D(2, 2);
        pressedTexture = new Texture2D(2, 2);

        // 构造 KeyImagePair 并注入
        var pair = new KeyImageSwitcher.KeyImagePair
        {
            key = KeyCode.Space,
            targetImage = image,
            normalTexture = normalTexture,
            pressedTexture = pressedTexture
        };

        // 用反射设置 keyImagePairs（private 类中 public 字段）
        switcher.keyImagePairs = new KeyImageSwitcher.KeyImagePair[] { pair };

        yield return null;
    }

    [UnityTest]
    public IEnumerator ManuallyTriggerKeyChange_SetsPressedTexture()
    {
        // 模拟按下状态
        image.texture = normalTexture;

        // 反射获取 keyImagePairs
        var field = typeof(KeyImageSwitcher).GetField("keyImagePairs", BindingFlags.Public | BindingFlags.Instance);
        var pairs = (KeyImageSwitcher.KeyImagePair[])field.GetValue(switcher);

        // 模拟按下：直接手动调用
        pairs[0].targetImage.texture = pairs[0].pressedTexture;

        yield return null;

        Assert.AreEqual(pressedTexture, image.texture);
    }

    [UnityTest]
    public IEnumerator ManuallyTriggerKeyChange_SetsNormalTexture()
    {
        // 模拟抬起状态
        image.texture = pressedTexture;

        var field = typeof(KeyImageSwitcher).GetField("keyImagePairs", BindingFlags.Public | BindingFlags.Instance);
        var pairs = (KeyImageSwitcher.KeyImagePair[])field.GetValue(switcher);

        // 模拟抬起
        pairs[0].targetImage.texture = pairs[0].normalTexture;

        yield return null;

        Assert.AreEqual(normalTexture, image.texture);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(obj);
        Object.DestroyImmediate(image.gameObject);
        Object.DestroyImmediate(normalTexture);
        Object.DestroyImmediate(pressedTexture);
    }
}