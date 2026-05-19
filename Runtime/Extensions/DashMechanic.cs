using UnityEngine;
using SAWC.Core;
using SAWC.Pipeline;
public class DashMechanic : VelocityModifierBase
{
    public override int Priority => 100;

    [Header("Настройки")]
    [SerializeField] private KeyCode _dashKey = KeyCode.LeftControl;
    [SerializeField] private float _dashSpeed = 25f;
    [SerializeField] private float _dashDuration = 0.2f;
    [SerializeField] private float _cooldown = 1f;
    [SerializeField] private float _inputBufferTime = 0.15f;

    private float _dashTimer;
    private float _cooldownEndTime;
    private float _bufferEndTime;
    private Vector3 _dashDirection;

    private bool IsDashing => _dashTimer > 0f;

    private void Update()
    {
        if (Input.GetKeyDown(_dashKey) && Time.time >= _cooldownEndTime)
        {
            _bufferEndTime = Time.time + _inputBufferTime;
        }

        bool isMoving = Controller.State.IntendedMoveDirection.sqrMagnitude > 0.01f;

        if (!IsDashing && Time.time < _bufferEndTime && isMoving)
        {
            _dashTimer = _dashDuration;
            _cooldownEndTime = Time.time + _cooldown;
            _bufferEndTime = 0f;
            _dashDirection = Vector3.zero;
        }
    }

    public override Vector3 ModifyVelocity(Vector3 currentVelocity, ref FrameContext ctx)
    {
        if (!IsDashing) return currentVelocity;

        if (_dashDirection == Vector3.zero)
        {
            _dashDirection = ctx.WorldMoveDirection;
            
            if (_dashDirection == Vector3.zero) 
            {
                _dashTimer = 0f; 
                return currentVelocity;
            }
        }

        _dashTimer -= ctx.DeltaTime;
        if (_dashTimer <= 0f) return currentVelocity;

        return new Vector3(_dashDirection.x * _dashSpeed, 0f, _dashDirection.z * _dashSpeed);
    }
}