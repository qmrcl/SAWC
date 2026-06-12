using System.Collections.Generic;
using UnityEngine;
using SAWC.Core;

namespace SAWC.Modifiers
{
    public sealed class CharacterModifiers
    {
        private readonly PrioritizedList<IContextModifier> _context = new();
        private readonly PrioritizedList<IVelocityModifier> _velocity = new();

        public IReadOnlyList<IContextModifier> Context => _context;
        public IReadOnlyList<IVelocityModifier> Velocity => _velocity;

        public void AddContextModifier(IContextModifier modifier)
        {
            if (modifier == null) return;
            _context.Add(modifier);
        }

        public void RemoveContextModifier(IContextModifier modifier)
        {
            if (modifier == null) return;
            _context.Remove(modifier);
        }

        public void UpdateContextModifierPriority(IContextModifier modifier)
        {
            if (modifier == null) return;
            _context.UpdatePriority(modifier);
        }

        public void AddVelocityModifier(IVelocityModifier modifier)
        {
            if (modifier == null) return;
            _velocity.Add(modifier);
        }

        public void RemoveVelocityModifier(IVelocityModifier modifier)
        {
            if (modifier == null) return;
            _velocity.Remove(modifier);
        }

        public void UpdateVelocityModifierPriority(IVelocityModifier modifier)
        {
            if (modifier == null) return;
            _velocity.UpdatePriority(modifier);
        }

        public void ProcessContext(ref FrameContext ctx)
        {
            for (int i = 0; i < _context.Count; i++)
            {
                _context[i].ModifyContext(ref ctx);
            }
        }

        public Vector3 ProcessVelocity(Vector3 currentVelocity, ref FrameContext ctx)
        {
            Vector3 finalVelocity = currentVelocity;
            for (int i = 0; i < _velocity.Count; i++)
            {
                finalVelocity = _velocity[i].ModifyVelocity(finalVelocity, ref ctx);
            }
            return finalVelocity;
        }
    }
}