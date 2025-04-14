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
