using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Mirror;

public class WaitingRoomUI : MonoBehaviour
{
    [Header("UI References")]
    public Transform playerListParent;
    public GameObject playerEntryPrefab;
    public Button quitButton;

    [Header("Scene Settings")]
    public string mainMenuSceneName = "MainMenu";

    [Header("Kick Popup")]
    public GameObject kickedPopup; // 一个简单的弹窗，包含 Text + OK 按钮
    public Text popupText;
    public Button popupOkButton;

    void Start()
    {
        quitButton.onClick.AddListener(OnQuitClicked);
        popupOkButton.onClick.AddListener(OnPopupConfirm);

        kickedPopup.SetActive(false);
        InvokeRepeating(nameof(UpdatePlayerList), 1f, 1f);

        XNetworkManager.OnClientDisconnectedExternally += OnDisconnected;
    }

    void UpdatePlayerList()
    {
        if (!NetworkClient.isConnected) return;

        foreach (Transform child in playerListParent)
            Destroy(child.gameObject);

        foreach (var identity in NetworkClient.spawned.Values)
        {
            XPlayer player = identity.GetComponent<XPlayer>();
            if (player != null)
            {
                GameObject entry = Instantiate(playerEntryPrefab, playerListParent);
                Text label = entry.GetComponentInChildren<Text>();
                label.text = player.playerName;

                // 设置高亮颜色
                bool isLocal = identity.connectionToServer == NetworkClient.connection;
                bool isHost = player.playerNumber == 0;

                if (isHost)
                {
                    label.color = new Color32(255, 150, 0, 255); // 橙色
                }
                else if (isLocal)
                {
                    label.text += " (You)";
                    label.color = new Color32(0, 150, 0, 255); // 绿色
                }
                else
                {
                    label.color = Color.black; // 普通玩家
                }
            }
        }
    }


    void OnQuitClicked()
    {
        if (NetworkClient.isConnected)
            NetworkManager.singleton.StopClient();

        if (NetworkServer.active)
            NetworkManager.singleton.StopHost();
    }

    void OnDisconnected()
    {
        kickedPopup.SetActive(true);
        popupText.text = "You have been disconnected from the server.";
    }

    void OnPopupConfirm()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    void OnDisable()
    {
        CancelInvoke(nameof(UpdatePlayerList));
    }

    void OnDestroy()
    {
        CancelInvoke(nameof(UpdatePlayerList));
        XNetworkManager.OnClientDisconnectedExternally -= OnDisconnected;
    }
}
