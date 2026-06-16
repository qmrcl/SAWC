using SAWC.Core;
using SAWC.Localization;
using UnityEngine;

namespace SAWC.Modules.CameraUtils
{
    [AddComponentMenu("SAWC/Modules/Camera Height Offset")]
    public class CameraHeightOffset : MonoBehaviour
    {
        [SerializeField, Loc] private SAWController _controller;

        [Space(5)]
        [SerializeField, Loc] private float _standingHeight = 1.6f;
        [SerializeField, Loc] private float _crouchingHeight = 0.6f;
        [SerializeField, Loc] private float _smoothTime = 0.1f;

        private float _velocity;

        private void Awake()
        {
            if (_controller == null)
                Debug.LogError($"Camera requires a {nameof(SAWController)} component!", this);
        }

        private void Update()
        {
            if (_controller == null || _controller.State == null) return;

            float targetY = _controller.State.IsCrouching ? _crouchingHeight : _standingHeight;

            Vector3 localPos = transform.localPosition;
            localPos.y = Mathf.SmoothDamp(localPos.y, targetY, ref _velocity, _smoothTime);
            transform.localPosition = localPos;
        }
    }
}