using UnityEngine;
using UnityEngine.SceneManagement;
/** 
     * AI-generated-content 
     * tool: grok 
     * version: 3.0
     * usage: I used the prompt "我想要基于unity制作一个赛车小游戏，现在我要实现如何设置点击某一辆小车就跳转到某个scene，并且同时还把小车的id给传过去？，你能帮我写一下控制脚本吗", and 
     * directly copy the code from its response 
     */
public class CarClickHandler : MonoBehaviour
{
    public string carID; // 设置为每辆车的唯一ID
    public string targetScene = "CarDetailScene"; // 你想跳转的目标场景

    private void OnMouseDown()
    {
        // 方式一：使用 PlayerPrefs 传递 ID
        PlayerPrefs.SetString("SelectedCarID", carID);
        PlayerPrefs.Save();

        // 加载新场景
        SceneManager.LoadScene(targetScene);
    }

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
