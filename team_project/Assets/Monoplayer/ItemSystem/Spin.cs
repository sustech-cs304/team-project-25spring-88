using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A Unity script that controls the rotation and emission effect of a spinning object in a racing game.
/// <para>
/// This script rotates a GameObject (e.g., an item box or collectible) around its Y-axis and applies a pulsating
/// emission effect to its material, using a yellow base color. It is designed to enhance visual appeal for objects
/// like item boxes in a racing game.
/// </para>
/// </summary>
public class Spin : MonoBehaviour
{
    /// <summary>
    /// The speed at which the object rotates around its Y-axis (in degrees per second).
    /// </summary>
    public float rotationSpeed = 30f;

    /// <summary>
    /// The Renderer component used to apply the emission effect.
    /// </summary>
    public Renderer cubeRenderer;

    /// <summary>
    /// The minimum emission intensity for the pulsating effect.
    /// </summary>
    public float emissionMin = -1f;

    /// <summary>
    /// The maximum emission intensity for the pulsating effect.
    /// </summary>
    public float emissionMax = 1f;

    /// <summary>
    /// The speed at which the emission intensity pulsates.
    /// </summary>
    public float emissionSpeed = 1f;

    /// <summary>
    /// The material of the Renderer, used to apply the emission color.
    /// </summary>
    private Material mat;

    /// <summary>
    /// Initializes the Renderer and enables the emission effect on the material.
    /// </summary>
    void Start()
    {
        if (cubeRenderer == null)
            cubeRenderer = GetComponent<Renderer>();

        mat = cubeRenderer.material;
        mat.EnableKeyword("_EMISSION");
    }

    /// <summary>
    /// Updates the object's rotation and applies a pulsating emission effect.
    /// <para>
    /// Rotates the object around the Y-axis in world space and updates the material's emission color
    /// using a yellow base color with a pulsating intensity.
    /// </para>
    /// </summary>
    void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);

        float emission = Mathf.PingPong(Time.time * emissionSpeed, emissionMax - emissionMin) + emissionMin;
        Color baseColor = Color.yellow; // 设定黄色的发光颜色
        Color finalColor = baseColor * Mathf.LinearToGammaSpace(emission);
        mat.SetColor("_EmissionColor", finalColor);
    }
}