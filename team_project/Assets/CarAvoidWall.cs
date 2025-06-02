using UnityEngine;
using UnityEngine.AI;

public class CarAvoidWall : MonoBehaviour
{
    private NavMeshAgent agent;
    public float wallAvoidDistance = 2f; // 避免墙壁的距离
    public LayerMask wallLayer; // 墙壁的Layer

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        // 使用射线检测墙壁
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, wallAvoidDistance, wallLayer))
        {
            // 如果距离墙壁过近，调整路径
            Vector3 avoidDirection = Vector3.Cross(Vector3.up, hit.normal); // 计算远离墙壁的方向
            Vector3 newTarget = transform.position + avoidDirection * 2f;
            agent.SetDestination(newTarget); // 临时调整目标点
        }
    }
}