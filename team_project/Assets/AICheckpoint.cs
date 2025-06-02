using UnityEngine;

public class AICheckpoint : MonoBehaviour
{
    private Vector3 spawnPosition; // 当前重生点位置
    private Quaternion spawnRotation; // 当前重生点旋转
    private Rigidbody rb; // 小车的刚体组件
    private float stuckTimer = 0f; // 停滞计时器
    private float lastCheckTime = 0f; // 上次检查时间
    public float stuckThreshold = 0.5f; // 速度阈值（m/s）
    public float stuckDuration = 3f; // 停滞持续时间（秒）
    public float checkInterval = 0.5f; // 检查间隔（秒）
    private int lastCheckpointIndex = -1; // 最后通过的检查点
    private int lastlujinpointindex=0;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError($"{gameObject.name}: Rigidbody component is missing!");
        }
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
    }

    void Update()
    {
        if (Time.time - lastCheckTime >= checkInterval)
        {
            CheckIfStuck();
            lastCheckTime = Time.time;
        }
    }

    public void UpdateSpawnPoint(Collider other,Vector3 position, Quaternion rotation)
    {
        spawnPosition = position;
        spawnRotation = rotation;
        lastlujinpointindex=other.GetComponent<AICarPathFollower>().currentWaypointIndex; // 更新路径点索引
        lastCheckpointIndex = FindObjectOfType<Checkpoint>().checkpointIndex; // 更新检查点序号
        Debug.Log($"{gameObject.name}: Spawn point updated to {position}, Checkpoint: {lastCheckpointIndex}");
    }

    private void CheckIfStuck()
    {
        if (rb != null && rb.velocity.magnitude < stuckThreshold)
        {
            stuckTimer += checkInterval;
            if (stuckTimer >= stuckDuration)
            {
                Respawn();
            }
        }
        else
        {
            stuckTimer = 0f;
        }
    }

    private void Respawn()
    {
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            transform.position = spawnPosition;
            transform.rotation = spawnRotation;
            transform.GetComponent<AICarPathFollower>().currentWaypointIndex=lastlujinpointindex;
            Debug.Log($"{gameObject.name}: Respawned at checkpoint {spawnPosition}");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 检测是否与玩家小车碰撞
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log($"{gameObject.name}: Collided with Player {collision.gameObject.name}");
            LapCounter lapCounter = collision.gameObject.GetComponent<LapCounter>();
            lapCounter.gameover();

        }
    }
}