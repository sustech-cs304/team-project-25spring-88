using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class WeatherSystemTests
{
    private GameObject obj;
    private WeatherSystem weather;
    private ParticleSystem rainParticle;
    private GameObject lightGO;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        obj = new GameObject("WeatherSystemObject");
        weather = obj.AddComponent<WeatherSystem>();

        // 添加雨粒子系统
        var rainGO = new GameObject("Rain");
        rainParticle = rainGO.AddComponent<ParticleSystem>();
        weather.rainParticle = rainParticle;

        // 添加方向光，避免 directionalLight 为空
        lightGO = new GameObject("DirectionalLight");
        Light dirLight = lightGO.AddComponent<Light>();
        dirLight.type = LightType.Directional;
        weather.directionalLight = dirLight;

        // 设置默认天气系统参数
        weather.fogColor = Color.gray;
        weather.fogDensity = 0.01f;
        weather.fogTransitionDuration = 0.5f;

        // 设置初始环境雾参数
        RenderSettings.fog = true;
        RenderSettings.fogColor = Color.gray;
        RenderSettings.fogDensity = 0.02f;

        yield return null;
    }

    [UnityTest]
    public IEnumerator CycleWeather_LoopsThroughStates()
    {
        weather.currentWeather = WeatherSystem.WeatherType.Clear;
        MethodInfo method = typeof(WeatherSystem).GetMethod("CycleWeather", BindingFlags.Public | BindingFlags.Instance);
        method.Invoke(weather, null);

        Assert.AreEqual(WeatherSystem.WeatherType.Rain, weather.currentWeather);

        method.Invoke(weather, null);
        Assert.AreEqual(WeatherSystem.WeatherType.Fog, weather.currentWeather);

        method.Invoke(weather, null);
        Assert.AreEqual(WeatherSystem.WeatherType.Clear, weather.currentWeather);

        yield return null;
    }

    [UnityTest]
    public IEnumerator ApplyWeather_Rain_ActivatesFogAndParticle()
    {
        weather.ApplyWeather(WeatherSystem.WeatherType.Rain);

        yield return null;

        Assert.IsTrue(RenderSettings.fog, "雾应该在下雨时开启");
        Assert.IsTrue(weather.rainParticle.isPlaying, "雨粒子应该播放");
    }

    [UnityTest]
    public IEnumerator ApplyWeather_Clear_DisablesFogEventually()
    {
        // 启用雾作为初始状态
        RenderSettings.fog = true;
        RenderSettings.fogColor = Color.gray;
        RenderSettings.fogDensity = 0.02f;

        // 切换天气为 Clear
        weather.ApplyWeather(WeatherSystem.WeatherType.Clear);

        // 等待渐变开始
        yield return new WaitForSeconds(0.2f);

        // 检查 fog 密度是否开始降低
        Assert.Less(RenderSettings.fogDensity, 0.02f, "雾密度应开始减少");
        Assert.IsTrue(RenderSettings.fog, "雾仍应处于开启状态直到完全清除");

        yield return null;
    }

    [TearDown]
    public void TearDown()
    {
        if (obj != null) Object.DestroyImmediate(obj);
        if (rainParticle != null && rainParticle.gameObject != null) Object.DestroyImmediate(rainParticle.gameObject);
        if (lightGO != null) Object.DestroyImmediate(lightGO);
    }
}