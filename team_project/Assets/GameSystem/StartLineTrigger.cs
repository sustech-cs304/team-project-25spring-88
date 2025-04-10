using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartLineTrigger : MonoBehaviour
{
    public LapCounter lapCounter; // 在Inspector中拖入LapCounter脚本的物体

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            lapCounter.OnStartLinePassed();
            Debug.Log("Start/Finish Triggered");
        }
    }
}