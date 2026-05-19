using UnityEngine;
using SAWC.Core;

namespace SAWC.Pipeline
{
    public sealed class CharacterPipeline
    {
        public readonly PrioritizedList<IFrameMiddleware> Middlewares = new();
        public readonly PrioritizedList<IVelocityModifier> VelocityModifiers = new();

        public void ProcessContext(ref FrameContext ctx)
        {
            var items = Middlewares.Items;
            for (int i = 0; i < items.Count; i++)
            {
                items[i].ProcessContext(ref ctx);
            }
        }

        public Vector3 ProcessVelocity(Vector3 currentVelocity, ref FrameContext ctx)
        {
            Vector3 finalVelocity = currentVelocity;
            var items = VelocityModifiers.Items;

            for (int i = 0; i < items.Count; i++)
            {
                finalVelocity = items[i].ModifyVelocity(finalVelocity, ref ctx);
            }

            return finalVelocity;
        }
    }
}