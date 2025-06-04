using UnityEngine;
using Mirror;

/// <summary>
/// A Unity script that manages checkpoint triggers in a multiplayer racing game.
/// <para>
/// This script detects when a local player's car passes through a checkpoint, updates the LapTracker,
/// and disables visual effects like light beams and particles for the local client.
/// </para>
/// </summary>
public class MultiplayerCheckpoint : MonoBehaviour
{
    /// <summary>
    /// The unique index of this checkpoint in the race sequence.
    /// </summary>
    public int checkpointIndex;

    /// <summary>
    /// The LineRenderer component for the checkpoint's light beam effect.
    /// </summary>
    public LineRenderer lightBeam; // 冲天的光束（Line Renderer）

    /// <summary>
    /// The ParticleSystem component for the checkpoint's light beam particles.
    /// </summary>
    public ParticleSystem lightBeamParticles; // 光束的粒子效果

    /// <summary>
    /// Handles the event when a collider enters the checkpoint trigger.
    /// <para>
    /// Updates the LapTracker and disables visual effects for the local player's car.
    /// </para>
    /// </summary>
    /// <param name="other">The collider that entered the trigger.</param>
    private void OnTriggerEnter(Collider other)
    {
        // 先判断是否是“Player”标签
        if (!other.CompareTag("Player"))
            return;

        // 再拿到这个 Player 上的 NetworkIdentity，检查它是不是本地玩家
        NetworkIdentity nid = other.GetComponent<NetworkIdentity>();
        if (nid == null || !nid.isOwned)
        {
            // 如果没有 NetworkIdentity，或不是本地玩家，就什么都不做
            return;
        }

        // 到这里说明：碰撞到的是本地玩家，才算“有效触发”
        LapTracker tracker = other.GetComponent<LapTracker>();
        if (tracker != null)
        {
            Debug.Log($"Checkpoint {checkpointIndex} passed by local player {other.name}");

            // 通知 LapTracker 记录逻辑
            tracker.OnCheckpointTriggered(checkpointIndex, transform.position, transform.rotation);

            // 关闭光束（只影响本地这一份）
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