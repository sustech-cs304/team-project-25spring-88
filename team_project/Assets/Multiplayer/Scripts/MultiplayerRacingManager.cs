using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// A Unity script that manages multiplayer race logic in a racing game.
/// <para>
/// This script coordinates race start, player spawning, countdown, and result reporting across the network.
/// It tracks player readiness and finish times, displaying final results when all players complete the race.
/// </para>
/// </summary>
public class MultiplayerRacingManager : NetworkBehaviour
{
    /// <summary>
    /// An array of spawn point Transforms for player cars.
    /// </summary>
    public Transform[] spawnPoints;

    /// <summary>
    /// An array of car prefabs to instantiate for players.
    /// </summary>
    public GameObject[] carPrefabs;

    /// <summary>
    /// The prefab for the game result UI to display race results.
    /// </summary>
    public GameResultUI gameResultUIPrefab;

    /// <summary>
    /// The prefab for the countdown UI to display the race start countdown.
    /// </summary>
    public CountdownUI countdownUIPrefab;

    /// <summary>
    /// A list of players who are ready to start the race.
    /// </summary>
    private readonly List<XPlayer> readyPlayers = new();

    /// <summary>
    /// A dictionary mapping players to their finish times.
    /// </summary>
    private readonly Dictionary<XPlayer, float> finishTimes = new();

    /// <summary>
    /// Registers the client disconnection handler when the script is enabled.
    /// </summary>
    private void OnEnable()
    {
        XNetworkManager.OnClientDisconnectedExternally += HandleClientDisconnected;
    }

    /// <summary>
    /// Unregisters the client disconnection handler when the script is disabled.
    /// </summary>
    private void OnDisable()
    {
        XNetworkManager.OnClientDisconnectedExternally -= HandleClientDisconnected;
    }

    /// <summary>
    /// Handles client disconnection by loading the main menu scene on the client.
    /// </summary>
    private void HandleClientDisconnected()
    {
        // 确保只在客户端执行 LoadScene
        if (isClient && !isServer)
        {
            // 这里假设主界面的场景名叫 "MainMenu"
            SceneManager.LoadScene("LandingScene");
        }
    }

    /// <summary>
    /// Marks a player as ready and starts the race when all players are ready.
    /// </summary>
    /// <param name="player">The player who is ready.</param>
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

    /// <summary>
    /// Coroutine that spawns player cars and starts the race countdown.
    /// </summary>
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

    /// <summary>
    /// Instructs clients to instantiate and start the countdown UI.
    /// </summary>
    /// <param name="seconds">The duration of the countdown in seconds.</param>
    [ClientRpc]
    void RpcStartCountdownUI(int seconds)
    {
        Instantiate(countdownUIPrefab).StartCountdown(seconds);
    }

    /// <summary>
    /// Instructs clients to enable car controls for the race start.
    /// </summary>
    [ClientRpc]
    void RpcStartRace()
    {
        foreach (var player in FindObjectsOfType<XPlayer>())
        {
            player.EnableControl();  // 让本地玩家激活控制权限
        }
    }

    /// <summary>
    /// Records a player's finish time and displays results when all players finish.
    /// </summary>
    /// <param name="player">The player who finished.</param>
    /// <param name="time">The player's finish time in seconds.</param>
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

    /// <summary>
    /// Instructs clients to display the final race results.
    /// </summary>
    /// <param name="results">An array of race results.</param>
    [ClientRpc]
    void RpcShowFinalResults(RaceResult[] results)
    {
        Instantiate(gameResultUIPrefab).Show(results);
    }
}