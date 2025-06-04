using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using System.Collections;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/components/network-manager
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkManager.html
*/

/// <summary>
/// A Unity script that extends Mirror's NetworkManager for a multiplayer racing game.
/// <para>
/// This script manages network connections, scene changes, and player spawning, providing custom events
/// for client connection and disconnection handling. It assigns host names and resets player numbers.
/// </para>
/// </summary>
public class XNetworkManager : NetworkManager
{
    /// <summary>
    /// Event invoked when a client disconnects from the server.
    /// </summary>
    public static event System.Action OnClientDisconnectedExternally;

    /// <summary>
    /// Event invoked when a client connects to the server.
    /// </summary>
    public static event System.Action OnClientConnectedExternally;

    /// <summary>
    /// Gets the singleton instance of XNetworkManager.
    /// </summary>
    public static new XNetworkManager singleton => (XNetworkManager)NetworkManager.singleton;

    /// <summary>
    /// Initializes the network manager before networking is active.
    /// </summary>
    public override void Awake()
    {
        base.Awake();
    }

    #region Unity Callbacks
    /// <summary>
    /// Validates the component configuration in the Unity Editor.
    /// </summary>
    public override void OnValidate()
    {
        base.OnValidate();
    }

    /// <summary>
    /// Initializes the network manager when the GameObject starts.
    /// </summary>
    public override void Start()
    {
        base.Start();
    }

    /// <summary>
    /// Updates the network manager after all Update calls.
    /// </summary>
    public override void LateUpdate()
    {
        base.LateUpdate();
    }

    /// <summary>
    /// Cleans up resources when the GameObject is destroyed.
    /// </summary>
    public override void OnDestroy()
    {
        base.OnDestroy();
    }
    #endregion

    #region Start & Stop
    /// <summary>
    /// Configures the frame rate for a headless server.
    /// </summary>
    public override void ConfigureHeadlessFrameRate()
    {
        base.ConfigureHeadlessFrameRate();
    }
    #endregion

    #region Scene Management
    /// <summary>
    /// Switches the server to a new scene and updates clients.
    /// </summary>
    /// <param name="newSceneName">The name of the new scene.</param>
    public override void ServerChangeScene(string newSceneName)
    {
        base.ServerChangeScene(newSceneName);
    }

    /// <summary>
    /// Prepares the server before a scene change.
    /// </summary>
    /// <param name="newSceneName">The name of the scene to be loaded.</param>
    public override void OnServerChangeScene(string newSceneName) { }

    /// <summary>
    /// Handles server-side logic after a scene is loaded.
    /// </summary>
    /// <param name="sceneName">The name of the loaded scene.</param>
    public override void OnServerSceneChanged(string sceneName) 
    {
        //
    }

    /// <summary>
    /// Prepares the client before a scene change.
    /// </summary>
    /// <param name="newSceneName">The name of the scene to be loaded.</param>
    /// <param name="sceneOperation">The scene operation type.</param>
    /// <param name="customHandling">Whether custom handling is used.</param>
    public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling) { }

    /// <summary>
    /// Handles client-side logic after a scene is loaded.
    /// </summary>
    public override void OnClientSceneChanged()
    {
        base.OnClientSceneChanged();
    }
    #endregion

    #region Server System Callbacks
    /// <summary>
    /// Handles a new client connection on the server.
    /// </summary>
    /// <param name="conn">The client connection.</param>
    public override void OnServerConnect(NetworkConnectionToClient conn) { }

    /// <summary>
    /// Marks a client as ready on the server.
    /// </summary>
    /// <param name="conn">The client connection.</param>
    public override void OnServerReady(NetworkConnectionToClient conn)
    {
        base.OnServerReady(conn);
    }

    /// <summary>
    /// Adds a new player for a client connection on the server.
    /// <para>
    /// Sets the host's player name to "Host" and resets player numbers.
    /// </para>
    /// </summary>
    /// <param name="conn">The client connection.</param>
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
        XPlayer.ResetPlayerNumbers(); 

         // 设置 Host 名字
        if (conn == NetworkServer.localConnection)
        {
            if (conn.identity != null)
            {
                var player = conn.identity.GetComponent<XPlayer>();
                if (player != null)
                {
                    player.playerName = "Host"; // 直接在服务器设置 SyncVar
                }
            }
        }
    }

    /// <summary>
    /// Handles a client disconnection on the server.
    /// <para>
    /// Resets player numbers after disconnection.
    /// </para>
    /// </summary>
    /// <param name="conn">The client connection.</param>
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);
        XPlayer.ResetPlayerNumbers();
    }

    /// <summary>
    /// Handles transport errors on the server.
    /// </summary>
    /// <param name="conn">The client connection, may be null.</param>
    /// <param name="transportError">The transport error type.</param>
    /// <param name="message">The error message.</param>
    public override void OnServerError(NetworkConnectionToClient conn, TransportError transportError, string message) { }

    /// <summary>
    /// Handles transport exceptions on the server.
    /// </summary>
    /// <param name="conn">The client connection, may be null.</param>
    /// <param name="exception">The exception thrown.</param>
    public override void OnServerTransportException(NetworkConnectionToClient conn, Exception exception) { }
    #endregion

    #region Client System Callbacks
    /// <summary>
    /// Handles client connection to the server.
    /// <para>
    /// Invokes the OnClientConnectedExternally event.
    /// </para>
    /// </summary>
    public override void OnClientConnect()
    {
        base.OnClientConnect();

        OnClientConnectedExternally?.Invoke();
    }

    /// <summary>
    /// Handles client disconnection from the server.
    /// <para>
    /// Invokes the OnClientDisconnectedExternally event.
    /// </para>
    /// </summary>
    public override void OnClientDisconnect() 
    {
        base.OnClientDisconnect();

        // 通知 UI
        OnClientDisconnectedExternally?.Invoke();
    }

    /// <summary>
    /// Handles when the client is no longer ready.
    /// </summary>
    public override void OnClientNotReady() { }

    /// <summary>
    /// Handles transport errors on the client.
    /// </summary>
    /// <param name="transportError">The transport error type.</param>
    /// <param name="message">The error message.</param>
    public override void OnClientError(TransportError transportError, string message) { }

    /// <summary>
    /// Handles transport exceptions on the client.
    /// </summary>
    /// <param name="exception">The exception thrown.</param>
    public override void OnClientTransportException(Exception exception) { }
    #endregion

    #region Start & Stop Callbacks
    /// <summary>
    /// Handles the start of a host session.
    /// </summary>
    public override void OnStartHost() { }

    /// <summary>
    /// Handles the start of a server session.
    /// </summary>
    public override void OnStartServer() { }

    /// <summary>
    /// Handles the start of a client session.
    /// </summary>
    public override void OnStartClient() { }

    /// <summary>
    /// Handles the stop of a host session.
    /// </summary>
    public override void OnStopHost() { }

    /// <summary>
    /// Handles the stop of a server session.
    /// </summary>
    public override void OnStopServer() { }

    /// <summary>
    /// Handles the stop of a client session.
    /// </summary>
    public override void OnStopClient() { }
    #endregion
}