using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// A Unity Editor tool for clearing all trees and grass details from terrains in the scene.
/// <para>
/// This tool provides a menu option in the Unity Editor to remove all terrain trees and grass details
/// from all terrains found in the current scene. It is useful for resetting terrain vegetation
/// during development of a racing game or similar projects.
/// </para>
/// <remarks>
/// AI-generated-content
/// <list type="bullet">
/// <item>tool: Grok</item>
/// <item>version: 3.0</item>
/// <item>usage: Generated using the prompt "我想要基于Unity制作一个赛车小游戏，现在我要实现清除地形上所有的草，你能帮我写一下控制脚本吗", and directly copied from its response.</item>
/// </list>
/// </remarks>
/// </summary>
public class ClearAllTerrainsTool : MonoBehaviour
{
    /// <summary>
    /// Clears all trees and grass details from all terrains in the scene.
    /// <para>
    /// This method is accessible via the Unity Editor menu under "Tools/Clear All Terrain Trees and Details".
    /// It iterates through all terrains in the scene, clears their grass details and tree instances,
    /// and refreshes the terrain data to apply changes.
    /// </para>
    /// </summary>
    [MenuItem("Tools/Clear All Terrain Trees and Details")]
    public static void ClearAllTerrains()
    {
        Terrain[] terrains = GameObject.FindObjectsOfType<Terrain>();
        int clearedCount = 0;

        foreach (var terrain in terrains)
        {
            TerrainData data = terrain.terrainData;

            // Clear grass details
            for (int i = 0; i < data.detailPrototypes.Length; i++)
            {
                int[,] emptyDetail = new int[data.detailWidth, data.detailHeight];
                data.SetDetailLayer(0, 0, i, emptyDetail);
            }

            // Clear tree instances
            data.treeInstances = new TreeInstance[0];

            // Optional: Clear tree and detail prototypes (commented out as not required)
            // data.treePrototypes = new TreePrototype[0];
            // data.detailPrototypes = new DetailPrototype[0];

            data.RefreshPrototypes();
            clearedCount++;
        }

        Debug.Log($"Cleared {clearedCount} terrains of all trees and grass details.");
    }
}