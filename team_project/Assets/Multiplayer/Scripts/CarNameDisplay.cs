using Mirror;
using TMPro;
using UnityEngine;

/// <summary>
/// A Unity script that displays a player's name above their car in a multiplayer racing game.
/// <para>
/// This script synchronizes the player's name across the network and updates the UI text color
/// to distinguish the local player's car (green) from others (white).
/// </para>
/// </summary>
public class CarNameDisplay : NetworkBehaviour
{
    /// <summary>
    /// The TextMeshPro component displaying the player's name.
    /// </summary>
    [SerializeField] private TextMeshPro nameText;

    /// <summary>
    /// The player's name, synchronized across the network.
    /// </summary>
    [SyncVar(hook = nameof(OnNameChanged))]
    public string playerName;

    /// <summary>
    /// Updates the name display and color when the player name changes.
    /// </summary>
    /// <param name="_">The old name (ignored).</param>
    /// <param name="newName">The new name to display.</param>
    void OnNameChanged(string _, string newName)
    {
        nameText.text = newName;

        // 比较本地连接的 identity 来判断是不是自己
        if (isOwned)
        {
            nameText.color = Color.green;
        }
        else
        {
            nameText.color = Color.white;
        }
    }

    /// <summary>
    /// Sets the player's name on the server.
    /// </summary>
    /// <param name="name">The name to set.</param>
    [Server]
    public void SetPlayerName(string name)
    {
        playerName = name;
    }

    /// <summary>
    /// Initializes the name display with the current player name.
    /// </summary>
    void Start()
    {
        nameText.text = playerName;
    }
}