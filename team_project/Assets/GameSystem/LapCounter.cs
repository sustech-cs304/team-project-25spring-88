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
    private int lastCheckpointIndex = 1;
    private int totalCheckpoints = 6;
    private Vector3 lastCheckpointPosition;
    private Quaternion lastCheckpointRotation;
    private bool[] checkpointsPassed;

    // UI引用
    public TMP_Text lapText;
    public TMP_Text timeText;
    public TMP_Text finishText;
    public Button quitButton;

    // 庆祝效果
    public ParticleSystem Effect1; // 第一个粒子效果（烟花）
    public ParticleSystem Effect2;   // 第二个粒子效果（彩带）
    public AudioSource celebrationAudio;

    // 赛车引用
    public GameObject playerCar;

    void Start()
    {
        UpdateLapDisplay();
        UpdateTimeDisplay();
        finishText.gameObject.SetActive(false);
        quitButton.gameObject.SetActive(false);
        quitButton.onClick.AddListener(QuitGame);

        checkpointsPassed = new bool[totalCheckpoints + 1];
        lastCheckpointPosition = playerCar.transform.position;
        lastCheckpointRotation = playerCar.transform.rotation;
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

        // 触发第一个粒子效果（烟花）
        if (Effect1 != null)
        {
            Effect1.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            Effect1.Play();
        }

        // 触发第二个粒子效果（彩带）
        if (Effect2 != null)
        {
            Effect2.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            Effect2.Play();
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
        finishText.gameObject.SetActive(false);
        quitButton.gameObject.SetActive(false);
    }
}