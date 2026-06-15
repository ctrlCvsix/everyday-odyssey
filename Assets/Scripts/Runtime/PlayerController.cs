using UnityEngine;
using UnityEngine.EventSystems;

namespace EverydayOdyssey
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        private enum WeaponMode
        {
            Laptop,
            AK47
        }

        [SerializeField] private float moveSpeed = 6.5f;
        [SerializeField] private float sprintSpeed = 11f;
        [SerializeField] private float jumpHeight = 1.6f;
        [SerializeField] private float gravity = -25f;
        [SerializeField] private float mouseSensitivity = 2.2f;
        [SerializeField] private float attackCooldown = 0.4f;
        [SerializeField] private float rifleCooldown = 0.18f;
        [SerializeField] private float rifleRange = 120f;
        [SerializeField] private float fallRespawnY = -6f;
        [SerializeField] private float thirdPersonCameraRadius = 0.22f;
        [SerializeField] private float minThirdPersonDistance = 0.9f;
        [SerializeField] private float aimAssistRadius = 1.2f;
        [SerializeField] private float aimAssistRange = 70f;
        [SerializeField] private float maxStamina = 5f;
        [SerializeField] private float staminaRecoveryPerSecond = 1.35f;
        [SerializeField] private float staminaDrainPerSecond = 1.1f;
        [SerializeField] private Transform firstPersonMount;
        [SerializeField] private Transform thirdPersonMount;
        [SerializeField] private Transform attackOrigin;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private CharacterMotionController characterAnimator;

        private CharacterController controller;
        private Renderer[] bodyRenderers;
        private Vector3 safeGroundPosition;
        private float verticalVelocity;
        private float pitch;
        private float yaw;
        private float attackTimer;
        private float footstepTimer;
        private float stamina;
        private bool thirdPerson = true;
        private WeaponMode weaponMode = WeaponMode.Laptop;
        private PlayerInteractable currentInteractable;

        private Transform firstPersonWeaponMount;
        private Transform thirdPersonWeaponMount;
        private Transform firstPersonDeviceMount;
        private Transform thirdPersonDeviceMount;
        private Vector3 firstPersonWeaponBasePosition;
        private Vector3 firstPersonDeviceBasePosition;
        private Vector3 thirdPersonWeaponBasePosition;
        private Vector3 thirdPersonDeviceBasePosition;
        private GameObject firstPersonRifleVisual;
        private GameObject thirdPersonRifleVisual;
        private GameObject firstPersonLaptopVisual;
        private GameObject thirdPersonLaptopVisual;
        private GameObject firstPersonLegsVisual;
        private float weaponSwitchTimer;
        private Texture2D crosshairTexture;

        public float StaminaNormalized => maxStamina <= 0.01f ? 0f : Mathf.Clamp01(stamina / maxStamina);

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            bodyRenderers = GetComponentsInChildren<Renderer>(true);
            safeGroundPosition = transform.position;
            stamina = maxStamina;
            SetCursorState(false);
        }

        private void Update()
        {
            if (GameManager.Instance != null && (GameManager.Instance.IsGameOver || !GameManager.Instance.GameplayActive))
            {
                return;
            }

            Look();
            Move();
            HandleViewToggle();
            HandleWeaponToggle();
            HandleAttack();
            HandleInteraction();
            HandleFailsafeRespawn();
            UpdateWeaponSwitchAnimation();
            ApplyViewState();
        }

        private void OnGUI()
        {
            if (GameManager.Instance != null && !GameManager.Instance.GameplayActive)
            {
                return;
            }

            EnsureCrosshairTexture();
            float size = weaponMode == WeaponMode.AK47 ? 18f : 24f;
            float x = (Screen.width - size) * 0.5f;
            float y = (Screen.height - size) * 0.5f;
            GUI.DrawTexture(new Rect(x, y, size, size), crosshairTexture);
        }

        public void Configure(Camera sceneCamera, Transform firstPersonPoint, Transform thirdPersonPoint, Transform firePoint, CharacterMotionController animatorRig)
        {
            playerCamera = sceneCamera;
            firstPersonMount = firstPersonPoint;
            thirdPersonMount = thirdPersonPoint;
            attackOrigin = firePoint;
            characterAnimator = animatorRig;
            bodyRenderers = GetComponentsInChildren<Renderer>(true);
            EnsureViewModels();
            ApplyViewState();
        }

        public void SetCursorState(bool unlocked)
        {
            Cursor.lockState = unlocked ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = unlocked;
        }

        public void RespawnToSafePoint(string message)
        {
            controller.enabled = false;
            transform.position = safeGroundPosition + Vector3.up * 0.15f;
            verticalVelocity = -2f;
            controller.enabled = true;
            GameManager.Instance?.Announce(message);
        }

        private void Look()
        {
            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, -55f, 70f);
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);

            if (playerCamera == null)
            {
                return;
            }

            Transform target = thirdPerson ? thirdPersonMount : firstPersonMount;
            if (target == null)
            {
                return;
            }

            playerCamera.transform.position = thirdPerson ? ResolveThirdPersonCameraPosition(target.position) : target.position;
            playerCamera.transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        private void Move()
        {
            Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;
            bool sprinting = Input.GetKey(KeyCode.LeftShift) && input.sqrMagnitude > 0.01f && stamina > 0.05f;
            float activeSpeed = sprinting ? sprintSpeed : moveSpeed;
            Vector3 move = transform.TransformDirection(input) * activeSpeed;

            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            if (Input.GetKeyDown(KeyCode.Space) && controller.isGrounded)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            verticalVelocity += gravity * Time.deltaTime;
            move.y = verticalVelocity;
            controller.Move(move * Time.deltaTime);
            characterAnimator?.SetLocomotion(Mathf.Clamp01(activeSpeed / sprintSpeed) * input.magnitude);
            HandleFootsteps(input.magnitude);
            stamina = sprinting
                ? Mathf.Max(0f, stamina - staminaDrainPerSecond * Time.deltaTime)
                : Mathf.Min(maxStamina, stamina + staminaRecoveryPerSecond * Time.deltaTime);

            if (controller.isGrounded && transform.position.y > -1f)
            {
                safeGroundPosition = transform.position;
            }
        }

        private void HandleViewToggle()
        {
            if (Input.GetKeyDown(KeyCode.V))
            {
                thirdPerson = !thirdPerson;
                ApplyViewState();
            }
        }

        private void HandleWeaponToggle()
        {
            if (!Input.GetKeyDown(KeyCode.F10))
            {
                return;
            }

            weaponMode = weaponMode == WeaponMode.Laptop ? WeaponMode.AK47 : WeaponMode.Laptop;
            weaponSwitchTimer = 0.45f;
            GameManager.Instance?.AudioBank?.PlayWeaponSwitch(transform.position);
            ApplyViewState();
            GameManager.Instance?.Announce(weaponMode == WeaponMode.AK47 ? "F10 hidden AK mode" : "Laptop mode");
        }

        private void HandleAttack()
        {
            attackTimer -= Time.deltaTime;
            bool pointerOverUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            if (!Input.GetMouseButtonDown(0) || attackTimer > 0f || pointerOverUi)
            {
                return;
            }

            Vector3 origin = attackOrigin != null ? attackOrigin.position : transform.position + transform.forward + Vector3.up;
            Vector3 direction = ResolveAttackDirection();
            attackTimer = weaponMode == WeaponMode.AK47 ? rifleCooldown : attackCooldown;

            if (weaponMode == WeaponMode.AK47)
            {
                FireRifle(origin, direction);
            }
            else
            {
                GameManager.Instance?.AudioBank?.PlayLaptopAttack(origin);
                CodeProjectile.Spawn(origin, direction);
            }

            characterAnimator?.TriggerAttack();
        }

        private void HandleInteraction()
        {
            currentInteractable = FindNearestInteractable();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetPrompt(currentInteractable != null
                    ? $"{currentInteractable.GetPromptText(GameManager.Instance.CurrentLanguage)}  [E]"
                    : GameManager.Instance.GetControlsPrompt());
            }

            if (currentInteractable != null && Input.GetKeyDown(KeyCode.E))
            {
                currentInteractable.Interact(this);
            }
        }

        private PlayerInteractable FindNearestInteractable()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, 2.6f);
            float bestDistance = float.MaxValue;
            PlayerInteractable best = null;

            foreach (Collider hit in hits)
            {
                PlayerInteractable interactable = hit.GetComponentInParent<PlayerInteractable>();
                if (interactable == null || !interactable.CanInteract(this))
                {
                    continue;
                }

                float distance = Vector3.Distance(transform.position, interactable.transform.position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = interactable;
                }
            }

            return best;
        }

        private void HandleFootsteps(float movementAmount)
        {
            footstepTimer -= Time.deltaTime;
            if (movementAmount < 0.2f || !controller.isGrounded || footstepTimer > 0f)
            {
                return;
            }

            footstepTimer = 0.38f;
            bool onGrass = transform.position.z > 4f && Mathf.Abs(transform.position.x) > 7f;
            GameManager.Instance?.AudioBank?.PlayFootstep(transform.position, onGrass);
        }

        private void HandleFailsafeRespawn()
        {
            if (transform.position.y < fallRespawnY)
            {
                RespawnToSafePoint("Fall protection triggered.");
            }
        }

        private Vector3 ResolveAttackDirection()
        {
            if (playerCamera == null)
            {
                return transform.forward;
            }

            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            RaycastHit[] hits = Physics.SphereCastAll(ray, aimAssistRadius, aimAssistRange, ~0, QueryTriggerInteraction.Collide);
            TeacherAI bestTeacher = null;
            float bestDistance = float.MaxValue;

            foreach (RaycastHit hit in hits)
            {
                TeacherAI teacher = hit.collider != null ? hit.collider.GetComponentInParent<TeacherAI>() : null;
                if (teacher != null && hit.distance < bestDistance)
                {
                    bestTeacher = teacher;
                    bestDistance = hit.distance;
                }
            }

            if (bestTeacher != null)
            {
                Vector3 origin = attackOrigin != null ? attackOrigin.position : transform.position + Vector3.up;
                return ((bestTeacher.transform.position + Vector3.up * 1.1f) - origin).normalized;
            }

            return playerCamera.transform.forward;
        }

        private void FireRifle(Vector3 origin, Vector3 direction)
        {
            GameManager.Instance?.AudioBank?.PlayGunshot(origin);
            Vector3 end = origin + direction * rifleRange;
            if (Physics.Raycast(origin, direction, out RaycastHit hit, rifleRange, ~0, QueryTriggerInteraction.Collide))
            {
                end = hit.point;
                hit.collider.GetComponentInParent<TeacherAI>()?.ApplyGunHit();
            }

            SpawnTracer(origin, end, new Color(1f, 0.85f, 0.2f), 0.08f);
        }

        private Vector3 ResolveThirdPersonCameraPosition(Vector3 desiredPosition)
        {
            if (firstPersonMount == null)
            {
                return desiredPosition;
            }

            Vector3 origin = firstPersonMount.position;
            Vector3 direction = desiredPosition - origin;
            float distance = direction.magnitude;
            if (distance <= 0.01f)
            {
                return desiredPosition;
            }

            direction /= distance;
            RaycastHit[] hits = Physics.SphereCastAll(origin, thirdPersonCameraRadius, direction, distance);
            float bestDistance = distance;

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider != null && !hit.collider.transform.IsChildOf(transform))
                {
                    bestDistance = Mathf.Min(bestDistance, Mathf.Max(minThirdPersonDistance, hit.distance - 0.08f));
                }
            }

            return origin + direction * bestDistance;
        }

        private void EnsureViewModels()
        {
            if (playerCamera != null)
            {
            firstPersonWeaponMount = CreateMount(playerCamera.transform, "FirstPersonWeaponMount", new Vector3(0.34f, -0.3f, 0.78f), Quaternion.identity);
                firstPersonDeviceMount = CreateMount(playerCamera.transform, "FirstPersonDeviceMount", new Vector3(-0.16f, -0.25f, 0.68f), Quaternion.Euler(10f, -8f, -4f));
                firstPersonLegsVisual = BuildFirstPersonLegs(playerCamera.transform);
            }

            thirdPersonWeaponMount = CreateMount(transform, "ThirdPersonWeaponMount", new Vector3(0.26f, 1.08f, 0.82f), Quaternion.identity);
            thirdPersonDeviceMount = CreateMount(transform, "ThirdPersonDeviceMount", new Vector3(-0.24f, 1.02f, 0.48f), Quaternion.Euler(10f, -8f, -6f));

            Material metal = new Material(Shader.Find("Standard")) { color = new Color(0.09f, 0.09f, 0.09f) };
            Material wood = new Material(Shader.Find("Standard")) { color = new Color(0.42f, 0.24f, 0.1f) };
            Material shell = new Material(Shader.Find("Standard")) { color = new Color(0.2f, 0.22f, 0.26f) };
            Material screen = new Material(Shader.Find("Standard")) { color = new Color(0.08f, 0.82f, 0.94f) };
            screen.EnableKeyword("_EMISSION");
            screen.SetColor("_EmissionColor", new Color(0.08f, 0.82f, 0.94f) * 1.4f);

            firstPersonRifleVisual = BuildRifleVisual("FirstPersonAK47", firstPersonWeaponMount, Vector3.one * 1.9f, metal, wood);
            thirdPersonRifleVisual = BuildRifleVisual("ThirdPersonAK47", thirdPersonWeaponMount, Vector3.one * 1.45f, metal, wood);
            firstPersonLaptopVisual = BuildLaptopVisual("FirstPersonLaptop", firstPersonDeviceMount, Vector3.one * 1.4f, shell, screen);
            thirdPersonLaptopVisual = BuildLaptopVisual("ThirdPersonLaptop", thirdPersonDeviceMount, Vector3.one * 1.3f, shell, screen);

            firstPersonWeaponBasePosition = firstPersonWeaponMount.localPosition;
            firstPersonDeviceBasePosition = firstPersonDeviceMount.localPosition;
            thirdPersonWeaponBasePosition = thirdPersonWeaponMount.localPosition;
            thirdPersonDeviceBasePosition = thirdPersonDeviceMount.localPosition;
        }

        private void ApplyViewState()
        {
            if (bodyRenderers != null)
            {
                foreach (Renderer renderer in bodyRenderers)
                {
                    if (renderer != null)
                    {
                        renderer.enabled = thirdPerson;
                    }
                }
            }

            bool showRifle = weaponMode == WeaponMode.AK47;
            firstPersonRifleVisual?.SetActive(showRifle && !thirdPerson);
            thirdPersonRifleVisual?.SetActive(showRifle && thirdPerson);
            firstPersonLaptopVisual?.SetActive(!showRifle && !thirdPerson);
            thirdPersonLaptopVisual?.SetActive(!showRifle && thirdPerson);
            firstPersonLegsVisual?.SetActive(!thirdPerson);

            SetRendererVisibility(firstPersonRifleVisual, showRifle && !thirdPerson);
            SetRendererVisibility(thirdPersonRifleVisual, showRifle && thirdPerson);
            SetRendererVisibility(firstPersonLaptopVisual, !showRifle && !thirdPerson);
            SetRendererVisibility(thirdPersonLaptopVisual, !showRifle && thirdPerson);
            SetRendererVisibility(firstPersonLegsVisual, !thirdPerson);
        }

        private void UpdateWeaponSwitchAnimation()
        {
            if (weaponSwitchTimer <= 0f)
            {
                return;
            }

            weaponSwitchTimer -= Time.deltaTime;
            float t = 1f - Mathf.Clamp01(weaponSwitchTimer / 0.45f);
            float dip = Mathf.Sin(t * Mathf.PI) * 0.28f;
            ApplySwitchOffset(firstPersonWeaponMount, firstPersonWeaponBasePosition, dip);
            ApplySwitchOffset(firstPersonDeviceMount, firstPersonDeviceBasePosition, dip);
            ApplySwitchOffset(thirdPersonWeaponMount, thirdPersonWeaponBasePosition, dip * 0.45f);
            ApplySwitchOffset(thirdPersonDeviceMount, thirdPersonDeviceBasePosition, dip * 0.45f);
        }

        private static void ApplySwitchOffset(Transform mount, Vector3 basePosition, float dip)
        {
            if (mount == null)
            {
                return;
            }

            mount.localPosition = basePosition + Vector3.down * dip;
        }

        private static Transform CreateMount(Transform parent, string name, Vector3 localPosition, Quaternion localRotation)
        {
            Transform existing = parent.Find(name);
            if (existing != null)
            {
                existing.localPosition = localPosition;
                existing.localRotation = localRotation;
                return existing;
            }

            GameObject mount = new GameObject(name);
            mount.transform.SetParent(parent, false);
            mount.transform.localPosition = localPosition;
            mount.transform.localRotation = localRotation;
            return mount.transform;
        }

        private static void SetRendererVisibility(GameObject root, bool visible)
        {
            if (root == null)
            {
                return;
            }

            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                renderer.enabled = visible;
            }
        }

        private void EnsureCrosshairTexture()
        {
            if (crosshairTexture != null)
            {
                return;
            }

            const int size = 32;
            crosshairTexture = new Texture2D(size, size, TextureFormat.ARGB32, false);
            Color clear = new Color(0f, 0f, 0f, 0f);
            Color mark = new Color(0.9f, 1f, 1f, 0.95f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool vertical = Mathf.Abs(x - 16) <= 1 && (y < 12 || y > 20);
                    bool horizontal = Mathf.Abs(y - 16) <= 1 && (x < 12 || x > 20);
                    bool center = Mathf.Abs(x - 16) <= 1 && Mathf.Abs(y - 16) <= 1;
                    crosshairTexture.SetPixel(x, y, vertical || horizontal || center ? mark : clear);
                }
            }
            crosshairTexture.Apply();
        }

        private static void SpawnTracer(Vector3 start, Vector3 end, Color color, float duration)
        {
            GameObject tracer = new GameObject("BallisticTracer");
            LineRenderer line = tracer.AddComponent<LineRenderer>();
            line.positionCount = 2;
            line.SetPosition(0, start);
            line.SetPosition(1, end);
            line.startWidth = 0.045f;
            line.endWidth = 0.012f;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = color;
            line.endColor = new Color(color.r, color.g, color.b, 0f);
            Destroy(tracer, duration);
        }

        private static GameObject BuildRifleVisual(string rootName, Transform parent, Vector3 localScale, Material metal, Material wood)
        {
            GameObject root = CreateVisualRoot(rootName, parent, localScale);
            CreateVisualCube("Receiver", root.transform, new Vector3(0f, 0.02f, 0.04f), new Vector3(0.1f, 0.11f, 0.3f), metal);
            CreateVisualCube("Barrel", root.transform, new Vector3(0f, 0.03f, 0.43f), new Vector3(0.045f, 0.045f, 0.62f), metal);
            CreateVisualCube("Muzzle", root.transform, new Vector3(0f, 0.03f, 0.78f), new Vector3(0.06f, 0.06f, 0.09f), metal);
            CreateVisualCube("Stock", root.transform, new Vector3(0f, 0.03f, -0.28f), new Vector3(0.08f, 0.12f, 0.3f), wood);
            CreateVisualCube("Grip", root.transform, new Vector3(0f, -0.13f, -0.03f), new Vector3(0.055f, 0.18f, 0.07f), wood);
            GameObject mag = CreateVisualCube("Magazine", root.transform, new Vector3(0f, -0.14f, 0.12f), new Vector3(0.06f, 0.2f, 0.09f), metal);
            mag.transform.localRotation = Quaternion.Euler(22f, 0f, 0f);
            CreateVisualCube("HandGuard", root.transform, new Vector3(0f, -0.015f, 0.31f), new Vector3(0.075f, 0.08f, 0.22f), wood);
            return root;
        }

        private static GameObject BuildLaptopVisual(string rootName, Transform parent, Vector3 localScale, Material shell, Material screen)
        {
            GameObject root = CreateVisualRoot(rootName, parent, localScale);
            CreateVisualCube("Base", root.transform, new Vector3(0f, -0.015f, 0f), new Vector3(0.36f, 0.035f, 0.24f), shell);
            CreateVisualCube("Keyboard", root.transform, new Vector3(0f, 0.006f, 0.03f), new Vector3(0.29f, 0.008f, 0.17f), CreateMaterial(new Color(0.03f, 0.03f, 0.04f)));
            GameObject lid = CreateVisualCube("Lid", root.transform, new Vector3(0f, 0.12f, -0.1f), new Vector3(0.36f, 0.24f, 0.025f), shell);
            lid.transform.localRotation = Quaternion.Euler(-72f, 0f, 0f);
            CreateVisualCube("Screen", lid.transform, new Vector3(0f, 0f, -0.55f), new Vector3(0.84f, 0.84f, 0.2f), screen);
            return root;
        }

        private static GameObject BuildFirstPersonLegs(Transform parent)
        {
            Material pants = CreateMaterial(new Color(0.12f, 0.18f, 0.32f));
            GameObject root = CreateVisualRoot("FirstPersonLegs", parent, Vector3.one);
            CreateVisualCube("LeftLeg", root.transform, new Vector3(-0.12f, -0.88f, 0.2f), new Vector3(0.12f, 0.55f, 0.16f), pants);
            CreateVisualCube("RightLeg", root.transform, new Vector3(0.12f, -0.88f, 0.2f), new Vector3(0.12f, 0.55f, 0.16f), pants);
            return root;
        }

        private static GameObject CreateVisualRoot(string name, Transform parent, Vector3 localScale)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = localScale;
            return root;
        }

        private static Material CreateMaterial(Color color)
        {
            Material material = new Material(Shader.Find("Standard")) { color = color };
            material.SetFloat("_Glossiness", 0.05f);
            return material;
        }

        private static GameObject CreateVisualCube(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = localPosition;
            cube.transform.localScale = localScale;
            cube.GetComponent<Renderer>().sharedMaterial = material;
            Destroy(cube.GetComponent<Collider>());
            return cube;
        }
    }
}
