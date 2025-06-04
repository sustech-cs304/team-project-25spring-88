using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.TestTools;
/** 
     * AI-generated-content 
     * tool: grok 
     * version: 3.0
     * usage: I used the prompt "我想要基于unity制作一个赛车小游戏，现在我要实现附着在车上的记圈脚本，同时实现游戏逻辑的控制，如....，你能帮我写一下控制脚本吗", and 
     * directly copy the code from its response 
     */
public class LapCounter : MonoBehaviour
{
    public int totalLaps = 1;
    private int currentLap = 0;
    private bool raceStarted = false;
    private float raceTimer = 0f;
    private bool hasCelebrated = false;

    // 存档点相关
    private int lastCheckpointIndex = 0;
    private int totalCheckpoints = 6;
    private Vector3 lastCheckpointPosition;
    private Quaternion lastCheckpointRotation;
    private bool[] checkpointsPassed;

    // UI引用
    public TMP_Text lapText;
    public TMP_Text timeText;
    public TMP_Text finishText;
    public TMP_Text speedText; // 新增：速度显示文本
    public TMP_Text distanceText; // 新增：距离显示文本
    public Button quitButton;
    public CountdownUI countdownscript; // 新增：倒计时脚本引用
    // 庆祝效果
    public ParticleSystem fireworkEffect;
    public ParticleSystem ribbonEffect;
    public AudioSource celebrationAudio;

    // 赛车引用
    public GameObject playerCar;
    public GameObject aiCar; // 新增：AI 警车引用

    // 存档点引用（用于控制光束）
    public Checkpoint[] checkpointObjects;

    private Rigidbody carRigidbody; // 用于计算速度

    // 结算界面元素
    public GameObject resultPanel; // 结算面板
    public TMP_Text resultTitleText; // 结算标题文本

    void Start()
    {
        Rigidbody PlayercarRigidbody = playerCar.GetComponent<Rigidbody>();
        if (PlayercarRigidbody != null)
        {
            PlayercarRigidbody.isKinematic = true; // 禁用物理运动
            // 或者使用 constraints
            // carRigidbody.constraints = RigidbodyConstraints.FreezeAll;
        }if(aiCar != null)
        {  
            AICarPathFollower aiCarPathFollower = aiCar.GetComponent<AICarPathFollower>();
            aiCarPathFollower.enabled = false; // 确保 AI 小车的路径跟随脚本启用
            Rigidbody AicarRigidbody = aiCar.GetComponent<Rigidbody>();
            if (AicarRigidbody != null)
            {
                AicarRigidbody.isKinematic = true; // 禁用物理运动
                // 或者使用 constraints
                // carRigidbody.constraints = RigidbodyConstraints.FreezeAll;
            } 
        }
        StartCoroutine(StartRace(PlayercarRigidbody,aiCar)); // 启动协程处理倒计时和激活逻辑

        UpdateLapDisplay();
        UpdateTimeDisplay();
        UpdateSpeedDisplay();
        UpdateDistanceDisplay(); // 新增：初始更新距离显示
        finishText.gameObject.SetActive(false);
        quitButton.gameObject.SetActive(false);

        checkpointsPassed = new bool[totalCheckpoints + 1];
        lastCheckpointPosition = playerCar.transform.position;
        lastCheckpointRotation = playerCar.transform.rotation;

        if (playerCar != null)
        {
            carRigidbody = playerCar.GetComponent<Rigidbody>();
            if (carRigidbody == null)
            {
                Debug.LogWarning("Player car does not have a Rigidbody component!");
            }
        }
        else
        {
            Debug.LogWarning("Player car is not assigned in LapCounter!");
        }

        // 检查 AI 警车是否分配
        if (aiCar == null)
        {
            Debug.LogWarning("AI car is not assigned in LapCounter!");
        }
    }
    IEnumerator StartRace(Rigidbody playerCar,GameObject aiCar)
    {
        countdownscript.StartCountdown(5); // 开始5秒倒计时
        yield return new WaitForSeconds(5.0f); // 等待倒计时结束
        playerCar.isKinematic = false; // 激活玩家小车的物理运动
        Debug.Log("玩家小车已激活！");
        yield return new WaitForSeconds(3.0f); // 再等待5秒
         if(aiCar != null)
        {
            AICarPathFollower aiCarPathFollower = aiCar.GetComponent<AICarPathFollower>();
            Rigidbody AicarRigidbody = aiCar.GetComponent<Rigidbody>();
            if (aiCarPathFollower != null)
            {
                aiCarPathFollower.enabled = true; // 确保 AI 小车的路径跟随脚本启用
                AicarRigidbody.isKinematic = false; // 激活 AI 小车的物理运动
            }
            else
            {
                Debug.LogWarning("AI car does not have AICarPathFollower component!");
            }
        }
        Debug.Log("AI 小车已激活！");
    }
 void Update()
    {
        if (raceStarted)
        {
            raceTimer += Time.deltaTime;
            UpdateTimeDisplay();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            RespawnToLastCheckpoint();
        }

        UpdateSpeedDisplay();
        UpdateDistanceDisplay(); // 新增：实时更新距离显示
    }

    // 新增：更新距离显示
    private void UpdateDistanceDisplay()
    {
        if (distanceText != null && playerCar != null && aiCar != null)
        {
            float distance = Vector3.Distance(playerCar.transform.position, aiCar.transform.position);
            distanceText.text = $"Distance to AI: {distance:F1} m";
        }
        else if (distanceText != null)
        {
            distanceText.text = "Distance to AI: N/A";
        }
    }
    public void OnStartLinePassed()
    {
        Debug.Log("Start Line Logic Executed");
        if (!raceStarted)
        {
            raceStarted = true;
            currentLap = 1;
            UpdateLapDisplay();
            Debug.Log("Race Started!");
        }
    }

    public void OnFinishLinePassed()
    {
        Debug.Log("Finish Line Logic Executed");
        if (raceStarted)
        {
            bool allCheckpointsPassed = true;
            for (int i = 1; i <= totalCheckpoints; i++)
            {
                if (!checkpointsPassed[i])
                {
                    allCheckpointsPassed = false;
                    break;
                }
            }

            if (allCheckpointsPassed)
            {
                currentLap++;
                UpdateLapDisplay();
                Debug.Log($"Lap {currentLap} Completed!");

                for (int i = 1; i <= totalCheckpoints; i++)
                {
                    checkpointsPassed[i] = false;
                }
                lastCheckpointIndex = 0;

                ResetCheckpoints();

                if (currentLap >= totalLaps)
                {
                    raceStarted = false;
                    Debug.Log($"Race Finished! Total Time: {raceTimer:F2}s");
                    TriggerCelebration();
                    ShowEndScreen(true);
                }
            }
            else
            {
                Debug.Log("Cannot count lap: Not all checkpoints passed!");
            }
        }
    }

    public void OnCheckpointPassed(int checkpointIndex, Vector3 position, Quaternion rotation)
    {
        if (checkpointIndex == lastCheckpointIndex + 1)
        {
            lastCheckpointIndex = checkpointIndex;
            checkpointsPassed[checkpointIndex] = true;
            lastCheckpointPosition = position;
            lastCheckpointRotation = rotation;
            Debug.Log($"Checkpoint {checkpointIndex} recorded as last checkpoint.");
        }
    }

    private void RespawnToLastCheckpoint()
    {
        if (playerCar != null)
        {
            Rigidbody rb = playerCar.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            playerCar.transform.position = lastCheckpointPosition;
            playerCar.transform.rotation = lastCheckpointRotation;
            Debug.Log("Respawned to last checkpoint at: " + lastCheckpointPosition);
        }
    }
    public void gameover()
    {    
        TriggerCelebration();     
        ShowEndScreen(false);
        Debug.Log("Game Over!");
        raceStarted = false;
        hasCelebrated = false;
        currentLap = 0;       
    }
    private void TriggerCelebration()
    {
        if (hasCelebrated) return;

        if (fireworkEffect != null)
        {
            fireworkEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            fireworkEffect.Play();
        }

        if (ribbonEffect != null)
        {
            ribbonEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ribbonEffect.Play();
        }

        if (celebrationAudio != null)
        {
            celebrationAudio.Play();
        }

        hasCelebrated = true;
    }

    private void ShowEndScreen(bool finish)
    {
        if (resultPanel != null)
        {
            // 显示结算面板
            resultPanel.SetActive(true);
            
            // 确保结算面板显示在最上层
            Canvas resultPanelCanvas = resultPanel.GetComponent<Canvas>();
            if (resultPanelCanvas != null)
            {
                resultPanelCanvas.sortingOrder = 10;
            }
        }

        if(finish){
            if (finishText != null && resultTitleText != null)
            {
                // 设置标题
                resultTitleText.text = "ESCAPE SUCCESSFUL";
                resultTitleText.color = new Color(0.2f, 0.8f, 0.2f); // 绿色表示成功
                
                // 设置详细信息
                finishText.text = $"You completed a lap and toyed with the captor in the palm of your hand.\n<b>Time: {raceTimer:F2}s</b>";
                
                // Reduce opacity of other UI elements to 0.5
                Color fadedColor = new Color(1, 1, 1, 0.5f);
                lapText.color = fadedColor;
                timeText.color = fadedColor;
                speedText.color = fadedColor;
                if(distanceText != null)
                {
                    distanceText.color = fadedColor;
                }
                
                // Keep these elements visible but faded
                lapText.gameObject.SetActive(true);
                timeText.gameObject.SetActive(true);
                speedText.gameObject.SetActive(true);
                if(distanceText != null)
                {
                    distanceText.gameObject.SetActive(true);
                }
                finishText.gameObject.SetActive(true);
            }
            if (quitButton != null)
            {
                quitButton.gameObject.SetActive(true);
                // Also ensure quit button appears on top
                Canvas quitButtonCanvas = quitButton.GetComponentInParent<Canvas>();
                if (quitButtonCanvas != null)
                {
                    quitButtonCanvas.sortingOrder = 11; // 确保按钮在面板之上
                }
            }
        }
        else{
            if (finishText != null && resultTitleText != null)
            {
                // 设置标题
                resultTitleText.text = "CAUGHT";
                resultTitleText.color = new Color(0.8f, 0.2f, 0.2f); // 红色表示失败
                
                // 设置详细信息
                finishText.text = $"You have been caught by the police.\n<b>Escape time: {raceTimer:F2}s</b>";
                
                // Reduce opacity of other UI elements to 0.5
                Color fadedColor = new Color(1, 1, 1, 0.5f);
                lapText.color = fadedColor;
                timeText.color = fadedColor;
                speedText.color = fadedColor;
                if(distanceText != null)
                {
                    distanceText.color = fadedColor;
                }
                
                // Keep these elements visible but faded
                lapText.gameObject.SetActive(true);
                timeText.gameObject.SetActive(true);
                speedText.gameObject.SetActive(true);
                if(distanceText != null)
                {
                    distanceText.gameObject.SetActive(true);
                }
                finishText.gameObject.SetActive(true);
            }
            if (quitButton != null)
            {
                quitButton.gameObject.SetActive(true);
                // Also ensure quit button appears on top
                Canvas quitButtonCanvas = quitButton.GetComponentInParent<Canvas>();
                if (quitButtonCanvas != null)
                {
                    quitButtonCanvas.sortingOrder = 11; // 确保按钮在面板之上
                }
            }
        }
    }

    private void QuitGame()
    {
        Debug.Log("Quitting Game...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void UpdateLapDisplay()
    {
        if (lapText != null)
        {
            lapText.text = $"Lap: {currentLap}/{totalLaps}";
        }
    }

    private void UpdateTimeDisplay()
    {
        if (timeText != null)
        {
            timeText.text = $"Time: {raceTimer:F2}s";
        }
    }

    private void UpdateSpeedDisplay()
    {
        if (speedText != null && carRigidbody != null)
        {
            // 计算速度（单位：m/s）
            float speed = carRigidbody.velocity.magnitude;
            // 转换为km/h（1 m/s = 3.6 km/h）
            float speedKmh = speed * 3.6f;
            // 更新UI，保留1位小数
            speedText.text = $"Speed: {speedKmh:F1} km/h";
        }
        else if (speedText != null)
        {
            speedText.text = "Speed: 0 km/h";
        }
    }

    public void ResetRace()
    {
        raceStarted = false;
        currentLap = 0;
        raceTimer = 0f;
        hasCelebrated = false;
        lastCheckpointIndex = 0;
        for (int i = 1; i <= totalCheckpoints; i++)
        {
            checkpointsPassed[i] = false;
        }
        lastCheckpointPosition = playerCar.transform.position;
        lastCheckpointRotation = playerCar.transform.rotation;
        UpdateLapDisplay();
        UpdateTimeDisplay();
        UpdateSpeedDisplay();
        UpdateDistanceDisplay(); // 新增：重置时更新距离显示
        finishText.gameObject.SetActive(false);
        quitButton.gameObject.SetActive(false);
        if (resultPanel != null)
        {
            resultPanel.SetActive(false); // 重置时隐藏结算面板
        }

        ResetCheckpoints();
    }

    private void ResetCheckpoints()
    {
        if (checkpointObjects == null || checkpointObjects.Length == 0)
        {
            Debug.LogWarning("Checkpoint objects not assigned in LapCounter!");
            return;
        }

        foreach (var checkpoint in checkpointObjects)
        {
            if (checkpoint == null) continue;

            if (checkpoint.lightBeam != null)
            {
                checkpoint.lightBeam.enabled = true;
            }
            if (checkpoint.lightBeamParticles != null)
            {
                checkpoint.lightBeamParticles.Play();
            }
        }
    }
}