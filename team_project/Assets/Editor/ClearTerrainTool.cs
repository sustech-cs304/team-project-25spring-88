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

            // ������в�
            for (int i = 0; i < data.detailPrototypes.Length; i++)
            {
                int[,] emptyDetail = new int[data.detailWidth, data.detailHeight];
                data.SetDetailLayer(0, 0, i, emptyDetail);
            }

            // ���������
            data.treeInstances = new TreeInstance[0];

            // ��ѡ�����ԭ�Ͷ��壨����ʹ�ã�
            // data.treePrototypes = new TreePrototype[0];
            // data.detailPrototypes = new DetailPrototype[0];

            data.RefreshPrototypes();
            clearedCount++;
        }

        Debug.Log($"����� {clearedCount} �� Terrain �е��������Ͳݣ�");
    }
}
