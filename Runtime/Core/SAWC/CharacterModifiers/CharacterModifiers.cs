using System.Collections.Generic;
using UnityEngine;
using SAWC.Core;

namespace SAWC.Modifiers
{
    public sealed class CharacterModifiers
    {
        private readonly PrioritizedList<IContextModifier> _context = new();
        private readonly PrioritizedList<IVelocityModifier> _velocity = new();
        private readonly List<List<IContextModifier>> _contextSnapshots = new();
        private readonly List<List<IVelocityModifier>> _velocitySnapshots = new();
        private int _contextProcessingDepth;
        private int _velocityProcessingDepth;

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
            List<IContextModifier> snapshot = GetSnapshot(_context, _contextSnapshots, _contextProcessingDepth++);
            try
            {
                for (int i = 0; i < snapshot.Count; i++)
                {
                    IContextModifier modifier = snapshot[i];
                    if (_context.Contains(modifier)) modifier.ModifyContext(ref ctx);
                }
            }
            finally
            {
                snapshot.Clear();
                _contextProcessingDepth--;
            }
        }

        public Vector3 ProcessVelocity(Vector3 currentVelocity, ref FrameContext ctx)
        {
            Vector3 finalVelocity = currentVelocity;
            List<IVelocityModifier> snapshot = GetSnapshot(_velocity, _velocitySnapshots, _velocityProcessingDepth++);
            try
            {
                for (int i = 0; i < snapshot.Count; i++)
                {
                    IVelocityModifier modifier = snapshot[i];
                    if (_velocity.Contains(modifier))
                        finalVelocity = modifier.ModifyVelocity(finalVelocity, ref ctx);
                }
            }
            finally
            {
                snapshot.Clear();
                _velocityProcessingDepth--;
            }
            return finalVelocity;
        }

        private static List<T> GetSnapshot<T>(PrioritizedList<T> source, List<List<T>> snapshots, int depth)
            where T : IPrioritized
        {
            if (depth == snapshots.Count) snapshots.Add(new List<T>());

            List<T> snapshot = snapshots[depth];
            for (int i = 0; i < source.Count; i++) snapshot.Add(source[i]);
            return snapshot;
        }
    }
}
