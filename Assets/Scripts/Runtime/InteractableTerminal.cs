using UnityEngine;

namespace EverydayOdyssey
{
    public class InteractableTerminal : PlayerInteractable
    {
        [SerializeField] private int terminalIndex;
        [SerializeField] private string terminalName = "\u8bc4\u5ba1\u7ec8\u7aef";
        [SerializeField] private string terminalNameKo = "\ud3c9\uac00 \ud130\ubbf8\ub110";
        [SerializeField] private MeshRenderer beaconRenderer;
        [SerializeField] private Renderer fragmentRenderer;
        [SerializeField] private GameObject fragmentRoot;
        [SerializeField] private Color completedColor = new Color(0.35f, 1f, 0.45f);

        private bool hacked;

        private void Update()
        {
            if (hacked || fragmentRoot == null)
            {
                return;
            }

            fragmentRoot.transform.Rotate(0f, 80f * Time.deltaTime, 0f, Space.World);
            float bob = Mathf.Sin(Time.time * 2.8f + terminalIndex) * 0.16f;
            Vector3 localPos = fragmentRoot.transform.localPosition;
            fragmentRoot.transform.localPosition = new Vector3(localPos.x, 1.75f + bob, localPos.z);
        }

        public override bool CanInteract(PlayerController player)
        {
            return !hacked && GameManager.Instance != null && !GameManager.Instance.IsGameOver;
        }

        public override string GetPromptText(LanguageOption language)
        {
            return language == LanguageOption.Korean
                ? $"{terminalNameKo} 조각 획득"
                : $"获取{terminalName}碎片";
        }

        public override void Interact(PlayerController player)
        {
            if (hacked)
            {
                return;
            }

            hacked = true;
            if (beaconRenderer != null)
            {
                beaconRenderer.material.color = completedColor;
            }

            if (fragmentRenderer != null)
            {
                fragmentRenderer.enabled = false;
            }

            if (fragmentRoot != null)
            {
                fragmentRoot.SetActive(false);
            }

            GameManager.Instance?.AudioBank?.PlayFragmentPickup(transform.position);
            GameManager.Instance?.RegisterTerminalHack(terminalIndex, terminalName);
        }

        public void Configure(int index, string newNameZh, string newNameKo, MeshRenderer rendererReference, Renderer fragmentRendererReference, GameObject fragmentRootReference)
        {
            terminalIndex = index;
            terminalName = newNameZh;
            terminalNameKo = newNameKo;
            beaconRenderer = rendererReference;
            fragmentRenderer = fragmentRendererReference;
            fragmentRoot = fragmentRootReference;
        }
    }
}
