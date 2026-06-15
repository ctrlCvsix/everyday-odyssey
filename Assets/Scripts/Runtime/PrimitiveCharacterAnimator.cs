using UnityEngine;

namespace EverydayOdyssey
{
    public class PrimitiveCharacterAnimator : CharacterMotionController
    {
        [SerializeField] private Transform torso;
        [SerializeField] private Transform head;
        [SerializeField] private Transform leftArm;
        [SerializeField] private Transform rightArm;
        [SerializeField] private Transform leftLeg;
        [SerializeField] private Transform rightLeg;
        [SerializeField] private Transform heldItem;
        [SerializeField] private float swingAngle = 32f;
        [SerializeField] private float swingSpeed = 9f;
        [SerializeField] private float attackDuration = 0.22f;

        private float locomotionAmount;
        private float attackTimer;
        private Quaternion leftArmBaseRotation;
        private Quaternion rightArmBaseRotation;
        private Quaternion leftLegBaseRotation;
        private Quaternion rightLegBaseRotation;
        private Vector3 heldItemBaseLocalPosition;

        private void Awake()
        {
            if (leftArm != null) leftArmBaseRotation = leftArm.localRotation;
            if (rightArm != null) rightArmBaseRotation = rightArm.localRotation;
            if (leftLeg != null) leftLegBaseRotation = leftLeg.localRotation;
            if (rightLeg != null) rightLegBaseRotation = rightLeg.localRotation;
            if (heldItem != null) heldItemBaseLocalPosition = heldItem.localPosition;
        }

        private void Update()
        {
            float swing = Mathf.Sin(Time.time * swingSpeed) * swingAngle * locomotionAmount;
            float reverseSwing = -swing;

            if (leftArm != null) leftArm.localRotation = leftArmBaseRotation * Quaternion.Euler(swing, 0f, 0f);
            if (rightArm != null) rightArm.localRotation = rightArmBaseRotation * Quaternion.Euler(reverseSwing, 0f, 0f);
            if (leftLeg != null) leftLeg.localRotation = leftLegBaseRotation * Quaternion.Euler(reverseSwing, 0f, 0f);
            if (rightLeg != null) rightLeg.localRotation = rightLegBaseRotation * Quaternion.Euler(swing, 0f, 0f);

            if (torso != null)
            {
                torso.localRotation = Quaternion.Euler(Mathf.Sin(Time.time * 3f) * locomotionAmount * 2f, 0f, 0f);
            }

            if (head != null)
            {
                head.localRotation = Quaternion.Euler(Mathf.Sin(Time.time * 2.5f) * locomotionAmount * 2f, 0f, 0f);
            }

            if (attackTimer > 0f)
            {
                attackTimer -= Time.deltaTime;
                float normalized = 1f - Mathf.Clamp01(attackTimer / attackDuration);
                float punch = Mathf.Sin(normalized * Mathf.PI);

                if (rightArm != null)
                {
                    rightArm.localRotation = rightArmBaseRotation * Quaternion.Euler(-78f * punch, 0f, 0f);
                }

                if (heldItem != null)
                {
                    heldItem.localPosition = heldItemBaseLocalPosition + new Vector3(0f, 0f, 0.18f * punch);
                }
            }
            else if (heldItem != null)
            {
                heldItem.localPosition = heldItemBaseLocalPosition;
            }
        }

        public override void SetLocomotion(float amount)
        {
            locomotionAmount = Mathf.Clamp01(amount);
        }

        public override void TriggerAttack()
        {
            attackTimer = attackDuration;
        }

        public void BindRig(
            Transform torsoRig,
            Transform headRig,
            Transform leftArmRig,
            Transform rightArmRig,
            Transform leftLegRig,
            Transform rightLegRig,
            Transform heldItemRig)
        {
            torso = torsoRig;
            head = headRig;
            leftArm = leftArmRig;
            rightArm = rightArmRig;
            leftLeg = leftLegRig;
            rightLeg = rightLegRig;
            heldItem = heldItemRig;
            Awake();
        }
    }
}
