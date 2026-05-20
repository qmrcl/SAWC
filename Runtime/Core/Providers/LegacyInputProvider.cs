using UnityEngine;

namespace SAWC.Input
{
    [AddComponentMenu("SAWC/Core/Input/Legacy Input Provider (Old)")]
    public class LegacyInputProvider : BaseInputProvider
    {
        [SerializeField] private string _horAxis = "Horizontal";
        [SerializeField] private string _verAxis = "Vertical";
        [SerializeField] private string _jumpButton = "Jump";
        [SerializeField] private string _sprintButton = "Fire3";
        [SerializeField] private string _crouchButton = "Crouch";

        protected override Vector2 GetRawMoveInput() =>
            new Vector2(UnityEngine.Input.GetAxisRaw(_horAxis), UnityEngine.Input.GetAxisRaw(_verAxis));

        protected override bool GetRawJumpInput() => UnityEngine.Input.GetButton(_jumpButton);
        protected override bool GetRawSprintInput() => UnityEngine.Input.GetButton(_sprintButton);
        protected override bool GetRawCrouchInput() => UnityEngine.Input.GetButton(_crouchButton);
    }
}