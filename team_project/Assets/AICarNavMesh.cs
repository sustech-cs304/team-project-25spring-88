using UnityEngine;
using UnityEngine.AI;

public class AICarNavMesh : MonoBehaviour
{
    public Transform player; // 玩家Transform
    public float chaseDelay = 3f; // 开局延迟
    public float catchDistance = 1f; // 触发结束的距离
    private NavMeshAgent agent;
    private bool isChasing = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        Invoke(nameof(StartChase), chaseDelay); // 延迟开始追击
        
    }

    void Update()
    {
        if (isChasing)
        {
            agent.SetDestination(player.position); // 设置玩家为目标
            if (Vector3.Distance(transform.position, player.position) < catchDistance)
            {
                EndGame(); // 触发结束界面
            }
        }
    }

    void StartChase()
    {
        isChasing = true;
    }

    void EndGame()
    {
        // 显示结束界面（需自行实现UI逻辑）
        Debug.Log("Game Over: AI caught the player!");
        Time.timeScale = 0; // 暂停游戏
    }
}