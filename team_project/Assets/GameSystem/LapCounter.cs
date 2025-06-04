using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// A Unity script that manages core race mechanics in a racing game, including lap counting and timing.
/// <para>
/// This script handles lap counting, checkpoint validation, race timing, UI updates, and celebration effects.
/// It also controls player and AI car states (e.g., enabling/disabling physics) and supports respawning to
/// checkpoints. The script ensures that all checkpoints must be passed to complete a lap.
/// </para>
/// <remarks>
/// AI-generated-content
/// <list type="bullet">
/// <item>tool: Grok</item>
/// <item>version: 3.0</item>
/// <item>usage: Generated using the prompt "我想要基于Unity制作一个赛车小游戏，现在我要实现附着在车上的记圈脚本，同时实现游戏逻辑的控制，如....，你能帮我写一下控制脚本吗", and directly copied from its response.</item>
/// </list>
/// </remarks>
/// </summary>
public class LapCounter : MonoBehaviour
{
    /// <summary>
    /// The total number of laps required to complete the race.
    /// </summary>
    public int totalLaps = 1;

    /// <summary>
    /// The GameObject representing the player car.
    /// </summary>
    public GameObject playerCar;

    /// <summary>
    /// The GameObject representing the AI car.
    /// </summary>
    public GameObject aiCar;

    /// <summary>
    /// An array of checkpoint objects in the race track.
    /// </summary>
    public Checkpoint[] checkpointObjects;

    /// <summary>
    /// The UI text element displaying the current lap number.
    /// </summary>
    public TMP_Text lapText;

    /// <summary>
    /// The UI text element displaying the race elapsed time.
    /// </summary>
    public TMP_Text timeText;
    public GameObject resultPanel; // 结算面板
    public TMP_Text resultTitleText; // 结算标题文本

    public bool existAI; //判断是否为ai模式

    /// <summary>
    /// The UI text element displaying race completion or game over messages.
    /// </summary>
    public TMP_Text finishText;

    /// <summary>
    /// The UI text element displaying the player car's speed.
    /// </summary>
    public TMP_Text speedText;

    /// <summary>
    /// The UI text element displaying the distance between player and AI cars.
    /// </summary>
    public TMP_Text distanceText;

    /// <summary>
    /// The UI button to quit the game.
    /// </summary>
    public Button quitButton;

    /// <summary>
    /// The script managing the countdown UI before the race starts.
    /// </summary>
    public CountdownUI countdownScript;

    /// <summary>
    /// The particle system for the firework celebration effect.
    /// </summary>
    public ParticleSystem fireworkEffect;

    /// <summary>
    /// The particle system for the ribbon celebration effect.
    /// </summary>
    public ParticleSystem ribbonEffect;

    /// <summary>
    /// The audio source for the celebration sound effect.
    /// </summary>
    public AudioSource celebrationAudio;

    /// <summary>
    /// The current lap number (starts at 0, increments to 1 when race starts).
    /// </summary>
    private int currentLap = 0;

    /// <summary>
    /// Whether the race has started.
    /// </summary>
    private bool raceStarted = false;

    /// <summary>
    /// The elapsed time since the race started (in seconds).
    /// </summary>
    private float raceTimer = 0f;

    /// <summary>
    /// Whether celebration effects have been triggered.
    /// </summary>
    private bool hasCelebrated = false;

    /// <summary>
    /// The index of the last checkpoint passed by the player.
    /// </summary>
    private int lastCheckpointIndex = 0;

    /// <summary>
    /// The total number of checkpoints in the race track.
    /// </summary>
    private int totalCheckpoints = 6;

    /// <summary>
    /// The position of the last checkpoint passed for respawn purposes.
    /// </summary>
    private Vector3 lastCheckpointPosition;

    /// <summary>
    /// The rotation of the last checkpoint passed for respawn purposes.
    /// </summary>
    private Quaternion lastCheckpointRotation;

    /// <summary>
    /// An array tracking which checkpoints have been passed.
    /// </summary>
    private bool[] checkpointsPassed;

    /// <summary>
    /// The Rigidbody component of the player car.
    /// </summary>
    private Rigidbody carRigidbody;

    /// <summary>
    /// Initializes the race state, disables car physics, and starts the countdown.
    /// </summary>
    void Start()
    {
        if (playerCar == null)
        {
            Debug.LogError($"[{nameof(LapCounter)}] Player car is not assigned!");
            return;
        }

        carRigidbody = playerCar.GetComponent<Rigidbody>();
        if (carRigidbody == null)
        {
            Debug.LogError($"[{nameof(LapCounter)}] Player car does not have a Rigidbody component!");
            return;
        }

        if (aiCar == null)
        {
            Debug.LogWarning($"[{nameof(LapCounter)}] AI car is not assigned!");
        }

        carRigidbody.isKinematic = true;
        if (aiCar != null)
        {
            var aiCarPathFollower = aiCar.GetComponent<AICarPathFollower>();
            var aiCarRigidbody = aiCar.GetComponent<Rigidbody>();
            if (aiCarPathFollower != null) aiCarPathFollower.enabled = false;
            if (aiCarRigidbody != null) aiCarRigidbody.isKinematic = true;
        }

        checkpointsPassed = new bool[totalCheckpoints + 1];
        lastCheckpointPosition = playerCar.transform.position;
        lastCheckpointRotation = playerCar.transform.rotation;

        UpdateLapDisplay();
        UpdateTimeDisplay();
        UpdateSpeedDisplay();
        UpdateDistanceDisplay();
        if (finishText != null) finishText.gameObject.SetActive(false);
        if (quitButton != null) quitButton.gameObject.SetActive(false);

        StartCoroutine(StartRace());
    }

    /// <summary>
    /// Coroutine that manages the race countdown and activates car physics.
    /// </summary>
    private IEnumerator StartRace()
    {
        if (countdownScript != null)
        {
            countdownScript.StartCountdown(5);
            yield return new WaitForSeconds(5f);
        }
        else
        {
            Debug.LogWarning($"[{nameof(LapCounter)}] Countdown script is not assigned!");
            yield return new WaitForSeconds(5f);
        }

        carRigidbody.isKinematic = false;
        Debug.Log($"[{nameof(LapCounter)}] Player car physics activated.");

        if (aiCar != null)
        {
            yield return new WaitForSeconds(3f);
            var aiCarPathFollower = aiCar.GetComponent<AICarPathFollower>();
            var aiCarRigidbody = aiCar.GetComponent<Rigidbody>();
            if (aiCarPathFollower != null) aiCarPathFollower.enabled = true;
            if (aiCarRigidbody != null) aiCarRigidbody.isKinematic = false;
            Debug.Log($"[{nameof(LapCounter)}] AI car physics and path follower activated.");
        }
    }

    /// <summary>
    /// Updates race timing, UI elements, and handles respawn input each frame.
    /// </summary>
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
        UpdateDistanceDisplay();
    }

    /// <summary>
    /// Updates the UI text displaying the distance between player and AI cars.
    /// </summary>
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

    /// <summary>
    /// Triggers the start of the race when the start line is passed.
    /// </summary>
    public void OnStartLinePassed()
    {
        if (!raceStarted)
        {
            raceStarted = true;
            currentLap = 1;
            UpdateLapDisplay();
            Debug.Log($"[{nameof(LapCounter)}] Race started!");
        }
    }

    /// <summary>
    /// Handles the event when the finish line is passed, validating checkpoints and updating laps.
    /// </summary>
    public void OnFinishLinePassed()
    {
        if (!raceStarted) return;

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
            Debug.Log($"[{nameof(LapCounter)}] Lap {currentLap} completed!");

            for (int i = 1; i <= totalCheckpoints; i++)
            {
                checkpointsPassed[i] = false;
            }
            lastCheckpointIndex = 0;
            ResetCheckpoints();

            if (currentLap > totalLaps)
            {
                raceStarted = false;
                Debug.Log($"[{nameof(LapCounter)}] Race finished! Total time: {raceTimer:F2}s");
                TriggerCelebration();
                ShowEndScreen(true);
            }
        }
        else
        {
            Debug.LogWarning($"[{nameof(LapCounter)}] Cannot count lap: Not all checkpoints passed!");
        }
    }

    /// <summary>
    /// Records a checkpoint passage and updates the last checkpoint state.
    /// </summary>
    /// <param name="checkpointIndex">The index of the checkpoint passed.</param>
    /// <param name="position">The position of the checkpoint for respawn.</param>
    /// <param name="rotation">The rotation of the checkpoint for respawn.</param>
    public void OnCheckpointPassed(int checkpointIndex, Vector3 position, Quaternion rotation)
    {
        if (checkpointIndex == lastCheckpointIndex + 1)
        {
            lastCheckpointIndex = checkpointIndex;
            checkpointsPassed[checkpointIndex] = true;
            lastCheckpointPosition = position;
            lastCheckpointRotation = rotation;
            Debug.Log($"[{nameof(LapCounter)}] Checkpoint {checkpointIndex} recorded.");
        }
    }

    /// <summary>
    /// Respawns the player car to the last recorded checkpoint position and rotation.
    /// </summary>
    private void RespawnToLastCheckpoint()
    {
        if (playerCar != null && carRigidbody != null)
        {
            carRigidbody.velocity = Vector3.zero;
            carRigidbody.angularVelocity = Vector3.zero;
            playerCar.transform.position = lastCheckpointPosition;
            playerCar.transform.rotation = lastCheckpointRotation;
            Debug.Log($"[{nameof(LapCounter)}] Player car respawned to checkpoint at: {lastCheckpointPosition}");
        }
    }

    /// <summary>
    /// Ends the race with a game over state when the player is caught by the AI car.
    /// </summary>
    public void GameOver()
    {
        if (!raceStarted) return;

        raceStarted = false;
        hasCelebrated = false;
        currentLap = 0;
        TriggerCelebration();
        ShowEndScreen(false);
        ResetAICar();
        Debug.Log($"[{nameof(LapCounter)}] Game over! Escaped for {raceTimer:F2} seconds.");
    }

    /// <summary>
    /// Resets the AI car's physics and path follower to a disabled state.
    /// </summary>
    private void ResetAICar()
    {
        if (aiCar != null)
        {
            var aiCarPathFollower = aiCar.GetComponent<AICarPathFollower>();
            var aiCarRigidbody = aiCar.GetComponent<Rigidbody>();
            if (aiCarPathFollower != null) aiCarPathFollower.enabled = false;
            if (aiCarRigidbody != null)
            {
                aiCarRigidbody.isKinematic = true;
                aiCarRigidbody.velocity = Vector3.zero;
                aiCarRigidbody.angularVelocity = Vector3.zero;
            }
            Debug.Log($"[{nameof(LapCounter)}] AI car reset.");
        }
    }

    /// <summary>
    /// Triggers visual and audio celebration effects for race completion or game over.
    /// </summary>
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

    /// <summary>
    /// Displays the end screen with a win or lose message and activates the quit button.
    /// </summary>
    /// <param name="finish">True for race completion, false for game over.</param>
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
            if (existAI){
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
            else if(!existAI){
                if (finishText != null && resultTitleText != null)
                    {
                        // 设置标题
                        resultTitleText.text = "GAME FINISH";
                        resultTitleText.color = new Color(0.2f, 0.8f, 0.2f); // 绿色表示成功
                        
                        // 设置详细信息
                        finishText.text = $"You completed a lap.\n<b>Time: {raceTimer:F2}s</b>";
                        
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

    /// <summary>
    /// Quits the game or stops play mode in the Unity Editor.
    /// </summary>
    private void QuitGame()
    {
        Debug.Log($"[{nameof(LapCounter)}] Quitting game...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// Updates the UI text displaying the current lap number.
    /// </summary>
    private void UpdateLapDisplay()
    {
        if (lapText != null)
        {
            lapText.text = $"Lap: {currentLap}/{totalLaps}";
        }
    }

    /// <summary>
    /// Updates the UI text displaying the race elapsed time.
    /// </summary>
    private void UpdateTimeDisplay()
    {
        if (timeText != null)
        {
            timeText.text = $"Time: {raceTimer:F2}s";
        }
    }

    /// <summary>
    /// Updates the UI text displaying the player car's speed in km/h.
    /// </summary>
    private void UpdateSpeedDisplay()
    {
        if (speedText != null && carRigidbody != null)
        {
            float speedKmh = carRigidbody.velocity.magnitude * 3.6f;
            speedText.text = $"Speed: {speedKmh:F1} km/h";
        }
        else if (speedText != null)
        {
            speedText.text = "Speed: 0 km/h";
        }
    }

    /// <summary>
    /// Resets the race state and UI to initial conditions for a new race.
    /// </summary>
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
        ResetAICar();
    }

    /// <summary>
    /// Resets all checkpoint objects to their initial state, enabling visual effects.
    /// </summary>
    private void ResetCheckpoints()
    {
        if (checkpointObjects == null || checkpointObjects.Length == 0)
        {
            Debug.LogWarning($"[{nameof(LapCounter)}] Checkpoint objects not assigned!");
            return;
        }

        foreach (var checkpoint in checkpointObjects)
        {
            if (checkpoint == null) continue;
            checkpoint.ResetCheckpoint();
        }
    }
}