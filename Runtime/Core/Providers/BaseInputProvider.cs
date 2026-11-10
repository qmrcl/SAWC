using UnityEngine;

namespace SAWC.Core.Input
{
    public abstract class BaseInputProvider : MonoBehaviour, IInputProvider
    {
        [SerializeField] protected Transform _cameraTransform;

        public Vector3 WorldMoveDirection { get; private set; }
        public Vector3 WorldLookDirection { get; private set; }

        public abstract Vector2 MoveInput { get; }
        public abstract bool JumpHeld { get; }
        public abstract bool SprintHeld { get; }
        public abstract bool CrouchHeld { get; }

        protected virtual void Awake()
        {
            if (_cameraTransform == null && Camera.main != null)
                _cameraTransform = Camera.main.transform;
        }

        protected virtual void Update()
        {
            CalculateWorldDirection();
        }

        protected virtual void OnDisable()
        {
            WorldMoveDirection = Vector3.zero;
        }

        private void CalculateWorldDirection()
        {
            if (_cameraTransform == null && Camera.main != null)
                _cameraTransform = Camera.main.transform;

            if (_cameraTransform == null)
            {
                WorldMoveDirection = Vector3.zero;
                WorldLookDirection = transform.forward;
                return;
            }

            // Build a yaw-only basis from the view direction. Flattening camera-right
            // instead couples camera roll and pitch into the movement heading.
            Vector3 camForward = Vector3.ProjectOnPlane(_cameraTransform.forward, Vector3.up);
            Vector3 camRight;
            if (camForward.sqrMagnitude > 0.000001f)
            {
                camForward.Normalize();
                camRight = Vector3.Cross(Vector3.up, camForward);
            }
            else
            {
                // At a vertical view angle forward has no horizontal heading.
                camRight = Vector3.ProjectOnPlane(_cameraTransform.right, Vector3.up).normalized;
                camForward = Vector3.Cross(camRight, Vector3.up);
            }
            WorldLookDirection = camForward;

            Vector2 currentInput = MoveInput;
            if (currentInput.sqrMagnitude < 0.001f)
            {
                WorldMoveDirection = Vector3.zero;
                return;
            }

            Vector3 direction = camRight * currentInput.x + camForward * currentInput.y;
            WorldMoveDirection = Vector3.ClampMagnitude(direction, 1f);
        }
    }
}
