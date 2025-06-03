#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using NUnit.Framework;

public class ClearAllTerrainsToolTests
{
    private GameObject terrainGO;
    private Terrain terrain;
    private TerrainData terrainData;

    [SetUp]
    public void SetUp()
    {
        // 创建地形对象
        terrainGO = GameObject.CreatePrimitive(PrimitiveType.Plane);
        terrainGO.name = "TestTerrain";
        terrain = terrainGO.AddComponent<Terrain>();
        var terrainCollider = terrainGO.AddComponent<TerrainCollider>();

        terrainData = new TerrainData();
        terrainData.heightmapResolution = 33;
        terrainData.size = new Vector3(10, 1, 10);

        // 创建假的 Detail 和 Tree
        var detailPrototype = new DetailPrototype { prototypeTexture = Texture2D.blackTexture };
        terrainData.detailPrototypes = new DetailPrototype[] { detailPrototype };
        terrainData.SetDetailResolution(32, 8);
        terrainData.SetDetailLayer(0, 0, 0, new int[32, 32]);

        var treePrototype = new TreePrototype { prefab = new GameObject("DummyTree") };
        terrainData.treePrototypes = new TreePrototype[] { treePrototype };
        terrainData.treeInstances = new TreeInstance[]
        {
            new TreeInstance { position = new Vector3(0.5f, 0f, 0.5f), prototypeIndex = 0, widthScale = 1f, heightScale = 1f, color = Color.green, lightmapColor = Color.green }
        };

        terrain.terrainData = terrainData;
        terrainCollider.terrainData = terrainData;
    }

    [Test]
    public void ClearAllTerrains_ClearsTreesAndDetails()
    {
        // 确保 terrain 初始化是非空的
        Assert.Greater(terrain.terrainData.treeInstances.Length, 0);
        Assert.Greater(terrain.terrainData.GetDetailLayer(0, 0, 1, 1, 0)[0, 0], -1);

        // 调用静态清除方法
        ClearAllTerrainsTool.ClearAllTerrains();

        // 断言地形中不再有树和草
        Assert.AreEqual(0, terrain.terrainData.treeInstances.Length);
        Assert.AreEqual(0, terrain.terrainData.GetDetailLayer(0, 0, 1, 1, 0)[0, 0]);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(terrainGO);
    }
}
#endif