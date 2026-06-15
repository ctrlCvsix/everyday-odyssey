using UnityEngine;

namespace EverydayOdyssey
{
    [RequireComponent(typeof(CharacterController))]
    public class TeacherAI : MonoBehaviour
    {
        [SerializeField] private string teacherName = "\u5bfc\u5e08";
        [SerializeField] private Transform[] patrolPoints;
        [SerializeField] private float patrolSpeed = 2.3f;
        [SerializeField] private float chaseSpeed = 3.5f;
        [SerializeField] private float detectionRadius = 12f;
        [SerializeField] private float catchDistance = 1.5f;
        [SerializeField] private CharacterMotionController characterAnimator;
        [SerializeField] private Renderer[] flashRenderers;
        [SerializeField] private Color flashColor = new Color(1f, 0.35f, 0.35f);
        [SerializeField] private float fallResetY = -6f;
        [SerializeField] private Transform stunIconRoot;

        private readonly Color[] originalColors = new Color[8];

        private CharacterController controller;
        private Transform player;
        private int patrolIndex;
        private float stunTimer;
        private float provokeTimer;
        private float touchCooldown;
        private float verticalVelocity;
        private float flashTimer;
        private string teacherNameKo = "\uad50\uc218";
        private Vector3 spawnPosition;
        private Transform stunIconFace;
        private bool isFallingDown;
        private float fallDownTimer;
        private Vector3 defeatStartPosition;
        private Quaternion defeatStartRotation;
        private Quaternion defeatTargetRotation;
        private GameObject hitBurst;

        public bool IsDefeated { get; private set; }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            CacheRendererColors();
            spawnPosition = transform.position;
            EnsureStunIcon();
        }

        private void Start()
        {
            PlayerController playerController = FindAnyObjectByType<PlayerController>();
            player = playerController != null ? playerController.transform : null;
        }

        private void Update()
        {
            if (player == null || GameManager.Instance == null || GameManager.Instance.IsGameOver)
            {
                characterAnimator?.SetLocomotion(0f);
                return;
            }

            if (IsDefeated)
            {
                UpdateDefeatAnimation();
                characterAnimator?.SetLocomotion(0f);
                return;
            }

            if (transform.position.y < fallResetY)
            {
                ResetToSpawn();
                return;
            }

            touchCooldown -= Time.deltaTime;

            if (stunTimer > 0f)
            {
                stunTimer -= Time.deltaTime;
                UpdateStunIcon(true);
                characterAnimator?.SetLocomotion(0f);
                return;
            }

            UpdateStunIcon(false);

            provokeTimer -= Time.deltaTime;
            UpdateFlash();

            Vector3 target = GetCurrentTarget();
            Vector3 planarOffset = target - transform.position;
            planarOffset.y = 0f;

            if (planarOffset.magnitude > 0.2f)
            {
                Vector3 moveDirection = planarOffset.normalized;
                float speed = IsChasing() ? chaseSpeed : patrolSpeed;

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(moveDirection),
                    Time.deltaTime * 7f);

                if (controller.isGrounded && verticalVelocity < 0f)
                {
                    verticalVelocity = -1f;
                }

                verticalVelocity += Physics.gravity.y * Time.deltaTime;
                Vector3 movement = moveDirection * speed;
                movement.y = verticalVelocity;
                controller.Move(movement * Time.deltaTime);
                characterAnimator?.SetLocomotion(Mathf.Clamp01(speed / chaseSpeed));
            }
            else
            {
                characterAnimator?.SetLocomotion(0f);
                if (!IsChasing() && patrolPoints.Length > 0)
                {
                    patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
                }
            }

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer <= catchDistance && touchCooldown <= 0f)
            {
                touchCooldown = 1.2f;
                GameManager.Instance.HandleTeacherCatch(teacherName, teacherNameKo);
            }
        }

        public void ApplyCodeHit(float duration)
        {
            if (IsDefeated)
            {
                return;
            }

            stunTimer = Mathf.Max(stunTimer, duration);
            provokeTimer = 5f;
            flashTimer = 0.18f;
            ApplyFlashColor();
            UpdateStunIcon(true);
            GameManager.Instance?.AudioBank?.PlayTeacherHit(transform.position);
            GameManager.Instance?.Announce(GameManager.Instance.CurrentLanguage == LanguageOption.Korean
                ? $"{teacherNameKo}\uc758 \ud589\ub3d9\uc744 \ucf54\ub4dc\ub85c \uc911\ub2e8\uc2dc\ucf30\uc2b5\ub2c8\ub2e4."
                : $"\u4f60\u7528\u4ee3\u7801\u653b\u51fb\u6253\u65ad\u4e86{teacherName}\u3002");
        }

        public void ApplyGunHit()
        {
            if (IsDefeated)
            {
                return;
            }

            IsDefeated = true;
            isFallingDown = true;
            fallDownTimer = 0f;
            defeatStartPosition = transform.position;
            defeatStartRotation = transform.rotation;
            defeatTargetRotation = Quaternion.Euler(90f, transform.eulerAngles.y, 0f);
            stunTimer = 0f;
            provokeTimer = 0f;
            touchCooldown = 999f;
            UpdateStunIcon(false);
            SpawnHitBurst();
            GameManager.Instance?.AudioBank?.PlayTeacherHit(transform.position);
            GameManager.Instance?.Announce(GameManager.Instance.CurrentLanguage == LanguageOption.Korean
                ? $"{teacherNameKo}\uac00 \uc644\uc804\ud788 \uc81c\uc555\ub418\uc5c8\uc2b5\ub2c8\ub2e4."
                : $"{teacherName}\u5df2\u88ab\u6c38\u4e45\u51fb\u5012\u3002");
        }

        public void Configure(string newNameZh, string newNameKo, Transform[] newPatrolPoints, CharacterMotionController animatorRig)
        {
            teacherName = newNameZh;
            teacherNameKo = newNameKo;
            patrolPoints = newPatrolPoints;
            characterAnimator = animatorRig;
            spawnPosition = transform.position;
        }

        public void SetFlashRenderers(Renderer[] renderers)
        {
            flashRenderers = renderers;
            CacheRendererColors();
        }

        private bool IsChasing()
        {
            if (player == null)
            {
                return false;
            }

            return provokeTimer > 0f || Vector3.Distance(transform.position, player.position) <= detectionRadius;
        }

        private Vector3 GetCurrentTarget()
        {
            if (IsChasing() && player != null)
            {
                return player.position;
            }

            if (patrolPoints != null && patrolPoints.Length > 0 && patrolPoints[patrolIndex] != null)
            {
                return patrolPoints[patrolIndex].position;
            }

            return transform.position;
        }

        private void CacheRendererColors()
        {
            if (flashRenderers == null)
            {
                return;
            }

            for (int i = 0; i < flashRenderers.Length && i < originalColors.Length; i++)
            {
                if (flashRenderers[i] != null)
                {
                    originalColors[i] = flashRenderers[i].material.color;
                }
            }
        }

        private void UpdateFlash()
        {
            if (flashTimer <= 0f)
            {
                return;
            }

            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0f)
            {
                RestoreFlashColor();
            }
        }

        private void ApplyFlashColor()
        {
            if (flashRenderers == null)
            {
                return;
            }

            for (int i = 0; i < flashRenderers.Length && i < originalColors.Length; i++)
            {
                if (flashRenderers[i] != null)
                {
                    flashRenderers[i].material.color = flashColor;
                }
            }
        }

        private void RestoreFlashColor()
        {
            if (flashRenderers == null)
            {
                return;
            }

            for (int i = 0; i < flashRenderers.Length && i < originalColors.Length; i++)
            {
                if (flashRenderers[i] != null)
                {
                    flashRenderers[i].material.color = originalColors[i];
                }
            }
        }

        private void ResetToSpawn()
        {
            controller.enabled = false;
            transform.position = spawnPosition;
            verticalVelocity = -1f;
            controller.enabled = true;
            characterAnimator?.SetLocomotion(0f);
        }

        private void EnsureStunIcon()
        {
            if (stunIconRoot != null)
            {
                return;
            }

            GameObject root = new GameObject("StunClockIcon");
            root.transform.SetParent(transform, false);
            root.transform.localPosition = new Vector3(0f, 2.15f, 0f);

            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "ClockRing";
            ring.transform.SetParent(root.transform, false);
            ring.transform.localPosition = Vector3.zero;
            ring.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            ring.transform.localScale = new Vector3(0.14f, 0.03f, 0.14f);
            ring.GetComponent<Renderer>().material.color = new Color(0.95f, 0.95f, 0.95f);
            Destroy(ring.GetComponent<Collider>());

            GameObject minute = GameObject.CreatePrimitive(PrimitiveType.Cube);
            minute.name = "MinuteHand";
            minute.transform.SetParent(root.transform, false);
            minute.transform.localPosition = new Vector3(0f, 0f, 0.05f);
            minute.transform.localScale = new Vector3(0.02f, 0.02f, 0.12f);
            minute.GetComponent<Renderer>().material.color = new Color(0.15f, 0.15f, 0.15f);
            Destroy(minute.GetComponent<Collider>());

            GameObject hour = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hour.name = "HourHand";
            hour.transform.SetParent(root.transform, false);
            hour.transform.localPosition = new Vector3(0.03f, 0f, 0.015f);
            hour.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
            hour.transform.localScale = new Vector3(0.02f, 0.02f, 0.08f);
            hour.GetComponent<Renderer>().material.color = new Color(0.15f, 0.15f, 0.15f);
            Destroy(hour.GetComponent<Collider>());

            stunIconRoot = root.transform;
            stunIconFace = ring.transform;
            stunIconRoot.gameObject.SetActive(false);
        }

        private void UpdateDefeatAnimation()
        {
            if (!isFallingDown)
            {
                return;
            }

            fallDownTimer += Time.deltaTime;
            float t = Mathf.Clamp01(fallDownTimer / 0.9f);
            float eased = Mathf.SmoothStep(0f, 1f, t);

            transform.rotation = Quaternion.Slerp(defeatStartRotation, defeatTargetRotation, eased);
            transform.position = Vector3.Lerp(defeatStartPosition, defeatStartPosition + new Vector3(0f, -0.72f, 0.36f), eased);

            if (t >= 1f)
            {
                isFallingDown = false;
                if (controller != null)
                {
                    controller.enabled = false;
                }
            }
        }

        private void SpawnHitBurst()
        {
            hitBurst = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hitBurst.name = "GunHitBurst";
            hitBurst.transform.SetParent(transform, false);
            hitBurst.transform.localPosition = new Vector3(0f, 1f, 0.2f);
            hitBurst.transform.localScale = Vector3.one * 0.18f;

            Renderer burstRenderer = hitBurst.GetComponent<Renderer>();
            burstRenderer.material = new Material(Shader.Find("Standard"));
            burstRenderer.material.color = new Color(1f, 0.72f, 0.18f, 0.9f);
            burstRenderer.material.EnableKeyword("_EMISSION");
            burstRenderer.material.SetColor("_EmissionColor", new Color(1f, 0.48f, 0.1f) * 2.2f);
            Destroy(hitBurst.GetComponent<Collider>());
            Destroy(hitBurst, 0.35f);
        }

        private void UpdateStunIcon(bool visible)
        {
            if (stunIconRoot == null)
            {
                return;
            }

            stunIconRoot.gameObject.SetActive(visible);
            if (!visible)
            {
                return;
            }

            if (Camera.main != null)
            {
                stunIconRoot.forward = Camera.main.transform.forward;
            }

            if (stunIconFace != null)
            {
                stunIconFace.Rotate(0f, 0f, 90f * Time.deltaTime, Space.Self);
            }
        }
    }
}
