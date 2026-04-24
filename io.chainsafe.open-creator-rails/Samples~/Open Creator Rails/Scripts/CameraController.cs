using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Io.ChainSafe.OpenCreatorRails.Samples
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Transform _playerTransform;
        
        [SerializeField] private Vector3 _followOffset = new Vector3(0f, 1f, - 5f);
        
        [SerializeField] private InputAction _lookInputAction;
        
        [SerializeField, Range(0f, 90f)] private float _pitchLimit = 45f;
        
        [SerializeField, Range(0f, 1f)] private float _lookSensitivity = 0.5f;

        private float _pitch;
        private float _yaw;
        
        private void OnEnable()
        {
            _lookInputAction.Enable();
        }

        private void OnDisable()
        {
            _lookInputAction.Disable();
        }

        private void LateUpdate()
        {
            Vector2 lookInput = _lookInputAction.ReadValue<Vector2>() * _lookSensitivity;

            Vector3 target = _playerTransform.position + new Vector3(_followOffset.x, _followOffset.y, 0f);
            
            _pitch = Mathf.Clamp(_pitch + lookInput.y, - _pitchLimit, _pitchLimit);

            _yaw += lookInput.x;
            
            Quaternion rotation = Quaternion.Euler((Vector3.up * _yaw) + (-Vector3.right * _pitch));

            Vector3 forward = rotation * Vector3.forward;
        
            transform.position = target + (- forward * - _followOffset.z);

            transform.rotation = rotation;
        }
    }
}