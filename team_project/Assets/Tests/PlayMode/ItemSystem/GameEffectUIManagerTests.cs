using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TMPro;

public class GameEffectUIManagerTests
{
    private GameObject canvasObj;
    private GameEffectUIManager manager;
    private TextMeshProUGUI effectText;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        // ���� Canvas �� Text ����
        canvasObj = new GameObject("Canvas");
        canvasObj.AddComponent<Canvas>();

        var textObj = new GameObject("EffectText");
        textObj.transform.SetParent(canvasObj.transform);
        effectText = textObj.AddComponent<TextMeshProUGUI>();

        var managerObj = new GameObject("EffectManager");
        manager = managerObj.AddComponent<GameEffectUIManager>();

        // �ֶ��� Text
        var effectField = typeof(GameEffectUIManager)
            .GetField("effectText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        effectField.SetValue(manager, effectText);

        yield return null;
    }

    [UnityTest]
    public IEnumerator ShowEffect_SetsTextAndFadesOut()
    {
        manager.Show("SpeedUp");

        // ��ʼ��ʾӦΪ SpeedUp �������ı�
        yield return new WaitForSeconds(0.2f);
        Assert.IsTrue(effectText.enabled);
        Assert.IsTrue(effectText.text.Contains("SpeedUp"));

        // �ȴ�����ʾʱ�� + ����
        yield return new WaitForSeconds(3f);
        Assert.IsFalse(effectText.enabled);
        Assert.IsTrue(string.IsNullOrEmpty(effectText.text));
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Object.Destroy(canvasObj);
        Object.Destroy(manager.gameObject);
        yield return null;
    }
}