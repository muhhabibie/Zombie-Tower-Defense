using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;

namespace LastBastion.Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 6f;
        public float dashSpeed = 12f;
        public float dashDuration = 0.15f;
        public float dashCooldown = 1.5f;

        [Header("Animation")]
        public Animator animator;

        [Header("Health")]
        public Health health;

        [Header("Ground Check")]
        public Transform groundCheck;
        public float groundDistance = 0.3f;
        public LayerMask groundMask;

        [Header("Coins")]
        public int coinCount = 0;
        public TextMeshProUGUI coinTextUI;

        private CharacterController controller;
        private Vector3 velocity;
        private bool isGrounded;
        private Vector3 moveDir;
        private Vector3 lastMoveDir;

        private bool isDashing;
        private float nextDashTime;

        // Input System
        private PlayerInput playerInput;
        private InputAction moveAction;
        private InputAction dashAction;

        // NEW: terima efek slow dari aura musuh
        private DebuffReceiver debuff; // <--- pastikan komponen ini terpasang di Player

        private void Start()
        {
            UpdateCoinUI();
        }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            playerInput = GetComponent<PlayerInput>();

            if (playerInput != null)
            {
                moveAction = playerInput.actions.FindAction("Move");
                dashAction = playerInput.actions.FindAction("Dash");
            }

            if (health == null) health = GetComponent<Health>();
            if (animator == null) animator = GetComponentInChildren<Animator>();

            // NEW: cache DebuffReceiver
            debuff = GetComponent<DebuffReceiver>();

            GameObject coinTextObject = GameObject.Find("Coin Text");
            if (coinTextObject != null)
            {
                coinTextUI = coinTextObject.GetComponent<TextMeshProUGUI>();
            }
            if (coinTextUI == null)
            {
                Debug.LogError("Tidak bisa menemukan komponen TextMeshProUGUI pada objek bernama 'Coin Text'!");
            }
        }

        private void Update()
        {
            HandleMovement();
        }

        private void HandleMovement()
        {
            // === Ground Check ===
            if (groundCheck != null)
                isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
            else
                isGrounded = controller.isGrounded;

            if (isGrounded && velocity.y < 0)
                velocity.y = -2f;

            // === Ambil input WASD ===
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

            // === Gerakan relatif kamera ===
            Vector3 forward = Camera.main.transform.forward;
            Vector3 right = Camera.main.transform.right;
            forward.y = 0; right.y = 0;
            forward.Normalize(); right.Normalize();

            moveDir = (forward * moveValue.y + right * moveValue.x).normalized;

            if (moveDir.sqrMagnitude > 0.01f)
                lastMoveDir = moveDir;

            // === Dash ===
            if ((dashAction != null && dashAction.WasPerformedThisFrame()) ||
                (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame))
            {
                TryDash();
            }

            // NEW: terapkan slow ke move speed (0..1). Clamp biar tidak nol total.
            float slowMul = debuff ? Mathf.Clamp(debuff.MoveSpeedMul, 0.05f, 1f) : 1f;

            // Jika ingin dash TETAP terpengaruh slow, gunakan baris ini:
            float baseSpeed = isDashing ? dashSpeed : moveSpeed;
            float speed = baseSpeed * slowMul;

            // Jika ingin dash TIDAK terpengaruh slow, ganti dua baris di atas menjadi:
            // float speed = isDashing ? dashSpeed : moveSpeed * slowMul;

            // === Movement ===
            Vector3 finalMove = moveDir * speed;
            controller.Move(finalMove * Time.deltaTime);

            // === Gravity ===
            velocity.y += Physics.gravity.y * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);

            // === Rotasi ke arah gerakan ===
            if (lastMoveDir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(lastMoveDir, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 15f);
            }

            // === Animator ===
            if (animator != null)
                animator.SetFloat("MoveZ", moveDir.magnitude, 0.1f, Time.deltaTime);
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
            if (animator != null) animator.SetTrigger("Dash");
            yield return new WaitForSeconds(dashDuration);
            isDashing = false;
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheck != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
            }
        }

        // =========================
        // PICKUP COINS
        // =========================
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Coin"))
            {
                coinCount++;
                Debug.Log($"Coin collected! Total coins: {coinCount}");
                UpdateCoinUI();
                Destroy(other.gameObject);
            }
        }

        private void UpdateCoinUI()
        {
            if (coinTextUI != null)
            {
                coinTextUI.text = coinCount.ToString();
            }
        }
    }
}
