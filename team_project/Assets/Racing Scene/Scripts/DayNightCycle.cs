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
    public Gradient lightColor;
    public AnimationCurve lightIntensity;

    void Update()
    {
        // 时间推进
        timeOfDay += Time.deltaTime / (dayLengthInMinutes * 60f);
        if (timeOfDay > 1f) timeOfDay -= 1f;

        // 太阳旋转
        float sunAngle = timeOfDay * 360f - 90f;
        sun.transform.rotation = Quaternion.Euler(sunAngle, -90f, 10f);

        // 设置光照颜色与强度
        sun.color = lightColor.Evaluate(timeOfDay);
        sun.intensity = lightIntensity.Evaluate(timeOfDay);
    }
}
