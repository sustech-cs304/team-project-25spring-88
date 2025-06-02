using UnityEngine;
using Mirror; // 记得引入 Mirror 命名空间

public class MultiplayerCheckpoint : MonoBehaviour
{
    public int checkpointIndex;
    public LineRenderer lightBeam;             // 冲天的光束（Line Renderer）
    public ParticleSystem lightBeamParticles; // 光束的粒子效果

    private void OnTriggerEnter(Collider other)
    {
        // 先判断是否是“Player”标签
        if (!other.CompareTag("Player"))
            return;

        // 再拿到这个 Player 上的 NetworkIdentity，检查它是不是本地玩家
        NetworkIdentity nid = other.GetComponent<NetworkIdentity>();
        if (nid == null || !nid.isLocalPlayer)
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
