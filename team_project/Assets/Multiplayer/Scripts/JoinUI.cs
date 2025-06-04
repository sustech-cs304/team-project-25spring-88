using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Mirror;

/// <summary>
/// A Unity script that manages the client join UI for a multiplayer racing game.
/// <para>
/// This script handles user input for joining a server, including IP, port, password, and player name,
/// and transitions to the waiting room upon successful connection.
/// </para>
/// </summary>
public class JoinUI : MonoBehaviour
{
    /// <summary>
    /// The UI input field for the player's name.
    /// </summary>
    [Header("UI References")]
    public InputField nameInput;

    /// <summary>
    /// The UI input field for the server IP address.
    /// </summary>
    public InputField ipInput;

    /// <summary>
    /// The UI input field for the server port number.
    /// </summary>
    public InputField portInput;

    /// <summary>
    /// The UI input field for the server password.
    /// </summary>
    public InputField passwordInput;

    /// <summary>
    /// The UI button to initiate the connection.
    /// </summary>
    public Button connectButton;

    /// <summary>
    /// The UI text element displaying connection status.
    /// </summary>
    public Text statusText;

    /// <summary>
    /// The TokenAuthenticator component for network authentication.
    /// </summary>
    private TokenAuthenticator authenticator;

    /// <summary>
    /// Initializes UI components and event listeners.
    /// </summary>
    void Start()
    {
        connectButton.onClick.AddListener(OnConnectClicked);

        // 清空状态栏
        statusText.text = "";

        // 获取 Authenticator
        authenticator = XNetworkManager.singleton.authenticator as TokenAuthenticator;
        if (authenticator == null)
        {
            statusText.text = "Authenticator is not assigned.";
            connectButton.interactable = false;
        }

        // 绑定错误回调
        XNetworkManager.OnClientConnectedExternally += OnConnected;
        XNetworkManager.OnClientDisconnectedExternally += OnDisconnected;
    }

    /// <summary>
    /// Handles the connect button click to initiate a server connection.
    /// </summary>
    void OnConnectClicked()
    {
        // 清空旧信息
        statusText.text = "";

        // 验证输入
        if (string.IsNullOrWhiteSpace(ipInput.text) ||
            string.IsNullOrWhiteSpace(portInput.text) ||
            string.IsNullOrWhiteSpace(passwordInput.text))
        {
            statusText.text = "Please fill in all fields.";
            return;
        }

        // 设置连接参数
        XNetworkManager.singleton.networkAddress = ipInput.text;

        if (ushort.TryParse(portInput.text, out ushort port))
        {
            var transport = XNetworkManager.singleton.GetComponent<TelepathyTransport>(); 
            if (transport != null)
            {
                transport.port = port;
            }
        }
        else
        {
            statusText.text = "Invalid port number.";
            return;
        }

        // 设置 token
        authenticator.clientToken = passwordInput.text;

        // 连接服务器
        XNetworkManager.singleton.StartClient();

        // 禁用按钮，避免重复点击
        connectButton.interactable = false;

        // 等待连接（或在 OnClientConnect 处理成功）
        statusText.text = "Connecting...";
    }

    /// <summary>
    /// Handles successful connection to the server.
    /// </summary>
    public void OnConnected()
    {
        statusText.text = "Connected. Loading...";

        string name = string.IsNullOrWhiteSpace(nameInput.text) ? "Player X" : nameInput.text;
        
        StartCoroutine(SetNameAndEnterWaitingRoom(name));
    }

    /// <summary>
    /// Handles disconnection from the server.
    /// </summary>
    void OnDisconnected()
    {
        statusText.text = "Connection failed or was rejected.";
        connectButton.interactable = true;
    }

    /// <summary>
    /// Unregisters event handlers when the GameObject is destroyed.
    /// </summary>
    void OnDestroy()
    {
        XNetworkManager.OnClientConnectedExternally -= OnConnected;
        XNetworkManager.OnClientDisconnectedExternally -= OnDisconnected;
    }

    /// <summary>
    /// Coroutine that sets the player's name and loads the waiting room scene.
    /// </summary>
    /// <param name="name">The player's name to set.</param>
    System.Collections.IEnumerator SetNameAndEnterWaitingRoom(string name)
    {
        float timeout = 5f;
        float timer = 0f;

        while (NetworkClient.connection?.identity == null)
        {
            yield return null;
            timer += Time.deltaTime;
            if (timer > timeout)
            {
                statusText.text = "Failed to find player object.";
                connectButton.interactable = true;
                yield break;
            }
        }

        var player = NetworkClient.connection.identity.GetComponent<XPlayer>();
        if (player != null)
        {
            player.CmdSetPlayerName(name);
            statusText.text = "Name set. Entering room...";
            yield return new WaitForSeconds(0.3f);
            SceneManager.LoadScene("MultiplayerWaitingRoom", LoadSceneMode.Additive);
        }
        else
        {
            statusText.text = "Player script not found.";
            connectButton.interactable = true;
        }
    }
}