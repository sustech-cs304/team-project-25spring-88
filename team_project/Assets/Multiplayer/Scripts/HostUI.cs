using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class HostUI : MonoBehaviour
{
    [Header("UI References")]
    public Text ipText;
    public Text portText;
    public Text passwordText;
    public Transform playerListParent;
    public GameObject playerEntryPrefab;

    [Header("Start Game")]
    public Button startGameButton;
    public string gameSceneName = "GameScene"; 

    private string generatedPassword;
    private ushort selectedPort;

    private void Start()
    {
        SetupHost();
    }

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
        string ip = GetInternalIP("10.16");
        ipText.text = $"IP: {ip}";

        // 4. 启动 Host
        XNetworkManager.singleton.StartHost();

        // 5. 启动玩家列表刷新
        InvokeRepeating(nameof(UpdatePlayerList), 1f, 1f);

        startGameButton.interactable = false;

        startGameButton.onClick.AddListener(OnStartGameClicked);
    }


    string GeneratePassword(int length)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        System.Random rng = new System.Random();
        return new string(Enumerable.Repeat(chars, length).Select(s => s[rng.Next(s.Length)]).ToArray());
    }

    ushort GetAvailablePort(int startPort, int endPort)
    {
        for (ushort port = (ushort)startPort; port <= endPort; port++)
        {
            if (IsPortAvailable(port))
                return port;
        }
        return 7777; // fallback
    }

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


    void KickPlayer(NetworkConnectionToClient conn)
    {
        if (conn != null && conn.isAuthenticated)
        {
            Debug.Log($"Kicking player {conn.connectionId}");
            conn.Disconnect();
        }
    }



    public string GetHostPassword()
    {
        return generatedPassword;
    }

    void OnDisable()
    {   
        CancelInvoke(nameof(UpdatePlayerList));
    }

    void OnDestroy()
    {
        CancelInvoke(nameof(UpdatePlayerList));
    }

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
