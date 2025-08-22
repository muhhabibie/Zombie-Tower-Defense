using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LastBastion.Player
{
    public class PlayerController : MonoBehaviour
    {
        public float placeRange = 5f;
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

        [Header("Ground Check")]
        public Transform groundCheck;
        public float groundDistance = 0.3f;
        public LayerMask groundMask;
        public float gravity = -9.81f;

        [Header("Animation")]
        public Animator animator;   // Animator reference

        [Header("Idle Sit")]
        public float idleToSitTime = 5f; // detik sebelum duduk
        private float idleTimer = 0f;
        private float idleSitParam = 0f;  // untuk parameter Blend Tree

        private CharacterController controller;
        private Vector3 movementInput;
        private Vector3 lastAim = Vector3.forward;
        private float nextMeleeTime;
        private bool isDashing;
        private float nextDashTime;

        private Vector3 velocity;
        private bool isGrounded;

        // Input System
        private PlayerInput playerInput;
        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction meleeAction;
        private InputAction dashAction;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            playerInput = GetComponent<PlayerInput>();

            if (playerInput != null)
            {
                moveAction = playerInput.actions.FindAction("Move");
                lookAction = playerInput.actions.FindAction("Look");
                meleeAction = playerInput.actions.FindAction("Attack");
                dashAction = playerInput.actions.FindAction("Dash");
            }

            if (health == null) health = GetComponent<Health>();

            if (animator == null) animator = GetComponentInChildren<Animator>();
        }

        private void Update()
        {
            // ===== Ground Check =====
            if (groundCheck != null)
            {
                isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
            }
            else
            {
                isGrounded = controller.isGrounded;
            }

            if (isGrounded && velocity.y < 0)
                velocity.y = -2f;

            // ===== Movement Input =====
            Vector2 moveValue = Vector2.zero;

            if (moveAction != null)
                moveValue = moveAction.ReadValue<Vector2>();
            else
            {
                moveValue = new Vector2(
                    (Keyboard.current.dKey.isPressed ? 1 : 0) - (Keyboard.current.aKey.isPressed ? 1 : 0),
                    (Keyboard.current.wKey.isPressed ? 1 : 0) - (Keyboard.current.sKey.isPressed ? 1 : 0)
                );
            }

            movementInput = new Vector3(moveValue.x, 0f, moveValue.y);

            // ===== Look / Aim =====
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

            // ===== Apply Rotation =====
            if (lastAim.sqrMagnitude > 0.05f)
            {
                Quaternion targetRot = Quaternion.LookRotation(lastAim, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 15f);
            }

            // ===== Attack =====
            if ((meleeAction != null && meleeAction.WasPerformedThisFrame()) ||
                (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame))
            {
                TryMelee();
            }

            // ===== Dash =====
            if ((dashAction != null && dashAction.WasPerformedThisFrame()) ||
                (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame))
            {
                TryDash();
            }

            // ===== Movement =====
            float speed = isDashing ? dashSpeed : moveSpeed;
            Vector3 moveDir = transform.forward * movementInput.z + transform.right * movementInput.x;
            controller.Move(moveDir.normalized * speed * Time.deltaTime);

            // ===== Gravity =====
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);

            // ===== Animator Update =====
            if (animator != null)
            {
                // Forward/backward movement
                float forwardInput = movementInput.z;
                animator.SetFloat("MoveZ", forwardInput, 0f, Time.deltaTime);

                // ===== Idle → Sit Blend Tree =====
                if (movementInput.magnitude < 0.1f && !isDashing)
                {
                    idleTimer += Time.deltaTime;
                    idleSitParam = Mathf.Clamp01(idleTimer / idleToSitTime);
                }
                else
                {
                    // kalau sedang duduk & tiba-tiba bergerak → bangun
                    if (idleSitParam >= 1f)
                        animator.SetTrigger("SitWake");

                    idleTimer = 0f;
                    idleSitParam = 0f;
                }
                animator.SetFloat("IdleSit", 1f); // paksa cheer
                animator.SetFloat("IdleSit", idleSitParam, 0f, Time.deltaTime);
            }
        }

        private void TryMelee()
        {
            if (Time.time < nextMeleeTime) return;
            nextMeleeTime = Time.time + meleeCooldown;

            // kalau sedang duduk → bangun dulu sebelum attack
            if (idleSitParam >= 1f && animator != null)
                animator.SetTrigger("SitWake");

            Vector3 center = transform.position + lastAim * meleeRange * 0.5f;
            float radius = meleeRange * 0.6f;
            var hits = Physics.OverlapSphere(center, radius, enemyMask);
            foreach (var hit in hits)
            {
                var dmg = hit.GetComponent<IDamageable>();
                if (dmg != null)
                    dmg.TakeDamage(meleeDamage);
            }

            if (animator != null)
                animator.SetTrigger("Attack");
        }

        private void TryDash()
        {
            if (Time.time < nextDashTime) return;

            // kalau sedang duduk → bangun dulu sebelum dash
            if (idleSitParam >= 1f && animator != null)
                animator.SetTrigger("SitWake");

            StartCoroutine(DashRoutine());
        }

        private IEnumerator DashRoutine()
        {
            nextDashTime = Time.time + dashCooldown;
            isDashing = true;

            if (animator != null)
                animator.SetTrigger("Dash");

            yield return new WaitForSeconds(dashDuration);
            isDashing = false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Vector3 center = Application.isPlaying
                ? transform.position + lastAim * meleeRange * 0.5f
                : transform.position + Vector3.forward * meleeRange * 0.5f;
            Gizmos.DrawWireSphere(center, meleeRange * 0.6f);

            if (groundCheck != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
            }
        }
    }
}
