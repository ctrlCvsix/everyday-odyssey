using UnityEngine;

namespace EverydayOdyssey
{
    public class PrototypeAudioBank : MonoBehaviour
    {
        [SerializeField] private AudioClip uiClick;
        [SerializeField] private AudioClip uiConfirm;
        [SerializeField] private AudioClip uiError;
        [SerializeField] private AudioClip projectileCast;
        [SerializeField] private AudioClip teacherHit;
        [SerializeField] private AudioClip fragmentPickup;
        [SerializeField] private AudioClip gunshot;
        [SerializeField] private AudioClip footstepConcreteA;
        [SerializeField] private AudioClip footstepConcreteB;
        [SerializeField] private AudioClip footstepGrassA;
        [SerializeField] private AudioClip footstepGrassB;

        private AudioSource sharedSource;

        private void Awake()
        {
            sharedSource = gameObject.AddComponent<AudioSource>();
            sharedSource.spatialBlend = 0f;
            sharedSource.playOnAwake = false;
        }

        public void PlayUiClick()
        {
            PlayUi(uiClick);
        }

        public void PlayUiConfirm()
        {
            PlayUi(uiConfirm);
        }

        public void PlayGameStart()
        {
            PlayUi(uiConfirm != null ? uiConfirm : uiClick);
        }

        public void PlayWeaponSwitch(Vector3 position)
        {
            PlayAtPoint(uiClick != null ? uiClick : uiConfirm, position, 0.85f);
        }

        public void PlayLaptopAttack(Vector3 position)
        {
            PlayAtPoint(projectileCast != null ? projectileCast : uiClick, position, 0.9f);
        }

        public void PlayUiError()
        {
            PlayUi(uiError);
        }

        public void PlayProjectileCast(Vector3 position)
        {
            PlayAtPoint(projectileCast != null ? projectileCast : uiClick, position, 0.75f);
        }

        public void PlayTeacherHit(Vector3 position)
        {
            PlayAtPoint(teacherHit, position, 0.7f);
        }

        public void PlayFragmentPickup(Vector3 position)
        {
            PlayAtPoint(fragmentPickup != null ? fragmentPickup : uiConfirm, position, 0.75f);
        }

        public void PlayGunshot(Vector3 position)
        {
            PlayAtPoint(gunshot != null ? gunshot : teacherHit, position, 0.85f);
        }

        public void PlayFootstep(Vector3 position, bool onGrass)
        {
            AudioClip clip = onGrass
                ? (Random.value > 0.5f ? footstepGrassA : footstepGrassB)
                : (Random.value > 0.5f ? footstepConcreteA : footstepConcreteB);
            PlayAtPoint(clip, position, 0.35f);
        }

        public void Configure(
            AudioClip clickClip,
            AudioClip confirmClip,
            AudioClip errorClip,
            AudioClip castClip,
            AudioClip hitClip,
            AudioClip pickupClip,
            AudioClip gunshotClip,
            AudioClip concreteA,
            AudioClip concreteB,
            AudioClip grassA,
            AudioClip grassB)
        {
            uiClick = clickClip;
            uiConfirm = confirmClip;
            uiError = errorClip;
            projectileCast = castClip;
            teacherHit = hitClip;
            fragmentPickup = pickupClip;
            gunshot = gunshotClip;
            footstepConcreteA = concreteA;
            footstepConcreteB = concreteB;
            footstepGrassA = grassA;
            footstepGrassB = grassB;
        }

        private void PlayUi(AudioClip clip)
        {
            if (clip == null || sharedSource == null)
            {
                return;
            }

            sharedSource.PlayOneShot(clip, 0.75f);
        }

        private static void PlayAtPoint(AudioClip clip, Vector3 position, float volume)
        {
            if (clip == null)
            {
                return;
            }

            AudioSource.PlayClipAtPoint(clip, position, volume);
        }
    }
}
