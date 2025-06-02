using UnityEngine;
using Mirror;

public class MultiplayerFinishLineTrigger : MonoBehaviour
{

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
                Debug.Log($"Finish line passed by {other.name}");

                // 通知LapTracker
                tracker.OnFinishLinePassed();
            }
            else
            {
                Debug.LogWarning($"No LapTracker found on object {other.name}");
            }
        }
    }
}
