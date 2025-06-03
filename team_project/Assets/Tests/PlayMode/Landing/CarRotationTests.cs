using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class CarRotationTests
{
    private GameObject car;
    private CarRotation rotationScript;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        car = new GameObject("TestCar");
        rotationScript = car.AddComponent<CarRotation>();

        // ģ��һ����ʼ�Ƕ�
        car.transform.rotation = Quaternion.Euler(0, 0, 0);

        yield return null;
    }

    [UnityTest]
    public IEnumerator CarRotatesTowardsTargetAngle()
    {
        // ��¼��ʼ�Ƕ�
        float startAngle = car.transform.eulerAngles.y;

        // �ȴ�����֡�۲���ת
        yield return new WaitForSeconds(0.5f); // �� 0.5 �룬��תӦ���ѿ�ʼ

        float currentAngle = car.transform.eulerAngles.y;

        // Assert that rotation has changed
        Assert.AreNotEqual(startAngle, currentAngle, "����Ӧ�� FixedUpdate �з�����ת");
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(car);
    }
}