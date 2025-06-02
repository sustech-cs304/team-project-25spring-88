using UnityEngine;
using System.Collections.Generic;
using VehicleBehaviour; // 引入WheelVehicle命名空间

public class AICarChaseController : MonoBehaviour
{
    [Header("AI目标")]
    public Transform playerCar;
    
    [Header("路径系统")]
    public Transform[] waypoints;
    public float waypointThreshold = 5f;
    public float lookAheadDistance = 10f;
    
    [Header("AI行为参数")]
    public float targetSpeed = 80f;
    public float aggressionLevel = 1f; // 激进程度
    public float catchUpBoost = 1.5f; // 追赶加速倍数
    public float rubberBandDistance = 50f; // 橡皮筋效应距离
    
    [Header("转向预测")]
    public float steerPredictionTime = 1f;
    public LayerMask obstacleLayer;
    
    // 获取WheelVehicle组件的引用
    private WheelVehicle wheelVehicle;
    private Rigidbody rb;
    private int currentWaypointIndex = 0;
    private Vector3 targetPoint;
    private float distanceToPlayer;
    private bool isRubberBanding = false;
    
    void Start()
    {
        wheelVehicle = GetComponent<WheelVehicle>();
        rb = GetComponent<Rigidbody>();
        // 确保WheelVehicle不是玩家控制
        wheelVehicle.IsPlayer = false;
        if (waypoints.Length == 0)
            GenerateWaypointsFromPlayer();
    }
    
    void Update()
    {
        CalculateTargetPoint();
        UpdateAIBehavior();
        
        // 通过WheelVehicle的接口控制车辆
        ControlVehicle();
    }
    
    void CalculateTargetPoint()
    {
        distanceToPlayer = Vector3.Distance(transform.position, playerCar.position);
        
        // 根据与玩家的距离选择目标点
        if (distanceToPlayer > rubberBandDistance)
        {
            // 距离太远，直接追向玩家
            targetPoint = playerCar.position;
            isRubberBanding = true;
        }
        else
        {
            // 正常跟踪路径点
            isRubberBanding = false;
            UpdateWaypointTarget();
        }
        
        // 添加前瞻距离
        Rigidbody playerRb = playerCar.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            Vector3 playerVelocity = playerRb.velocity;
            targetPoint += playerVelocity * steerPredictionTime;
        }
    }
    
    void UpdateWaypointTarget()
    {
        if (waypoints.Length == 0) return;
        
        Vector3 currentWaypoint = waypoints[currentWaypointIndex].position;
        
        // 检查是否到达当前路径点
        if (Vector3.Distance(transform.position, currentWaypoint) < waypointThreshold)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
        
        // 寻找最佳路径点（最接近玩家方向的）
        int bestWaypointIndex = FindBestWaypoint();
        targetPoint = waypoints[bestWaypointIndex].position;
    }
    
    int FindBestWaypoint()
    {
        float bestScore = float.MaxValue;
        int bestIndex = currentWaypointIndex;
        
        // 检查前方几个路径点
        for (int i = 0; i < Mathf.Min(3, waypoints.Length); i++)
        {
            int checkIndex = (currentWaypointIndex + i) % waypoints.Length;
            Vector3 waypointPos = waypoints[checkIndex].position;
            
            // 计算路径点到玩家的距离 + AI到路径点的距离
            float scoreToPlayer = Vector3.Distance(waypointPos, playerCar.position);
            float scoreToAI = Vector3.Distance(transform.position, waypointPos);
            float totalScore = scoreToPlayer + scoreToAI * 0.3f;
            
            if (totalScore < bestScore)
            {
                bestScore = totalScore;
                bestIndex = checkIndex;
            }
        }
        
        return bestIndex;
    }
    
    void UpdateAIBehavior()
    {
        // 根据距离调整激进程度
        float distanceFactor = Mathf.Clamp01(distanceToPlayer / rubberBandDistance);
        float currentAggression = aggressionLevel * (2f - distanceFactor);
        
        // 动态调整目标速度
        if (isRubberBanding)
        {
            targetSpeed = 100f * catchUpBoost; // 追赶时加速
        }
        else if (distanceToPlayer < 10f)
        {
            targetSpeed = 60f; // 接近时减速，避免撞击
        }
        else
        {
            targetSpeed = 80f + (currentAggression * 20f);
        }
    }
    
    void ControlVehicle()
    {
        // 计算转向输入 (-1 到 1)
        Vector3 relativeVector = transform.InverseTransformPoint(targetPoint);
        float steerInput = Mathf.Clamp(relativeVector.x / relativeVector.magnitude, -1f, 1f);
        
        // 避障检测
        steerInput = ApplyObstacleAvoidance(steerInput);
        
        // 计算油门输入 (-1 到 1)
        float currentSpeed = rb.velocity.magnitude * 3.6f; // km/h
        float speedDifference = targetSpeed - currentSpeed;
        float throttleInput = Mathf.Clamp(speedDifference / 50f, -1f, 1f);
        
        // 橡皮筋效应加成
        if (isRubberBanding && throttleInput > 0)
        {
            throttleInput = Mathf.Min(throttleInput * catchUpBoost, 1f);
        }
        
        // 刹车逻辑
        bool shouldBrake = ShouldBrake();
        if (shouldBrake)
        {
            throttleInput = -1f; // 全力刹车
        }
        
        // 设置WheelVehicle的控制输入
        wheelVehicle.Steering = steerInput;
        wheelVehicle.Throttle = throttleInput;
        wheelVehicle.Handbrake = shouldBrake && distanceToPlayer < 5f; // 紧急手刹
        
        // 可选：AI使用Boost
        wheelVehicle.boosting = isRubberBanding && wheelVehicle.Boost > 2f;
        
        // 调试信息
        Debug.Log($"AI控制 - 转向: {steerInput:F2}, 油门: {throttleInput:F2}, 当前速度: {currentSpeed:F1}, 目标速度: {targetSpeed:F1}");
    }
    
    float ApplyObstacleAvoidance(float currentSteerInput)
    {
        Vector3 forward = transform.forward;
        Vector3 rayStart = transform.position + Vector3.up * 0.5f;
        
        // 前方检测
        if (Physics.Raycast(rayStart, forward, out RaycastHit hit, lookAheadDistance, obstacleLayer))
        {
            Vector3 avoidDirection = Vector3.Cross(Vector3.up, (hit.point - transform.position).normalized);
            float avoidInput = Vector3.Dot(avoidDirection, transform.right) > 0 ? 1f : -1f;
            return Mathf.Lerp(currentSteerInput, avoidInput, 0.5f);
        }
        
        return currentSteerInput;
    }
    
    bool ShouldBrake()
    {
        // 转弯前刹车
        Vector3 targetDirection = (targetPoint - transform.position).normalized;
        float angleDifference = Vector3.Angle(transform.forward, targetDirection);
        if (angleDifference > 45f && rb.velocity.magnitude > 15f)
        {
            return true;
        }
        
        // 接近玩家时刹车
        if (distanceToPlayer < 8f && Vector3.Dot(rb.velocity.normalized, (playerCar.position - transform.position).normalized) > 0.8f)
        {
            return true;
        }
        
        return false;
    }
    
    void GenerateWaypointsFromPlayer()
    {
        // 如果没有设置路径点，基于玩家路径动态生成
        // 这里可以实现一个简单的路径记录系统
        Debug.LogWarning("请在Inspector中设置waypoints数组或使用NavMeshPathExtractor！");
    }
    
    void OnDrawGizmos()
    {
        // 绘制调试信息
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(targetPoint, 2f);
        
        Gizmos.color = Color.blue;
        if (waypoints != null && waypoints.Length > 0)
        {
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] != null)
                {
                    Gizmos.DrawWireSphere(waypoints[i].position, 1f);
                    if (i < waypoints.Length - 1 && waypoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                    }
                }
            }
        }
        
        // 绘制前方检测射线
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position + Vector3.up * 0.5f, transform.forward * lookAheadDistance);
    }
}