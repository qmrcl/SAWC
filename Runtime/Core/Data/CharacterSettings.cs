using UnityEngine;
using System;

namespace SAWC.Core.Data
{
    [Flags]
    public enum SprintAllowedDirections
    {
        None = 0,
        Forward = 1 << 0,
        Backward = 1 << 1,
        Left = 1 << 2,
        Right = 1 << 3,
        All = Forward | Backward | Left | Right
    }

    [Serializable]
    public struct MovementSettings
    {
        public bool CanMove;
        public bool CanSprint;

        public SprintAllowedDirections AllowedSprintDirections;
        public bool UseInertia;

        [Min(0f)] public float MoveSpeed;
        [Min(0f)] public float SprintSpeed;

        [Tooltip("Минимальный инпут для начала движения")]
        [Min(0.001f)] public float MinMoveThreshold;

        [Min(0f)] public float BaseAcceleration;
        [SerializeField] internal AnimationCurve AccelerationCurve;

        [Min(0f)] public float BaseDeceleration;
        [SerializeField] internal AnimationCurve DecelerationCurve;
    }

    [Serializable]
    public struct JumpSettings
    {
        public bool CanJump;
        public bool EnableAutoJump;

        [Min(0f)] public float JumpForce;

        [Tooltip("Сколько секунд помним нажатие прыжка")]
        [Min(0f)] public float JumpBufferTime;

        [Tooltip("Время для прыжка после падения с уступа")]
        [Min(0f)] public float CoyoteTime;

        [Range(0f, 5f)] public float AirControlMultiplier;
    }

    [Serializable]
    public struct CrouchSettings
    {
        public bool CanCrouch;
        public bool CanJumpWhileCrouching;

        [Min(0f)] public float CrouchSpeed;
        [Min(0.1f)] public float StandingHeight;
        [Min(0.1f)] public float CrouchHeight;

        public LayerMask EnvironmentMask;
    }

    [Serializable]
    public struct PhysicsSettings
    {
        public bool UseGravity;

        [Tooltip("Гравитация всегда тянет вниз (отрицательная)")]
        public float Gravity;
        public float TerminalVelocity;
        public float GroundedGravity;
        [Min(0.1f)] public float FallMultiplier;
    }

    [Serializable]
    public struct RotationSettings
    {
        public bool RotateWithMovement;
        [Range(0.01f, 0.5f)] public float MovementRotationSmoothTime;
        [Range(0.01f, 0.5f)] public float StrafeRotationSmoothTime;
    }

    [Serializable]
    public struct ThresholdSettings
    {
        [Min(0.001f)] public float InputThreshold;
        public float InputThresholdSq => InputThreshold * InputThreshold;

        [Min(0.01f)] public float VerticalVelocityThreshold;

        [Range(0.01f, 1f)] public float IdleTransitionMultiplier;
    }

    [Serializable]
    public struct CharacterSettingsData
    {
        public MovementSettings Movement;
        public JumpSettings Jump;
        public CrouchSettings Crouch;
        public PhysicsSettings Physics;
        public RotationSettings Rotation;
        public ThresholdSettings Thresholds;
    }

    [CreateAssetMenu(fileName = "CharacterSettings", menuName = "SAWC/Character Settings")]
    public class CharacterSettings : ScriptableObject
    {
        public CharacterSettingsData Data = new CharacterSettingsData
        {
            Movement = new MovementSettings
            {
                CanMove = true,
                CanSprint = true,
                AllowedSprintDirections = SprintAllowedDirections.Forward,
                UseInertia = true,
                MoveSpeed = 3f,
                SprintSpeed = 6f,
                MinMoveThreshold = 0.5f,
                BaseAcceleration = 30f,
                BaseDeceleration = 30f
            },
            Jump = new JumpSettings
            {
                CanJump = true,
                EnableAutoJump = false,
                JumpForce = 5f,
                JumpBufferTime = 0.2f,
                CoyoteTime = 0.3f,
                AirControlMultiplier = 0.8f
            },
            Crouch = new CrouchSettings
            {
                CanCrouch = true,
                CanJumpWhileCrouching = false,
                CrouchSpeed = 1f,
                StandingHeight = 2f,
                CrouchHeight = 1.1f,
                EnvironmentMask = 1
            },
            Physics = new PhysicsSettings
            {
                UseGravity = true,
                Gravity = -9.8f,
                TerminalVelocity = -50f,
                GroundedGravity = -2f,
                FallMultiplier = 2f
            },
            Rotation = new RotationSettings
            {
                RotateWithMovement = false,
                MovementRotationSmoothTime = 0.165f,
                StrafeRotationSmoothTime = 0.01f
            },
            Thresholds = new ThresholdSettings
            {
                InputThreshold = 0.01f,
                VerticalVelocityThreshold = 0.1f,
                IdleTransitionMultiplier = 0.8f
            }
        };

        private void Reset()
        {
            Data.Movement.AccelerationCurve = AnimationCurve.Linear(0f, 1f, 1f, 0.2f);
            Data.Movement.DecelerationCurve = AnimationCurve.Linear(0f, 1f, 1f, 0.5f);
        }

        private void OnValidate()
        {
            if (Data.Movement.MoveSpeed < 0.01f) Data.Movement.MoveSpeed = 0.01f;
            if (Data.Movement.SprintSpeed < 0.01f) Data.Movement.SprintSpeed = 0.01f;

            if (Data.Movement.SprintSpeed < Data.Movement.MoveSpeed)
                Data.Movement.SprintSpeed = Data.Movement.MoveSpeed;

            if (Data.Crouch.CrouchSpeed > Data.Movement.MoveSpeed)
                Data.Crouch.CrouchSpeed = Data.Movement.MoveSpeed;

            if (Data.Crouch.CrouchHeight > Data.Crouch.StandingHeight)
                Data.Crouch.CrouchHeight = Data.Crouch.StandingHeight;

            if (Data.Physics.GroundedGravity > 0f)
                Data.Physics.GroundedGravity = -Data.Physics.GroundedGravity;

            if (Data.Physics.TerminalVelocity > 0f)
                Data.Physics.TerminalVelocity = -Data.Physics.TerminalVelocity;

            if (Data.Movement.AllowedSprintDirections == SprintAllowedDirections.None)
            {
                Data.Movement.AllowedSprintDirections = SprintAllowedDirections.Forward;
            }
        }
    }
}