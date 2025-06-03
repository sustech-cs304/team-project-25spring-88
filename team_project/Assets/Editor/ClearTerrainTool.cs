using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class ClearAllTerrainsTool
/** 
     * AI-generated-content 
     * tool: grok 
     * version: 3.0
     * usage: I used the prompt "我想要基于unity制作一个赛车小游戏，现在我要实现清楚地形上所有的草，你能帮我写一下控制脚本吗", and 
     * directly copy the code from its response 
     */
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
