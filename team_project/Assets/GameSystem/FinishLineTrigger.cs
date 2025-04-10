using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishLineTrigger : MonoBehaviour
{
    public LapCounter lapCounter; // 在Inspector中拖入LapCounter脚本的物体

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            lapCounter.OnFinishLinePassed();
        }
    }
}