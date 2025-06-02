using System.Collections.Generic;
using UnityEngine;
using Mirror;
using VehicleBehaviour;
using System.Collections;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/guides/networkbehaviour
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

public class XPlayer : NetworkBehaviour
{
    public event System.Action<byte> OnPlayerNumberChanged;

    // Players List to manage playerNumber
    static readonly List<XPlayer> playersList = new List<XPlayer>();

    #region SyncVars

    [Header("SyncVars")]

    [SyncVar(hook = nameof(PlayerNumberChanged))]
    public byte playerNumber = 0;

    [SyncVar(hook = nameof(PlayerNameChanged))]
    public string playerName = "";

    [SyncVar] 
    public int selectedCarIndex = 0;  // 玩家选择的赛车编号


    [SyncVar]
    public uint carNetId;

    void PlayerNumberChanged(byte _, byte newPlayerNumber)
    {
        OnPlayerNumberChanged?.Invoke(newPlayerNumber);
    }

    void PlayerNameChanged(string _, string newName)
    {
        // 可扩展事件通知或更新本地 UI
    }

    #endregion


    #region Unity Callbacks

    /// <summary>
    /// Add your validation code here after the base.OnValidate(); call.
    /// </summary>
    protected override void OnValidate()
    {
        base.OnValidate();
    }

    // NOTE: Do not put objects in DontDestroyOnLoad (DDOL) in Awake.  You can do that in Start instead.
    void Awake()
    {
    }

    void Start()
    {
    }

    #endregion

    #region Start & Stop Callbacks

    /// <summary>
    /// This is invoked for NetworkBehaviour objects when they become active on the server.
    /// <para>This could be triggered by NetworkServer.Listen() for objects in the scene, or by NetworkServer.Spawn() for objects that are dynamically created.</para>
    /// <para>This will be called for objects on a "host" as well as for object on a dedicated server.</para>
    /// </summary>
    public override void OnStartServer() {
        base.OnStartServer();

        // Add this to the static Players List
        playersList.Add(this);

     }

    /// <summary>
    /// Invoked on the server when the object is unspawned
    /// <para>Useful for saving object data in persistent storage</para>
    /// </summary>
    public override void OnStopServer() {

        playersList.Remove(this);
     }

    /// <summary>
    /// Called on every NetworkBehaviour when it is activated on a client.
    /// <para>Objects on the host have this function called, as there is a local client on the host. The values of SyncVars on object are guaranteed to be initialized correctly with the latest state from the server when this function is called on the client.</para>
    /// </summary>
    public override void OnStartClient() 
    {
        if (isLocalPlayer)
        {
            StartCoroutine(NotifyRaceReadyWhenSceneIsLoaded());
        }
    }


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
    /// This is invoked on clients when the server has caused this object to be destroyed.
    /// <para>This can be used as a hook to invoke effects or do client specific cleanup.</para>
    /// </summary>
    public override void OnStopClient() { }

    /// <summary>
    /// Called when the local player object has been set up.
    /// <para>This happens after OnStartClient(), as it is triggered by an ownership message from the server. This is an appropriate place to activate components or functionality that should only be active for the local player, such as cameras and input.</para>
    /// </summary>
    public override void OnStartLocalPlayer() { }

    /// <summary>
    /// Called when the local player object is being stopped.
    /// <para>This happens before OnStopClient(), as it may be triggered by an ownership message from the server, or because the player object is being destroyed. This is an appropriate place to deactivate components or functionality that should only be active for the local player, such as cameras and input.</para>
    /// </summary>
    public override void OnStopLocalPlayer() {}

    /// <summary>
    /// This is invoked on behaviours that have authority, based on context and <see cref="NetworkIdentity.hasAuthority">NetworkIdentity.hasAuthority</see>.
    /// <para>This is called after <see cref="OnStartServer">OnStartServer</see> and before <see cref="OnStartClient">OnStartClient.</see></para>
    /// <para>When <see cref="NetworkIdentity.AssignClientAuthority">AssignClientAuthority</see> is called on the server, this will be called on the client that owns the object. When an object is spawned with <see cref="NetworkServer.Spawn">NetworkServer.Spawn</see> with a NetworkConnectionToClient parameter included, this will be called on the client that owns the object.</para>
    /// </summary>
    public override void OnStartAuthority() 
    {
        CmdSelectCar(PlayerPrefs.GetInt("SelectedCarIndex", 0)); // 或 UI 中传入
    }

    /// <summary>
    /// This is invoked on behaviours when authority is removed.
    /// <para>When NetworkIdentity.RemoveClientAuthority is called on the server, this will be called on the client that owns the object.</para>
    /// </summary>
    public override void OnStopAuthority() { }

    [Command]
    public void CmdSetPlayerName(string name)
    {
        playerName = name;
    }


    [ServerCallback]
    internal static void ResetPlayerNumbers()
    {
        byte playerNumber = 0;
        foreach (XPlayer player in playersList)
            player.playerNumber = playerNumber++;
    }

    [Command]
    void CmdSelectCar(int carIndex)
    {
        selectedCarIndex = carIndex;
    }

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

    [Command]
    void CmdNotifyReady()
    {
        MultiplayerRacingManager mgr = FindObjectOfType<MultiplayerRacingManager>();
        if (mgr != null)
        {
            mgr.PlayerReady(this);
        }
    }

    #endregion
}
