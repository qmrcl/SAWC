using System;
using UnityEngine;

namespace SAWC.Core
{
    internal sealed class CharacterStateTracker
    {
        private bool _wasGrounded;
        private bool _isMoving;

        internal bool IsMoving => _isMoving;

        internal event Action LandPerformed;
        internal event Action StartMoving;
        internal event Action StopMoving;

        internal void Initialize(bool isGrounded)
        {
            _wasGrounded = isGrounded;
        }

        internal void Tick(bool isGrounded, Vector3 horizontalVelocity)
        {
            CheckLanding(isGrounded);
            CheckMovementState(horizontalVelocity);
            _wasGrounded = isGrounded;
        }

        private void CheckLanding(bool isGrounded)
        {
            if (!_wasGrounded && isGrounded) LandPerformed?.Invoke();
        }

        private void CheckMovementState(Vector3 horizontalVelocity)
        {
            float speedSq = horizontalVelocity.sqrMagnitude;

            if (speedSq > 0.01f && !_isMoving)
            {
                _isMoving = true;
                StartMoving?.Invoke();
            }
            else if (speedSq <= 0.005f && _isMoving)
            {
                _isMoving = false;
                StopMoving?.Invoke();
            }
        }
    }
}