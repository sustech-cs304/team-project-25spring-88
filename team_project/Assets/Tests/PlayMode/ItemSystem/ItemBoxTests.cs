using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ItemBoxTests
{
    private GameObject itemBoxObj;
    private ItemBox itemBox;
    private GameObject playerCar;
    private Rigidbody rb;
    private DummyWheelVehicle dummyCar;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        itemBoxObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        itemBoxObj.transform.position = Vector3.zero;
        itemBox = itemBoxObj.AddComponent<ItemBox>();

        playerCar = new GameObject("PlayerCar");
        playerCar.tag = "Player";
        playerCar.AddComponent<BoxCollider>(); // ✅ 添加 Collider
        rb = playerCar.AddComponent<Rigidbody>();
        dummyCar = playerCar.AddComponent<DummyWheelVehicle>();

        yield return null;
    }

    [UnityTest]
    public IEnumerator OnTriggerEnter_AppliesEffectAndHides()
    {
        // 记录初始显示状态
        Assert.IsTrue(itemBoxObj.GetComponent<MeshRenderer>().enabled);
        Assert.IsTrue(itemBoxObj.GetComponent<Collider>().enabled);

        // 模拟触发
        itemBoxObj.GetComponent<Collider>().isTrigger = true;
        playerCar.transform.position = itemBoxObj.transform.position;

        // 触发碰撞
        itemBoxObj.GetComponent<Collider>().enabled = true;
        itemBoxObj.GetComponent<Collider>().isTrigger = true;
        itemBoxObj.SendMessage("OnTriggerEnter", playerCar.GetComponent<Collider>());

        yield return new WaitForSeconds(0.1f);

        // 确认 Mesh 被隐藏
        Assert.IsFalse(itemBoxObj.GetComponent<MeshRenderer>().enabled);
        Assert.IsFalse(itemBoxObj.GetComponent<Collider>().enabled);

        // 等待 respawn
        yield return new WaitForSeconds(itemBox.respawnTime + 0.1f);

        // 确认恢复显示
        Assert.IsTrue(itemBoxObj.GetComponent<MeshRenderer>().enabled);
        Assert.IsTrue(itemBoxObj.GetComponent<Collider>().enabled);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(itemBoxObj);
        Object.DestroyImmediate(playerCar);
    }

    // Dummy 替代 WheelVehicle 防止因原方法缺失报错
    public class DummyWheelVehicle : MonoBehaviour
    {
        public float BoostForce = 100f;
        public float DiffGearing = 2f;

        public void SetReverseControl(bool enabled) { }
        public void TriggerSpinOut(float duration, float angle) { }
    }
}