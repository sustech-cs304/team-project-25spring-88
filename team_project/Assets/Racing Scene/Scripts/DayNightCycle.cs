using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Header("ʱ�����")]
    public float dayLengthInMinutes = 1f; // һ��ĳ��ȣ�1����=һ�죩
    private float timeOfDay = 0f; // ��Χ 0.0~1.0 ��ʾһ����Ľ���

    [Header("̫������")]
    public Light sun;
    public Gradient lightColor;
    public AnimationCurve lightIntensity;

    void Update()
    {
        // ʱ���ƽ�
        timeOfDay += Time.deltaTime / (dayLengthInMinutes * 60f);
        if (timeOfDay > 1f) timeOfDay -= 1f;

        // ̫����ת
        float sunAngle = timeOfDay * 360f - 90f;
        sun.transform.rotation = Quaternion.Euler(sunAngle, -90f, 10f);

        // ���ù�����ɫ��ǿ��
        sun.color = lightColor.Evaluate(timeOfDay);
        sun.intensity = lightIntensity.Evaluate(timeOfDay);
    }
}
