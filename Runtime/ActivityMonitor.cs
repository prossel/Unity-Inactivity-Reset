using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace InactivityReset
{
    public sealed class ActivityMonitor : MonoBehaviour
    {
        public event Action OnActivity;

        private void OnEnable()
        {
            InputSystem.onEvent += HandleInputEvent;
        }

        private void OnDisable()
        {
            InputSystem.onEvent -= HandleInputEvent;
        }

        private void Update()
        {
            var touchscreen = Touchscreen.current;
            if (touchscreen != null && touchscreen.touches.Count > 0)
            {
                foreach (var touch in touchscreen.touches)
                {
                    if (touch.press.isPressed)
                    {
                        NotifyActivity();
                        return;
                    }
                }
            }

            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.anyKey.wasPressedThisFrame)
            {
                NotifyActivity();
                return;
            }

            var mouse = Mouse.current;
            if (mouse != null)
            {
                if (mouse.leftButton.wasPressedThisFrame || mouse.leftButton.isPressed)
                {
                    NotifyActivity();
                    return;
                }

                var delta = mouse.delta.ReadValue();
                if (Mathf.Abs(delta.x) > 0.01f || Mathf.Abs(delta.y) > 0.01f)
                {
                    NotifyActivity();
                }
            }
        }

        private void HandleInputEvent(InputEventPtr eventPtr, InputDevice device)
        {
            if (eventPtr == default)
            {
                return;
            }

            if (device is Mouse || device is Keyboard || device is Touchscreen || device is Gamepad || device is Pen)
            {
                NotifyActivity();
            }
        }

        public void NotifyActivity()
        {
            OnActivity?.Invoke();
        }
    }
}
