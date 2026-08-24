using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace JTH.Scripts.Input
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
            InputSystem.onAfterUpdate -= SamplePointer;
            InputSystem.onAfterUpdate += SamplePointer;
        }

        private void OnDisable()
        {
            InputSystem.onAfterUpdate -= SamplePointer;
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
        }

        public void OnPointerPress(InputAction.CallbackContext context)
        {
        }

        public void Tick()
        {
            SamplePointer();
        }

        public Vector3 GetWorldPointerPosition()
        {
            _screenPointerPosition = ReadScreenPointer(_screenPointerPosition);

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

        private void SamplePointer()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            Vector2 screenPos = ReadScreenPointer(_screenPointerPosition);
            bool pressed = IsTouchHeld() || IsMouseHeld();
            bool moved = screenPos != _screenPointerPosition;
            _screenPointerPosition = screenPos;

            if (pressed)
            {
                _isPointerPressed = true;
                if (moved)
                {
                    OnPointerChange?.Invoke(_screenPointerPosition);
                }

                return;
            }

            if (!_isPointerPressed)
            {
                return;
            }

            _isPointerPressed = false;
            OnPointerReleased?.Invoke();
        }

        private static Vector2 ReadScreenPointer(Vector2 fallback)
        {
            if (TryReadTouchPosition(out Vector2 touchPosition))
            {
                return touchPosition;
            }

            Mouse mouse = Mouse.current;
            return mouse != null ? mouse.position.ReadValue() : fallback;
        }

        private static bool IsTouchHeld()
        {
            return TryReadTouchPosition(out _);
        }

        private static bool IsMouseHeld()
        {
            Mouse mouse = Mouse.current;
            return mouse != null && mouse.leftButton.isPressed;
        }

        private static bool TryReadTouchPosition(out Vector2 position)
        {
            Touchscreen touchscreen = Touchscreen.current;
            if (touchscreen == null)
            {
                position = default;
                return false;
            }

            if (touchscreen.primaryTouch.press.isPressed)
            {
                position = touchscreen.primaryTouch.position.ReadValue();
                return true;
            }

            foreach (TouchControl touch in touchscreen.touches)
            {
                if (!touch.press.isPressed)
                {
                    continue;
                }

                position = touch.position.ReadValue();
                return true;
            }

            position = default;
            return false;
        }
    }
}
