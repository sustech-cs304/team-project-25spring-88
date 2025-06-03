using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class CameraSwitchTests
{
    private GameObject cameraSwitchObject;
    private CameraSwitch cameraSwitch;
    private GameObject cam1;
    private GameObject cam2;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        // 创建测试 GameObjects
        cameraSwitchObject = new GameObject("CameraSwitchObj");
        cam1 = new GameObject("Camera1");
        cam2 = new GameObject("Camera2");

        // 添加脚本并绑定摄像机
        cameraSwitch = cameraSwitchObject.AddComponent<CameraSwitch>();
        cameraSwitch.Camera1 = cam1;
        cameraSwitch.Camera2 = cam2;

        yield return null;
    }

    [UnityTest]
    public IEnumerator Camera1_Activates_Correctly()
    {
        // 模拟相机1按钮按下
        cam1.SetActive(false);
        cam2.SetActive(true);

        cameraSwitch.Camera1.SetActive(true);
        cameraSwitch.Camera2.SetActive(false);

        yield return null;

        Assert.IsTrue(cam1.activeSelf);
        Assert.IsFalse(cam2.activeSelf);
    }

    [UnityTest]
    public IEnumerator Camera2_Activates_Correctly()
    {
        // 模拟相机2按钮按下
        cam1.SetActive(true);
        cam2.SetActive(false);

        cameraSwitch.Camera1.SetActive(false);
        cameraSwitch.Camera2.SetActive(true);

        yield return null;

        Assert.IsFalse(cam1.activeSelf);
        Assert.IsTrue(cam2.activeSelf);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(cameraSwitchObject);
        Object.DestroyImmediate(cam1);
        Object.DestroyImmediate(cam2);
    }
}