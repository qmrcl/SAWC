using SAWC.Localization;
using UnityEngine;

namespace SAWC.Core.Input.Readers
{
    [AddComponentMenu("SAWC/Core/Input/Readers/Legacy Input Reader")]
    public class LegacyInputReader : BaseInputReader
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        [SerializeField, Loc] private string _horAxis = "Horizontal";
        [SerializeField, Loc] private string _verAxis = "Vertical";

        [Space(5)]
        [SerializeField, Loc] private KeyCode _jumpKey = KeyCode.Space;
        [SerializeField, Loc] private KeyCode _sprintKey = KeyCode.LeftShift;
        [SerializeField, Loc] private KeyCode _crouchKey = KeyCode.LeftControl;

        public override Vector2 Move => new Vector2(UnityEngine.Input.GetAxisRaw(_horAxis), UnityEngine.Input.GetAxisRaw(_verAxis));

        public override bool Jump => UnityEngine.Input.GetKey(_jumpKey);
        public override bool Sprint => UnityEngine.Input.GetKey(_sprintKey);
        public override bool Crouch => UnityEngine.Input.GetKey(_crouchKey);
#else
        public override Vector2 Move => Vector2.zero;
        public override bool Jump => false;
        public override bool Sprint => false;
        public override bool Crouch => false;
#endif
    }
}