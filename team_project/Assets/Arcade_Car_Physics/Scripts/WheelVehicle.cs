/*
 * This code is part of Arcade Car Physics for Unity by Saarg (2018)
 * 
 * This is distributed under the MIT Licence (see LICENSE.md for details)
 */

using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

#if MULTIOSCONTROLS
using MOSC;
#endif

[assembly: InternalsVisibleTo("VehicleBehaviour.Dots")]
namespace VehicleBehaviour
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(PlayerInput))] // 依赖 PlayerInput
    public class WheelVehicle : MonoBehaviour
    {
        [Header("Inputs")]
#if MULTIOSCONTROLS
        [SerializeField] PlayerNumber playerId;
#endif
        // If isPlayer is false inputs are ignored
        [SerializeField] bool isPlayer = true;
        public bool IsPlayer { get => isPlayer; set => isPlayer = value; }

        // 新的 Input System 变量
        private PlayerInput playerInput;
        private InputAction throttleAction;
        private InputAction brakeAction;
        private InputAction steerAction;
        private InputAction jumpAction;
        private InputAction driftAction;
        private InputAction boostAction;
        private InputAction handbrakeAction;

        /* Turn input curve: x real input, y value used */
        [SerializeField] AnimationCurve turnInputCurve = AnimationCurve.Linear(-1.0f, -1.0f, 1.0f, 1.0f);

        [Header("Speed Sensitive Steering")]
        [SerializeField] AnimationCurve speedSteerCurve = new AnimationCurve(
            new Keyframe(0, 1),    // 0km/h时100%转向
            new Keyframe(150, 0.25f) // 150km/h时25%转向
        );

        [Header("Wheels")]
        [SerializeField] WheelCollider[] driveWheel = new WheelCollider[0];
        public WheelCollider[] DriveWheel => driveWheel;
        [SerializeField] WheelCollider[] turnWheel = new WheelCollider[0];
        public WheelCollider[] TurnWheel => turnWheel;

        // Ground check
        bool isGrounded = false;
        int lastGroundCheck = 0;
        public bool IsGrounded
        {
            get
            {
                if (lastGroundCheck == Time.frameCount)
                    return isGrounded;

                lastGroundCheck = Time.frameCount;
                isGrounded = true;
                foreach (WheelCollider wheel in wheels)
                {
                    if (!wheel.gameObject.activeSelf || !wheel.isGrounded)
                        isGrounded = false;
                }
                return isGrounded;
            }
        }

        [Header("Behaviour")]
        [SerializeField] AnimationCurve motorTorque = new AnimationCurve(
            new Keyframe(0, 180),
            new Keyframe(50, 130),
            new Keyframe(120, 0));
        [Range(2, 16)]
        [SerializeField] float diffGearing = 4.0f;
        public float DiffGearing { get => diffGearing; set => diffGearing = value; }
        [SerializeField] float brakeForce = 1500.0f;
        public float BrakeForce { get => brakeForce; set => brakeForce = value; }
        [Range(0f, 50.0f)]
        [SerializeField] float steerAngle = 30.0f;
        public float SteerAngle { get => steerAngle; set => steerAngle = Mathf.Clamp(value, 0.0f, 50.0f); }
        [Range(0.001f, 1.0f)]
        [SerializeField] float steerSpeed = 0.2f;
        public float SteerSpeed { get => steerSpeed; set => steerSpeed = Mathf.Clamp(value, 0.001f, 1.0f); }
        [Range(1f, 1.5f)]
        [SerializeField] float jumpVel = 1.3f;
        public float JumpVel { get => jumpVel; set => jumpVel = Mathf.Clamp(value, 1.0f, 1.5f); }
        [Range(0.0f, 20f)]
        [SerializeField] float driftIntensity = 1f;
        public float DriftIntensity { get => driftIntensity; set => driftIntensity = Mathf.Clamp(value, 0.0f, 2.0f); }

        Vector3 spawnPosition;
        Quaternion spawnRotation;
        [SerializeField] Transform centerOfMass = null;
        [Range(0.5f, 30f)]
        [SerializeField] float downforce = 1.0f;
        public float Downforce { get => downforce; set => downforce = Mathf.Clamp(value, 0, 5); }

        float steering;
        public float Steering { get => steering; set => steering = Mathf.Clamp(value, -1f, 1f); }
        float throttle;
        public float Throttle { get => throttle; set => throttle = Mathf.Clamp(value, -1f, 1f); }
        [SerializeField] bool handbrake;
        public bool Handbrake { get => handbrake; set => handbrake = value; }
        [HideInInspector] public bool allowDrift = true;
        bool drift;
        public bool Drift { get => drift; set => drift = value; }
        [SerializeField] float speed = 0.0f;
        public float Speed => speed;

        [Header("Particles")]
        [SerializeField] ParticleSystem[] gasParticles = new ParticleSystem[0];

        [Header("Boost")]
        [HideInInspector] public bool allowBoost = true;
        [SerializeField] float maxBoost = 10f;
        public float MaxBoost { get => maxBoost; set => maxBoost = value; }
        [SerializeField] float boost = 10f;
        public float Boost { get => boost; set => boost = Mathf.Clamp(value, 0f, maxBoost); }
        [Range(0f, 1f)]
        [SerializeField] float boostRegen = 0.2f;
        public float BoostRegen { get => boostRegen; set => boostRegen = Mathf.Clamp01(value); }
        [SerializeField] float boostForce = 5000;
        public float BoostForce { get => boostForce; set => boostForce = value; }
        public bool boosting = false;
        public bool jumping = false;

        [SerializeField] ParticleSystem[] boostParticles = new ParticleSystem[0];
        [SerializeField] AudioClip boostClip = default;
        [SerializeField] AudioSource boostSource = default;

        Rigidbody rb = default;
        internal WheelCollider[] wheels = new WheelCollider[0];
        [Header("Waypoint Settings")]
        public List<Vector3> waypoints = new List<Vector3>();
        public float waypointInterval = 5f; // 基于距离的间隔
        public float angleThreshold = 30f; // 转向角度阈值（度）
        public LayerMask trackLayer; 
        public float waypointHeightOffset = 0.2f; // 路径点高度偏移
        public int waypointsmaxnumber = 100; // 最大路径点数量
        private Vector3 lastWaypoint;
        private Vector3 lastForward;

        [Header("Debug")]
        public bool showWaypoints = true; // 调试显示路径点
        public Color waypointColor = Color.red;
        public float waypointSize = 0.5f;
        void Start()
        {
#if MULTIOSCONTROLS
            Debug.Log("[ACP] Using MultiOSControls");
#endif

            playerInput = GetComponent<PlayerInput>();
            throttleAction = playerInput.actions["Throttle"];
            brakeAction = playerInput.actions["Brake"];
            steerAction = playerInput.actions["Steer"];
            jumpAction = playerInput.actions["Jump"];
            driftAction = playerInput.actions["Drift"];
            boostAction = playerInput.actions["Boost"];
            handbrakeAction = playerInput.actions["Handbrake"];

            if (boostClip != null)
            {
                boostSource.clip = boostClip;
            }

            boost = maxBoost;

            rb = GetComponent<Rigidbody>();
            spawnPosition = transform.position;
            spawnRotation = transform.rotation;

            if (rb != null && centerOfMass != null)
            {
                rb.centerOfMass = centerOfMass.localPosition;
            }

            wheels = GetComponentsInChildren<WheelCollider>();

            foreach (WheelCollider wheel in wheels)
            {
                wheel.motorTorque = 0.0001f;
            }
            lastWaypoint = transform.position;
            lastForward = transform.forward;
            waypoints.Add(AdjustWaypointHeight(lastWaypoint)); // 初始路径点
            visualParts = new Transform[5];
            visualParts[0] = transform.Find("Body");
            visualParts[1] = transform.Find("FLWheel");
            visualParts[2] = transform.Find("FRWheel");
            visualParts[3] = transform.Find("BLWheel");
            visualParts[4] = transform.Find("BRWheel");
            for (int i = 0; i < visualParts.Length; i++)
            {
                if (visualParts[i] == null)
                {
                    Debug.LogWarning($"[WheelVehicle] Missing visual part at index {i}. Check name or hierarchy.");
                }
            }
        }

        private Transform[] visualParts;

        private bool isSpinning = false;
        private float spinTimer = 0f;
        private float spinDuration = 0f;
        private float spinSpeed = 0f;
        private bool disableSteering = false;

        public void TriggerSpinOut(float duration = 2f, float speed = 1080f)
        {
            if (isSpinning) return;

            isSpinning = true;
            spinTimer = 0f;
            spinDuration = duration;
            spinSpeed = speed;

            disableSteering = true; // 禁止转向控制
        }


        private bool reverseControl = false;
        public void SetReverseControl(bool enable)
        {
            reverseControl = enable;
        }

        void Update()
        {
            foreach (ParticleSystem gasParticle in gasParticles)
            {
                gasParticle.Play();
                ParticleSystem.EmissionModule em = gasParticle.emission;
                em.rateOverTime = handbrake ? 0 : Mathf.Lerp(em.rateOverTime.constant, Mathf.Clamp(150.0f * throttle, 30.0f, 100.0f), 0.1f);
            }

            if (isSpinning && visualParts != null)
            {
                spinTimer += Time.deltaTime;
                if (spinTimer >= spinDuration)
                {
                    isSpinning = false;
                    disableSteering = false;

                    // ✅ 强制将所有视觉物体角度归正
                    foreach (var part in visualParts)
                    {
                        if (part != null)
                        {
                            part.rotation = transform.rotation;
                        }
                    }
                }
                else
                {
                    foreach (var part in visualParts)
                    {
                        if (part != null)
                        {
                            part.Rotate(Vector3.up * spinSpeed * Time.deltaTime, Space.Self);
                        }
                    }
                }
            }

            if (isPlayer && allowBoost) {
                boost += Time.deltaTime * boostRegen;
                if (boost > maxBoost) { boost = maxBoost; }
            }
        }

        void FixedUpdate()
        {
            // Measure current speed
            speed = rb.velocity.magnitude * 3.6f; // m/s to km/h
            float currentSpeed = Mathf.Abs(speed);
            float speedFactor = speedSteerCurve.Evaluate(currentSpeed);

            if (isPlayer)
            {
                // 油门 & 刹车
                throttle = throttleAction.ReadValue<float>() - brakeAction.ReadValue<float>();

                // 转向，速度越快转得越慢
                // steering = turnInputCurve.Evaluate(steerAction.ReadValue<float>()) * steerAngle;
                if (!disableSteering)
                {
                    steering = turnInputCurve.Evaluate(steerAction.ReadValue<float>())
                            * steerAngle
                            * speedFactor // 应用速度系数
                            * (reverseControl ? -1 : 1);
                }

                // 手刹
                handbrake = handbrakeAction.ReadValue<float>() > 0.5f;
                drift = driftAction.ReadValue<float>() > 0.5f && rb.velocity.sqrMagnitude > 100;
                jumping = jumpAction.ReadValue<float>() > 0.5f;
                boosting = boostAction.ReadValue<float>() > 0.5f;
            }

            // Direction
            foreach (WheelCollider wheel in turnWheel)
            {
                wheel.steerAngle = Mathf.Lerp(wheel.steerAngle, steering, steerSpeed);
            }

            foreach (WheelCollider wheel in wheels)
            {
                wheel.motorTorque = 0.0001f;
                wheel.brakeTorque = 0;
            }

            // Handbrake
            if (handbrake)
            {
                foreach (WheelCollider wheel in wheels)
                {
                    wheel.motorTorque = 0.0001f;
                    wheel.brakeTorque = brakeForce;
                }
            }
            else if (throttle != 0 && (Mathf.Abs(speed) < 25 || Mathf.Sign(speed) == Mathf.Sign(throttle)))
            {
                foreach (WheelCollider wheel in driveWheel)
                {
                    wheel.motorTorque = throttle * motorTorque.Evaluate(speed) * diffGearing / driveWheel.Length;
                }
            }
            else if (throttle != 0)
            {
                foreach (WheelCollider wheel in wheels)
                {
                    wheel.brakeTorque = Mathf.Abs(throttle) * brakeForce;
                }
            }

            // Jump
            if (jumping && isPlayer)
            {
                if (!IsGrounded)
                    return;
                rb.velocity += transform.up * jumpVel;
            }

            // Boost
            if (boosting && allowBoost && boost > 0.1f)
            {
                rb.AddForce(transform.forward * boostForce);
                boost -= Time.fixedDeltaTime;
                if (boost < 0f) { boost = 0f; }

                if (boostParticles.Length > 0 && !boostParticles[0].isPlaying)
                {
                    foreach (ParticleSystem boostParticle in boostParticles)
                    {
                        boostParticle.Play();
                    }
                }

                if (boostSource != null && !boostSource.isPlaying)
                {
                    boostSource.Play();
                }
            }
            else
            {
                if (boostParticles.Length > 0 && boostParticles[0].isPlaying)
                {
                    foreach (ParticleSystem boostParticle in boostParticles)
                    {
                        boostParticle.Stop();
                    }
                }

                if (boostSource != null && boostSource.isPlaying)
                {
                    boostSource.Stop();
                }
            }

            // Drift
            if (drift && allowDrift)
            {
                float rawSteerInput = turnInputCurve.Evaluate(steerAction.ReadValue<float>());
                Vector3 driftForce = -transform.right;
                driftForce.y = 0.0f;
                driftForce.Normalize();

                if (steering != 0)
                    driftForce *= rb.mass * speed / 7f * throttle * steering / steerAngle;
                Vector3 driftTorque = transform.up * 0.1f * steering / steerAngle;

                rb.AddForce(driftForce * driftIntensity, ForceMode.Force);
                rb.AddTorque(driftTorque * driftIntensity, ForceMode.VelocityChange);
            }

            // Downforce
            rb.AddForce(-transform.up * speed * downforce);
        // 路径点生成逻辑（修改部分）
            float angleDifference = Vector3.Angle(lastForward, transform.forward);
            float distance = Vector3.Distance(transform.position, lastWaypoint);

            // 满足以下任一条件时生成新路径点
            if (isPlayer && (distance > waypointInterval || angleDifference > angleThreshold))
            {
                Vector3 newWaypoint = transform.position;
                RaycastHit hit;
                
                // 向下投射射线确保路径点在赛道上
                if (Physics.Raycast(newWaypoint + Vector3.up * 2f, Vector3.down, out hit, 5f, trackLayer))
                {
                    newWaypoint = hit.point + Vector3.up * waypointHeightOffset;
                }
                
                waypoints.Add(AdjustWaypointHeight(newWaypoint));
                lastWaypoint = newWaypoint;
                lastForward = transform.forward;

                // 限制路径点数量
                if (waypoints.Count > waypointsmaxnumber)
                {
                    waypoints.RemoveAt(0);
                }
            }
        }

        public void ResetPos()
        {
            transform.position = spawnPosition;
            transform.rotation = spawnRotation;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        public void ToogleHandbrake(bool h)
        {
            handbrake = h;
        }
        // 调整路径点高度使其贴合赛道
        private Vector3 AdjustWaypointHeight(Vector3 position)
        {
            RaycastHit hit;
            if (Physics.Raycast(position + Vector3.up * 2f, Vector3.down, out hit, 5f, trackLayer))
            {
                return hit.point + Vector3.up * waypointHeightOffset;
            }
            return position;
        }// 调试绘制路径点
        void OnDrawGizmos()
        {
            if (!showWaypoints || waypoints == null || waypoints.Count == 0) return;
            
            Gizmos.color = waypointColor;
            for (int i = 0; i < waypoints.Count; i++)
            {
                Gizmos.DrawSphere(waypoints[i], waypointSize);
                if (i > 0)
                {
                    Gizmos.DrawLine(waypoints[i - 1], waypoints[i]);
                }
            }
        }
#if MULTIOSCONTROLS
        private static MultiOSControls _controls;
#endif

        private float GetInput(string input)
        {
#if MULTIOSCONTROLS
            return MultiOSControls.GetValue(input, playerId);
#else
            return Input.GetAxis(input);
#endif
        }
    }
}