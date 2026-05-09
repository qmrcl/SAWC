using UnityEngine;

namespace SAWC.Core
{
    internal sealed class CharacterPosture
    {
        private readonly CharacterSettings _settings;
        private readonly CharacterController _controller;
        private readonly Transform _transform;
        
        private float _heightVelocity;
        private readonly RaycastHit[] _hitBuffer = new RaycastHit[5];

        internal CharacterPosture(CharacterSettings settings, CharacterController controller, Transform transform)
        {
            _settings = settings;
            _controller = controller;
            _transform = transform;

            _controller.height = _settings.StandingHeight;
            _controller.center = new Vector3(0, _settings.StandingHeight / 2f, 0);
        }

        internal bool CheckCrouchState(bool crouchInput)
        {
            if (!_settings.CanCrouch) return false;
            return crouchInput || !CanStandUp();
        }

        private bool CanStandUp()
        {
            float radius = _controller.radius * 0.9f;
            Vector3 rayStart = _transform.position + Vector3.up * _controller.height;
            float distance = _settings.StandingHeight - _controller.height;

            if (distance <= 0) return true;

            int hitCount = Physics.SphereCastNonAlloc(
                rayStart, radius, Vector3.up, _hitBuffer, distance, 
                ~0, QueryTriggerInteraction.Ignore
            );

            for (int i = 0; i < hitCount; i++)
            {
                Transform hitTransform = _hitBuffer[i].transform;
                if (hitTransform != _transform && !hitTransform.IsChildOf(_transform))
                {
                    return false; 
                }
            }

            return true;
        }

        internal void Tick(bool isCrouching)
        {
            float targetHeight = isCrouching ? _settings.CrouchHeight : _settings.StandingHeight;

            if (Mathf.Abs(_controller.height - targetHeight) > 0.001f)
            {
                _controller.height = Mathf.SmoothDamp(_controller.height, targetHeight, ref _heightVelocity, _settings.CrouchSmoothTime);
                _controller.center = new Vector3(0, _controller.height / 2f, 0);
            }
        }
    }
}