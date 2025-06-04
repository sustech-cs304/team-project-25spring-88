using Mirror;
using UnityEngine;
using TMPro;
using System.Linq;

/// <summary>
/// A Unity script that tracks lap progress for a player's car in a multiplayer racing game.
/// <para>
/// This script manages lap counting, checkpoint validation, race timing, and UI updates for a racing game.
/// It supports respawning to checkpoints and reports race completion to the server for the local player.
/// </para>
/// </summary>
public class LapTracker : NetworkBehaviour
{
    /// <summary>
    /// The total number of laps required to complete the race.
    /// </summary>
    [Header("Lap Settings")]
    public int totalLaps = 1;

    /// <summary>
    /// The total number of checkpoints in the race track.
    /// </summary>
    public int totalCheckpoints = 6;

    /// <summary>
    /// The current lap number (starts at 0).
    /// </summary>
    private int currentLap = 0;

    /// <summary>
    /// The index of the last checkpoint passed.
    /// </summary>
    private int lastCheckpointIndex = 0;

    /// <summary>
    /// An array tracking which checkpoints have been passed.
    /// </summary>
    private bool[] checkpointsPassed;

    /// <summary>
    /// The elapsed time since the race started (in seconds).
    /// </summary>
    private float raceTimer = 0f;

    /// <summary>
    /// Whether the race has finished for this player.
    /// </summary>
    private bool hasFinished = false;

    /// <summary>
    /// Whether racing is enabled for this player.
    /// </summary>
    private bool racingEnabled = false;

    /// <summary>
    /// The UI text element displaying the current lap number.
    /// </summary>
    [SerializeField] public TMP_Text lapText;

    /// <summary>
    /// The UI text element displaying the race timer.
    /// </summary>
    [SerializeField] public TMP_Text timerText;

    /// <summary>
    /// The position of the last checkpoint passed for respawn purposes.
    /// </summary>
    private Vector3 lastCheckpointPosition;

    /// <summary>
    /// The rotation of the last checkpoint passed for respawn purposes.
    /// </summary>
    private Quaternion lastCheckpointRotation;

    /// <summary>
    /// The Rigidbody component of the car.
    /// </summary>
    private Rigidbody carRigidbody;

    /// <summary>
    /// Initializes the checkpoint tracking array and sets initial state.
    /// </summary>
    void Start()
    {
        checkpointsPassed = new bool[totalCheckpoints + 1];

        // 尝试获取刚体组件
        carRigidbody = GetComponent<Rigidbody>();
        if (carRigidbody == null)
        {
            Debug.LogWarning($"[{nameof(LapTracker)}] No Rigidbody found on car!");
        }

        lastCheckpointPosition = transform.position;
        lastCheckpointRotation = transform.rotation;
    }

    /// <summary>
    /// Updates the race timer and handles respawn input for the local player.
    /// </summary>
    void Update()
    {
        if (!isOwned || !racingEnabled) return;

        raceTimer += Time.deltaTime;
        UpdateTimerUI();

        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log($"[{nameof(LapTracker)}] R is pressed.");
            RespawnToLastCheckpoint();
        }
    }

    /// <summary>
    /// Enables racing mode for the local player and initializes race state.
    /// </summary>
    public void EnableRacing()
    {
        if (!isOwned) return;

        Debug.Log($"[{nameof(LapTracker)}] Racing is enabled.");
        racingEnabled = true;
        currentLap = 1;
        UpdateLapUI();
        raceTimer = 0;
    }

    /// <summary>
    /// Checks if all checkpoints have been passed in the current lap.
    /// </summary>
    /// <returns>True if all checkpoints are passed, false otherwise.</returns>
    bool AllCheckpointsPassed()
    {
        for (int i = 1; i <= totalCheckpoints; i++)
            if (!checkpointsPassed[i]) return false;
        return true;
    }

    /// <summary>
    /// Handles the event when the local player's car crosses the finish line.
    /// </summary>
    public void OnFinishLinePassed()
    {
        if (!racingEnabled || !AllCheckpointsPassed()) return;

        currentLap++;
        UpdateLapUI();

        if (currentLap > totalLaps)
        {
            racingEnabled = false;
            CmdReportFinish(raceTimer);
        }
    }

    /// <summary>
    /// Records a checkpoint passage and updates checkpoint state.
    /// </summary>
    /// <param name="index">The index of the checkpoint passed.</param>
    /// <param name="position">The position of the checkpoint for respawn.</param>
    /// <param name="rotation">The rotation of the checkpoint for respawn.</param>
    public void OnCheckpointTriggered(int index, Vector3 position, Quaternion rotation)
    {
        if (!racingEnabled || hasFinished) return;

        Debug.Log($"[{nameof(LapTracker)}] Checkpoint {index} triggered at {position}, last checkpoint was {lastCheckpointIndex}");

        if (index == lastCheckpointIndex + 1)
        {
            Debug.Log($"[{nameof(LapTracker)}] Checkpoint updated.");
            lastCheckpointIndex = index;
            checkpointsPassed[index] = true;

            lastCheckpointPosition = position;
            lastCheckpointRotation = rotation;
        }
    }

    /// <summary>
    /// Respawns the car to the last passed checkpoint.
    /// </summary>
    void RespawnToLastCheckpoint()
    {
        if (!isOwned || hasFinished) return;

        if (carRigidbody != null)
        {
            carRigidbody.velocity = Vector3.zero;
            carRigidbody.angularVelocity = Vector3.zero;
        }

        transform.position = lastCheckpointPosition;
        transform.rotation = lastCheckpointRotation;

        Debug.Log($"[{nameof(LapTracker)}] Respawned to checkpoint {lastCheckpointIndex} at {lastCheckpointPosition}");
    }

    /// <summary>
    /// Reports the player's finish time to the server.
    /// </summary>
    /// <param name="time">The player's finish time in seconds.</param>
    [Command]
    void CmdReportFinish(float time)
    {
        // 1. 在服务器端获取所有 XPlayer 实例
        XPlayer[] allPlayers = FindObjectsOfType<XPlayer>();

        // 2. 匹配 connectionToClient，找到当前调用 Cmd 的那个玩家
        //    注意：这里的 connectionToClient 是 Mirror 提供的、指向发起命令的客户端的连接
        XPlayer xp = allPlayers.FirstOrDefault(p => p.connectionToClient == connectionToClient);

        if (xp == null)
        {
            Debug.LogError($"[{nameof(LapTracker)}] CmdReportFinish: 无法通过 connectionToClient 匹配到 XPlayer，NetId={netId}");
            return;
        }

        // 3. 找到 Race Manager 并上报
        MultiplayerRacingManager mgr = FindObjectOfType<MultiplayerRacingManager>();
        if (mgr == null)
        {
            Debug.LogError($"[{nameof(LapTracker)}] CmdReportFinish: 找不到 MultiplayerRacingManager");
            return;
        }

        Debug.Log($"[{nameof(LapTracker)}] Received finish report from {xp.playerName} ({xp.netId}), time = {time:F2}");
        mgr.ReportPlayerFinished(xp, time);
    }

    /// <summary>
    /// Updates the lap count display in the UI.
    /// </summary>
    private void UpdateLapUI()
    {
        if (lapText) lapText.text = $"Lap: {currentLap - 1}/{totalLaps}";
    }

    /// <summary>
    /// Updates the race timer display in the UI.
    /// </summary>
    private void UpdateTimerUI()
    {
        if (timerText) timerText.text = $"Time: {raceTimer:F2}s";
    }
}