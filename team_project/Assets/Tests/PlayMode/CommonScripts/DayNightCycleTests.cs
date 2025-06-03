using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class DayNightCycleTests
{
    private GameObject obj;
    private DayNightCycle cycle;
    private Light sun;
    private Light moon;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        obj = new GameObject("DayNightCycleObject");
        cycle = obj.AddComponent<DayNightCycle>();

        sun = new GameObject("Sun").AddComponent<Light>();
        moon = new GameObject("Moon").AddComponent<Light>();

        cycle.sun = sun;
        cycle.moon = moon;

        // �ṩ�������ݷ�ֹ null
        cycle.lightIntensity = AnimationCurve.Linear(0, 1, 1, 1);
        cycle.moonIntensity = AnimationCurve.Linear(0, 1, 1, 1);

        // ��������˽���ֶ� timeOfDay = 0.5
        typeof(DayNightCycle)
            .GetField("timeOfDay", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(cycle, 0.5f);

        // �ȴ�һ֡���� Unity ���� Update()
        yield return null;
    }

    [UnityTest]
    public IEnumerator Update_SetsCorrectSunAndMoonRotation()
    {
        // ����Ԥ�ڽǶ�
        float expectedSunAngle = 0.5f * 360f - 90f;   // 90
        float expectedMoonAngle = expectedSunAngle + 180f; // 270

        // һ֮֡��Update() Ӧ��������
        yield return null;

        Assert.AreEqual(expectedSunAngle, sun.transform.rotation.eulerAngles.x, 1f);
        Assert.AreEqual(expectedMoonAngle % 360f, moon.transform.rotation.eulerAngles.x, 1f);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(obj);
        Object.DestroyImmediate(sun.gameObject);
        Object.DestroyImmediate(moon.gameObject);
    }
}
