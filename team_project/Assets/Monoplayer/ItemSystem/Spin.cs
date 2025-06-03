using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spin : MonoBehaviour
{
    public float rotationSpeed = 30f;
    public Renderer cubeRenderer;
    public float emissionMin = -1f;
    public float emissionMax = 1f;
    public float emissionSpeed = 1f;
    private Material mat;

    void Start()
    {
        if (cubeRenderer == null)
            cubeRenderer = GetComponent<Renderer>();

        mat = cubeRenderer.material;
        mat.EnableKeyword("_EMISSION");
    }

    void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);

        float emission = Mathf.PingPong(Time.time * emissionSpeed, emissionMax - emissionMin) + emissionMin;
        Color baseColor = Color.yellow; // 你可以换成其他颜色
        Color finalColor = baseColor * Mathf.LinearToGammaSpace(emission);
        mat.SetColor("_EmissionColor", finalColor);
    }
}