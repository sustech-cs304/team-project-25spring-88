using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherSystem : MonoBehaviour
{
    public enum WeatherType { Clear, Rain, Fog }

    [Header("天气设置")]
    public WeatherType currentWeather = WeatherType.Clear;
    public ParticleSystem rainParticle;
    public Light directionalLight;

    [Header("雾设置")]
    public Color fogColor = new Color(0.7f, 0.7f, 0.7f);
    public float fogDensity = 0.01f;

    [Header("天气切换周期")]
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