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
    [Header("ʱ�����")]
    public float dayLengthInMinutes = 1f; // һ��ĳ��ȣ�1����=һ�죩
    private float timeOfDay = 0f; // ��Χ 0.0~1.0 ��ʾһ����Ľ���

    [Header("̫������")]
    public Light sun;
    public AnimationCurve lightIntensity;


    [Header("����Ӱ�����ǿ��")]
    public WeatherSystem weatherSystem;
    public float maxIntensityClear = 1.2f;
    public float maxIntensityRain = 0.6f;
    public float maxIntensityFog = 0.4f;

    [Header("��ͬ�����Ĺ�����ɫ����")]
    public Gradient clearColor;
    public Gradient rainColor;
    public Gradient fogColor;

    void Start()
    {
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
        // ʱ���ƽ�
        timeOfDay += Time.deltaTime / (dayLengthInMinutes * 60f);
        if (timeOfDay > 1f) timeOfDay -= 1f;

        // ̫����ת
        float sunAngle = timeOfDay * 360f - 90f;
        sun.transform.rotation = Quaternion.Euler(sunAngle, -90f, 10f);

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
    }
}
