using UnityEngine;

namespace SAWC.Core
{
    [CreateAssetMenu(fileName = "CharacterSettings", menuName = "SAWC/Character Settings")]
    public class CharacterSettings : ScriptableObject
    {
        [Header("Movement")]
        public bool CanJump = true;
        public bool CanSprint = true;
        public bool SprintOnlyForward = true;
        public bool EnableAutoJump = false;

        public float MoveSpeed = 5f;
        public float SprintSpeed = 10f;
        public float JumpForce = 6f;

        [Header("Game Feel (Прыжок)")]
        [Tooltip("Сколько секунд помним нажатие прыжка до касания земли")]
        [Min(0)] public float JumpBufferTime = 0.1f;

        [Tooltip("Сколько секунд можно прыгать после потери земли под ногами")]
        [Min(0)] public float CoyoteTime = 0.15f;

        [Header("Crouch (Приседание)")]
        public bool CanCrouch = true;
        public float CrouchSpeed = 2.5f;
        public float StandingHeight = 2f;
        public float CrouchHeight = 1f;
        [Range(0.01f, 0.5f)]
        public float CrouchSmoothTime = 0.1f;

        [Tooltip("Слои, которые мешают встать из приседа")]
        public LayerMask EnvironmentMask = 1;

        [Header("Physics")]
        [Range(-30, 0f)] public float Gravity = -9.81f;
        public float TerminalVelocity = -50f;
        [Range(-10f, 0f)] public float GroundedGravity = -2f;
        [Range(1f, 5f)] public float FallMultiplier = 2.5f;

        [Header("Thresholds")]
        public float MinMoveThreshold = 0.5f;

        [Header("Acceleration & Air Control")]
        [Range(1f, 100f)] public float Acceleration = 25f;
        [Range(1f, 100f)] public float Deceleration = 35f;
        [Range(0f, 1f)] public float AirControlMultiplier = 0.5f;

        [Header("Rotation")]
        public bool RotateWithMovement = false;
        [Range(0.01f, 0.5f)] public float MovementRotationSmoothTime = 0.12f;
        [Range(0.01f, 0.5f)] public float StrafeRotationSmoothTime = 0.05f;

        [Header("Physics Thresholds")]
        public float InputThreshold = 0.01f;
        public float InputThresholdSq => InputThreshold * InputThreshold;

        public float VerticalVelocityThreshold = 0.1f;
        public float IdleTransitionMultiplier = 0.8f;

        private void OnValidate()
        {
            if (SprintSpeed < MoveSpeed) SprintSpeed = MoveSpeed;
            if (CrouchSpeed > MoveSpeed) CrouchSpeed = MoveSpeed;
            if (Gravity > 0f) Gravity = -Mathf.Abs(Gravity);
            if (GroundedGravity > 0f) GroundedGravity = -Mathf.Abs(GroundedGravity);
            if (JumpForce < 0f) JumpForce = Mathf.Abs(JumpForce);
            if (CrouchHeight > StandingHeight) CrouchHeight = StandingHeight;

            if (InputThreshold < 0.001f) InputThreshold = 0.001f;
            if (VerticalVelocityThreshold < 0f) VerticalVelocityThreshold = Mathf.Abs(VerticalVelocityThreshold);
            if (VerticalVelocityThreshold == 0f) VerticalVelocityThreshold = 0.01f;

            if (IdleTransitionMultiplier <= 0f) IdleTransitionMultiplier = 0.01f;
            if (IdleTransitionMultiplier > 1f) IdleTransitionMultiplier = 1f;
        }
    }
}