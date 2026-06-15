using UnityEngine;

namespace EverydayOdyssey
{
    public class SimpleModelAnimator : CharacterMotionController
    {
        [SerializeField] private Transform visualRoot;
        [SerializeField] private float bounceHeight = 0.08f;
        [SerializeField] private float bounceSpeed = 7.5f;
        [SerializeField] private float tiltAmount = 6f;
        [SerializeField] private float swayAmount = 5f;
        [SerializeField] private float attackKickAngle = 18f;
        [SerializeField] private float attackDuration = 0.22f;
        [SerializeField] private float strideYawAmount = 8f;
        [SerializeField] private float verticalEase = 10f;
        [SerializeField] private float rotationEase = 12f;

        private float locomotion;
        private float smoothedLocomotion;
        private float attackTimer;
        private Vector3 baseLocalPosition;
        private Quaternion baseLocalRotation;
        private Vector3 velocityPosition;

        private void Awake()
        {
            if (visualRoot == null)
            {
                visualRoot = transform;
            }

            baseLocalPosition = visualRoot.localPosition;
            baseLocalRotation = visualRoot.localRotation;
        }

        private void Update()
        {
            if (visualRoot == null)
            {
                return;
            }

            smoothedLocomotion = Mathf.Lerp(smoothedLocomotion, locomotion, Time.deltaTime * 8f);

            float bounce = Mathf.Sin(Time.time * bounceSpeed) * bounceHeight * smoothedLocomotion;
            float roll = Mathf.Sin(Time.time * bounceSpeed * 0.5f) * tiltAmount * smoothedLocomotion;
            float pitch = smoothedLocomotion * tiltAmount;
            float sway = Mathf.Sin(Time.time * (bounceSpeed * 0.6f)) * swayAmount * smoothedLocomotion;
            float strideYaw = Mathf.Sin(Time.time * bounceSpeed * 0.55f) * strideYawAmount * smoothedLocomotion;

            float attackKick = 0f;
            if (attackTimer > 0f)
            {
                attackTimer -= Time.deltaTime;
                float normalized = 1f - Mathf.Clamp01(attackTimer / attackDuration);
                attackKick = Mathf.Sin(normalized * Mathf.PI) * attackKickAngle;
            }

            Vector3 targetPosition = baseLocalPosition + new Vector3(0f, bounce, Mathf.Sin(Time.time * bounceSpeed * 0.35f) * 0.03f * smoothedLocomotion);
            Quaternion targetRotation = baseLocalRotation * Quaternion.Euler(-pitch - attackKick, sway + strideYaw, -roll);

            visualRoot.localPosition = Vector3.SmoothDamp(visualRoot.localPosition, targetPosition, ref velocityPosition, 1f / verticalEase);
            visualRoot.localRotation = Quaternion.Slerp(visualRoot.localRotation, targetRotation, Time.deltaTime * rotationEase);
        }

        public override void SetLocomotion(float amount)
        {
            locomotion = Mathf.Clamp01(amount);
        }

        public override void TriggerAttack()
        {
            attackTimer = attackDuration;
        }

        public void Configure(Transform target)
        {
            visualRoot = target;
            baseLocalPosition = visualRoot.localPosition;
            baseLocalRotation = visualRoot.localRotation;
        }
    }
}
