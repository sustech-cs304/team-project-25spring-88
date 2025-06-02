using Mirror;
using UnityEngine;
using VehicleBehaviour;
using VehicleBehaviour.Utils;
[RequireComponent(typeof(NetworkIdentity))]
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
    }
}
