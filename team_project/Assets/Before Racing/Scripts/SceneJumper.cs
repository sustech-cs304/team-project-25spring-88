using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;



/** 
     * AI-generated-content 
     * tool: grok 
     * version: 3.0
     * usage: I used the prompt "我想要基于unity制作一个赛车小游戏，现在我要实现怎么让某个按钮被点击了之后跳转到某个场景？，你能帮我写一下控制脚本吗", and 
     * directly copy the code from its response 
     */
public class MyButtonHandler : MonoBehaviour
{
    [Tooltip("跳转的场景名称（必须在 Build Settings 中注册）")]
    public string targetSceneName = "GameScene";

    void Awake()
    {
        // 自动获取当前 GameObject 上的 Button 组件
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(() => {
                SceneManager.LoadScene(targetSceneName);
            });
        }
        else
        {
            Debug.LogWarning("未在该物体上找到 Button 组件！");
        }
    }
}
