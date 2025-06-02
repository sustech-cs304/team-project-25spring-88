using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameResultUI : MonoBehaviour
{
    public Transform entryParent;
    public GameObject entryPrefab;
    public Button quitButton;

    void Start()
    {
        quitButton.onClick.AddListener(() => {
            UnityEngine.SceneManagement.SceneManager.LoadScene("LandingScene");
        });
    }

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
