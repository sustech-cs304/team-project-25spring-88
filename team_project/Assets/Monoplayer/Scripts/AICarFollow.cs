using UnityEngine;
using UnityEngine.UI;
using VehicleBehaviour;
using System.Collections.Generic;

/// <summary>
/// A Unity script that controls an AI car's path-following behavior in a racing game.
/// <para>
/// This script enables an AI car to follow a predefined path of waypoints, applying movement forces and rotation
/// to navigate the track. It includes a picture-in-picture (PIP) camera for monitoring the AI car and handles collision
/// detection with the player car to trigger game over. It also manages stuck detection and respawn logic.
/// </para>
/// </summary>
public class AICarPathFollower : MonoBehaviour
{
    /// <summary>
    /// The target vehicle containing the waypoints for the AI car to follow.
    /// </summary>
    [Header("Path Following")]
    public WheelVehicle targetVehicle;

    /// <summary>
    /// The speed at which the AI car moves towards waypoints (in meters per second).
    /// </summary>
    public float moveSpeed = 8f;

    /// <summary>
    /// The speed at which the AI car rotates towards the target direction (in degrees per second).
    /// </summary>
    public float turnSpeed = 5f;

    /// <summary>
    /// The distance threshold (in meters) for considering a waypoint reached.
    /// </summary>
    public float waypointThreshold = 1.5f;

    /// <summary>
    /// The distance ahead to look for smoother path following (in meters).
    /// </summary>
    public float lookAheadDistance = 4f;

    /// <summary>
    /// The maximum force applied to move the AI car (in Newtons).
    /// </summary>
    [Header("Physics Settings")]
    public float maxForce = 15f;

    /// <summary>
    /// The factor by which velocity is reduced when braking.
    /// </summary>
    public float brakingFactor = 0.94f;

    /// <summary>
    /// The maximum speed of the AI car (in meters per second).
    /// </summary>
    public float maxSpeed = 15f;

    /// <summary>
    /// The radius within which the AI car slows down as it approaches a waypoint (in meters).
    /// </summary>
    public float slowdownRadius = 5f;

    /// <summary>
    /// The speed limit for turning to maintain stability (in meters per second).
    /// </summary>
    [Header("Stability Settings")]
    [Range(1f, 20f)] public float turnSpeedLimit = 8f;

    /// <summary>
    /// The maximum angle the AI car can turn per frame (in degrees).
    /// </summary>
    [Range(0f, 90f)] public float maxTurnAngle = 45f;

    /// <summary>
    /// The stiffness applied to keep the car level with the ground.
    /// </summary>
    [Range(0f, 1f)] public float groundStiffness = 0.7f;

    /// <summary>
    /// Whether to enable the picture-in-picture camera for the AI car. // 是否启用画中画相机
    /// </summary>
    [Header("Picture-in-Picture Camera")]
    public bool enablePIPCamera = true;

    /// <summary>
    /// The position of the PIP display in screen coordinates (bottom-left corner).
    /// </summary>
    public Vector2 pipPosition = new Vector2(20, 20); // 左下角位置

    /// <summary>
    /// The size of the PIP display in pixels.
    /// </summary>
    public Vector2 pipSize = new Vector2(300, 200); // 小窗口大小

    /// <summary>
    /// The height of the PIP camera above the AI car (in meters).
    /// </summary>
    public float pipCameraHeight = 4f; // 相机高度

    /// <summary>
    /// The distance of the PIP camera behind the AI car (in meters).
    /// </summary>
    public float pipCameraDistance = 8f; // 相机距离

    /// <summary>
    /// The pitch angle of the PIP camera (in degrees).
    /// </summary>
    public float pipCameraAngle = 30f; // 相机俯角

    /// <summary>
    /// The lateral offset of the PIP camera from the AI car (in meters).
    /// </summary>
    public float pipCameraOffset = 0f; // 横向偏移量

    /// <summary>
    /// The picture-in-picture camera following the AI car.
    /// </summary>
    private Camera pipCamera; // 画中画相机

    /// <summary>
    /// The render texture used by the PIP camera.
    /// </summary>
    private RenderTexture pipRenderTexture; // 渲染纹理

    /// <summary>
    /// The UI element displaying the PIP camera feed.
    /// </summary>
    private RawImage pipDisplay; // 显示小窗口的UI元素

    /// <summary>
    /// The GameObject serving as the parent for the PIP camera.
    /// </summary>
    private GameObject cameraRig; // 相机支架

    /// <summary>
    /// The time of the last stuck check.
    /// </summary>
    private float lastCheckTime = 0f; // 上次检查时间

    /// <summary>
    /// The speed threshold below which the AI car is considered stuck (in meters per second).
    /// </summary>
    public float stuckThreshold = 0.5f; // 速度阈值（m/s）

    /// <summary>
    /// The duration the AI car must remain stuck to trigger a respawn (in seconds).
    /// </summary>
    public float stuckDuration = 3f; // 停滞持续时间（秒）

    /// <summary>
    /// The interval between stuck checks (in seconds).
    /// </summary>
    public float checkInterval = 0.5f; // 检查间隔（秒）

    /// <summary>
    /// The Rigidbody component of the AI car.
    /// </summary>
    private Rigidbody rb;

    /// <summary>
    /// The index of the current waypoint the AI car is targeting.
    /// </summary>
    public int currentWaypointIndex = 0;

    /// <summary>
    /// The timer tracking how long the AI car has been stuck.
    /// </summary>
    private float stuckTimer = 0f;

    /// <summary>
    /// The last recorded position of the AI car for stuck detection.
    /// </summary>
    private Vector3 lastPosition;

    /// <summary>
    /// Whether the AI car is grounded (at least one wheel is touching the ground).
    /// </summary>
    private bool isGrounded = false;

    /// <summary>
    /// The frame count of the last ground check to prevent redundant checks.
    /// </summary>
    private int lastGroundCheck = 0;

    /// <summary>
    /// The wheel colliders of the AI car for ground detection.
    /// </summary>
    private WheelCollider[] wheels;

    /// <summary>
    /// The position of the current target waypoint.
    /// </summary>
    private Vector3 currentTarget;

    /// <summary>
    /// The time spent at the current waypoint to detect prolonged stops.
    /// </summary>
    private float timeAtCurrentWaypoint = 0f;

    /// <summary>
    /// The maximum time allowed at a waypoint before forcing an update (in seconds).
    /// </summary>
    private const float MAX_TIME_AT_WAYPOINT = 3f;

    /// <summary>
    /// Initializes the AI car's Rigidbody, wheels, and PIP camera system.
    /// </summary>
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        wheels = GetComponentsInChildren<WheelCollider>();
        
        if (targetVehicle == null)
        {
            Debug.LogError("Target Vehicle not assigned!");
            enabled = false;
            return;
        }
        
        UpdateTargetWaypoint();
        
        // 创建画中画相机
        if (enablePIPCamera)
        {
            CreatePIPCamera();
        }
    }

    /// <summary>
    /// Creates the picture-in-picture camera system for monitoring the AI car.
    /// </summary>
    void CreatePIPCamera()
    {
        // 创建渲染纹理
        pipRenderTexture = new RenderTexture((int)pipSize.x, (int)pipSize.y, 24);
        pipRenderTexture.name = "PIP_RenderTexture";
        
        // 创建相机支架
        cameraRig = new GameObject("PIP_CameraRig");
        cameraRig.transform.SetParent(transform);
        cameraRig.transform.localPosition = Vector3.zero;
        cameraRig.transform.localRotation = Quaternion.identity;
        
        // 创建相机
        pipCamera = new GameObject("PIP_Camera").AddComponent<Camera>();
        pipCamera.transform.SetParent(cameraRig.transform);
        pipCamera.targetTexture = pipRenderTexture;
        
        // 配置相机
        pipCamera.nearClipPlane = 0.1f;
        pipCamera.farClipPlane = 1000f;
        pipCamera.fieldOfView = 60f; // 更窄的视野使小车更居中
        pipCamera.depth = 1;
        
        // 设置初始位置
        UpdatePIPCameraPosition();
        
        // 创建UI显示
        CreatePIPDisplay();
    }

    /// <summary>
    /// Creates the UI display for the picture-in-picture camera feed in the bottom-left corner.
    /// </summary>
    void CreatePIPDisplay()
    {
        // 查找Canvas，如果没有则创建
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("PIP_Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }
        
        // 创建RawImage显示渲染纹理
        GameObject displayGO = new GameObject("PIP_Display");
        displayGO.transform.SetParent(canvas.transform);
        pipDisplay = displayGO.AddComponent<RawImage>();
        pipDisplay.texture = pipRenderTexture;
        
        // 设置位置和大小（左下角）
        RectTransform rect  = pipDisplay.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0); // 左下角锚点
        rect.anchorMax = new Vector2(0, 0); // 左下角锚点
        rect.pivot = new Vector2(0, 0);     // 左下角轴心
        rect.anchoredPosition = pipPosition;
        rect.sizeDelta = pipSize;
        
        // 添加边框
        pipDisplay.color = Color.white;
        Outline outline = displayGO.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, -2);
    }

    /// <summary>
    /// Updates the position and orientation of the picture-in-picture camera to follow the AI car.
    /// </summary>
    void UpdatePIPCameraPosition()
    {
        if (pipCamera == null) return;
        
        // 计算相机位置（在车辆后方和上方，添加横向偏移）
        Vector3 offset = new Vector3(pipCameraOffset, pipCameraHeight, -pipCameraDistance);
        Quaternion rotation = Quaternion.Euler(pipCameraAngle, 0, 0);
        Vector3 desiredPosition = transform.position + rotation * offset;
        
        // 设置相机位置
        pipCamera.transform.position = desiredPosition;
        
        // 相机看向车辆中心位置，确保小车居中
        Vector3 lookTarget = transform.position + Vector3.up * 0.5f; // 稍微抬高目标点使小车更居中
        pipCamera.transform.LookAt(lookTarget);
    }

    /// <summary>
    /// Checks if the AI car is stuck and triggers a respawn if necessary.
    /// </summary>
    private void CheckIfStuck()
    {
        if (rb != null && rb.velocity.magnitude < stuckThreshold)
        {
            stuckTimer += checkInterval;
            if (stuckTimer >= stuckDuration)
            {
                Respawn();
                stuckTimer = 0f; // 重置停滞计时器
            }
        }
        else
        {
            stuckTimer = 0f;
        }
    }

    /// <summary>
    /// Respawns the AI car to the next waypoint.
    /// </summary>
    private void Respawn()
    {
        if (rb != null)
        {
            currentWaypointIndex+=1;
            UpdateTargetWaypoint();
            transform.position=currentTarget;
            Debug.Log($"{gameObject.name}: Respawned at position {transform.position}");
        }
    }

    /// <summary>
    /// Handles collisions with the player car to trigger game over.
    /// </summary>
    /// <param name="collision">The collision data.</param>
    private void OnCollisionEnter(Collision collision)
    {
        // 检测是否与玩家小车碰撞
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log($"{gameObject.name}: Collided with Player {collision.gameObject.name}");
            LapCounter lapCounter = collision.gameObject.GetComponent<LapCounter>();
            lapCounter.GameOver();
        }
    }

    /// <summary>
    /// Updates the AI car's movement, rotation, and PIP camera each fixed frame.
    /// </summary>
    void FixedUpdate()
    {
        if (targetVehicle == null || targetVehicle.waypoints == null || 
            targetVehicle.waypoints.Count == 0 || currentWaypointIndex >= targetVehicle.waypoints.Count)
            return;

        // 更新地面检测
        UpdateGroundStatus();
        
        if (Time.time - lastCheckTime >= checkInterval)
        {
            CheckIfStuck();
            lastCheckTime = Time.time;
        }
        CheckWaypointReached();
        
        Vector3 direction = CalculateMovementDirection();
        ApplyMovementForce(direction);
        HandleRotation(direction);
        
        // 保持车辆稳定
        EnforceLevelOrientation();
        
        lastPosition = transform.position;
        
        // 跟踪在当前路径点停留的时间
        timeAtCurrentWaypoint += Time.fixedDeltaTime;
        
        // 更新画中画相机
        if (enablePIPCamera && pipCamera != null)
        {
            UpdatePIPCameraPosition();
        }
    }

    /// <summary>
    /// Toggles the PIP camera between top-down and follow modes.
    /// </summary>
    void TogglePIPCameraMode()
    {
        if (pipCameraAngle < 45f)
        {
            // 切换到俯视模式（确保小车居中）
            pipCameraAngle = 70f;
            pipCameraDistance = 15f;
            pipCameraHeight = 15f;
            pipCameraOffset = 0f;
        }
        else
        {
            // 切换到跟随模式（确保小车居中）
            pipCameraAngle = 30f;
            pipCameraDistance = 8f;
            pipCameraHeight = 4f;
            pipCameraOffset = 0f;
        }
    }
    
    /// <summary>
    /// Toggles the visibility of the PIP display.
    /// </summary>
    void TogglePIPVisibility()
    {
        if (pipDisplay != null)
        {
            pipDisplay.enabled = !pipDisplay.enabled;
        }
    }
    
    /// <summary>
    /// Updates the grounded status of the AI car based on wheel colliders.
    /// </summary>
    void UpdateGroundStatus()
    {
        if (lastGroundCheck == Time.frameCount) 
            return;
        
        lastGroundCheck = Time.frameCount;
        isGrounded = false;
        
        foreach (WheelCollider wheel in wheels)
        {
            if (wheel.isGrounded)
            {
                isGrounded = true;
                break;
            }
        }
    }

    /// <summary>
    /// Checks if the AI car has reached the current waypoint and updates the target if necessary.
    /// </summary>
    void CheckWaypointReached()
    {
        if (currentWaypointIndex >= targetVehicle.waypoints.Count)
            return;

        float distance = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(currentTarget.x, 0, currentTarget.z)
        );

        if (distance < waypointThreshold)
        {
            currentWaypointIndex = Mathf.Min(currentWaypointIndex + 1, targetVehicle.waypoints.Count - 1);
            UpdateTargetWaypoint();
            timeAtCurrentWaypoint = 0f;
        }
    }
    
    /// <summary>
    /// Updates the current target waypoint position.
    /// </summary>
    void UpdateTargetWaypoint()
    {
        if (currentWaypointIndex < targetVehicle.waypoints.Count)
        {
            currentTarget = targetVehicle.waypoints[currentWaypointIndex];
        }
    }

    /// <summary>
    /// Calculates the movement direction towards the target waypoint or look-ahead point.
    /// </summary>
    /// <returns>The normalized direction vector for movement.</returns>
    Vector3 CalculateMovementDirection()
    {
        if (targetVehicle.waypoints.Count == 0)
            return transform.forward;

        if (currentWaypointIndex < targetVehicle.waypoints.Count - 1)
        {
            Vector3 baseTarget = targetVehicle.waypoints[currentWaypointIndex];
            Vector3 nextWaypoint = targetVehicle.waypoints[currentWaypointIndex + 1];
            
            float distanceBetween = Vector3.Distance(baseTarget, nextWaypoint);
            
            float dynamicLookAhead = Mathf.Clamp(
                lookAheadDistance * (rb.velocity.magnitude / maxSpeed), 
                2f,
                lookAheadDistance * 2f
            );
            
            float actualLookAhead = Mathf.Min(dynamicLookAhead, distanceBetween * 0.9f);
            
            float t = actualLookAhead / distanceBetween;
            Vector3 lookAheadPoint = Vector3.Lerp(baseTarget, nextWaypoint, t);
            
            return (lookAheadPoint - transform.position).normalized;
        }
        
        return (currentTarget - transform.position).normalized;
    }

    /// <summary>
    /// Applies movement forces to the AI car based on the target direction and speed.
    /// </summary>
    /// <param name="direction">The normalized direction vector for movement.</param>
    void ApplyMovementForce(Vector3 direction)
    {
        // 检查路径点列表是否为空
        if (targetVehicle == null || targetVehicle.waypoints == null || targetVehicle.waypoints.Count == 0)
        {
            Debug.LogWarning("路径点列表为空或未分配！");
            return;
        }

        // 检查 Rigidbody 是否有效
        if (rb == null)
        {
            Debug.LogError("Rigidbody 未分配！");
            return;
        }

        // 检查是否允许移动（结合 LapCounter 的 canMove 逻辑）
        if (rb.isKinematic)
        {
            Debug.Log("AI 小车物理运动被禁用，跳过施加力。");
            return;
        }

        // 计算到目标点的距离
        float distanceToTarget = Vector3.Distance(transform.position, currentTarget);
        if (float.IsNaN(distanceToTarget) || float.IsInfinity(distanceToTarget))
        {
            Debug.LogWarning("到目标点的距离无效，跳过施加力。");
            return;
        }

        // 计算速度因子
        float speedFactor = Mathf.Clamp01(distanceToTarget / slowdownRadius);
        float targetSpeed = Mathf.Lerp(0, moveSpeed, speedFactor);

        // 确保最小速度
        float minSpeed = 2.0f;
        if (distanceToTarget > waypointThreshold * 2f)
        {
            targetSpeed = Mathf.Max(targetSpeed, minSpeed);
        }

        // 限制目标速度
        targetSpeed = Mathf.Clamp(targetSpeed, -maxSpeed * 0.3f, maxSpeed);
        if (float.IsNaN(targetSpeed) || float.IsInfinity(targetSpeed))
        {
            Debug.LogWarning("目标速度无效，设置为 0。");
            return;
        }

        // 检查方向向量是否有效
        if (direction.sqrMagnitude < 0.0001f) // 避免零向量
        {
            Debug.LogWarning("方向向量无效，跳过施加力。");
            return;
        }

        // 计算目标速度向量
        Vector3 targetVelocity = direction.normalized * targetSpeed;
        if (float.IsNaN(targetVelocity.x) || float.IsNaN(targetVelocity.y) || float.IsNaN(targetVelocity.z))
        {
            Debug.LogWarning("目标速度向量包含 NaN，跳过施加力。");
            return;
        }

        // 计算力
        Vector3 force = (targetVelocity - rb.velocity) * rb.mass;
        if (float.IsNaN(force.x) || float.IsNaN(force.y) || float.IsNaN(force.z))
        {
            Debug.LogWarning("计算的力包含 NaN，跳过施加力。");
            return;
        }

        // 限制力的大小并确保 y 轴为 0
        force = Vector3.ClampMagnitude(force, maxForce);
        force.y = 0;

        // 应用力
        rb.AddForce(force, ForceMode.Force);

        // 刹车逻辑
        float dotProduct = Vector3.Dot(rb.velocity.normalized, direction.normalized);
        if (float.IsNaN(dotProduct))
        {
            Debug.LogWarning("点积计算无效，跳过刹车逻辑。");
            return;
        }

        if (dotProduct < 0.3f && rb.velocity.magnitude > 2f)
        {
            rb.velocity *= brakingFactor;
        }
    }

    /// <summary>
    /// Handles the rotation of the AI car to face the target direction.
    /// </summary>
    /// <param name="direction">The normalized direction vector for rotation.</param>
    void HandleRotation(Vector3 direction)
    {
        direction.y = 0;
        if (direction.magnitude < 0.1f) return;

        Vector3 currentForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Vector3 targetDirection = direction.normalized;
        
        float angleDifference = Vector3.SignedAngle(currentForward, targetDirection, Vector3.up);
        
        float speedFactor = Mathf.Clamp01(rb.velocity.magnitude / maxSpeed);
        float adjustedTurnSpeed = turnSpeed * (0.3f + 0.7f * speedFactor);
        
        float maxRotationAngle = maxTurnAngle * Time.fixedDeltaTime;
        float targetRotationAngle = Mathf.Clamp(angleDifference, -maxRotationAngle, maxRotationAngle);
        
        Quaternion deltaRotation = Quaternion.AngleAxis(targetRotationAngle, Vector3.up);
        Quaternion targetRotation = deltaRotation * rb.rotation;
        
        rb.MoveRotation(Quaternion.Slerp(
            rb.rotation, 
            targetRotation, 
            adjustedTurnSpeed * Time.fixedDeltaTime
        ));
    }
    
    /// <summary>
    /// Enforces level orientation to keep the AI car stable and prevent flipping.
    /// </summary>
    void EnforceLevelOrientation()
    {
        Vector3 currentEuler = rb.rotation.eulerAngles;
        
        Quaternion levelRotation = Quaternion.Euler(
            0,
            currentEuler.y,
            0
        );
        
        float stiffness = isGrounded ? groundStiffness : groundStiffness * 0.5f;
        
        rb.MoveRotation(Quaternion.Slerp(
            rb.rotation,
            levelRotation,
            stiffness * 8f * Time.fixedDeltaTime
        ));
    }
    
    /// <summary>
    /// Cleans up resources such as the PIP camera and render texture when the AI car is destroyed.
    /// </summary>
    void OnDestroy()
    {
        if (pipRenderTexture != null)
        {
            pipRenderTexture.Release();
            Destroy(pipRenderTexture);
        }
        
        if (pipDisplay != null)
        {
            Destroy(pipDisplay.gameObject);
        }
        
        if (pipCamera != null)
        {
            Destroy(pipCamera.gameObject);
        }
        
        if (cameraRig != null)
        {
            Destroy(cameraRig);
        }
    }
}