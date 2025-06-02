using System.Collections;
using TMPro;
using UnityEngine;

public class CountdownUI : MonoBehaviour
{
    public TMP_Text countdownText;

    public void StartCountdown(int seconds)
    {
        StartCoroutine(CountdownCoroutine(seconds));
    }

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
