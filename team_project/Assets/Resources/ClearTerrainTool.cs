using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ClearAllTerrainsTool : MonoBehaviour
{
    [MenuItem("Tools/Clear All Terrain Trees and Details")]
    public static void ClearAllTerrains()
    {
        Terrain[] terrains = GameObject.FindObjectsOfType<Terrain>();
        int clearedCount = 0;

        foreach (var terrain in terrains)
        {
            TerrainData data = terrain.terrainData;

            // 清除所有草
            for (int i = 0; i < data.detailPrototypes.Length; i++)
            {
                int[,] emptyDetail = new int[data.detailWidth, data.detailHeight];
                data.SetDetailLayer(0, 0, i, emptyDetail);
            }

            // 清除所有树
            data.treeInstances = new TreeInstance[0];

            // 可选：清除原型定义（谨慎使用）
            // data.treePrototypes = new TreePrototype[0];
            // data.detailPrototypes = new DetailPrototype[0];

            data.RefreshPrototypes();
            clearedCount++;
        }

        Debug.Log($"已清除 {clearedCount} 个 Terrain 中的所有树和草！");
    }
}
