using UnityEngine;
using SAWC.Core;

namespace SAWC.Modifiers
{
    [RequireComponent(typeof(SAWController))]
    public abstract class CharacterModifierBase : MonoBehaviour
    {
        protected SAWController Controller { get; private set; }

        protected virtual void Awake()
        {
            Controller = GetComponent<SAWController>();
        }

        protected virtual void OnEnable()
        {
            if (Controller == null)
            {
                Debug.LogError($"The controller on '{gameObject.name}' is null!", this); return;
            }

            if (this is IContextModifier contextMod)
            {
                Controller.Modifiers.Context.Add(contextMod);
            }

            if (this is IVelocityModifier velocityMod)
            {
                Controller.Modifiers.Velocity.Add(velocityMod);
            }
        }

        protected virtual void OnDisable()
        {
            if (Controller == null) return;

            if (this is IContextModifier contextMod)
            {
                Controller.Modifiers.Context.Remove(contextMod);
            }

            if (this is IVelocityModifier velocityMod)
            {
                Controller.Modifiers.Velocity.Remove(velocityMod);
            }
        }
    }
}