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

        // ���� RawImage
        var imageGO = new GameObject("Image");
        image = imageGO.AddComponent<RawImage>();

        // ��������ͼ
        normalTexture = new Texture2D(2, 2);
        pressedTexture = new Texture2D(2, 2);

        // ���� KeyImagePair ��ע��
        var pair = new KeyImageSwitcher.KeyImagePair
        {
            key = KeyCode.Space,
            targetImage = image,
            normalTexture = normalTexture,
            pressedTexture = pressedTexture
        };

        // �÷������� keyImagePairs��private ���� public �ֶΣ�
        switcher.keyImagePairs = new KeyImageSwitcher.KeyImagePair[] { pair };

        yield return null;
    }

    [UnityTest]
    public IEnumerator ManuallyTriggerKeyChange_SetsPressedTexture()
    {
        // ģ�ⰴ��״̬
        image.texture = normalTexture;

        // �����ȡ keyImagePairs
        var field = typeof(KeyImageSwitcher).GetField("keyImagePairs", BindingFlags.Public | BindingFlags.Instance);
        var pairs = (KeyImageSwitcher.KeyImagePair[])field.GetValue(switcher);

        // ģ�ⰴ�£�ֱ���ֶ�����
        pairs[0].targetImage.texture = pairs[0].pressedTexture;

        yield return null;

        Assert.AreEqual(pressedTexture, image.texture);
    }

    [UnityTest]
    public IEnumerator ManuallyTriggerKeyChange_SetsNormalTexture()
    {
        // ģ��̧��״̬
        image.texture = pressedTexture;

        var field = typeof(KeyImageSwitcher).GetField("keyImagePairs", BindingFlags.Public | BindingFlags.Instance);
        var pairs = (KeyImageSwitcher.KeyImagePair[])field.GetValue(switcher);

        // ģ��̧��
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