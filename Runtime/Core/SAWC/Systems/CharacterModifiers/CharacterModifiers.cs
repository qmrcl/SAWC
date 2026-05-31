using UnityEngine;
using SAWC.Core;

namespace SAWC.Modifiers
{
    public sealed class CharacterModifiers
    {
        public readonly PrioritizedList<IContextModifier> Context = new();
        public readonly PrioritizedList<IVelocityModifier> Velocity = new();

        public void ProcessContext(ref FrameContext ctx)
        {
            var items = Context.Items;
            for (int i = 0; i < items.Count; i++)
            {
                items[i].ModifyContext(ref ctx);
            }
        }

        public Vector3 ProcessVelocity(Vector3 currentVelocity, ref FrameContext ctx)
        {
            Vector3 finalVelocity = currentVelocity;
            var items = Velocity.Items;

            for (int i = 0; i < items.Count; i++)
            {
                finalVelocity = items[i].ModifyVelocity(finalVelocity, ref ctx);
            }

            return finalVelocity;
        }
    }
}