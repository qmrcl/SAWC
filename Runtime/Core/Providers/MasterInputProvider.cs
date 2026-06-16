using SAWC.Core.Input.Readers;
using SAWC.Localization;
using UnityEngine;

namespace SAWC.Core.Input
{
    [AddComponentMenu("SAWC/Core/Input/Master Input Provider")]
    public class MasterInputProvider : BaseInputProvider
    {
        private enum ButtonType { Jump, Sprint, Crouch }

        [SerializeField] private BaseInputReader[] _readers;

        [Space(5)]
        [SerializeField, Loc] private float _deadZone = 0.05f;
        [SerializeField, Loc] private float _switchThreshold = 0.15f;

        private BaseInputReader _activeReader;

        protected override void Awake()
        {
            base.Awake();
            if (_readers != null && _readers.Length > 0 && _readers[0] != null)
            {
                _activeReader = _readers[0];
            }
        }

        public override Vector2 MoveInput
        {
            get
            {
                if (_activeReader == null) return Vector2.zero;
                Vector2 move = _activeReader.Move;
                return move.sqrMagnitude > _deadZone * _deadZone ? move : Vector2.zero;
            }
        }

        protected override void Update()
        {
            EvaluateActiveDeviceByMovement();
            base.Update();
        }

        private void EvaluateActiveDeviceByMovement()
        {
            if (_readers == null) return;

            if (_activeReader != null && _activeReader.Move.sqrMagnitude > _switchThreshold * _switchThreshold)
            {
                return;
            }

            float maxMoveSq = _activeReader != null ? _activeReader.Move.sqrMagnitude : 0f;
            float thresholdSq = _switchThreshold * _switchThreshold;

            for (int i = 0; i < _readers.Length; i++)
            {
                var reader = _readers[i];
                if (reader == null || reader == _activeReader) continue;

                float moveSq = reader.Move.sqrMagnitude;

                if (moveSq > thresholdSq && moveSq > maxMoveSq)
                {
                    _activeReader = reader;
                    maxMoveSq = moveSq;
                }
            }
        }

        protected override bool GetJumpInput() => GetButtonInput(ButtonType.Jump);
        protected override bool GetSprintInput() => GetButtonInput(ButtonType.Sprint);
        protected override bool GetCrouchInput() => GetButtonInput(ButtonType.Crouch);

        private bool GetButtonInput(ButtonType buttonType)
        {
            if (_activeReader != null && CheckButtonState(_activeReader, buttonType))
                return true;

            if (_readers == null) return false;

            for (int i = 0; i < _readers.Length; i++)
            {
                var reader = _readers[i];
                if (reader == null || reader == _activeReader) continue;

                if (CheckButtonState(reader, buttonType)) return true;
            }
            return false;
        }

        private bool CheckButtonState(BaseInputReader reader, ButtonType buttonType) => buttonType switch
        {
            ButtonType.Jump => reader.Jump,
            ButtonType.Sprint => reader.Sprint,
            ButtonType.Crouch => reader.Crouch,
            _ => false
        };
    }
}