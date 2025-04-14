using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherSystem : MonoBehaviour
{
    /** 
     * AI-generated-content 
     * tool: grok 
     * version: 3.0
     * usage: I used the prompt "æˆ‘æƒ³è¦åŸºäºŽunityåˆ¶ä½œä¸€ä¸ªèµ›è½¦å°æ¸¸æˆï¼ŒçŽ°åœ¨æˆ‘è¦å®žçŽ°å¤©æ°”æŽ§åˆ¶ï¼Œä½ èƒ½å¸®æˆ‘å†™ä¸€ä¸‹æŽ§åˆ¶è„šæœ¬å—", and 
     * directly copy the code from its response 
     */
    public enum WeatherType { Clear, Rain, Fog }

    [Header("ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½")]
    public WeatherType currentWeather = WeatherType.Clear;
    public ParticleSystem rainParticle;
    public Light directionalLight;

    [Header("ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½")]
    public Color fogColor = new Color(0.7f, 0.7f, 0.7f);
    public float fogDensity = 0.01f;

    [Header("ï¿½ï¿½ï¿½ï¿½ï¿½Ð»ï¿½ï¿½ï¿½ï¿½ï¿½")]
    public float weatherChangeInterval = 60f;
    private float weatherTimer;

    [Header("Îí¹ý¶É")]
    public float fogTransitionDuration = 3f; // ½¥±äÊ±¼ä£¨Ãë£©

    private Color fogStartColor;
    private Color fogTargetColor;
    private float fogStartDensity;
    private float fogTargetDensity;
    private float fogLerpTimer = 0f;
    private bool isFogTransitioning = false;

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

        if (isFogTransitioning)
        {
            fogLerpTimer += Time.deltaTime;
            float t = Mathf.Clamp01(fogLerpTimer / fogTransitionDuration);
            RenderSettings.fogColor = Color.Lerp(fogStartColor, fogTargetColor, t);
            RenderSettings.fogDensity = Mathf.Lerp(fogStartDensity, fogTargetDensity, t);
            if (t >= 1f) isFogTransitioning = false;

            // ÌØÊâ´¦Àí£ºÈôÊÇ¹Ø±ÕÎí
            if (t >= 1f && fogTargetDensity == 0f)
                RenderSettings.fog = false;
        }
    }

    void StartFogTransition(Color fromColor, Color toColor, float fromDensity, float toDensity)
    {
        fogStartColor = fromColor;
        fogTargetColor = toColor;
        fogStartDensity = fromDensity;
        fogTargetDensity = toDensity;
        fogLerpTimer = 0f;
        isFogTransitioning = true;
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
            case WeatherType.Rain:
                RenderSettings.fog = true;
                StartFogTransition(RenderSettings.fogColor, fogColor, RenderSettings.fogDensity, fogDensity);
                if (rainParticle != null) rainParticle.Play();
                break;

            case WeatherType.Fog:
                RenderSettings.fog = true;
                StartFogTransition(RenderSettings.fogColor, Color.gray, RenderSettings.fogDensity, fogDensity * 1.5f);
                if (rainParticle != null) rainParticle.Stop();
                break;

            case WeatherType.Clear:
                StartFogTransition(RenderSettings.fogColor, Color.clear, RenderSettings.fogDensity, 0f);
                break;
        }
    }
}