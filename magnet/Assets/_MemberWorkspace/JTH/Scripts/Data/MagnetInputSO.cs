using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace JTH.Scripts.Data
{
    /// <summary>
    /// Input System 콜백을 SO에서 소유하고 C# event로 노출한다.
    /// MonoBehaviour는 이 SO를 [SerializeField]로 참조해 구독만 한다.
    /// </summary>
    [CreateAssetMenu(fileName = "MagnetInput", menuName = "Magnet/Input")]
    public sealed class MagnetInputSO : ScriptableObject, Controls.IPlayerActions
    {
        public event Action<int> OnSlotSelected;
        public event Action<Vector2> OnPointerChange;

        private Controls _controls;
        private Vector2 _screenPointerPosition;
        private Vector3 _worldPointerPosition;
        private Camera _mainCam;

        public Camera MainCam
        {
            get
            {
                if (_mainCam == null)
                {
                    _mainCam = Camera.main;
                }

                return _mainCam;
            }
        }

        private void OnEnable()
        {
            if (_controls == null)
            {
                _controls = new Controls();
                _controls.Player.SetCallbacks(this);
            }

            _controls.Enable();
        }

        private void OnDisable()
        {
            _controls?.Disable();
        }

        public void OnSelectSlot1(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                OnSlotSelected?.Invoke(0);
            }
        }

        public void OnSelectSlot2(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                OnSlotSelected?.Invoke(1);
            }
        }

        public void OnSelectSlot3(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                OnSlotSelected?.Invoke(2);
            }
        }

        public void OnPointer(InputAction.CallbackContext context)
        {
            _screenPointerPosition = context.ReadValue<Vector2>();
            OnPointerChange?.Invoke(_screenPointerPosition);
        }

        public Vector3 GetWorldPointerPosition()
        {
            if (MainCam == null)
            {
                return _worldPointerPosition;
            }

            float depth = Mathf.Abs(MainCam.transform.position.z);
            _worldPointerPosition = MainCam.ScreenToWorldPoint(
                new Vector3(_screenPointerPosition.x, _screenPointerPosition.y, depth));
            return _worldPointerPosition;
        }
    }
}
