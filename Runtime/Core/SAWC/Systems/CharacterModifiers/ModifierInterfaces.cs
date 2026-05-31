using UnityEngine;
using SAWC.Core;

namespace SAWC.Modifiers
{
    public interface IPrioritized
    {
        int Priority { get; }
    }

    public interface IContextModifier : IPrioritized
    {
        void ModifyContext(ref FrameContext ctx);
    }

    public interface IVelocityModifier : IPrioritized
    {
        Vector3 ModifyVelocity(Vector3 currentVelocity, ref FrameContext ctx);
    }
}