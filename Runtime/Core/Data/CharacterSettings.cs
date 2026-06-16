using SAWC.Localization;
using System;
using UnityEngine;

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

        [Min(0.001f)] public float MinMoveThreshold;

        [Loc]
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
        [Min(0f)] public float JumpBufferTime;
        [Min(0f)] public float CoyoteTime;
        [Range(0f, 5f)] public float AirControlMultiplier;

        public float CeilingBounceVelocity;
        [Min(0f)] public float JumpCooldownDuration;
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

        public float FallVelocityThreshold;

        [Range(0f, 1f)] public float AirStateDebounceTime;
        [Range(0f, 1f)] public float SprintDirectionThreshold;

        [Min(0.001f)] public float VelocityThreshold;
    }

    [Serializable]
    public struct CharacterSettingsData
    {
        [Loc] public MovementSettings Movement;
        [Loc] public JumpSettings Jump;
        [Loc] public CrouchSettings Crouch;
        [Loc] public PhysicsSettings Physics;
        [Loc] public RotationSettings Rotation;
        [Loc] public ThresholdSettings Thresholds;
    }

    [CreateAssetMenu(fileName = "CharacterSettings", menuName = "SAWC/Core/Character Settings")]
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
                EnableAutoJump = true,
                JumpForce = 4f,
                JumpBufferTime = 0.2f,
                CoyoteTime = 0.2f,
                AirControlMultiplier = 0.6f,
                CeilingBounceVelocity = -1.5f,
                JumpCooldownDuration = 0.1f
            },
            Crouch = new CrouchSettings
            {
                CanCrouch = true,
                CanJumpWhileCrouching = false,
                CrouchSpeed = 1f,
                StandingHeight = 2f,
                CrouchHeight = 1f,
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
                InputThreshold = 0.05f, 
                VerticalVelocityThreshold = 0.1f,
                IdleTransitionMultiplier = 0.2f,
                FallVelocityThreshold = -0.1f,
                AirStateDebounceTime = 0.2f,
                SprintDirectionThreshold = 0.38f,
                VelocityThreshold = 0.1f
            }
        };

        private void Reset()
        {
            Data.Movement.AccelerationCurve = AnimationCurve.Linear(0f, 1f, 1f, 0.2f);
            Data.Movement.DecelerationCurve = AnimationCurve.Linear(0f, 1f, 1f, 0.5f);
        }
    }
}