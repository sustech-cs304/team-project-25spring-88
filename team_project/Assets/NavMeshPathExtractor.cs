using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Collections;

public class NavMeshPathExtractor : MonoBehaviour
{
    [Header("NavMesh设置")]
    public Transform playerTarget;
    public float pathUpdateInterval = 0.8f;
    public float pathOptimizationDistance = 5f; // 路径优化距离
    
    [Header("路径转换")]
    public float waypointSpacing = 8f; // 路径点间距
    public int maxWaypoints = 15; // 最大路径点数量
    
    private NavMeshPath navMeshPath;
    private List<Vector3> optimizedPath = new List<Vector3>();
    private AICarChaseController aiController;
    private Coroutine pathUpdateCoroutine;
    
    void Start()
    {
        aiController = GetComponent<AICarChaseController>();
        navMeshPath = new NavMeshPath();
        
        // 启动路径更新协程
        pathUpdateCoroutine = StartCoroutine(UpdateNavMeshPathRoutine());
    }
    
    void OnDestroy()
    {
        if (pathUpdateCoroutine != null)
            StopCoroutine(pathUpdateCoroutine);
    }
    
    IEnumerator UpdateNavMeshPathRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(pathUpdateInterval);
            
            if (playerTarget != null)
            {
                UpdatePathFromNavMesh();
            }
        }
    }
    
    void UpdatePathFromNavMesh()
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = playerTarget.position;
        
        // 预测玩家移动方向
        Rigidbody playerRb = playerTarget.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            // 预测1.5秒后的位置
            Vector3 predictedPos = targetPos + playerRb.velocity * 1.5f;
            
            // 确保预测位置在NavMesh上
            NavMeshHit hit;
            if (NavMesh.SamplePosition(predictedPos, out hit, 20f, NavMesh.AllAreas))
            {
                targetPos = hit.position;
            }
        }
        
        // 使用NavMesh计算路径（但不使用NavMeshAgent移动）
        if (NavMesh.CalculatePath(startPos, targetPos, NavMesh.AllAreas, navMeshPath))
        {
            if (navMeshPath.status == NavMeshPathStatus.PathComplete || 
                navMeshPath.status == NavMeshPathStatus.PathPartial)
            {
                // 将NavMesh路径转换为适合车辆的路径点
                ConvertNavMeshPathToWaypoints();
                UpdateAIController();
            }
        }
    }
    
    void ConvertNavMeshPathToWaypoints()
    {
        if (navMeshPath.corners.Length < 2) return;
        
        optimizedPath.Clear();
        
        // 路径优化：移除不必要的中间点
        List<Vector3> rawPath = new List<Vector3>(navMeshPath.corners);
        optimizedPath = OptimizePathForVehicle(rawPath);
        
        // 控制路径点数量
        if (optimizedPath.Count > maxWaypoints)
        {
            optimizedPath = ReducePathPoints(optimizedPath, maxWaypoints);
        }
    }
    
    List<Vector3> OptimizePathForVehicle(List<Vector3> originalPath)
    {
        if (originalPath.Count < 3) return originalPath;
        
        List<Vector3> optimized = new List<Vector3>();
        optimized.Add(originalPath[0]); // 起点
        
        for (int i = 1; i < originalPath.Count - 1; i++)
        {
            Vector3 prevPoint = optimized[optimized.Count - 1];
            Vector3 currentPoint = originalPath[i];
            Vector3 nextPoint = originalPath[i + 1];
            
            // 检查是否需要这个路径点
            if (ShouldKeepWaypoint(prevPoint, currentPoint, nextPoint))
            {
                optimized.Add(currentPoint);
            }
        }
        
        optimized.Add(originalPath[originalPath.Count - 1]); // 终点
        
        return optimized;
    }
    
    bool ShouldKeepWaypoint(Vector3 prev, Vector3 current, Vector3 next)
    {
        // 计算转向角度
        Vector3 dir1 = (current - prev).normalized;
        Vector3 dir2 = (next - current).normalized;
        float angle = Vector3.Angle(dir1, dir2);
        
        // 如果转向角度大于25度，保留这个路径点
        if (angle > 25f) return true;
        
        // 如果距离太远，也保留
        float distance = Vector3.Distance(prev, current);
        if (distance > waypointSpacing * 1.5f) return true;
        
        return false;
    }
    
    List<Vector3> ReducePathPoints(List<Vector3> path, int maxPoints)
    {
        if (path.Count <= maxPoints) return path;
        
        List<Vector3> reduced = new List<Vector3>();
        float totalDistance = 0f;
        
        // 计算总路径长度
        for (int i = 0; i < path.Count - 1; i++)
        {
            totalDistance += Vector3.Distance(path[i], path[i + 1]);
        }
        
        // 按等距离间隔选择路径点
        float targetSpacing = totalDistance / (maxPoints - 1);
        float currentDistance = 0f;
        
        reduced.Add(path[0]); // 起点
        
        for (int i = 1; i < path.Count - 1; i++)
        {
            currentDistance += Vector3.Distance(path[i - 1], path[i]);
            
            if (currentDistance >= targetSpacing)
            {
                reduced.Add(path[i]);
                currentDistance = 0f;
            }
        }
        
        reduced.Add(path[path.Count - 1]); // 终点
        
        return reduced;
    }
    
    void UpdateAIController()
    {
        if (aiController == null || optimizedPath.Count == 0) return;
        
        // 清理旧的动态路径点
        ClearDynamicWaypoints();
        
        // 创建新的路径点Transform数组
        Transform[] waypoints = new Transform[optimizedPath.Count];
        
        for (int i = 0; i < optimizedPath.Count; i++)
        {
            GameObject waypointObj = new GameObject($"NavMesh_Waypoint_{i}");
            waypointObj.transform.position = optimizedPath[i];
            waypointObj.transform.SetParent(transform);
            waypoints[i] = waypointObj.transform;
            
            // 可选：添加可视化组件
            #if UNITY_EDITOR
            waypointObj.AddComponent<WaypointVisualizer>();
            #endif
        }
        
        // 更新AI控制器的路径点
        aiController.waypoints = waypoints;
    }
    
    void ClearDynamicWaypoints()
    {
        // 清理之前创建的动态路径点
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child.name.StartsWith("NavMesh_Waypoint_"))
            {
                #if UNITY_EDITOR
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
                #else
                Destroy(child.gameObject);
                #endif
            }
        }
    }
    
    // 公共方法：手动更新路径（可以在特定事件时调用）
    public void ForceUpdatePath()
    {
        if (playerTarget != null)
        {
            UpdatePathFromNavMesh();
        }
    }
    
    // 获取当前路径信息（调试用）
    public Vector3[] GetCurrentPath()
    {
        return optimizedPath.ToArray();
    }
    
    public float GetPathLength()
    {
        float length = 0f;
        for (int i = 0; i < optimizedPath.Count - 1; i++)
        {
            length += Vector3.Distance(optimizedPath[i], optimizedPath[i + 1]);
        }
        return length;
    }
    
    void OnDrawGizmos()
    {
        // 绘制原始NavMesh路径
        if (navMeshPath != null && navMeshPath.corners.Length > 1)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < navMeshPath.corners.Length - 1; i++)
            {
                Gizmos.DrawLine(navMeshPath.corners[i], navMeshPath.corners[i + 1]);
            }
        }
        
        // 绘制优化后的路径
        if (optimizedPath.Count > 1)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < optimizedPath.Count - 1; i++)
            {
                Gizmos.DrawLine(optimizedPath[i], optimizedPath[i + 1]);
                Gizmos.DrawWireSphere(optimizedPath[i], 2f);
            }
            Gizmos.DrawWireSphere(optimizedPath[optimizedPath.Count - 1], 2f);
        }
        
        // 绘制当前目标
        if (playerTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerTarget.position, 3f);
        }
    }
}

#if UNITY_EDITOR
// 路径点可视化组件（仅在编辑器中使用）
public class WaypointVisualizer : MonoBehaviour
{
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}
#endif