using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Mirror;

public class JoinUI : MonoBehaviour
{
    [Header("UI References")]
    public InputField nameInput;
    public InputField ipInput;
    public InputField portInput;
    public InputField passwordInput;
    public Button connectButton;
    public Text statusText;

    private TokenAuthenticator authenticator;

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

    // Called when connected successfully
    public void OnConnected()
    {
        statusText.text = "Connected. Loading...";

        string name = string.IsNullOrWhiteSpace(nameInput.text) ? "Player X" : nameInput.text;
        
        StartCoroutine(SetNameAndEnterWaitingRoom(name));
    }


    void OnDisconnected()
    {
        statusText.text = "Connection failed or was rejected.";
        connectButton.interactable = true;
    }

    void OnDestroy()
    {
        XNetworkManager.OnClientConnectedExternally -= OnConnected;
        XNetworkManager.OnClientDisconnectedExternally -= OnDisconnected;
    }


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
