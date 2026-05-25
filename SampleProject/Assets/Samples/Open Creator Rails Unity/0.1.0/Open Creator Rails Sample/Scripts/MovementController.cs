using UnityEngine;
using UnityEngine.InputSystem;

namespace Io.ChainSafe.OpenCreatorRails.Samples
{
    [RequireComponent(typeof(Rigidbody))]
    public class MovementController : MonoBehaviour
    {
        [SerializeField] private float _speed = 5f;

        private Vector3 _moveDirection;

        private Rigidbody _rigidbody;

        private Transform _cameraTransform;

        private InputAction MovementInputAction => Player.Instance.MovementInputAction;
        
        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            _cameraTransform = Camera.main?.transform;
        }

        private void Update()
        {
            Vector2 moveInput = MovementInputAction.ReadValue<Vector2>();
            
            Vector3 forward = _cameraTransform.forward;
            Vector3 right = _cameraTransform.right;

            forward.y = 0;
            right.y = 0;

            _moveDirection = (moveInput.x * right + moveInput.y * forward).normalized;

            if (_moveDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(_moveDirection), Time.deltaTime * _speed);
            }
        }

        private void FixedUpdate()
        {
            _rigidbody.linearVelocity = _moveDirection * _speed;
        }
    }
}