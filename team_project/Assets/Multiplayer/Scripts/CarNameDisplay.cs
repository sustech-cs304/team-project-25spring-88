using Mirror;
using TMPro;
using UnityEngine;

public class CarNameDisplay : NetworkBehaviour
{
    [SerializeField] private TextMeshPro nameText;

    [SyncVar(hook = nameof(OnNameChanged))]
    public string playerName;

    void OnNameChanged(string _, string newName)
    {
        nameText.text = newName;

        // 比较本地连接的 identity 来判断是不是自己
        if (NetworkClient.connection != null &&
            NetworkClient.connection.identity == connectionToClient?.identity)
        {
            nameText.color = Color.green;
        }
        else
        {
            nameText.color = Color.white;
        }
    }

    [Server]
    public void SetPlayerName(string name)
    {
        playerName = name;
    }

    void Start()
    {
        nameText.text = playerName;
    }
}
