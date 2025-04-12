using System.Collections;
using System.Collections.Generic;using UnityEngine;
using TMPro;

public class LapTimer : MonoBehaviour
{
    public TextMeshProUGUI timerText; // UI文本显示时间
    public TextMeshProUGUI lapText; // UI文本显示圈数
    public int totalLaps = 3; // 总圈数
    private float elapsedTime;
    private int currentLap = 1;
    private bool isLapCompleted = false;

    void Start()
    {
        if (timerText == null || lapText == null)
        {
            Debug.LogError("请在Inspector中赋值TimerText和LapText！");
        }
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;
        UpdateUI();
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger Enter: " + other.gameObject.name); // 调试信息
        if (other.gameObject.CompareTag("Player") && !isLapCompleted) // 修改为检测玩家
        {
            currentLap++;
            isLapCompleted = true;
            Debug.Log("Lap increased to: " + currentLap); // 调试圈数
            if (currentLap > totalLaps)
            {
                Debug.Log("比赛结束！用时: " + elapsedTime.ToString("F2") + "秒");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log("Trigger Exit: " + other.gameObject.name); // 调试信息
        if (other.gameObject.CompareTag("Player"))
        {
            isLapCompleted = false;
            Debug.Log("Lap completion reset");
        }
    }

   void UpdateUI()
{
    if (timerText != null)
        timerText.text = "Time: " + elapsedTime.ToString("F2") + "s";
    if (lapText != null)
    {
        lapText.text = "Lap: " + currentLap + "/" + totalLaps;
        Debug.Log("UI Updated: Lap: " + currentLap + "/" + totalLaps); // 调试 UI 更新
    }
}
}