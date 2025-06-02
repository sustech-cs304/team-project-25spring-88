using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiplayerRacingManager : NetworkBehaviour
{
    public Transform[] spawnPoints;
    public GameObject[] carPrefabs;
    public GameResultUI gameResultUIPrefab;
    public CountdownUI countdownUIPrefab;

    private readonly List<XPlayer> readyPlayers = new();
    private readonly Dictionary<XPlayer, float> finishTimes = new();


    // 在客户端被服务器断开时会被 XNetworkManager 调用
    private void OnEnable()
    {
        XNetworkManager.OnClientDisconnectedExternally += HandleClientDisconnected;
    }

    private void OnDisable()
    {
        XNetworkManager.OnClientDisconnectedExternally -= HandleClientDisconnected;
    }


    // <summary>
    // 一旦断开，就立刻跳到主界面
    // </summary>
    private void HandleClientDisconnected()
    {
        // 确保只在客户端执行 LoadScene
        if (isClient && !isServer)
        {
            // 这里假设主界面的场景名叫 "MainMenu"
            SceneManager.LoadScene("LandingScene");
        }
    }

    [Server]
    public void PlayerReady(XPlayer player)
    {
        if (!readyPlayers.Contains(player))
        {
            readyPlayers.Add(player);
            Debug.Log($"{player.playerName} is ready ({readyPlayers.Count}/{NetworkServer.connections.Count})");
        }

        if (readyPlayers.Count >= NetworkServer.connections.Count)
            StartCoroutine(BeginRaceCountdown());
    }

    [Server]
    private IEnumerator BeginRaceCountdown()
    {
        var players = FindObjectsOfType<XPlayer>();
        for (int i = 0; i < players.Length; i++)
        {
            Transform spawn = spawnPoints[i % spawnPoints.Length];
            players[i].SpawnCar(carPrefabs, spawn);
        }

        RpcStartCountdownUI(10);
        yield return new WaitForSeconds(10f);

        RpcStartRace(); // 通知所有客户端开始比赛
    }

    [ClientRpc]
    void RpcStartCountdownUI(int seconds)
    {
        Instantiate(countdownUIPrefab).StartCountdown(seconds);
    }

    [ClientRpc]
    void RpcStartRace()
    {
        foreach (var player in FindObjectsOfType<XPlayer>())
        {
            player.EnableControl();  // 让本地玩家激活控制权限
        }
    }

    [Server]
    public void ReportPlayerFinished(XPlayer player, float time)
    {
        if (!finishTimes.ContainsKey(player))
        {
            finishTimes[player] = time;
            Debug.Log($"{player.playerName} finished in {time:F2}s");

            if (finishTimes.Count == NetworkServer.connections.Count)
            {
                var ordered = finishTimes.OrderBy(p => p.Value)
                                         .Select(p => new RaceResult(p.Key.playerName, p.Value)).ToArray();
                RpcShowFinalResults(ordered);
            }
        }
    }

    [ClientRpc]
    void RpcShowFinalResults(RaceResult[] results)
    {
        Instantiate(gameResultUIPrefab).Show(results);
    }
}
