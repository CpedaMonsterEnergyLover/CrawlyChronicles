using System;
using System.Collections;
using Camera;
using Gameplay.AI.Locators;
using UnityEngine;
using Util;

namespace Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Movement : MonoBehaviour, ILocatorTarget
    {
        private static Rigidbody2D rb;
        private Coroutine dashRoutine;
        public static float MoveSpeedAmplifier { get; set; } = 1;

        public static Vector2 Position => rb.position;
        public static float Rotation => rb.rotation;
        public static Transform Transform => rb.transform;
        
        private void Awake() => rb = GetComponent<Rigidbody2D>();

        private void Update()
        {
            Vector2 mousePos = MainCamera.WorldMousePos;
            rb.rotation = PhysicsUtility.RotateTowardsPosition(rb.position, rb.rotation, mousePos, Manager.PlayerStats.RotationSpeed);

            if (Input.GetMouseButton(0))
            {
                rb.velocity = transform.up * Manager.PlayerStats.MovementSpeed * MoveSpeedAmplifier;
            }
        }



        public void Knockback(Vector2 attacker, float duration, float speed)
        {
            StartCoroutine(KnockbackRoutine(attacker, duration, speed));
        }
        
        public bool Dash(Vector2 position, float duration, Action onEnd)
        {
            if (dashRoutine is null)
            {
                float direction = PhysicsUtility.RotateTowardsPosition(rb.position, rb.rotation,position, 360);
                if(Mathf.Abs(rb.rotation - direction) < 30f) 
                    dashRoutine = StartCoroutine(StraightDashRoutine(position, duration, onEnd));
                else
                    dashRoutine = StartCoroutine(SideDashRoutine(position, direction, duration, onEnd));
                
                StopCoroutine(nameof(KnockbackRoutine));
                return true;
            }

            return false;
        }

        public bool ComboDash(float duration, float speed, Action onEnd)
        {
            if (dashRoutine is null)
            {
                dashRoutine = StartCoroutine(ComboDashRoutine(duration, speed, onEnd));
                return true;
            }

            return false;
        }
        

        
        private IEnumerator SideDashRoutine(Vector2 position, float direction, float duration, Action onEnd)
        {
            rb.velocity = (position - rb.position).normalized * 
                          Mathf.Clamp(Manager.PlayerStats.MovementSpeed * 3 * MoveSpeedAmplifier, 0, 20);
            float t = 0f;
            enabled = false;

            while (t < duration && Mathf.Abs(rb.rotation - direction) > 10f)
            {
                rb.rotation = PhysicsUtility.RotateTowardsPosition(rb.position, rb.rotation, position, 8);
                t += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForEndOfFrame();
            enabled = true;
            dashRoutine = null;
            onEnd();
        }
        
        private IEnumerator StraightDashRoutine(Vector2 position, float duration, Action onEnd)
        {
            rb.velocity = (position - rb.position).normalized * 
                          Mathf.Clamp(Manager.PlayerStats.MovementSpeed * 3 * MoveSpeedAmplifier, 0, 20);
            float t = 0f;
            enabled = false;

            while (t < duration)
            {
                t += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForEndOfFrame();
            enabled = true;
            dashRoutine = null;
            onEnd();
        }

        private IEnumerator ComboDashRoutine(float duration, float speed, Action onEnd)
        {
            enabled = false;
            rb.angularVelocity = speed;
            float t = 0;

            while (t < duration)
            {
                rb.rotation += speed;
                rb.velocity = ((Vector2) MainCamera.WorldMousePos - rb.position).normalized * 
                              Mathf.Clamp(Manager.PlayerStats.MovementSpeed * MoveSpeedAmplifier, 0, 20);
                t += Time.deltaTime;
                yield return null;
            }
            
            enabled = true;
            dashRoutine = null;
            rb.angularVelocity = 0;
            rb.rotation = PhysicsUtility.RotateTowardsPosition(rb.position, rb.rotation,MainCamera.WorldMousePos, 360);
            onEnd();
        }

        private IEnumerator KnockbackRoutine(Vector2 attacker, float duration, float speed)
        {
            enabled = false;
            rb.velocity = PhysicsUtility.GetKnockbackVelocity(rb.position, attacker, speed);
            yield return new WaitForSeconds(duration);
            enabled = true;
        }
    }
}