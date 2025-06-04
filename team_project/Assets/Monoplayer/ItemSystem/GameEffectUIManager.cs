using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// A Unity script that manages the display of effect text in a racing game, such as speed boosts or penalties.
/// <para>
/// This singleton script handles the display of effect names (e.g., "SpeedUp", "SlowDown") with fade-in/out animations
/// and color coding. It uses TextMeshProUGUI to show the effect name and duration on the UI.
/// </para>
/// </summary>
public class GameEffectUIManager : MonoBehaviour
{
    /// <summary>
    /// The singleton instance of the GameEffectUIManager.
    /// </summary>
    public static GameEffectUIManager Instance { get; private set; }

    /// <summary>
    /// The TextMeshProUGUI component used to display effect text.
    /// </summary>
    [SerializeField] private TextMeshProUGUI effectText;

    /// <summary>
    /// The duration (in seconds) for which the effect text is displayed.
    /// </summary>
    [SerializeField] private float displayDuration = 2f;

    /// <summary>
    /// The duration (in seconds) for fade-in and fade-out animations.
    /// </summary>
    [SerializeField] private float fadeDuration = 0.5f;

    /// <summary>
    /// The default color used for effect text when no specific color is defined.
    /// </summary>
    [SerializeField] private Color defaultColor = Color.white;

    /// <summary>
    /// The current coroutine handling the effect display animation.
    /// </summary>
    private Coroutine currentRoutine;

    /// <summary>
    /// A dictionary mapping effect names to their display colors.
    /// </summary>
    private Dictionary<string, Color> effectColors = new Dictionary<string, Color>
    {
        { "SpeedUp", Color.green },
        { "SlowDown", Color.cyan },
        { "Stop", Color.red },
        { "ReverseControl", new Color(1f, 0.5f, 0f) },
        { "SpinOut", Color.yellow }
    };

    /// <summary>
    /// Initializes the singleton instance and sets up the effect text component.
    /// </summary>
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        if (effectText == null)
        {
            effectText = GameObject.Find("EffectText")?.GetComponent<TextMeshProUGUI>();
            if (effectText == null)
            {
                Debug.LogWarning("[GameEffectUIManager] 没找到 EffectText 对象");
            }
        }

        HideImmediate();
    }

    /// <summary>
    /// Displays an effect name with a fade-in/out animation and color coding.
    /// </summary>
    /// <param name="effectName">The name of the effect to display.</param>
    public void Show(string effectName)
    {
        if (effectText == null) return;

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(ShowEffectCoroutine(effectName));
    }

    /// <summary>
    /// Coroutine that handles the fade-in, display, and fade-out of the effect text.
    /// </summary>
    /// <param name="effectName">The name of the effect to display.</param>
    private IEnumerator ShowEffectCoroutine(string effectName)
    {
        float timer = 0f;

        string displayText = $"{effectName}";
        effectText.text = displayText;
        effectText.color = effectColors.ContainsKey(effectName) ? effectColors[effectName] : defaultColor;

        // 渐显
        effectText.alpha = 0;
        effectText.enabled = true;
        while (effectText.alpha < 1)
        {
            effectText.alpha += Time.deltaTime / fadeDuration;
            yield return null;
        }

        // 倒计时更新文字（如 “Stop (2.3s)”）
        float displayLeft = displayDuration;
        while (displayLeft > 0)
        {
            effectText.text = $"{effectName} ({displayLeft:F1}s)";
            displayLeft -= Time.deltaTime;
            yield return null;
        }

        // 渐隐
        while (effectText.alpha > 0)
        {
            effectText.alpha -= Time.deltaTime / fadeDuration;
            yield return null;
        }

        HideImmediate();
    }

    /// <summary>
    /// Immediately hides the effect text by clearing it and disabling the component.
    /// </summary>
    public void HideImmediate()
    {
        if (effectText != null)
        {
            effectText.text = "";
            effectText.alpha = 0;
            effectText.enabled = false;
        }
    }
}