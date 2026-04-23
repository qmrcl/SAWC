using UnityEngine;

namespace SAWC.Core
{
    [CreateAssetMenu(fileName = "CharacterSettings", menuName = "SAWC/Character Settings")]
    public class CharacterSettings : ScriptableObject
    {
        [Header("Movement")]
        public bool CanJump = true;
        public bool CanSprint = true;

        [Tooltip("Если включено, персонаж будет прыгать без остановки, пока зажата кнопка.")]
        public bool EnableAutoJump = false;

        public float MoveSpeed = 5f;

        public float SprintSpeed = 10f;

        public float JumpForce = 6f;

        [Range(-30, 0f)]
        public float Gravity = -9.81f;
        public float TerminalVelocity = -50f;

        [Header("Acceleration & Air Control")]
        [Range(1f, 100f)]
        [Tooltip("Насколько быстро персонаж набирает скорость")]
        public float Acceleration = 25f;

        [Range(1f, 100f)]
        [Tooltip("Насколько быстро персонаж замедляется")]
        public float Deceleration = 35f;

        [Range(0f, 1f)]
        [Tooltip("Множитель контроля в воздухе (0 = нет контроля, 1 = полный контроль)")]
        public float AirControlMultiplier = 0.5f;

        [Header("Rotation")]
        [Tooltip("ВКЛ = Персонаж крутится куда идет (Adventure). ВЫКЛ = Персонаж всегда смотрит за камерой (Strafe/Combat)")]
        public bool RotateWithMovement = false;

        [Range(0.01f, 0.5f)]
        [Tooltip("Плавность поворота при БЕГЕ (Adventure). Чем больше, тем плавнее.")]
        public float MovementRotationSmoothTime = 0.12f;

        [Range(0.01f, 0.5f)]
        [Tooltip("Плавность поворота за КАМЕРОЙ (Strafe). Тут лучше ставить меньше, чтобы было резче.")]
        public float StrafeRotationSmoothTime = 0.05f;

        [Header("Physics")]
        [Range(-10f, 0f)]
        [Tooltip("Гравитация когда персонаж на земле (должна быть отрицательной)")]
        public float GroundedGravity = -2f;

        [Range(1f, 5f)]
        [Tooltip("Множитель гравитации при падении (больше = падаем быстрее)")]
        public float FallMultiplier = 2.5f;

        private void OnValidate()
        {
            if (SprintSpeed < MoveSpeed) SprintSpeed = MoveSpeed;
            if (Gravity > 0f) Gravity = -Mathf.Abs(Gravity);
            if (GroundedGravity > 0f) GroundedGravity = -Mathf.Abs(GroundedGravity);
            if (JumpForce < 0f) JumpForce = Mathf.Abs(JumpForce);
        }
    }
}