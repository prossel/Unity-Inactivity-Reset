using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InactivityReset
{
    public sealed class FourCornerGestureDetector : MonoBehaviour
    {
        public delegate void FourCornerGestureHandler();
        public event FourCornerGestureHandler OnFourCornerGesture;

        [SerializeField] private float cornerMarginRatio = 0.1f;
        [SerializeField] private float requiredSimultaneousWindow = 0.15f;
        [SerializeField] private bool enableEditorMouseSimulation = true;

        private float? gestureStartTime;

        private void Update()
        {
#if UNITY_EDITOR
            if (enableEditorMouseSimulation && Keyboard.current != null && Keyboard.current.leftAltKey.isPressed)
            {
                SimulateCornerGesture();
                return;
            }
#endif

            if (TryGetTouchPositions(out var touchPositions))
            {
                if (HasFourCornerGesture(touchPositions))
                {
                    if (!gestureStartTime.HasValue)
                    {
                        gestureStartTime = Time.unscaledTime;
                    }

                    if (Time.unscaledTime - gestureStartTime.Value >= requiredSimultaneousWindow)
                    {
                        OnFourCornerGesture?.Invoke();
                        gestureStartTime = null;
                    }
                }
                else
                {
                    gestureStartTime = null;
                }
            }
            else
            {
                gestureStartTime = null;
            }
        }

        public void SimulateCornerGesture()
        {
            OnFourCornerGesture?.Invoke();
        }

        private bool TryGetTouchPositions(out List<Vector2> touchPositions)
        {
            touchPositions = new List<Vector2>();

            if (Touchscreen.current != null)
            {
                foreach (var touch in Touchscreen.current.touches)
                {
                    if (touch != null && touch.press.isPressed)
                    {
                        touchPositions.Add(touch.position.ReadValue());
                    }
                }

                return touchPositions.Count > 0;
            }

            return false;
        }

        private bool HasFourCornerGesture(IReadOnlyList<Vector2> touchPositions)
        {
            if (touchPositions.Count < 4)
            {
                return false;
            }

            var margin = new Vector2(Screen.width * cornerMarginRatio, Screen.height * cornerMarginRatio);
            var detectedCorners = new HashSet<int>();

            foreach (var position in touchPositions)
            {
                var cornerIndex = GetClosestCornerIndex(position, margin);
                if (cornerIndex < 0)
                {
                    return false;
                }

                if (!detectedCorners.Add(cornerIndex))
                {
                    return false;
                }
            }

            return detectedCorners.Count == 4;
        }

        private int GetClosestCornerIndex(Vector2 position, Vector2 margin)
        {
            var cornerPositions = new[]
            {
                new Vector2(margin.x, margin.y),
                new Vector2(Screen.width - margin.x, margin.y),
                new Vector2(margin.x, Screen.height - margin.y),
                new Vector2(Screen.width - margin.x, Screen.height - margin.y)
            };

            var bestIndex = -1;
            var bestDistance = float.MaxValue;

            for (var i = 0; i < cornerPositions.Length; i++)
            {
                var distance = Vector2.Distance(position, cornerPositions[i]);
                if (distance <= bestDistance)
                {
                    bestDistance = distance;
                    bestIndex = i;
                }
            }

            if (bestDistance > Mathf.Max(margin.x, margin.y))
            {
                return -1;
            }

            return bestIndex;
        }
    }
}
