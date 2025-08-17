using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LastBastion.Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 6f;
        public float dashSpeed = 12f;
        public float dashDuration = 0.15f;
        public float dashCooldown = 1.5f;

        [Header("Combat")]
        public float meleeRange = 1.2f;
        public int meleeDamage = 20;
        public float meleeCooldown = 0.5f;
        public LayerMask enemyMask;

        [Header("Health")]
        public Health health;

        private Rigidbody rb;
        private Vector3 movementInput;
        private Vector3 lastAim = Vector3.forward;
        private float nextMeleeTime;
        private bool isDashing;
        private float nextDashTime;

        // Input System
        private PlayerInput playerInput;
        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction meleeAction;
        private InputAction dashAction;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true; // biar nggak miring

            playerInput = GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                moveAction = playerInput.actions.FindAction("Move");
                lookAction = playerInput.actions.FindAction("Look");
                meleeAction = playerInput.actions.FindAction("Attack");
                dashAction = playerInput.actions.FindAction("Dash");
            }

            if (health == null) health = GetComponent<Health>();
        }

        private void Update()
        {
            // ===== Movement Input =====
            Vector2 moveValue = Vector2.zero;

            if (moveAction != null)
            {
                moveValue = moveAction.ReadValue<Vector2>();
            }
            else
            {
                // PC fallback
                moveValue = new Vector2(
                    (Keyboard.current.dKey.isPressed ? 1 : 0) - (Keyboard.current.aKey.isPressed ? 1 : 0),
                    (Keyboard.current.wKey.isPressed ? 1 : 0) - (Keyboard.current.sKey.isPressed ? 1 : 0)
                );

                // Android fallback
                if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
                {
                    moveValue = Touchscreen.current.primaryTouch.delta.ReadValue().normalized;
                }
            }

            movementInput = new Vector3(moveValue.x, 0f, moveValue.y);

            // ===== Aim =====
            Vector2 lookValue = Vector2.zero;

            if (lookAction != null)
            {
                lookValue = lookAction.ReadValue<Vector2>();
            }

            // PC mouse aim
            if (Mouse.current != null)
            {
                Vector3 mousePos = Mouse.current.position.ReadValue();
                Ray ray = Camera.main.ScreenPointToRay(mousePos);
                Plane plane = new Plane(Vector3.up, Vector3.zero);
                if (plane.Raycast(ray, out float dist))
                {
                    Vector3 hitPoint = ray.GetPoint(dist);
                    Vector3 dir = hitPoint - transform.position;
                    dir.y = 0;
                    if (dir.sqrMagnitude > 0.01f)
                        lastAim = dir.normalized;
                }
            }

            // Android touch aim
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            {
                Vector2 touchDelta = Touchscreen.current.primaryTouch.delta.ReadValue();
                if (touchDelta.sqrMagnitude > 0.01f)
                    lastAim = new Vector3(touchDelta.x, 0, touchDelta.y).normalized;
            }

            // ===== Apply Rotation =====
            if (lastAim.sqrMagnitude > 0.05f)
            {
                Quaternion targetRot = Quaternion.LookRotation(lastAim, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 15f);
            }

            // ===== Attack =====
            if ((meleeAction != null && meleeAction.WasPerformedThisFrame()) ||
                (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
                (Touchscreen.current != null && Touchscreen.current.primaryTouch.tapCount.ReadValue() > 0))
            {
                TryMelee();
            }

            // ===== Dash =====
            if ((dashAction != null && dashAction.WasPerformedThisFrame()) ||
                (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame))
            {
                TryDash();
            }
        }

        private void FixedUpdate()
        {
            float speed = isDashing ? dashSpeed : moveSpeed;

            // Gerakan relatif terhadap rotasi player
            Vector3 moveDir = transform.forward * movementInput.z + transform.right * movementInput.x;
            moveDir.y = 0f;

            rb.linearVelocity = moveDir.normalized * speed;
        }

        private void TryMelee()
        {
            if (Time.time < nextMeleeTime) return;
            nextMeleeTime = Time.time + meleeCooldown;

            Vector3 center = transform.position + lastAim * meleeRange * 0.5f;
            float radius = meleeRange * 0.6f;
            var hits = Physics.OverlapSphere(center, radius, enemyMask);
            foreach (var hit in hits)
            {
                var dmg = hit.GetComponent<IDamageable>();
                if (dmg != null)
                    dmg.TakeDamage(meleeDamage);
            }
        }

        private void TryDash()
        {
            if (Time.time < nextDashTime) return;
            StartCoroutine(DashRoutine());
        }

        private IEnumerator DashRoutine()
        {
            nextDashTime = Time.time + dashCooldown;
            isDashing = true;
            yield return new WaitForSeconds(dashDuration);
            isDashing = false;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Vector3 center = Application.isPlaying
                ? transform.position + lastAim * meleeRange * 0.5f
                : transform.position + Vector3.forward * meleeRange * 0.5f;
            Gizmos.DrawWireSphere(center, meleeRange * 0.6f);
        }
#endif
    }
}
