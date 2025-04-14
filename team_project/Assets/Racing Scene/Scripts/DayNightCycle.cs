using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Header("时间控制")]
    public float dayLengthInMinutes = 1f; // 一天的长度（1分钟=一天）
    private float timeOfDay = 0f; // 范围 0.0~1.0 表示一整天的进度

    [Header("太阳设置")]
    public Light sun;
    public AnimationCurve lightIntensity;


    [Header("天气影响光照强度")]
    public WeatherSystem weatherSystem;
    public float maxIntensityClear = 1.2f;
    public float maxIntensityRain = 0.6f;
    public float maxIntensityFog = 0.4f;

    [Header("不同天气的光照颜色曲线")]
    public Gradient clearColor;
    public Gradient rainColor;
    public Gradient fogColor;

    void Start()
    {
        // ?? Clear（晴天）颜色曲线
        clearColor = new Gradient();
        clearColor.SetKeys(
            new GradientColorKey[]
            {
            new GradientColorKey(new Color(15f / 255f, 20f / 255f, 50f / 255f), 0.0f),
            new GradientColorKey(new Color(255f / 255f, 140f / 255f, 60f / 255f), 0.25f),
            new GradientColorKey(new Color(255f / 255f, 255f / 255f, 240f / 255f), 0.5f),
            new GradientColorKey(new Color(255f / 255f, 120f / 255f, 80f / 255f), 0.75f),
            new GradientColorKey(new Color(15f / 255f, 20f / 255f, 50f / 255f), 1.0f)
            },
            new GradientAlphaKey[]
            {
            new GradientAlphaKey(1.0f, 0.0f),
            new GradientAlphaKey(1.0f, 1.0f)
            }
        );

        // ??? Rain（雨天）颜色曲线
        rainColor = new Gradient();
        rainColor.SetKeys(
            new GradientColorKey[]
            {
            new GradientColorKey(new Color(115f / 255f, 120f / 255f, 133f / 255f), 0.0f),
            new GradientColorKey(new Color(107f / 255f, 112f / 255f, 128f / 255f), 0.25f),
            new GradientColorKey(new Color(102f / 255f, 107f / 255f, 125f / 255f), 0.5f),
            new GradientColorKey(new Color(110f / 255f, 115f / 255f, 130f / 255f), 0.75f),
            new GradientColorKey(new Color(120f / 255f, 122f / 255f, 135f / 255f), 1.0f)
            },
            new GradientAlphaKey[]
            {
            new GradientAlphaKey(1.0f, 0.0f),
            new GradientAlphaKey(1.0f, 1.0f)
            }
        );

        // ??? Fog（雾天）颜色曲线
        fogColor = new Gradient();
        fogColor.SetKeys(
            new GradientColorKey[]
            {
            new GradientColorKey(new Color(191f / 255f, 191f / 255f, 191f / 255f), 0.0f),
            new GradientColorKey(new Color(204f / 255f, 204f / 255f, 204f / 255f), 0.25f),
            new GradientColorKey(new Color(217f / 255f, 217f / 255f, 217f / 255f), 0.5f),
            new GradientColorKey(new Color(209f / 255f, 209f / 255f, 209f / 255f), 0.75f),
            new GradientColorKey(new Color(199f / 255f, 199f / 255f, 199f / 255f), 1.0f)
            },
            new GradientAlphaKey[]
            {
            new GradientAlphaKey(1.0f, 0.0f),
            new GradientAlphaKey(1.0f, 1.0f)
            }
        );
    }

    void Update()
    {
        // 时间推进
        timeOfDay += Time.deltaTime / (dayLengthInMinutes * 60f);
        if (timeOfDay > 1f) timeOfDay -= 1f;

        // 太阳旋转
        float sunAngle = timeOfDay * 360f - 90f;
        sun.transform.rotation = Quaternion.Euler(sunAngle, -90f, 10f);

        // 设置光照颜色
        Gradient currentGradient = clearColor;
        if (weatherSystem != null)
        {
            switch (weatherSystem.currentWeather)
            {
                case WeatherSystem.WeatherType.Rain: currentGradient = rainColor; break;
                case WeatherSystem.WeatherType.Fog: currentGradient = fogColor; break;
            }
        }
        sun.color = currentGradient.Evaluate(timeOfDay);

        // 设置光照强度
        float weatherMultiplier = 1f;
        if (weatherSystem != null)
        {
            switch (weatherSystem.currentWeather)
            {
                case WeatherSystem.WeatherType.Clear: weatherMultiplier = maxIntensityClear; break;
                case WeatherSystem.WeatherType.Rain: weatherMultiplier = maxIntensityRain; break;
                case WeatherSystem.WeatherType.Fog: weatherMultiplier = maxIntensityFog; break;
            }
        }
        sun.intensity = lightIntensity.Evaluate(timeOfDay) * weatherMultiplier;
    }
}
