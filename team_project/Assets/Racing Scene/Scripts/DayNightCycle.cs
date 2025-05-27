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
public class DayNightCycle : MonoBehaviour
{
    [Header("时间设置")]
    public float dayLengthInMinutes = 1f; // 一天的长度（1分钟 = 一天）
    private float timeOfDay = 0f; // 范围 0.0~1.0，表示一天的进度

    [Header("太阳设置")]
    public Light sun;
    public AnimationCurve lightIntensity;

    [Header("月亮设置")]
    public Light moon;
    public AnimationCurve moonIntensity; // 控制月亮亮度随时间变化
    public Gradient moonColor;           // 控制月亮颜色随时间变化

    [Header("天气影响强度")]
    public WeatherSystem weatherSystem;
    public float maxIntensityClear = 1.2f;
    public float maxIntensityRain = 0.6f;
    public float maxIntensityFog = 0.4f;

    [Header("不同天气的光照颜色渐变")]
    public Gradient clearColor;
    public Gradient rainColor;
    public Gradient fogColor;

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

        // ?? Clear�����죩��ɫ����
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

        // ??? Rain�����죩��ɫ����
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

        // ??? Fog�����죩��ɫ����
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
        // 更新时间
        timeOfDay += Time.deltaTime / (dayLengthInMinutes * 60f);
        if (timeOfDay > 1f) timeOfDay -= 1f;

        // 太阳旋转
        float sunAngle = timeOfDay * 360f - 90f;
        sun.transform.rotation = Quaternion.Euler(sunAngle, -90f, 10f);

        // 月亮旋转（与太阳相反）
        float moonAngle = sunAngle + 180f;
        moon.transform.rotation = Quaternion.Euler(moonAngle, -90f, 10f);

        // ���ù�����ɫ
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

        // ���ù���ǿ��
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
