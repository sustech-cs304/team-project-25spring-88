using Mirror;
using UnityEngine;
using TMPro;
using System.Linq;

public class LapTracker : NetworkBehaviour
{
    [Header("Lap Settings")]
    public int totalLaps = 1;
    public int totalCheckpoints = 6;

    private int currentLap = 0;
    private int lastCheckpointIndex = 0;
    private bool[] checkpointsPassed;

    private float raceTimer = 0f;
    private bool hasFinished = false;
    private bool racingEnabled = false;

    [SerializeField] private TMP_Text lapText;
    [SerializeField] private TMP_Text timerText;

    private Vector3 lastCheckpointPosition;
    private Quaternion lastCheckpointRotation;
    private Rigidbody carRigidbody;

    void Start()
    {
        checkpointsPassed = new bool[totalCheckpoints + 1];

        // 尝试获取刚体组件
        carRigidbody = GetComponent<Rigidbody>();
        if (carRigidbody == null)
        {
            Debug.LogWarning("LapTracker: No Rigidbody found on car!");
        }

        lastCheckpointPosition = transform.position;
        lastCheckpointRotation = transform.rotation;
    }

    void Update()
    {
        if (!isOwned || !racingEnabled) return;

        raceTimer += Time.deltaTime;
        UpdateTimerUI();

        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("R is pressed.");
            RespawnToLastCheckpoint();
        }
    }

    public void EnableRacing()
    {
        if (!isOwned) return;

        Debug.Log($"Racing is enabled.");
        racingEnabled = true;
        currentLap = 1;
        UpdateLapUI();
        raceTimer = 0;
    }

    bool AllCheckpointsPassed()
    {
        for (int i = 1; i <= totalCheckpoints; i++)
            if (!checkpointsPassed[i]) return false;
        return true;
    }

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

    public void OnCheckpointTriggered(int index, Vector3 position, Quaternion rotation)
    {
        if (!racingEnabled || hasFinished) return;

        Debug.Log($"Checkpoint {index} triggered at {position}, last checkpoint was {lastCheckpointIndex}");

        if (index == lastCheckpointIndex + 1)
        {
            Debug.Log("Checkpoint updated.");
            lastCheckpointIndex = index;
            checkpointsPassed[index] = true;

            lastCheckpointPosition = position;
            lastCheckpointRotation = rotation;
        }
    }

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

        Debug.Log($"Respawned to checkpoint {lastCheckpointIndex} at {lastCheckpointPosition}");
    }

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
            Debug.LogError($"[Server] CmdReportFinish: 无法通过 connectionToClient 匹配到 XPlayer，NetId={netId}");
            return;
        }

        // 3. 找到 Race Manager 并上报
        MultiplayerRacingManager mgr = FindObjectOfType<MultiplayerRacingManager>();
        if (mgr == null)
        {
            Debug.LogError("[Server] CmdReportFinish: 找不到 MultiplayerRacingManager");
            return;
        }

        Debug.Log($"[Server] Received finish report from {xp.playerName} ({xp.netId}), time = {time:F2}");
        mgr.ReportPlayerFinished(xp, time);
    }

    private void UpdateLapUI()
    {
        if (lapText) lapText.text = $"Lap: {currentLap}/{totalLaps}";
    }

    private void UpdateTimerUI()
    {
        if (timerText) timerText.text = $"Time: {raceTimer:F2}s";
    }
}
