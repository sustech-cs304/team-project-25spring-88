using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube_Controller : MonoBehaviour
{
    public float speed = 1f; // 移动速度
    public float rotationSpeed = 100f; // 转向速度
    public float brakeForce = 0.5f; // 刹车力度

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // 加速度（W或上箭头）
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            Debug.Log("W or UpArrow pressed! Applying forward force.");
            rb.AddForce(transform.forward * speed);
        }
        // 刹车（S或下箭头）
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            Debug.Log("S or DownArrow pressed! Applying brake force.");
            rb.AddForce(-transform.forward * brakeForce);
        }

        // 转向（A和D或左右箭头）
        float rotation = Input.GetAxis("Horizontal") * rotationSpeed * Time.deltaTime;
        transform.Rotate(0, rotation, 0);
    }
}
