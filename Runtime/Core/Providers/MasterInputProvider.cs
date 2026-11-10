using SAWC.Core.Input.Readers;
using UnityEngine;

namespace SAWC.Core.Input
{
    [AddComponentMenu("SAWC/Core/Input/Master Input Provider")]
    [DefaultExecutionOrder(10)]
    public class MasterInputProvider : BaseInputProvider
    {
        [SerializeField] private BaseInputReader[] _readers;
        [SerializeField, Min(0f)] private float _deadZone = 0.05f;

        private Vector2 _cachedMove;
        private bool _cachedJump;
        private bool _cachedSprint;
        private bool _cachedCrouch;

        public override Vector2 MoveInput => _cachedMove;
        public override bool JumpHeld => _cachedJump;
        public override bool SprintHeld => _cachedSprint;
        public override bool CrouchHeld => _cachedCrouch;

        protected override void Update()
        {
            PollReaders();
            base.Update();
        }

        protected override void OnDisable()
        {
            ClearInput();
            base.OnDisable();
        }

        private void PollReaders()
        {
            ClearInput();

            if (_readers == null) return;

            float bestMoveSq = _deadZone * _deadZone;

            for (int i = 0; i < _readers.Length; i++)
            {
                var reader = _readers[i];
                if (reader == null || !reader.isActiveAndEnabled) continue;

                Vector2 move = reader.Move;
                float moveSq = move.sqrMagnitude;
                if (moveSq > bestMoveSq)
                {
                    bestMoveSq = moveSq;
                    _cachedMove = move;
                }

                if (reader.Jump) _cachedJump = true;
                if (reader.Sprint) _cachedSprint = true;
                if (reader.Crouch) _cachedCrouch = true;
            }
        }

        private void ClearInput()
        {
            _cachedMove = Vector2.zero;
            _cachedJump = false;
            _cachedSprint = false;
            _cachedCrouch = false;
        }
    }
}
