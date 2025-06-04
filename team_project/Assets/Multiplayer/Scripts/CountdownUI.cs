using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// A Unity script that displays a countdown timer UI in a racing game.
/// <para>
/// This script manages a countdown sequence, updating a UI text element and destroying itself
/// after displaying "Go!" at the end of the countdown.
/// </para>
/// </summary>
public class CountdownUI : MonoBehaviour
{
    /// <summary>
    /// The TextMeshProUGUI component displaying the countdown text.
    /// </summary>
    public TMP_Text countdownText;

    /// <summary>
    /// Starts a countdown sequence for the specified number of seconds.
    /// </summary>
    /// <param name="seconds">The duration of the countdown in seconds.</param>
    public void StartCountdown(int seconds)
    {
        StartCoroutine(CountdownCoroutine(seconds));
    }

    /// <summary>
    /// Coroutine that handles the countdown sequence, updating the UI and destroying the GameObject.
    /// </summary>
    /// <param name="seconds">The duration of the countdown in seconds.</param>
    private IEnumerator CountdownCoroutine(int seconds)
    {
        while (seconds > 0)
        {
            countdownText.text = seconds.ToString();
            yield return new WaitForSeconds(1);
            seconds--;
        }

        countdownText.text = "Go!";
        yield return new WaitForSeconds(1);
        Destroy(gameObject);
    }
}