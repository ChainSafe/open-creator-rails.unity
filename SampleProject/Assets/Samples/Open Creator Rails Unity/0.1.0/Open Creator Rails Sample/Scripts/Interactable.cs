using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Io.ChainSafe.OpenCreatorRails.Samples
{
    [RequireComponent(typeof(Collider))]
    public abstract class Interactable : MonoBehaviour
    {
        private Collider _collider;

        private bool _enabled;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            
            _collider.isTrigger = true;
        }
        
        private void OnEnable()
        {
            Player.Instance.InteractInputAction.performed += InputPerformed;
        }
        
        private void OnDisable()
        {
            Player.Instance.InteractInputAction.performed -= InputPerformed;
        }

        private void InputPerformed(InputAction.CallbackContext _)
        {
            TryInteract();
        }

        protected virtual void Enable()
        {
            _enabled = true;
        }

        protected virtual void Disable()
        {
            _enabled = false;
        }

        private void TryInteract()
        {
            if (_enabled)
            {
                Interact();
            }
        }
        
        protected abstract void Interact();

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out Player player))
            {
                Enable();
                
                player.Interactable = _enabled;
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out Player player))
            {
                Disable();
                
                player.Interactable = _enabled;
            }
        }
    }
}