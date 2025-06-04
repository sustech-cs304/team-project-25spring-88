using System.Collections.Generic;
using UnityEngine;
using Mirror;
using VehicleBehaviour;
using System.Collections;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/guides/networkbehaviour
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

/// <summary>
/// A Unity script that manages player state and behavior in a multiplayer racing game.
/// <para>
/// This script handles player number assignment, name synchronization, car selection, and spawning,
/// as well as enabling control for the local player's car. It maintains a static player list for
/// managing player numbers and notifies the race manager when ready.
/// </para>
/// </summary>
public class XPlayer : NetworkBehaviour
{
    /// <summary>
    /// Event invoked when the player's number changes.
    /// </summary>
    public event System.Action<byte> OnPlayerNumberChanged;

    /// <summary>
    /// A static list of all active XPlayer instances for player number management.
    /// </summary>
    static readonly List<XPlayer> playersList = new List<XPlayer>();

    #region SyncVars
    /// <summary>
    /// The player's unique number, synchronized across the network.
    /// </summary>
    [Header("SyncVars")]
    [SyncVar(hook = nameof(PlayerNumberChanged))]
    public byte playerNumber = 0;

    /// <summary>
    /// The player's name, synchronized across the network.
    /// </summary>
    [SyncVar(hook = nameof(PlayerNameChanged))]
    public string playerName = "";

    /// <summary>
    /// The index of the selected car prefab, synchronized across the network.
    /// </summary>
    [SyncVar]
    public int selectedCarIndex = 0;  // 玩家选择的赛车编号

    /// <summary>
    /// The network ID of the player's spawned car, synchronized across the network.
    /// </summary>
    [SyncVar]
    public uint carNetId;

    /// <summary>
    /// Handles the player number change event.
    /// </summary>
    /// <param name="_">The old player number (ignored).</param>
    /// <param name="newPlayerNumber">The new player number.</param>
    void PlayerNumberChanged(byte _, byte newPlayerNumber)
    {
        OnPlayerNumberChanged?.Invoke(newPlayerNumber);
    }

    /// <summary>
    /// Handles the player name change event and saves the name locally.
    /// </summary>
    /// <param name="_">The old player name (ignored).</param>
    /// <param name="newName">The new player name.</param>
    void PlayerNameChanged(string _, string newName)
    {
        Debug.Log($"[{nameof(XPlayer)}] Player name changed to: {newName}");
        if (isLocalPlayer)
        {
            PlayerPrefs.SetString("XPlayerName", newName);
            PlayerPrefs.Save();
        }
    }
    #endregion

    #region Unity Callbacks
    /// <summary>
    /// Validates the component configuration in the Unity Editor.
    /// </summary>
    protected override void OnValidate()
    {
        base.OnValidate();
    }

    /// <summary>
    /// Initializes the player instance.
    /// </summary>
    void Awake()
    {
    }

    /// <summary>
    /// Performs initial setup for the player.
    /// </summary>
    void Start()
    {
    }
    #endregion

    #region Start & Stop Callbacks
    /// <summary>
    /// Initializes the player on the server and adds it to the player list.
    /// </summary>
    public override void OnStartServer()
    {
        base.OnStartServer();

        // Add this to the static Players List
        playersList.Add(this);
    }

    /// <summary>
    /// Cleans up the player on the server and removes it from the player list.
    /// </summary>
    public override void OnStopServer()
    {
        playersList.Remove(this);
    }

    /// <summary>
    /// Initializes the player on the client and notifies readiness for the local player.
    /// </summary>
    public override void OnStartClient()
    {
        if (isLocalPlayer)
        {
            StartCoroutine(NotifyRaceReadyWhenSceneIsLoaded());
        }
    }

    /// <summary>
    /// Coroutine that notifies the race manager when the local player is ready.
    /// </summary>
    IEnumerator NotifyRaceReadyWhenSceneIsLoaded()
    {
        // 等待1帧以确保场景完全初始化
        yield return null;

        // 找到 RacingManager 并发送准备消息
        MultiplayerRacingManager mgr = FindObjectOfType<MultiplayerRacingManager>();
        if (mgr != null)
        {
            CmdNotifyReady();
        }
    }

    /// <summary>
    /// Cleans up the player on the client when destroyed.
    /// </summary>
    public override void OnStopClient() { }

    /// <summary>
    /// Initializes the local player and sets its name.
    /// </summary>
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        string savedName = PlayerPrefs.GetString("XPlayerName", "");
        if (string.IsNullOrEmpty(savedName))
        {
            savedName = $"Player{netId}";
        }
        CmdSetPlayerName(savedName);
    }

    /// <summary>
    /// Cleans up the local player when stopped.
    /// </summary>
    public override void OnStopLocalPlayer() { }

    /// <summary>
    /// Initializes the player when authority is assigned and selects the car.
    /// </summary>
    public override void OnStartAuthority()
    {
        CmdSelectCar(PlayerPrefs.GetInt("SelectedCarIndex", 0)); // 或 UI 中传入
    }

    /// <summary>
    /// Cleans up the player when authority is removed.
    /// </summary>
    public override void OnStopAuthority() { }

    /// <summary>
    /// Sets the player's name on the server.
    /// </summary>
    /// <param name="name">The player's name to set.</param>
    [Command]
    public void CmdSetPlayerName(string name)
    {
        playerName = name;
    }

    /// <summary>
    /// Resets player numbers for all players on the server.
    /// </summary>
    [ServerCallback]
    internal static void ResetPlayerNumbers()
    {
        byte playerNumber = 0;
        foreach (XPlayer player in playersList)
            player.playerNumber = playerNumber++;
    }

    /// <summary>
    /// Selects a car prefab index on the server.
    /// </summary>
    /// <param name="carIndex">The index of the car prefab to select.</param>
    [Command]
    void CmdSelectCar(int carIndex)
    {
        selectedCarIndex = carIndex;
    }

    /// <summary>
    /// Spawns the player's car on the server.
    /// </summary>
    /// <param name="carPrefabs">An array of car prefabs to choose from.</param>
    /// <param name="spawnPoint">The spawn point Transform for the car.</param>
    [Server]
    public void SpawnCar(GameObject[] carPrefabs, Transform spawnPoint)
    {
        GameObject car = Instantiate(carPrefabs[selectedCarIndex], spawnPoint.position, spawnPoint.rotation);
        NetworkServer.Spawn(car, connectionToClient);

        carNetId = car.GetComponent<NetworkIdentity>().netId;

        var nameDisplay = car.GetComponentInChildren<CarNameDisplay>();
        if (nameDisplay != null)
        {
            nameDisplay.SetPlayerName(playerName);
        }
    }

    /// <summary>
    /// Notifies the race manager that the player is ready on the server.
    /// </summary>
    [Command]
    void CmdNotifyReady()
    {
        MultiplayerRacingManager mgr = FindObjectOfType<MultiplayerRacingManager>();
        if (mgr != null)
        {
            mgr.PlayerReady(this);
        }
    }

    /// <summary>
    /// Enables control for the local player's car.
    /// </summary>
    public void EnableControl()
    {
        if (!isLocalPlayer) return;

        if (NetworkClient.spawned.TryGetValue(carNetId, out NetworkIdentity identity))
        {
            var vehicle = identity.GetComponent<WheelVehicle>();
            if (vehicle != null)
            {
                vehicle.IsPlayer = true;
            }

            var lapTracker = identity.GetComponent<LapTracker>();
            if (lapTracker != null)
            {
                lapTracker.EnableRacing();
            }
        }
        else
        {
            Debug.LogWarning($"[{nameof(XPlayer)}] Car not found in NetworkClient.spawned. Delaying control assignment...");
        }
    }
    #endregion
}