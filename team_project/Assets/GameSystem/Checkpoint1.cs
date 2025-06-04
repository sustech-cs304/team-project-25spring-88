using UnityEngine;

/// <summary>
/// A Unity script that manages checkpoint behavior in a racing game.
/// <para>
/// This script is attached to checkpoint trigger colliders to detect when a player car passes through,
/// updating the race state via LapCounter and disabling visual effects like light beams and particle systems.
/// It also supports resetting the checkpoint to its initial state.
/// </para>
/// <remarks>
/// AI-generated-content
/// <list type="bullet">
/// <item>tool: Grok</item>
/// <item>version: 3.0</item>
/// <item>usage: Generated using the prompt "我想要基于Unity制作一个赛车小游戏，现在我要实现存档点和记录点实现存档和如果不经过这些点不能正常结束游戏，你能帮我写一下控制脚本吗", and directly copied from its response.</item>
/// </list>
/// </remarks>
/// </summary>
public class Checkpoint : MonoBehaviour
{
    /// <summary>
    /// The unique index of this checkpoint in the race sequence.
    /// </summary>
    public int checkpointIndex;

    /// <summary>
    /// The LineRenderer component used to display a light beam effect.
    /// </summary>
    public LineRenderer lightBeam; // 冲天的光束（Line Renderer）

    /// <summary>
    /// The ParticleSystem component used for light beam particle effects.
    /// </summary>
    public ParticleSystem lightBeamParticles; // 光束的粒子效果

    /// <summary>
    /// Reference to the LapCounter script managing race logic.
    /// </summary>
    private LapCounter lapCounter;

    /// <summary>
    /// Initializes the checkpoint by finding the LapCounter and validating visual effects.
    /// </summary>
    void Start()
    {
        lapCounter = FindObjectOfType<LapCounter>();
        if (lapCounter == null)
        {
            Debug.LogError($"[{nameof(Checkpoint)}] LapCounter not found for checkpoint {checkpointIndex}!");
        }

        if (lightBeam == null)
        {
            Debug.LogWarning($"[{nameof(Checkpoint)}] LightBeam (Line Renderer) not assigned on checkpoint {checkpointIndex}!");
        }
        if (lightBeamParticles == null)
        {
            Debug.LogWarning($"[{nameof(Checkpoint)}] LightBeamParticles (Particle System) not assigned on checkpoint {checkpointIndex}!");
        }
    }

    /// <summary>
    /// Handles the event when a collider enters the checkpoint trigger.
    /// <para>
    /// Records the checkpoint passage for a player car (tagged "Player") in LapCounter
    /// and disables light beam effects.
    /// </para>
    /// </summary>
    /// <param name="other">The collider that entered the trigger.</param>
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

    /// <summary>
    /// Resets the checkpoint to its initial state, enabling light beam and particle effects.
    /// </summary>
    public void ResetCheckpoint()
    {
        if (lightBeam != null) lightBeam.enabled = true;
        if (lightBeamParticles != null)
        {
            lightBeamParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            lightBeamParticles.Play();
        }
        Debug.Log($"[{nameof(Checkpoint)}] Checkpoint {checkpointIndex} reset.");
    }
}