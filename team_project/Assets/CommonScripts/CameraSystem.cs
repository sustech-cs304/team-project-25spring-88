using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages camera switching functionality in the game.
/// </summary>
public class CameraSwitch : MonoBehaviour
{
    /// <summary>
    /// The first camera GameObject.
    /// </summary>
    public GameObject Camera1;

    /// <summary>
    /// The second camera GameObject.
    /// </summary>
    public GameObject Camera2;

    /// <summary>
    /// Updates the camera states based on input, called once per frame.
    /// </summary>
    void Update()
    {
        if (Input.GetButtonDown("camera1"))
        {
            // Toggle the active state of both cameras
            Camera1.SetActive(true);
            Camera2.SetActive(false);
        }
        if (Input.GetButtonDown("camera2"))
        {
            // Toggle the active state of both cameras
            Camera1.SetActive(false);
            Camera2.SetActive(true);
        }
    }
}