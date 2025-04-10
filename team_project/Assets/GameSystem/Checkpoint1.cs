using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public int checkpointIndex;
    private LapCounter lapCounter;
    public LineRenderer lightBeam; // 冲天的光束（Line Renderer）
    public ParticleSystem lightBeamParticles; // 光束的粒子效果

    void Start()
    {
        lapCounter = FindObjectOfType<LapCounter>();
        if (lightBeam == null)
        {
            Debug.LogWarning($"Checkpoint {checkpointIndex}: LightBeam (Line Renderer) is not assigned!");
        }
        if (lightBeamParticles == null)
        {
            Debug.LogWarning($"Checkpoint {checkpointIndex}: LightBeamParticles (Particle System) is not assigned!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Checkpoint {checkpointIndex} Passed by: {other.name}");
            lapCounter.OnCheckpointPassed(checkpointIndex, transform.position, transform.rotation);

            // 关闭光束
            if (lightBeam != null)
            {
                lightBeam.enabled = false;
            }
            else
            {
                Debug.LogWarning($"Checkpoint {checkpointIndex}: LightBeam is null when trying to disable!");
            }

            if (lightBeamParticles != null)
            {
                lightBeamParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
            else
            {
                Debug.LogWarning($"Checkpoint {checkpointIndex}: LightBeamParticles is null when trying to stop!");
            }
        }
    }
}