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

        // 模拟一个初始角度
        car.transform.rotation = Quaternion.Euler(0, 0, 0);

        yield return null;
    }

    [UnityTest]
    public IEnumerator CarRotatesTowardsTargetAngle()
    {
        // 记录初始角度
        float startAngle = car.transform.eulerAngles.y;

        // 等待若干帧观察旋转
        yield return new WaitForSeconds(0.5f); // 等 0.5 秒，旋转应该已开始

        float currentAngle = car.transform.eulerAngles.y;

        // Assert that rotation has changed
        Assert.AreNotEqual(startAngle, currentAngle, "车辆应在 FixedUpdate 中发生旋转");
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(car);
    }
}