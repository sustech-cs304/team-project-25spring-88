using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public int checkpointIndex; // 存档点的序号（1, 2, 3...）
    private LapCounter lapCounter;

    void Start()
    {
        lapCounter = FindObjectOfType<LapCounter>(); // 查找LapCounter脚本
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Checkpoint {checkpointIndex} Passed by: {other.name}");
            lapCounter.OnCheckpointPassed(checkpointIndex, transform.position, transform.rotation);
        }
    }
}