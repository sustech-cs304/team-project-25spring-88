using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages switching of RawImage textures based on keyboard input.
/// </summary>
public class KeyImageSwitcher : MonoBehaviour
{
    /// <summary>
    /// Represents a pair of a key and its associated RawImage textures.
    /// </summary>
    [System.Serializable]
    public class KeyImagePair
    {
        /// <summary>
        /// The key associated with the image switch.
        /// </summary>
        public KeyCode key;

        /// <summary>
        /// The target RawImage component to update.
        /// </summary>
        public RawImage targetImage;

        /// <summary>
        /// The texture to display when the key is not pressed.
        /// </summary>
        public Texture normalTexture;

        /// <summary>
        /// The texture to display when the key is pressed.
        /// </summary>
        public Texture pressedTexture;
    }

    /// <summary>
    /// Array of key-image pairs configured in the Inspector.
    /// </summary>
    public KeyImagePair[] keyImagePairs;

    /// <summary>
    /// Updates the texture of each RawImage based on the corresponding key's state, called once per frame.
    /// </summary>
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