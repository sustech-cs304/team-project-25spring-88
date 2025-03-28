using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // 要跟随的目标（车辆）
    public Vector3 offset = new Vector3(0f, 10f, -5f); // 摄像机相对于车辆的偏移
    public float smoothSpeed = 0.125f; // 平滑跟随的速度
    public Vector3 rotation = new Vector3(30f, 0f, 0f); // 摄像机的旋转角度

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("CameraFollow: Target is not assigned!");
        }
    }

    void LateUpdate()
{
    if (target == null) return;

    Vector3 desiredPosition = target.position + target.TransformDirection(offset);
    Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
    RaycastHit hit;
    if (Physics.Raycast(smoothedPosition + Vector3.up * 10f, Vector3.down, out hit, 20f))
    {
        smoothedPosition.y = Mathf.Max(hit.point.y + 1f, smoothedPosition.y); // 确保摄像机不低于地面1个单位
    }
    transform.position = smoothedPosition;

    // 让摄像机始终朝向车辆
    transform.LookAt(target);
}
}