using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class SpinTests
{
    private GameObject obj;
    private Spin spin;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        obj = new GameObject("SpinObject");
        spin = obj.AddComponent<Spin>();

        // 添加 renderer 和材质（防止报错）
        var renderer = obj.AddComponent<MeshRenderer>();
        renderer.material = new Material(Shader.Find("Standard"));
        spin.cubeRenderer = renderer;

        obj.SetActive(true); // ✅ 激活 GameObject
        yield return null;
    }

    [UnityTest]
    public IEnumerator Spin_ComponentExists_AndRendererIsEnabled()
    {
        yield return null;

        Assert.IsNotNull(spin);
        Assert.IsNotNull(spin.cubeRenderer);
        Assert.IsTrue(spin.cubeRenderer.enabled, "Renderer is not enabled.");
    }

    [UnityTest]
    public IEnumerator Spin_ObjectStillActiveAfterWait()
    {
        yield return new WaitForSeconds(0.5f);

        Assert.IsTrue(obj.activeInHierarchy, "Object is not active in hierarchy.");
        Assert.Pass("Spin script ran without critical errors.");
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(obj);
    }
}