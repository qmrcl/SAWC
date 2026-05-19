using UnityEngine;
using SAWC.Core;

namespace SAWC.Pipeline
{
    [RequireComponent(typeof(SAWController))]
    public abstract class VelocityModifierBase : MonoBehaviour, IVelocityModifier
    {
        public abstract int Priority { get; }

        protected SAWController Controller { get; private set; }

        protected virtual void Awake()
        {
            Controller = GetComponent<SAWController>();
        }

        protected virtual void OnEnable()
        {
            if (Controller != null)
                Controller.Pipeline.VelocityModifiers.Add(this);
        }

        protected virtual void OnDisable()
        {
            if (Controller != null)
                Controller.Pipeline.VelocityModifiers.Remove(this);
        }

        public abstract Vector3 ModifyVelocity(Vector3 currentVelocity, ref FrameContext ctx);
    }
}