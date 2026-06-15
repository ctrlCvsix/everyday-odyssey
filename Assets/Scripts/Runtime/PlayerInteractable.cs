using UnityEngine;

namespace EverydayOdyssey
{
    public abstract class PlayerInteractable : MonoBehaviour
    {
        public virtual bool CanInteract(PlayerController player)
        {
            return true;
        }

        public virtual string GetPromptText(LanguageOption language)
        {
            return language == LanguageOption.Korean ? "상호작용" : "交互";
        }

        public abstract void Interact(PlayerController player);
    }
}
