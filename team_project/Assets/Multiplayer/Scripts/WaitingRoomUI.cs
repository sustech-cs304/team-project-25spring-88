using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Mirror;

/// <summary>
/// A Unity script that manages the waiting room UI in a multiplayer racing game.
/// <para>
/// This script displays a list of connected players, highlights the local player and host, and handles
/// disconnection scenarios with a popup. It also allows players to quit to the main menu.
/// </para>
/// </summary>
public class WaitingRoomUI : MonoBehaviour
{
    /// <summary>
    /// The parent Transform for player list UI entries.
    /// </summary>
    [Header("UI References")]
    public Transform playerListParent;

    /// <summary>
    /// The prefab for player list UI entries.
    /// </summary>
    public GameObject playerEntryPrefab;

    /// <summary>
    /// The UI button to quit the waiting room and return to the main menu.
    /// </summary>
    public Button quitButton;

    /// <summary>
    /// The name of the main menu scene to load when quitting.
    /// </summary>
    [Header("Scene Settings")]
    public string mainMenuSceneName = "MainMenu";

    /// <summary>
    /// The popup GameObject displayed when a player is disconnected.
    /// </summary>
    [Header("Kick Popup")]
    public GameObject kickedPopup; // 一个简单的弹窗，包含 Text + OK 按钮

    /// <summary>
    /// The UI text element in the kick popup displaying the disconnection message.
    /// </summary>
    public Text popupText;

    /// <summary>
    /// The UI button in the kick popup to confirm and return to the main menu.
    /// </summary>
    public Button popupOkButton;

    /// <summary>
    /// Initializes UI components, event listeners, and player list updates.
    /// </summary>
    void Start()
    {
        quitButton.onClick.AddListener(OnQuitClicked);
        popupOkButton.onClick.AddListener(OnPopupConfirm);

        kickedPopup.SetActive(false);
        InvokeRepeating(nameof(UpdatePlayerList), 1f, 1f);

        XNetworkManager.OnClientDisconnectedExternally += OnDisconnected;
    }

    /// <summary>
    /// Updates the player list UI with current connected players.
    /// </summary>
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

    /// <summary>
    /// Handles the quit button click to disconnect and return to the main menu.
    /// </summary>
    void OnQuitClicked()
    {
        if (NetworkClient.isConnected)
            NetworkManager.singleton.StopClient();

        if (NetworkServer.active)
            NetworkManager.singleton.StopHost();
    }

    /// <summary>
    /// Displays a popup when the client is disconnected from the server.
    /// </summary>
    void OnDisconnected()
    {
        kickedPopup.SetActive(true);
        popupText.text = "You have been disconnected from the server.";
    }

    /// <summary>
    /// Handles the popup confirm button click to load the main menu scene.
    /// </summary>
    void OnPopupConfirm()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    /// <summary>
    /// Stops player list updates when the script is disabled.
    /// </summary>
    void OnDisable()
    {
        CancelInvoke(nameof(UpdatePlayerList));
    }

    /// <summary>
    /// Unregisters event handlers and stops player list updates when the GameObject is destroyed.
    /// </summary>
    void OnDestroy()
    {
        CancelInvoke(nameof(UpdatePlayerList));
        XNetworkManager.OnClientDisconnectedExternally -= OnDisconnected;
    }
}