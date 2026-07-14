using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

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
        public event Action OnPointerPressed;
        public event Action OnPointerReleased;

        private Controls _controls;
        private Vector2 _screenPointerPosition;
        private Vector3 _worldPointerPosition;
        private bool _isPointerPressed;

        public bool IsPointerPressed => _isPointerPressed;

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

        public void OnSelectSlot(InputAction.CallbackContext context)
        {
            if (!context.performed)
            {
                return;
            }

            if (context.control is not KeyControl keyControl)
            {
                return;
            }

            int? slotIndex = keyControl.keyCode switch
            {
                Key.Digit1 or Key.Numpad1 => 0,
                Key.Digit2 or Key.Numpad2 => 1,
                Key.Digit3 or Key.Numpad3 => 2,
                Key.Digit4 or Key.Numpad4 => 3,
                _ => null
            };

            if (slotIndex.HasValue)
            {
                OnSlotSelected?.Invoke(slotIndex.Value);
            }
        }

        public void OnPointer(InputAction.CallbackContext context)
        {
            _screenPointerPosition = context.ReadValue<Vector2>();
            OnPointerChange?.Invoke(_screenPointerPosition);
        }

        public void OnPointerPress(InputAction.CallbackContext context)
        {
            if (context.started || context.performed)
            {
                _screenPointerPosition = _controls.Player.Pointer.ReadValue<Vector2>();

                if (!_isPointerPressed)
                {
                    _isPointerPressed = true;
                    OnPointerPressed?.Invoke();
                }

                return;
            }

            if (context.canceled && _isPointerPressed)
            {
                _isPointerPressed = false;
                OnPointerReleased?.Invoke();
            }
        }

        public Vector3 GetWorldPointerPosition()
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                camera = UnityEngine.Object.FindFirstObjectByType<Camera>();
            }

            if (camera == null)
            {
                Debug.LogWarning("[MagnetInputSO] Main Camera not found. Tag scene camera as MainCamera.", this);
                return _worldPointerPosition;
            }

            float depth = Mathf.Abs(camera.transform.position.z);
            _worldPointerPosition = camera.ScreenToWorldPoint(
                new Vector3(_screenPointerPosition.x, _screenPointerPosition.y, depth));
            return _worldPointerPosition;
        }
    }
}
