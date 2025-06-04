using UnityEngine;
using VehicleBehaviour;

// AI-Generated Content
// Prmopt: 请你帮我写一下跳转之后的游戏界面脚本，根据PlayerPrefs的选择来激活对应的车。请把对应的车设置为可配置参数。请把激活的车挂载的WheelVehicle的IsPlayer设置为true，其它没有激活的车设置为false

public class CarActivation : MonoBehaviour
{
    [System.Serializable]
    public class CarConfiguration
    {
        public string carID;
        public GameObject carObject;
    }

    [Header("车辆配置")]
    [Tooltip("配置所有可选车辆及其ID")]
    public CarConfiguration[] carConfigurations;
    
    [Header("摄像机设置")]
    [Tooltip("摄像机切换脚本")]
    public CameraSwitch cameraSwitch;
    [Tooltip("摄像机跟随脚本")]
    public CameraFollow cameraFollow;

    [Header("圈数计数器")]
    [Tooltip("圈数计数器脚本")]
    public LapCounter lapCounter;

    [Header("Speedometer")]
    public Speedometer speedometer;

    void Start()
    {
        // 获取玩家选择的车辆ID
        string selectedCarID = PlayerPrefs.GetString("SelectedCarID", "");
        
        if (string.IsNullOrEmpty(selectedCarID))
        {
            Debug.LogWarning("未找到选择的车辆ID，使用默认第一辆车");
            if (carConfigurations.Length > 0)
            {
                selectedCarID = carConfigurations[0].carID;
            }
            else
            {
                Debug.LogError("未配置任何车辆！");
                return;
            }
        }

        bool foundSelectedCar = false;
        GameObject selectedCar = null;

        // 遍历所有配置的车辆
        foreach (CarConfiguration config in carConfigurations)
        {
            if (config.carObject == null)
            {
                Debug.LogWarning($"车辆ID {config.carID} 未关联游戏对象，跳过");
                continue;
            }

            // 激活/停用车辆对象
            bool isSelected = config.carID == selectedCarID;
            config.carObject.SetActive(isSelected);

            // 获取WheelVehicle组件并设置玩家状态
            WheelVehicle wheelVehicle = config.carObject.GetComponent<WheelVehicle>();
            if (wheelVehicle != null)
            {
                wheelVehicle.IsPlayer = isSelected;
            }
            else
            {
                Debug.LogWarning($"车辆 {config.carID} 未找到WheelVehicle组件");
            }

            if (isSelected)
            {
                foundSelectedCar = true;
                selectedCar = config.carObject;
                Debug.Log($"已激活玩家车辆: {config.carID}");
            }
        }

        if (foundSelectedCar && selectedCar != null)
        {
            SetupCamerasForCar(selectedCar);
            SetupLapCounterForCar(selectedCar);
            SetupSpeedometerForCar(selectedCar);
        }
        else
        {
            Debug.LogError($"未找到匹配的车辆ID: {selectedCarID}");
        }
    }

    private void SetupCamerasForCar(GameObject car)
    {
        // 1. 查找车内的摄像机（tag为"carCamera"）
        Transform carCamera = FindCarCamera(car.transform);
        
        if (carCamera != null)
        {
            // 2. 设置摄像机切换脚本的车内摄像机
            if (cameraSwitch != null)
            {
                cameraSwitch.Camera2 = carCamera.gameObject;
                Debug.Log($"已将车内摄像机分配给CameraSwitch: {carCamera.name}");
            }
            else
            {
                Debug.LogWarning("未找到CameraSwitch脚本引用");
            }
        }
        else
        {
            Debug.LogWarning($"未在车辆 {car.name} 中找到tag为'carCamera'的摄像机");
        }

        // 3. 设置摄像机跟随目标
        if (cameraFollow != null)
        {
            cameraFollow.FollowTarget(car.transform);
            Debug.Log($"已将摄像机跟随目标设置为: {car.name}");
        }
        else
        {
            Debug.LogWarning("未找到CameraFollow脚本引用");
        }
    }

    private Transform FindCarCamera(Transform parent)
    {
        // 在车辆的子物体中查找tag为"carCamera"的摄像机
        foreach (Transform child in parent)
        {
            if (child.CompareTag("carCamera"))
            {
                return child;
            }
            
            // 递归查找子物体
            Transform found = FindCarCamera(child);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }

    private void SetupLapCounterForCar(GameObject car)
    {
        if (lapCounter == null)
        {
            Debug.LogWarning("未找到LapCounter脚本引用");
            return;
        }
        
        // 设置玩家车辆
        lapCounter.playerCar = car;
        Debug.Log($"已将圈数计数器的玩家车辆设置为: {car.name}");
    }

    private void SetupSpeedometerForCar(GameObject car)
    {
        if (speedometer == null)
        {
            Debug.LogWarning("未找到Speedometer脚本引用");
            return;
        }

        Rigidbody carRigidbody = car.GetComponent<Rigidbody>();
        
        // 设置玩家车辆
        speedometer.target = carRigidbody;
        Debug.Log($"已将speedometer的target设置为: {car.name}");
    }
}