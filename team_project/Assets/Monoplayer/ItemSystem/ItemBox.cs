using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VehicleBehaviour;

/// <summary>
/// A Unity script that manages item boxes in a racing game, applying random effects to the player car upon collision.
/// <para>
/// This script is attached to item box GameObjects. When a player car collides with the box, it applies a random effect
/// (e.g., SpeedUp, SlowDown) to the car and temporarily hides the box before respawning it. Effects are displayed via GameEffectUIManager.
/// </para>
/// </summary>
public class ItemBox : MonoBehaviour
{
    /// <summary>
    /// Enum defining the possible effects that can be applied to the player car.
    /// </summary>
    public enum EffectType { SpeedUp, SlowDown, Stop, ReverseControl, SpinOut }

    /// <summary>
    /// The time (in seconds) before the item box respawns after being collected.
    /// </summary>
    public float respawnTime = 3f;

    /// <summary>
    /// The multiplier applied to the car's boost force for the SpeedUp effect.
    /// </summary>
    public float speedMultiplier = 3f;

    /// <summary>
    /// Called when another collider enters the item box's trigger collider.
    /// <para>
    /// If the collider is tagged as "Player", applies a random effect to the car and starts the hide/respawn coroutine.
    /// </para>
    /// </summary>
    /// <param name="other">The collider that entered the trigger.</param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            EffectType randomEffect = (EffectType)Random.Range(0, System.Enum.GetValues(typeof(EffectType)).Length);

            WheelVehicle car = other.GetComponent<WheelVehicle>();
            if (car != null)
            {
                StartCoroutine(ApplyEffect(car, randomEffect));
            }

            StartCoroutine(HideAndRespawn());
        }
    }

    /// <summary>
    /// Coroutine that applies the specified effect to the player car and displays it via GameEffectUIManager.
    /// </summary>
    /// <param name="car">The WheelVehicle component of the player car.</param>
    /// <param name="effect">The effect to apply.</param>
    private IEnumerator ApplyEffect(WheelVehicle car, EffectType effect)
    {
        GameEffectUIManager.Instance?.Show(effect.ToString());
        switch (effect)
        {
            case EffectType.SpeedUp:
                Debug.Log("SpeedUp effect triggered.");
                float originalBoostForce = car.BoostForce;
                car.BoostForce *= speedMultiplier;

                Rigidbody rbSpeed = car.GetComponent<Rigidbody>();
                if (rbSpeed != null)
                {
                    rbSpeed.AddForce(car.transform.forward * 5000f, ForceMode.Impulse); // 瞬时爆发推力
                }

                yield return new WaitForSeconds(2f);
                car.BoostForce = originalBoostForce;
                break;

            case EffectType.SlowDown:
                Debug.Log("SlowDown effect triggered.");
                float originalDiff = car.DiffGearing;
                car.DiffGearing = originalDiff / speedMultiplier;

                // 强制降低速度
                Rigidbody rbSlow = car.GetComponent<Rigidbody>();
                if (rbSlow != null)
                {
                    rbSlow.velocity *= 0.5f;
                }

                yield return new WaitForSeconds(2f);
                car.DiffGearing = originalDiff;
                break;

            case EffectType.Stop:
                Debug.Log("Stop effect triggered.");
                Rigidbody rb = car.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 originalVelocity = rb.velocity;
                    rb.velocity = Vector3.zero;
                    yield return new WaitForSeconds(2f);
                    rb.velocity = originalVelocity;
                }
                break;

            case EffectType.ReverseControl:
                Debug.Log("ReverseControl effect triggered.");
                car.SetReverseControl(true);
                yield return new WaitForSeconds(2f);
                car.SetReverseControl(false);
                break;

            case EffectType.SpinOut:
                Debug.Log("SpinOut effect triggered.");
                car.TriggerSpinOut(2f, 1080f);
                break;
        }
        yield break; // 确保协程正确返回值
    }

    /// <summary>
    /// Coroutine that hides the item box and respawns it after a delay.
    /// </summary>
    private IEnumerator HideAndRespawn()
    {
        // 隐藏 Mesh 和触发 Collider
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;

        // 等待一段时间
        yield return new WaitForSeconds(respawnTime);

        // 恢复显示和触发
        GetComponent<MeshRenderer>().enabled = true;
        GetComponent<Collider>().enabled = true;
    }
}