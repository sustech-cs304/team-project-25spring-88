using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the weather system, controlling rain, fog, and clear conditions in the game.
/// </summary>
public class WeatherSystem : MonoBehaviour
{
    /// <summary>
    /// Defines the possible weather states.
    /// </summary>
    public enum WeatherType { 
        /// <summary>Clear sky with no weather effects</summary>
        Clear, 
        /// <summary>Rainy weather with precipitation</summary>
        Rain, 
        /// <summary>Foggy conditions reducing visibility</summary>
        Fog 
    }
    /// <summary>
    /// The current active weather state
    /// </summary>
    public WeatherType currentWeather = WeatherType.Clear;

    /// <summary>
    /// Particle system for rain visual effects
    /// </summary>
    public ParticleSystem rainParticle;

    /// <summary>
    /// The main directional light affecting scene lighting
    /// </summary>
    public Light directionalLight;

    /// <summary>
    /// Base color of the fog effect
    /// </summary>
    public Color fogColor = new Color(0.7f, 0.7f, 0.7f);

    /// <summary>
    /// Density of the fog effect (0-1 range)
    /// </summary>
    [Range(0, 0.1f)]
    public float fogDensity = 0.01f;

    /// <summary>
    /// Time interval between automatic weather changes (in seconds)
    /// </summary>
    [Min(10f)]
    public float weatherChangeInterval = 60f;
    /// <summary>
    /// Duration for fog transitions (in seconds)
    /// </summary>
    [Min(0.5f)]
    public float fogTransitionDuration = 3f;

    /// <summary>
    /// Internal timer for weather changes
    /// </summary>
    private float weatherTimer;

    /// <summary>
    /// Initial fog color at transition start
    /// </summary>
    private Color fogStartColor;

    /// <summary>
    /// Target fog color for transition
    /// </summary>
    private Color fogTargetColor;

    /// <summary>
    /// Initial fog density at transition start
    /// </summary>
    private float fogStartDensity;

    /// <summary>
    /// Target fog density for transition
    /// </summary>
    private float fogTargetDensity;

    /// <summary>
    /// Timer tracking fog transition progress
    /// </summary>
    private float fogLerpTimer = 0f;

    /// <summary>
    /// Flag indicating active fog transition
    /// </summary>
    private bool isFogTransitioning = false;

    /// <summary>
    /// Initializes the weather system
    /// </summary>
    void Start()
    {
        ApplyWeather(currentWeather);
    }

    /// <summary>
    /// Updates weather system state each frame
    /// </summary>
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
            
            if (t >= 1f) 
            {
                isFogTransitioning = false;
                if (fogTargetDensity == 0f) RenderSettings.fog = false;
            }
        }
    }

    /// <summary>
    /// Starts a fog transition between two states
    /// </summary>
    /// <param name="fromColor">Initial fog color</param>
    /// <param name="toColor">Target fog color</param>
    /// <param name="fromDensity">Initial fog density</param>
    /// <param name="toDensity">Target fog density</param>
    void StartFogTransition(Color fromColor, Color toColor, float fromDensity, float toDensity)
    {
        fogStartColor = fromColor;
        fogTargetColor = toColor;
        fogStartDensity = fromDensity;
        fogTargetDensity = toDensity;
        fogLerpTimer = 0f;
        isFogTransitioning = true;
        RenderSettings.fog = true;
    }

    /// <summary>
    /// Cycles to the next weather state in sequence
    /// </summary>
    public void CycleWeather()
    {
        int next = (int)currentWeather + 1;
        if (next >= System.Enum.GetValues(typeof(WeatherType)).Length) next = 0;
        currentWeather = (WeatherType)next;
        ApplyWeather(currentWeather);
    }

    /// <summary>
    /// Applies a specific weather state
    /// </summary>
    /// <param name="weather">Weather state to apply</param>
    public void ApplyWeather(WeatherType weather)
    {
        switch (weather)
        {
            case WeatherType.Rain:
                StartFogTransition(RenderSettings.fogColor, fogColor, 
                                  RenderSettings.fogDensity, fogDensity);
                if (rainParticle != null) rainParticle.Play();
                break;

            case WeatherType.Fog:
                StartFogTransition(RenderSettings.fogColor, Color.gray, 
                                  RenderSettings.fogDensity, fogDensity * 1.5f);
                if (rainParticle != null) rainParticle.Stop();
                break;

            case WeatherType.Clear:
                StartFogTransition(RenderSettings.fogColor, Color.clear, 
                                  RenderSettings.fogDensity, 0f);
                if (rainParticle != null) rainParticle.Stop();
                break;
        }
    }
}