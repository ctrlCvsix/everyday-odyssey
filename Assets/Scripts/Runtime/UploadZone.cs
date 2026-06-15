using System.Collections;
using UnityEngine;

namespace EverydayOdyssey
{
    public class UploadZone : PlayerInteractable
    {
        [SerializeField] private float uploadDuration = 8f;
        [SerializeField] private float allowedRadius = 3.6f;

        private Coroutine uploadRoutine;

        public override bool CanInteract(PlayerController player)
        {
            return uploadRoutine == null && GameManager.Instance.AllTerminalsHacked && !GameManager.Instance.IsGameOver;
        }

        public override string GetPromptText(LanguageOption language)
        {
            return language == LanguageOption.Korean
                ? "\ucd5c\uc885 \ube4c\ub4dc \uc5c5\ub85c\ub4dc"
                : "\u4e0a\u4f20\u6700\u7ec8\u7b54\u8fa9\u7a0b\u5e8f";
        }

        public override void Interact(PlayerController player)
        {
            if (uploadRoutine != null || !GameManager.Instance.AllTerminalsHacked)
            {
                return;
            }

            uploadRoutine = StartCoroutine(UploadSequence(player));
        }

        private IEnumerator UploadSequence(PlayerController player)
        {
            float timer = uploadDuration;
            while (timer > 0f)
            {
                if (player == null || Vector3.Distance(player.transform.position, transform.position) > allowedRadius)
                {
                    GameManager.Instance.Announce(GameManager.Instance.CurrentLanguage == LanguageOption.Korean
                        ? "\uc5c5\ub85c\ub4dc\uac00 \uc911\ub2e8\ub418\uc5c8\uc2b5\ub2c8\ub2e4. \uac80\uc740 \uc30d\uae30\ub465 \uc0ac\uc774\ub85c \ub3cc\uc544\uc624\uc138\uc694."
                        : "\u4e0a\u4f20\u88ab\u4e2d\u65ad\u4e86\uff0c\u8bf7\u56de\u5230\u53cc\u9ed1\u67f1\u4e4b\u95f4\u91cd\u65b0\u4e0a\u4f20\u3002");
                    uploadRoutine = null;
                    yield break;
                }

                timer -= Time.deltaTime;
                GameManager.Instance.SetUploadProgress(1f - (timer / uploadDuration));
                yield return null;
            }

            uploadRoutine = null;
            GameManager.Instance.CompleteUpload();
        }
    }
}
