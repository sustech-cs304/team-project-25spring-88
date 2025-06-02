using Mirror;
using UnityEngine;
using VehicleBehaviour;
using VehicleBehaviour.Utils;
using TMPro;

[RequireComponent(typeof(NetworkIdentity))]
[RequireComponent(typeof(Rigidbody))]
public class CarInitializer : NetworkBehaviour
{
    void Start()
    {
        var netId = GetComponent<NetworkIdentity>();

        // 只让本地玩家控制自己拥有的车辆
        if (netId != null && !netId.isOwned) return;

        // 启用本地玩家控制
        var vehicle = GetComponent<WheelVehicle>();
        // if (vehicle != null)
        // {
        //     vehicle.IsPlayer = true;
        // }

        // 设置摄像机跟随
        var cam = Camera.main?.GetComponent<CameraFollow>();
        if (cam != null)
        {
            cam.FollowTarget(transform);
        }
        else
        {
            Debug.LogWarning("CameraFollow not found on main camera.");
        }

        var speedo = FindObjectOfType<Speedometer>();
        if (speedo != null)
        {
            // 获取当前车辆的 Rigidbody（RequireComponent 已经确保有）
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                speedo.target = rb;
                // 可以根据需要调整 maxSpeed，比如从 WheelVehicle 里取最大速度值
                // speedo.maxSpeed = vehicle.MaxSpeedKmh; 
            }
            else
            {
                Debug.LogWarning("Rigidbody not found on this vehicle, cannot bind to Speedometer.");
            }
        }
        else
        {
            Debug.LogWarning("Speedometer not found in the scene.");
        }



        var lapTracker = GetComponent<LapTracker>();
        if (lapTracker != null)
        {
            // 根据标签查找场景中的 LapText
            var lapObj = GameObject.FindWithTag("lapText");
            if (lapObj != null)
            {
                var lapTMP = lapObj.GetComponent<TMP_Text>();
                if (lapTMP != null)
                {
                    lapTracker.lapText = lapTMP;
                }
                else
                {
                    Debug.LogWarning("GameObject with tag 'LapText' does not have a TMP_Text component.");
                }
            }
            else
            {
                Debug.LogWarning("Cannot find any GameObject tagged 'LapText' in the scene.");
            }

            // 根据标签查找场景中的 TimeText
            var timeObj = GameObject.FindWithTag("timeText");
            if (timeObj != null)
            {
                var timeTMP = timeObj.GetComponent<TMP_Text>();
                if (timeTMP != null)
                {
                    lapTracker.timerText = timeTMP;
                }
                else
                {
                    Debug.LogWarning("GameObject with tag 'TimeText' does not have a TMP_Text component.");
                }
            }
            else
            {
                Debug.LogWarning("Cannot find any GameObject tagged 'TimeText' in the scene.");
            }
        }
        else
        {
            Debug.LogWarning("LapTracker component not found on this vehicle.");
        }
    }
}
