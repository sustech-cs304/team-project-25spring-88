using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// A Unity script that handles click events on a car to load a target scene and pass the car's ID.
/// <para>
/// This script detects mouse clicks on a car GameObject, stores the car's unique ID using PlayerPrefs,
/// and loads a specified scene (e.g., a car detail scene) in a racing game. It also includes raycast-based
/// click detection for debugging purposes.
/// </para>
/// <remarks>
/// AI-generated-content
/// <list type="bullet">
/// <item>tool: Grok</item>
/// <item>version: 3.0</item>
/// <item>usage: Generated using the prompt "我想要基于Unity制作一个赛车小游戏，现在我要实现如何设置点击某一辆小车就跳转到某个scene，并且同时还把小车的id给传过去？，你能帮我写一下控制脚本吗", and directly copied from its response.</item>
/// </list>
/// </remarks>
/// </summary>
public class CarClickHandler : MonoBehaviour
{
    /// <summary>
    /// The unique ID of the car, set in the Unity Inspector.
    /// </summary>
    public string carID; // 设置为每辆车的唯一ID

    /// <summary>
    /// The name of the target scene to load when the car is clicked.
    /// </summary>
    public string targetScene = "CarDetailScene"; // 你想跳转的目标场景

    /// <summary>
    /// Handles mouse click events when the car is clicked, storing the car ID and loading the target scene.
    /// </summary>
    private void OnMouseDown()
    {
        // 方式一：使用 PlayerPrefs 传递 ID
        PlayerPrefs.SetString("SelectedCarID", carID);
        PlayerPrefs.Save();

        // 加载新场景
        SceneManager.LoadScene(targetScene);
    }

    /// <summary>
    /// Updates to detect mouse clicks via raycasting for debugging purposes.
    /// </summary>
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Debug.Log("点击到了：" + hit.collider.name);
            }
        }
    }
}