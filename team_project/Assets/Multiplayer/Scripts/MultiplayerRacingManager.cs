using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MultiplayerRacingManager : NetworkBehaviour
{
    public Transform[] spawnPoints;
    public GameObject[] carPrefabs;

    private readonly List<XPlayer> readyPlayers = new List<XPlayer>();

    [Server]
    public void PlayerReady(XPlayer player)
    {
        if (!readyPlayers.Contains(player))
        {
            readyPlayers.Add(player);
            Debug.Log($"Player ready: {player.playerName} ({readyPlayers.Count}/{NetworkServer.connections.Count})");
        }

        if (readyPlayers.Count >= NetworkServer.connections.Count)
        {
            Debug.Log("All players ready. Starting race...");
            StartRace();
        }
    }

    [Server]
    public void StartRace()
    {
        Debug.Log("StartRace is called.");
        var players = FindObjectsOfType<XPlayer>();

        Debug.Log($"Found {players.Length} players.");
        for (int i = 0; i < players.Length; i++)
        {
            Debug.Log("Spawning car for " + players[i].playerName);
            Transform spawn = spawnPoints[i % spawnPoints.Length];
            players[i].SpawnCar(carPrefabs, spawn);
        }
    }
}
