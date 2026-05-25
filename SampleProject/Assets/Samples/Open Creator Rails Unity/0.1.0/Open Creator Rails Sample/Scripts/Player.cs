using Io.ChainSafe.OpenCreatorRails.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Io.ChainSafe.OpenCreatorRails.Samples
{
    public class Player : Singleton<Player>
    {
        [field: SerializeField] public string SubscriberId { get; private set; } = "player_subscriber_id";
        
        [field: SerializeField] public InputAction InteractInputAction { get; private set; }
        
        [field: SerializeField] public InputAction LookInputAction { get; private set; }
        
        [field: SerializeField] public InputAction MovementInputAction { get; private set; }

        public bool Interactable { get; set; }

        private void OnEnable()
        {
            Enable();
        }

        private void Enable()
        {
            InteractInputAction.Enable();
            LookInputAction.Enable();
            MovementInputAction.Enable();
        }
        
        private void Disable()
        {
            InteractInputAction.Disable();
            LookInputAction.Disable();
            MovementInputAction.Disable();
        }
        
        private void OnDisable()
        {
            Disable();
        }

        public void Pause()
        {
            Time.timeScale = 0;
            
            Disable();
        }
        
        public void Resume()
        {
            Time.timeScale = 1;
            
            Enable();
        }
    }
}