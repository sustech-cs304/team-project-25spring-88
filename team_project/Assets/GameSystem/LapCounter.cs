using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LapCounter : MonoBehaviour
{
    public int totalLaps = 3;
    private int currentLap = 0;
    private bool raceStarted = false;
    private float raceTimer = 0f;
    private bool hasCelebrated = false;

    // 存档点相关
    private int lastCheckpointIndex = 0;
    private int totalCheckpoints = 3;
    private Vector3 lastCheckpointPosition;
    private Quaternion lastCheckpointRotation;
    private bool[] checkpointsPassed;

    // UI引用
    public TMP_Text lapText;
    public TMP_Text timeText;
    public TMP_Text finishText;
    public TMP_Text speedText; // 新增：速度显示文本
    public Button quitButton;

    // 庆祝效果
    public ParticleSystem fireworkEffect;
    public ParticleSystem ribbonEffect;
    public AudioSource celebrationAudio;

    // 赛车引用
    public GameObject playerCar;

    // 存档点引用（用于控制光束）
    public Checkpoint[] checkpointObjects;

    private Rigidbody carRigidbody; // 用于计算速度

    void Start()
    {
        UpdateLapDisplay();
        UpdateTimeDisplay();
        UpdateSpeedDisplay(); // 初始更新速度显示
        finishText.gameObject.SetActive(false);
        quitButton.gameObject.SetActive(false);
        quitButton.onClick.AddListener(QuitGame);

        checkpointsPassed = new bool[totalCheckpoints + 1];
        lastCheckpointPosition = playerCar.transform.position;
        lastCheckpointRotation = playerCar.transform.rotation;

        // 获取赛车的Rigidbody组件
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

        // 实时更新速度显示
        UpdateSpeedDisplay();
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
                    ShowEndScreen();
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

    private void ShowEndScreen()
    {
        if (finishText != null)
        {
            finishText.gameObject.SetActive(true);
        }
        if (quitButton != null)
        {
            quitButton.gameObject.SetActive(true);
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
        finishText.gameObject.SetActive(false);
        quitButton.gameObject.SetActive(false);

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