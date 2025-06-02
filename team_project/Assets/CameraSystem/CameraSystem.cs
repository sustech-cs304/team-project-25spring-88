using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitch : MonoBehaviour
{
    public GameObject Camera1; // First camera
    public GameObject Camera2; // Second camera

    // Update is called once per frame
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
