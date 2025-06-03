using UnityEngine;
using UnityEngine.UI;

public class KeyImageSwitcher : MonoBehaviour
{
    [System.Serializable]
    public class KeyImagePair
    {
        public KeyCode key;           // 对应的按键
        public RawImage targetImage;  // 目标Raw Image组件
        public Texture normalTexture; // 普通状态的贴图
        public Texture pressedTexture;// 按下状态的贴图
    }

    public KeyImagePair[] keyImagePairs; // 在Inspector中设置按键和图片的对应关系

    void Update()
    {
        foreach (var pair in keyImagePairs)
        {
            if (Input.GetKeyDown(pair.key))
            {
                // 按键按下时切换到pressed贴图
                pair.targetImage.texture = pair.pressedTexture;
            }
            else if (Input.GetKeyUp(pair.key))
            {
                // 按键抬起时恢复normal贴图
                pair.targetImage.texture = pair.normalTexture;
            }
        }
    }
}