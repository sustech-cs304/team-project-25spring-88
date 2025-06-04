using System.Text;
using UnityEngine;
using UnityEngine.UI;
using VehicleBehaviour;

/// <summary>
/// A Unity script that controls a camera to follow a target car in a racing game.
/// <para>
/// This script enables the camera to smoothly follow a specified car (player or AI) with a configurable offset,
/// maintaining a minimum height to avoid clipping through the ground. It also supports switching between multiple
/// target cars and optionally displays the car's speed on a UI speedometer.
/// </para>
/// <remarks>
/// This code is part of Arcade Car Physics for Unity by Saarg (2018), distributed under the MIT License (see LICENSE.md).
/// AI-generated-content:
/// <list type="bullet">
/// <item>tool: Grok</item>
/// <item>version: 3.0</item>
/// <item>usage: Generated using the prompt "我想要基于Unity制作一个赛车小游戏，现在我要实现camera跟随小车脚本，你能帮我写一下控制脚本吗", and directly copied from its response.</item>
/// </list>
/// </remarks>
/// </summary>
public class CameraFollow : MonoBehaviour {
    /// <summary>
    /// Whether the camera should follow the target car.
    /// </summary>
    [SerializeField] bool follow = false;

    /// <summary>
    /// Gets or sets whether the camera should follow the target car.
    /// </summary>
    public bool Follow
    {
        get => follow;
        set => follow = value;
    }

    /// <summary>
    /// The current target Transform to follow (e.g., player or AI car).
    /// </summary>
    [SerializeField] Transform target = default;

    /// <summary>
    /// An array of possible target Transforms to switch between.
    /// </summary>
    [SerializeField] Transform[] targets = new Transform[0];

    /// <summary>
    /// The offset from the target car's position for camera placement.
    /// </summary>
    [SerializeField] Vector3 offset = -Vector3.forward;

    /// <summary>
    /// The speed multiplier for smoothly interpolating the camera's position.
    /// </summary>
    [Range(0, 10)]
    [SerializeField] float lerpPositionMultiplier = 1f;

    /// <summary>
    /// The speed multiplier for smoothly interpolating the camera's rotation.
    /// </summary>
    [Range(0, 10)]		
    [SerializeField] float lerpRotationMultiplier = 1f;

    /// <summary>
    /// The UI Text component to display the car's speed (optional).
    /// </summary>
    [SerializeField] Text speedometer = null;

    /// <summary>
    /// The Rigidbody component of the camera for collision handling.
    /// </summary>
    Rigidbody rb;

    /// <summary>
    /// The Rigidbody component of the target car (not used in current logic).
    /// </summary>
    Rigidbody targetRb;

    /// <summary>
    /// The WheelVehicle component of the target car for speed information.
    /// </summary>
    WheelVehicle vehicle;

    /// <summary>
    /// Initializes the camera's Rigidbody component.
    /// </summary>
    void Start () {
        rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Sets the target car by index from the targets array and updates player status.
    /// </summary>
    /// <param name="i">The index of the target car in the targets array.</param>
    public void SetTargetIndex(int i) {
        WheelVehicle v;

        foreach(Transform t in targets)
        {
            v = t != null ? t.GetComponent<WheelVehicle>() : null;
            if (v != null)
            {
                v.IsPlayer = false;
                v.Handbrake = true;
            }
        }

        target = targets[i % targets.Length];

        vehicle = target != null ? target.GetComponent<WheelVehicle>() : null;
        if (vehicle != null)
        {
            vehicle.IsPlayer = true;
            vehicle.Handbrake = false;
        }
    }

    /// <summary>
    /// Sets the camera to follow a specific car Transform and enables following.
    /// </summary>
    /// <param name="carTransform">The Transform of the car to follow.</param>
    public void FollowTarget(Transform carTransform)
    {
        target = carTransform;
        vehicle = target.GetComponent<WheelVehicle>();
        follow = true;
    }

    /// <summary>
    /// Updates the camera's position and rotation to follow the target car each fixed frame.
    /// </summary>
    void FixedUpdate() {
        // If we don't follow or target is null return
        if (!follow || target == null) return;

        // normalise velocity so it doesn't jump too far
        this.rb.velocity.Normalize();

        // Save transform localy
        Quaternion curRot = transform.rotation;
        Vector3 tPos = target.position + target.TransformDirection(offset);

        // Look at the target
        transform.LookAt(target);

        // Keep the camera above the target y position
        if (tPos.y < target.position.y) {
            tPos.y = target.position.y;
        }

        // Set transform with lerp
        transform.position = Vector3.Lerp(transform.position, tPos, Time.fixedDeltaTime * lerpPositionMultiplier);
        transform.rotation = Quaternion.Lerp(curRot, transform.rotation, Time.fixedDeltaTime * lerpRotationMultiplier);

        // Keep camera above the y:0.5f to prevent camera going underground
        if (transform.position.y < 0.5f) {
            transform.position = new Vector3(transform.position.x , 0.5f, transform.position.z);
        }

        // // Update speedometer
        // if (speedometer != null && vehicle != null)
        // {
        // 	StringBuilder sb = new StringBuilder();
        // 	sb.Append("Speed:");
        // 	sb.Append(((int) (vehicle.Speed)).ToString());
        // 	sb.Append(" Kph");

        // 	speedometer.text = sb.ToString();
        // }
        // else if (speedometer.text != "")
        // {
        // 	speedometer.text = "";
        // }
    }
}