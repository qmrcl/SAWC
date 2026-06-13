using UnityEngine;

namespace SAWC.Core.Input.Readers
{
    [AddComponentMenu("SAWC/Core/Input/Readers/Legacy Input Reader")]
    public class LegacyInputReader : BaseInputReader
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        [SerializeField] private string _horAxis = "Horizontal";
        [SerializeField] private string _verAxis = "Vertical";
        [SerializeField] private string _jumpButton = "Jump";
        [SerializeField] private string _sprintButton = "Fire3";
        [SerializeField] private string _crouchButton = "Crouch";

        public override Vector2 Move => new Vector2(UnityEngine.Input.GetAxisRaw(_horAxis), UnityEngine.Input.GetAxisRaw(_verAxis));
        public override bool Jump => UnityEngine.Input.GetButton(_jumpButton);
        public override bool Sprint => UnityEngine.Input.GetButton(_sprintButton);
        public override bool Crouch => UnityEngine.Input.GetButton(_crouchButton);
#else
        public override Vector2 Move => Vector2.zero;
        public override bool Jump => false;
        public override bool Sprint => false;
        public override bool Crouch => false;
#endif
    }
}