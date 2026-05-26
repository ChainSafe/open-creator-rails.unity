using Io.ChainSafe.OpenCreatorRails.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Io.ChainSafe.OpenCreatorRails.Samples
{
    public class Player : Singleton<Player>
    {
        [field: SerializeField] public string SubscriberId { get; private set; } = "player_subscriber_id";
        
        [field: SerializeField] public InputAction PauseInputAction { get; private set; }
        
        [field: SerializeField] public InputAction InteractInputAction { get; private set; }
        
        [field: SerializeField] public InputAction LookInputAction { get; private set; }
        
        [field: SerializeField] public InputAction MovementInputAction { get; private set; }

        public bool Interactable { get; set; }

        public bool Paused { get; private set; }

        private void OnEnable()
        {
            Enable();
            
            PauseInputAction.performed += PausePerformed;
        }

        private void PausePerformed(InputAction.CallbackContext _)
        {
            UIController.Instance.LoadWithModel<LandingController, LandingModel>(new LandingModel());
        }

        private void Enable()
        {
            PauseInputAction.Enable();
            InteractInputAction.Enable();
            LookInputAction.Enable();
            MovementInputAction.Enable();
        }
        
        private void Disable()
        {
            PauseInputAction.Disable();
            InteractInputAction.Disable();
            LookInputAction.Disable();
            MovementInputAction.Disable();
        }
        
        private void OnDisable()
        {
            PauseInputAction.performed -= PausePerformed;
            
            Disable();
        }

        public void Pause()
        {
            Time.timeScale = 0;
            
            Disable();

            Paused = true;
        }
        
        public void Resume()
        {
            Time.timeScale = 1;
            
            Enable();
            
            Paused = false;
        }
    }
}