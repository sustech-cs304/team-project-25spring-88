using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameEffectUIManager : MonoBehaviour
{
    public static GameEffectUIManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI effectText;
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private Color defaultColor = Color.white;

    private Coroutine currentRoutine;

    // 映射：效果名 → 显示颜色
    private Dictionary<string, Color> effectColors = new Dictionary<string, Color>
    {
        { "SpeedUp", Color.green },
        { "SlowDown", Color.cyan },
        { "Stop", Color.red },
        { "ReverseControl", new Color(1f, 0.5f, 0f) },
        { "SpinOut", Color.yellow }
    };


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

    public void Show(string effectName)
    {
        if (effectText == null) return;

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(ShowEffectCoroutine(effectName));
    }

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
