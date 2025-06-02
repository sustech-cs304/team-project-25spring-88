using UnityEngine;
using System.Collections.Generic;

public class CameraVisibilityManager : MonoBehaviour
{
    [System.Serializable]
    public class CameraObjectPair
    {
        public Camera targetCamera;
        public GameObject[] visibleObjects;
        public string targetLayer;
    }

    [SerializeField]
    private CameraObjectPair[] cameraObjectPairs;
    private Dictionary<Camera, int> originalCullingMasks = new Dictionary<Camera, int>();

    void Start()
    {
        if (cameraObjectPairs == null || cameraObjectPairs.Length == 0) return;

        foreach (var pair in cameraObjectPairs)
        {
            if (pair.targetCamera == null || pair.visibleObjects == null) continue;

            // 保存相机的原始 culling mask
            originalCullingMasks[pair.targetCamera] = pair.targetCamera.cullingMask;

            // 设置物体到指定的layer
            foreach (var obj in pair.visibleObjects)
            {
                if (obj != null)
                {
                    obj.layer = LayerMask.NameToLayer(pair.targetLayer);
                }
            }

            // 更新相机的 culling mask，只包含指定的 layer
            int layerMask = (1 << LayerMask.NameToLayer(pair.targetLayer));
            pair.targetCamera.cullingMask = layerMask;
        }
    }

    void OnDestroy()
    {
        // 恢复相机的原始 culling mask
        foreach (var pair in originalCullingMasks)
        {
            if (pair.Key != null)
            {
                pair.Key.cullingMask = pair.Value;
            }
        }
    }

    // 动态添加需要对特定相机可见的物体
    public void AddVisibleObject(Camera camera, GameObject obj, string layerName)
    {
        if (camera == null || obj == null) return;

        obj.layer = LayerMask.NameToLayer(layerName);
        camera.cullingMask |= (1 << LayerMask.NameToLayer(layerName));
    }

    // 动态移除物体对特定相机的可见性
    public void RemoveVisibleObject(Camera camera, GameObject obj, string defaultLayer = "Default")
    {
        if (camera == null || obj == null) return;

        obj.layer = LayerMask.NameToLayer(defaultLayer);
    }
}