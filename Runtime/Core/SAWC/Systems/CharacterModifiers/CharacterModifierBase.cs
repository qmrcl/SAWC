using UnityEngine;
using SAWC.Core;
using SAWC.Localization;

namespace SAWC.Modifiers
{
    public abstract class CharacterModifierBase : MonoBehaviour
    {
        protected SAWController Controller { get; private set; }

        [SerializeField, Loc] private int _priority;

        public int Priority
        {
            get => _priority;
            private set => _priority = value;
        }

        protected virtual void Awake()
        {
            Controller = GetComponent<SAWController>();
        }

        protected virtual void OnEnable()
        {
            if (Controller == null)
            {
                Debug.LogError($"The controller on '{gameObject.name}' is null!", this);
                return;
            }

            if (this is IContextModifier contextMod)
            {
                Controller.Modifiers.AddContextModifier(contextMod);
            }

            if (this is IVelocityModifier velocityMod)
            {
                Controller.Modifiers.AddVelocityModifier(velocityMod);
            }
        }

        protected virtual void OnDisable()
        {
            if (Controller == null) return;

            if (this is IContextModifier contextMod)
            {
                Controller.Modifiers.RemoveContextModifier(contextMod);
            }

            if (this is IVelocityModifier velocityMod)
            {
                Controller.Modifiers.RemoveVelocityModifier(velocityMod);
            }
        }

        protected void SetPriority(int newPriority)
        {
            if (_priority == newPriority) return;

            _priority = newPriority;

            if (this is IContextModifier contextMod)
            {
                Controller.Modifiers.UpdateContextModifierPriority(contextMod);
            }

            if (this is IVelocityModifier velocityMod)
            {
                Controller.Modifiers.UpdateVelocityModifierPriority(velocityMod);
            }
        }
    }
}