using UnityEngine;

namespace EverydayOdyssey
{
    public abstract class CharacterMotionController : MonoBehaviour
    {
        public abstract void SetLocomotion(float amount);

        public virtual void TriggerAttack()
        {
        }
    }
}
