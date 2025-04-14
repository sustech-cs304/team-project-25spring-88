using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/** 
     * AI-generated-content 
     * tool: grok 
     * version: 3.0
     * usage: I used the prompt "我想要基于unity制作一个赛车小游戏，现在我要实现小车经过startline之后开始游戏，你能帮我写一下控制脚本吗", and 
     * directly copy the code from its response 
     */
public class StartLineTrigger : MonoBehaviour
{
    public LapCounter lapCounter; // 在Inspector中拖入LapCounter脚本的物体

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            lapCounter.OnStartLinePassed();
            Debug.Log("Start/Finish Triggered");
        }
    }
}