using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherSystem : MonoBehaviour
{
    /** 
     * AI-generated-content 
     * tool: grok 
     * version: 3.0
     * usage: I used the prompt "我想要基于unity制作一个赛车小游戏，现在我要实现天气控制，你能帮我写一下控制脚本吗", and 
     * directly copy the code from its response 
     */
    public enum WeatherType { Clear, Rain, Fog }

    [Header("��������")]
    public WeatherType currentWeather = WeatherType.Clear;
    public ParticleSystem rainParticle;
    public Light directionalLight;

    [Header("������")]
    public Color fogColor = new Color(0.7f, 0.7f, 0.7f);
    public float fogDensity = 0.01f;

    [Header("�����л�����")]
    public float weatherChangeInterval = 60f;
    private float weatherTimer;

    void Start()
    {
        ApplyWeather(currentWeather);
    }

    void Update()
    {
        weatherTimer += Time.deltaTime;
        if (weatherTimer >= weatherChangeInterval)
        {
            CycleWeather();
            weatherTimer = 0f;
        }
    }

    void CycleWeather()
    {
        int next = (int)currentWeather + 1;
        if (next >= System.Enum.GetValues(typeof(WeatherType)).Length) next = 0;
        currentWeather = (WeatherType)next;
        ApplyWeather(currentWeather);
    }

    void ApplyWeather(WeatherType weather)
    {
        switch (weather)
        {
            case WeatherType.Clear:
                RenderSettings.fog = false;
                if (rainParticle != null) rainParticle.Stop();
                break;

            case WeatherType.Rain:
                RenderSettings.fog = true;
                RenderSettings.fogColor = fogColor;
                RenderSettings.fogDensity = fogDensity;
                if (rainParticle != null) rainParticle.Play();
                break;

            case WeatherType.Fog:
                RenderSettings.fog = true;
                RenderSettings.fogColor = Color.gray;
                RenderSettings.fogDensity = fogDensity * 1.5f;
                if (rainParticle != null) rainParticle.Stop();
                break;
        }
    }
}