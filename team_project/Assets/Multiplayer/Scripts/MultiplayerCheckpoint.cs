using UnityEngine;

public class MultiplayerCheckpoint : MonoBehaviour
{
    public int checkpointIndex;
    public LineRenderer lightBeam; // 冲天的光束（Line Renderer）
    public ParticleSystem lightBeamParticles; // 光束的粒子效果

    private void OnTriggerEnter(Collider other)
    {
        // 只对有玩家控制的车反应
        if (other.CompareTag("Player"))
        {
            // 尝试找到LapTracker组件（应该挂在车上）
            LapTracker tracker = other.GetComponent<LapTracker>();

            if (tracker != null)
            {
                Debug.Log($"Checkpoint {checkpointIndex} passed by {other.name}");

                // 通知LapTracker
                tracker.OnCheckpointTriggered(checkpointIndex, transform.position, transform.rotation);

                // 关闭光束
                if (lightBeam != null)
                    lightBeam.enabled = false;
                if (lightBeamParticles != null)
                    lightBeamParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
            else
            {
                Debug.LogWarning($"No LapTracker found on object {other.name}");
            }
        }
    }
}
