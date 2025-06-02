using UnityEngine;
using UnityEngine.UI;
using VehicleBehaviour;
using System.Collections.Generic;

public class AICarPathFollower : MonoBehaviour
{
    [Header("Path Following")]
    public WheelVehicle targetVehicle;
    public float moveSpeed = 8f;
    public float turnSpeed = 5f;
    public float waypointThreshold = 1.5f;
    public float lookAheadDistance = 4f;

    [Header("Physics Settings")]
    public float maxForce = 15f;
    public float brakingFactor = 0.94f;
    public float maxSpeed = 15f;
    public float slowdownRadius = 5f;

    [Header("Stability Settings")]
    [Range(1f, 20f)] public float turnSpeedLimit = 8f;
    [Range(0f, 90f)] public float maxTurnAngle = 45f;
    [Range(0f, 1f)] public float groundStiffness = 0.7f;

    [Header("Picture-in-Picture Camera")]
    public bool enablePIPCamera = true;           // 是否启用画中画相机
    public Vector2 pipPosition = new Vector2(20, 20); // 左下角位置
    public Vector2 pipSize = new Vector2(300, 200);   // 小窗口大小
    public float pipCameraHeight = 4f;            // 相机高度
    public float pipCameraDistance = 8f;          // 相机距离
    public float pipCameraAngle = 30f;            // 相机俯角
    public float pipCameraOffset = 0f;            // 横向偏移量
    
    private Camera pipCamera;                     // 画中画相机
    private RenderTexture pipRenderTexture;       // 渲染纹理
    private RawImage pipDisplay;                  // 显示小窗口的UI元素
    private GameObject cameraRig;                 // 相机支架

    private Rigidbody rb;
    private int currentWaypointIndex = 0;
    private float stuckTimer = 0f;
    private Vector3 lastPosition;
    private bool isGrounded = false;
    private int lastGroundCheck = 0;
    private WheelCollider[] wheels;
    private Vector3 currentTarget;
    
    private float timeAtCurrentWaypoint = 0f;
    private const float MAX_TIME_AT_WAYPOINT = 3f;

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

    // 创建画中画相机系统
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
        
        // 打印控制提示到控制台
        Debug.Log("PIP Camera Controls:\n" +
                  "< and > : Rotate Camera\n" +
                  "- and + : Zoom Camera\n" +
                  "[ and ] : Adjust Height\n" +
                  "P : Toggle View Mode\n" +
                  "O : Toggle Visibility");
    }

    // 创建画中画显示UI（左下角）
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
        RectTransform rect = pipDisplay.GetComponent<RectTransform>();
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

    // 更新画中画相机位置（确保小车居中）
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

    void FixedUpdate()
    {
        if (targetVehicle == null || targetVehicle.waypoints == null || 
            targetVehicle.waypoints.Count == 0 || currentWaypointIndex >= targetVehicle.waypoints.Count)
            return;

        // 更新地面检测
        UpdateGroundStatus();
        
        CheckIfStuck();
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
    
    void Update()
    {
        // 画中画相机控制
        HandlePIPCameraControls();
    }
    
    // 画中画相机控制
    void HandlePIPCameraControls()
    {
        if (!enablePIPCamera || pipCamera == null) return;
        
        // 旋转相机
        if (Input.GetKey(KeyCode.Comma)) // < 键
        {
            cameraRig.transform.Rotate(0, -50 * Time.deltaTime, 0);
        }
        if (Input.GetKey(KeyCode.Period)) // > 键
        {
            cameraRig.transform.Rotate(0, 50 * Time.deltaTime, 0);
        }
        
        // 缩放相机
        if (Input.GetKey(KeyCode.Minus)) // - 键
        {
            pipCameraDistance = Mathf.Clamp(pipCameraDistance + 2f * Time.deltaTime, 4f, 20f);
        }
        if (Input.GetKey(KeyCode.Equals)) // + 键
        {
            pipCameraDistance = Mathf.Clamp(pipCameraDistance - 2f * Time.deltaTime, 4f, 20f);
        }
        
        // 调整高度
        if (Input.GetKey(KeyCode.LeftBracket)) // [ 键
        {
            pipCameraHeight = Mathf.Clamp(pipCameraHeight + 2f * Time.deltaTime, 2f, 15f);
        }
        if (Input.GetKey(KeyCode.RightBracket)) // ] 键
        {
            pipCameraHeight = Mathf.Clamp(pipCameraHeight - 2f * Time.deltaTime, 2f, 15f);
        }
        
        // 调整横向偏移
        if (Input.GetKey(KeyCode.Semicolon)) // ; 键
        {
            pipCameraOffset = Mathf.Clamp(pipCameraOffset - 1f * Time.deltaTime, -3f, 3f);
        }
        if (Input.GetKey(KeyCode.Quote)) // ' 键
        {
            pipCameraOffset = Mathf.Clamp(pipCameraOffset + 1f * Time.deltaTime, -3f, 3f);
        }
        
        // 切换相机模式
        if (Input.GetKeyDown(KeyCode.P))
        {
            TogglePIPCameraMode();
        }
        
        // 显示/隐藏小窗口
        if (Input.GetKeyDown(KeyCode.O))
        {
            TogglePIPVisibility();
        }
    }
    
    // 切换相机模式
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
    
    // 显示/隐藏小窗口
    void TogglePIPVisibility()
    {
        if (pipDisplay != null)
        {
            pipDisplay.enabled = !pipDisplay.enabled;
        }
    }
    
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

    void CheckIfStuck()
    {
        if (Vector3.Distance(transform.position, lastPosition) < 0.5f)
        {
            stuckTimer += Time.fixedDeltaTime;
            
            if (stuckTimer > 2f || timeAtCurrentWaypoint > MAX_TIME_AT_WAYPOINT)
            {
                rb.AddForce(transform.forward * maxForce * 0.7f, ForceMode.Impulse);
                
                currentWaypointIndex = Mathf.Min(currentWaypointIndex + 1, targetVehicle.waypoints.Count - 1);
                UpdateTargetWaypoint();
                
                stuckTimer = 0f;
                timeAtCurrentWaypoint = 0f;
            }
        }
        else
        {
            stuckTimer = 0f;
        }
    }

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
    
    void UpdateTargetWaypoint()
    {
        if (currentWaypointIndex < targetVehicle.waypoints.Count)
        {
            currentTarget = targetVehicle.waypoints[currentWaypointIndex];
        }
    }

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

    void ApplyMovementForce(Vector3 direction)
    {
        if (targetVehicle.waypoints.Count == 0) return;

        float distanceToTarget = Vector3.Distance(transform.position, currentTarget);
        
        float speedFactor = Mathf.Clamp01(distanceToTarget / slowdownRadius);
        float targetSpeed = Mathf.Lerp(0, moveSpeed, speedFactor);
        
        float minSpeed = 2.0f;
        if (distanceToTarget > waypointThreshold * 2f)
        {
            targetSpeed = Mathf.Max(targetSpeed, minSpeed);
        }
        
        targetSpeed = Mathf.Clamp(targetSpeed, -maxSpeed * 0.3f, maxSpeed);
        
        Vector3 targetVelocity = direction * targetSpeed;
        Vector3 force = (targetVelocity - rb.velocity) * rb.mass;
        
        force = Vector3.ClampMagnitude(force, maxForce);
        force.y = 0;
        
        rb.AddForce(force, ForceMode.Force);
        
        float dotProduct = Vector3.Dot(rb.velocity.normalized, direction);
        if (dotProduct < 0.3f && rb.velocity.magnitude > 2f)
        {
            rb.velocity *= brakingFactor;
        }
    }

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
    
    // 清理资源
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