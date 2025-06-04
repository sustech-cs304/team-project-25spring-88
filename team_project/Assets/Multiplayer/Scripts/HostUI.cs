using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

/// <summary>
/// A Unity script that manages the host UI for a multiplayer racing game.
/// <para>
/// This script sets up a network host, generates a password, selects a port, displays network details,
/// manages the player list, and allows the host to start the game or kick players.
/// </para>
/// </summary>
public class HostUI : MonoBehaviour
{
    /// <summary>
    /// The UI text displaying the host's IP address.
    /// </summary>
    [Header("UI References")]
    public Text ipText;

    /// <summary>
    /// The UI text displaying the selected port.
    /// </summary>
    public Text portText;

    /// <summary>
    /// The UI text displaying the generated password.
    /// </summary>
    public Text passwordText;

    /// <summary>
    /// The parent Transform for player list UI entries.
    /// </summary>
    public Transform playerListParent;

    /// <summary>
    /// The prefab for player list UI entries.
    /// </summary>
    public GameObject playerEntryPrefab;

    /// <summary>
    /// The UI button to start the game.
    /// </summary>
    [Header("Start Game")]
    public Button startGameButton;

    /// <summary>
    /// The name of the game scene to load when starting the race.
    /// </summary>
    public string gameSceneName = "GameScene";

    /// <summary>
    /// The generated password for client authentication.
    /// </summary>
    private string generatedPassword;

    /// <summary>
    /// The selected port for the network host.
    /// </summary>
    private ushort selectedPort;

    /// <summary>
    /// Initializes the host UI, sets up network parameters, and starts the host.
    /// </summary>
    private void Start()
    {
        SetupHost();
    }

    /// <summary>
    /// Configures the network host with a password, port, IP, and player list updates.
    /// </summary>
    void SetupHost()
    {
        // 1. 生成密码
        generatedPassword = GeneratePassword(6);
        passwordText.text = $"Password: {generatedPassword}";

        TokenAuthenticator authenticator = XNetworkManager.singleton.authenticator as TokenAuthenticator;
        if (authenticator != null)
        {
            authenticator.serverToken = generatedPassword;
            authenticator.clientToken = generatedPassword;
        }
        else
        {
            Debug.LogWarning("TokenAuthenticator not found on XNetworkManager.");
        }

        // 2. 自动选择端口
        selectedPort = GetAvailablePort(7777, 7800);
        XNetworkManager.singleton.GetComponent<TelepathyTransport>().port = selectedPort;
        portText.text = $"Port: {selectedPort}";

        // 3. 获取 IP
        string ip = GetInternalIP("10.");
        ipText.text = $"IP: {ip}";

        // 4. 启动 Host
        XNetworkManager.singleton.StartHost();

        // 5. 启动玩家列表刷新
        InvokeRepeating(nameof(UpdatePlayerList), 1f, 1f);

        startGameButton.interactable = false;

        startGameButton.onClick.AddListener(OnStartGameClicked);
    }

    /// <summary>
    /// Generates a random password of specified length.
    /// </summary>
    /// <param name="length">The length of the password.</param>
    /// <returns>The generated password.</returns>
    string GeneratePassword(int length)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        System.Random rng = new System.Random();
        return new string(Enumerable.Repeat(chars, length).Select(s => s[rng.Next(s.Length)]).ToArray());
    }

    /// <summary>
    /// Finds an available network port within a specified range.
    /// </summary>
    /// <param name="startPort">The starting port number.</param>
    /// <param name="endPort">The ending port number.</param>
    /// <returns>The available port number or fallback (7777).</returns>
    ushort GetAvailablePort(int startPort, int endPort)
    {
        for (ushort port = (ushort)startPort; port <= endPort; port++)
        {
            if (IsPortAvailable(port))
                return port;
        }
        return 7777; // fallback
    }

    /// <summary>
    /// Checks if a port is available for use.
    /// </summary>
    /// <param name="port">The port number to check.</param>
    /// <returns>True if the port is available, false otherwise.</returns>
    bool IsPortAvailable(ushort port)
    {
        try
        {
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            listener.Stop();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Retrieves the internal IP address starting with a specified prefix.
    /// </summary>
    /// <param name="prefix">The IP address prefix (e.g., "10.").</param>
    /// <returns>The internal IP address or "Unknown" if not found.</returns>
    string GetInternalIP(string prefix)
    {
        string fallback = "Unknown";
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork && ip.ToString().StartsWith(prefix))
                return ip.ToString();
        }
        return fallback;
    }

    /// <summary>
    /// Updates the player list UI with current connected players.
    /// </summary>
    void UpdatePlayerList()
    {
        if (!NetworkServer.active) return;

        foreach (Transform child in playerListParent)
            Destroy(child.gameObject);

        int playerCount = 0;

        foreach (var conn in NetworkServer.connections.Values)
        {
            if (conn.identity != null)
            {
                XPlayer player = conn.identity.GetComponent<XPlayer>();
                if (player != null)
                {
                    playerCount++;

                    GameObject entry = Instantiate(playerEntryPrefab, playerListParent);

                    // 设置名字
                    Text label = entry.GetComponentInChildren<Text>();
                    label.text = player.playerName;

                    // 查找按钮（建议命名为 KickButton）
                    Button kickButton = entry.GetComponentInChildren<Button>();

                    // 禁用自己（host）对应的按钮
                    if (conn.connectionId == NetworkServer.localConnection.connectionId)
                    {
                        kickButton.gameObject.SetActive(false);
                    }
                    else
                    {
                        // 绑定 Kick 回调
                        kickButton.onClick.AddListener(() => KickPlayer(conn));
                    }
                }
            }
        }
        // 启用/禁用 Start 按钮
        startGameButton.interactable = playerCount >= 1;
    }

    /// <summary>
    /// Kicks a player from the server by disconnecting their connection.
    /// </summary>
    /// <param name="conn">The network connection to disconnect.</param>
    void KickPlayer(NetworkConnectionToClient conn)
    {
        if (conn != null && conn.isAuthenticated)
        {
            Debug.Log($"Kicking player {conn.connectionId}");
            conn.Disconnect();
        }
    }

    /// <summary>
    /// Retrieves the generated password for client authentication.
    /// </summary>
    /// <returns>The generated password.</returns>
    public string GetHostPassword()
    {
        return generatedPassword;
    }

    /// <summary>
    /// Stops player list updates when the script is disabled.
    /// </summary>
    void OnDisable()
    {
        CancelInvoke(nameof(UpdatePlayerList));
    }

    /// <summary>
    /// Stops player list updates when the GameObject is destroyed.
    /// </summary>
    void OnDestroy()
    {
        CancelInvoke(nameof(UpdatePlayerList));
    }

    /// <summary>
    /// Starts the game by switching to the game scene if sufficient players are connected.
    /// </summary>
    void OnStartGameClicked()
    {
        // 确保是 Host 且至少有 2 个客户端连接
        if (!NetworkServer.active)
        {
            Debug.LogWarning("Only the host can start the game.");
            return;
        }

        if (NetworkServer.connections.Count < 1)
        {
            Debug.LogWarning("No clients connected.");
            return;
        }

        // 切换场景：所有客户端会自动跟随加载
        XNetworkManager.singleton.ServerChangeScene(gameSceneName);
    }
}