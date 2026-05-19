using UnityEngine;
using SAWC.Core;

namespace SAWC.Pipeline
{
    public interface IPrioritized
    {
        int Priority { get; }
    }

    public interface IFrameMiddleware : IPrioritized
    {
        void ProcessContext(ref FrameContext ctx);
    }

    public interface IVelocityModifier : IPrioritized
    {
        Vector3 ModifyVelocity(Vector3 currentVelocity, ref FrameContext ctx);
    }
}