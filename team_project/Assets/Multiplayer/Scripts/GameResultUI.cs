using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A Unity script that displays race results in a multiplayer racing game.
/// <para>
/// This script populates a UI panel with race results, showing player ranks, names, and finish times,
/// and provides a button to return to the main menu.
/// </para>
/// </summary>
public class GameResultUI : MonoBehaviour
{
    /// <summary>
    /// The parent Transform for result entry UI elements.
    /// </summary>
    public Transform entryParent;

    /// <summary>
    /// The prefab for individual result entry UI elements.
    /// </summary>
    public GameObject entryPrefab;

    /// <summary>
    /// The UI button to quit the game and return to the main menu.
    /// </summary>
    public Button quitButton;

    /// <summary>
    /// Initializes the quit button to load the main menu scene.
    /// </summary>
    void Start()
    {
        quitButton.onClick.AddListener(() => {
            UnityEngine.SceneManagement.SceneManager.LoadScene("LandingScene");
        });
    }

    /// <summary>
    /// Displays the race results in the UI.
    /// </summary>
    /// <param name="results">An array of race results containing player names and finish times.</param>
    public void Show(RaceResult[] results)
    {
        foreach (Transform child in entryParent)
            Destroy(child.gameObject);

        for (int i = 0; i < results.Length; i++)
        {
            var entry = Instantiate(entryPrefab, entryParent);
            var texts = entry.GetComponentsInChildren<TextMeshProUGUI>();
            texts[0].text = $"#{i + 1}"; // Rank
            texts[1].text = results[i].playerName;
            texts[2].text = $"{results[i].finishTime:F2}s";
        }

        gameObject.SetActive(true);
    }
}