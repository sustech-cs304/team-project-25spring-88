using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VehicleBehaviour;

public class ItemBox : MonoBehaviour
{
    //public enum EffectType { SpeedUp, SlowDown, Stop, ReverseControl, SpinOut }
    public enum EffectType { SpinOut }

    public float respawnTime = 3f;

    public float speedMultiplier = 3f;

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

    private IEnumerator ApplyEffect(WheelVehicle car, EffectType effect)
    {
        switch (effect)
        {
            //case EffectType.SpeedUp:
            //    Debug.Log("SpeedUp effect triggered.");
            //    float originalBoostForce = car.BoostForce;
            //    car.BoostForce *= speedMultiplier;

            //    Rigidbody rbSpeed = car.GetComponent<Rigidbody>();
            //    if (rbSpeed != null)
            //    {
            //        rbSpeed.AddForce(car.transform.forward * 5000f, ForceMode.Impulse); // 瞬间爆发加速
            //    }

            //    yield return new WaitForSeconds(3f);
            //    car.BoostForce = originalBoostForce;
            //    break;

            //case EffectType.SlowDown:
            //    Debug.Log("SlowDown effect triggered.");
            //    float originalDiff = car.DiffGearing;
            //    car.DiffGearing = originalDiff / speedMultiplier;

            //    // 强制拖慢速度
            //    Rigidbody rbSlow = car.GetComponent<Rigidbody>();
            //    if (rbSlow != null)
            //    {
            //        rbSlow.velocity *= 0.5f;
            //    }

            //    yield return new WaitForSeconds(3f);
            //    car.DiffGearing = originalDiff;
            //    break;

            //case EffectType.Stop:
            //    Debug.Log("Stop effect triggered.");
            //    Rigidbody rb = car.GetComponent<Rigidbody>();
            //    if (rb != null)
            //    {
            //        Vector3 originalVelocity = rb.velocity;
            //        rb.velocity = Vector3.zero;
            //        yield return new WaitForSeconds(4f);
            //        rb.velocity = originalVelocity;
            //    }
            //    break;

            //case EffectType.ReverseControl:
            //    Debug.Log("ReverseControl effect triggered.");
            //    car.SetReverseControl(true);
            //    yield return new WaitForSeconds(3f);
            //    car.SetReverseControl(false);
            //    break;

            case EffectType.SpinOut:
                Debug.Log("SpinOut effect triggered.");
                car.TriggerSpinOut(2f, 1080f);
                break;
        }
        yield break; // 保证所有路径都有返回
    }

    private IEnumerator HideAndRespawn()
    {
        // 隐藏 Mesh 和禁用 Collider
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;

        // 等待一段时间
        yield return new WaitForSeconds(respawnTime);

        // 恢复显示与触发
        GetComponent<MeshRenderer>().enabled = true;
        GetComponent<Collider>().enabled = true;
    }
}
