using UnityEngine;
using Mirror;

/// <summary>
/// A Unity script that detects when a local player's car crosses the finish line in a multiplayer racing game.
/// <para>
/// This script triggers the LapTracker to update race progress when a locally controlled car passes the finish line.
/// It ensures only player-controlled cars with local ownership are processed.
/// </para>
/// </summary>
public class MultiplayerFinishLineTrigger : MonoBehaviour
{
    /// <summary>
    /// Handles the event when a collider enters the finish line trigger.
    /// <para>
    /// Notifies the LapTracker if the local player's car crosses the finish line.
    /// </para>
    /// </summary>
    /// <param name="other">The collider that entered the trigger.</param>
    private void OnTriggerEnter(Collider other)
    {
        // 只对有玩家控制的车反应
        if (other.CompareTag("Player"))
        {
            NetworkIdentity nid = other.GetComponent<NetworkIdentity>();
            if (nid == null || !nid.isOwned)
            {
                // 如果没有 NetworkIdentity，或不是本地玩家，就什么都不做
                return;
            }

            // 尝试找到LapTracker组件（应该挂在车上）
            LapTracker tracker = other.GetComponent<LapTracker>();

            if (tracker != null)
            {
                Debug.Log($"[{nameof(MultiplayerFinishLineTrigger)}] Finish line passed by {other.name}");

                // 通知LapTracker
                tracker.OnFinishLinePassed();
            }
            else
            {
                Debug.LogWarning($"[{nameof(MultiplayerFinishLineTrigger)}] No LapTracker found on object {other.name}");
            }
        }
    }
}