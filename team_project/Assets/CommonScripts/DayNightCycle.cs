using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/** 
     * AI-generated-content 
     * tool: grok 
     * version: 3.0
     * usage: I used the prompt "我想要基于unity制作一个赛车小游戏，现在我要实现时间变化脚本，你能帮我写一下控制脚本吗", and 
     * directly copy the code from its response 
     */
/// <summary>
/// Manages the day-night cycle in the game, controlling sun and moon lighting based on time and weather.
/// </summary>
public class DayNightCycle : MonoBehaviour
{
    /// <summary>
    /// The length of a full day in minutes.
    /// </summary>
    public float dayLengthInMinutes = 1f;

    /// <summary>
    /// The current progress of the day, ranging from 0.0 to 1.0.
    /// </summary>
    private float timeOfDay = 0f;

    /// <summary>
    /// The light representing the sun.
    /// </summary>
    public Light sun;

    /// <summary>
    /// Animation curve controlling the sun's light intensity over time.
    /// </summary>
    public AnimationCurve lightIntensity;

    /// <summary>
    /// The light representing the moon.
    /// </summary>
    public Light moon;

    /// <summary>
    /// Animation curve controlling the moon's light intensity over time.
    /// </summary>
    public AnimationCurve moonIntensity;

    /// <summary>
    /// Gradient controlling the moon's color changes over time.
    /// </summary>
    public Gradient moonColor;

    /// <summary>
    /// Reference to the weather system affecting light intensity.
    /// </summary>
    public WeatherSystem weatherSystem;

    /// <summary>
    /// Maximum light intensity during clear weather.
    /// </summary>
    public float maxIntensityClear = 1.2f;

    /// <summary>
    /// Maximum light intensity during rainy weather.
    /// </summary>
    public float maxIntensityRain = 0.6f;

    /// <summary>
    /// Maximum light intensity during foggy weather.
    /// </summary>
    public float maxIntensityFog = 0.4f;
    /// <summary>
    /// Gradient for light color during clear weather.
    /// </summary>
    public Gradient clearColor;

    /// <summary>
    /// Gradient for light color during rainy weather.
    /// </summary>
    public Gradient rainColor;

    /// <summary>
    /// Gradient for light color during foggy weather.
    /// </summary>
    public Gradient fogColor;

    /// <summary>
    /// Initializes the day-night cycle, setting up default gradients for moon and weather-based lighting.
    /// </summary>
    void Start()
    {
        // 设置默认月亮颜色渐变（夜晚蓝白色）
        moonColor = new Gradient();
        moonColor.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(0.2f, 0.25f, 0.35f), 0.0f),
                new GradientColorKey(new Color(0.6f, 0.65f, 0.8f), 0.5f),
                new GradientColorKey(new Color(0.2f, 0.25f, 0.35f), 1.0f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1.0f, 0.0f),
                new GradientAlphaKey(1.0f, 1.0f)
            }
        );

        // 设置Clear（晴天）颜色渐变
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

        // 设置Rain（雨天）颜色渐变
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

        // 设置Fog（雾天）颜色渐变
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

    /// <summary>
    /// Updates the day-night cycle, adjusting sun and moon positions, colors, and intensities based on time and weather.
    /// </summary>
    void Update()
    {
        // 更新时间
        timeOfDay += Time.deltaTime / (dayLengthInMinutes * 60f);
        if (timeOfDay > 1f) timeOfDay -= 1f;

        // 太阳旋转
        float sunAngle = timeOfDay * 360f - 90f;
        sun.transform.rotation = Quaternion.Euler(sunAngle, -90f, 10f);

        // 月亮旋转（与太阳相反）
        float moonAngle = sunAngle + 180f;
        moon.transform.rotation = Quaternion.Euler(moonAngle, -90f, 10f);

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

        // 设置月亮光颜色和强度
        moon.color = moonColor.Evaluate(timeOfDay);
        float moonMultiplier = 1f;
        if (weatherSystem != null)
        {
            switch (weatherSystem.currentWeather)
            {
                case WeatherSystem.WeatherType.Clear: moonMultiplier = 1.0f; break;
                case WeatherSystem.WeatherType.Rain: moonMultiplier = 0.3f; break;
                case WeatherSystem.WeatherType.Fog: moonMultiplier = 0.4f; break;
            }
        }
        moon.intensity = moonIntensity.Evaluate(timeOfDay) * moonMultiplier * 0.4f;
    }
}