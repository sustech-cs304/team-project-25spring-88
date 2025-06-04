using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A Unity script that controls the smooth rotation of a car GameObject in a racing game.
/// <para>
/// This script applies a continuous, smooth rotation to a car around its Y-axis, with the target
/// angle determined based on the car's initial orientation. It is designed for visual effects or
/// specific gameplay mechanics in a racing game.
/// </para>
/// <remarks>
/// Contribution Clarification: Originally sourced from
/// <a href="https://assetstore-fallback.unity.com/packages/3d/vehicles/land/low-poly-playable-vehicles-154577">Unity Asset Store - Low Poly Playable Vehicles</a>.
/// </remarks>
/// </summary>
public class CarRotation : MonoBehaviour {

    /// <summary>
    /// The target rotation angle (in degrees) for the car's Y-axis.
    /// </summary>
    private float targetAngle;

    /// <summary>
    /// The velocity variable used for rotation smoothing (not directly used in current logic).
    /// </summary>
    private float vel;

    /// <summary>
    /// The speed at which the car rotates towards the target angle.
    /// </summary>
    private float smoothSpeed = 10f;

    /// <summary>
    /// Initializes the target rotation angle based on the car's initial Y-axis orientation.
    /// </summary>
    private void Start () 
    {
        if (transform.localEulerAngles.y > 0) 
        {
            targetAngle = 25f + transform.localEulerAngles.y;
        }
        else
        {
            targetAngle = -25f - transform.localEulerAngles.y;
        }
    }

    /// <summary>
    /// Updates the car's rotation smoothly towards the target angle each fixed frame.
    /// </summary>
    private void FixedUpdate () 
    {
        Quaternion target = Quaternion.Euler (new Vector3(transform.localEulerAngles.x, targetAngle, transform.localEulerAngles.z));
        transform.rotation = Quaternion.Slerp (transform.rotation, target, Time.deltaTime * smoothSpeed);
        if (targetAngle > 0)
            targetAngle++;
        else
            targetAngle--;
    }
}