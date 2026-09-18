using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InactivityReset
{
    public sealed class InactivityResetOverlay : MonoBehaviour
    {
        private const float DarkOverlayAlpha = 0.7f;

        private Canvas canvas;
        private Image backgroundImage;
        private TextMeshProUGUI countdownText;
        private Button countdownButton;
        private Button cancelButton;
        private InactivityResetManager manager;

        public void Initialize(InactivityResetManager owner)
        {
            manager = owner;
            BuildUI();
            Hide();
        }

        public void Show(float remainingSeconds)
        {
            if (canvas == null)
            {
                BuildUI();
            }

            gameObject.SetActive(true);
            UpdateCountdown(remainingSeconds);
        }

        public void Hide()
        {
            if (gameObject != null)
            {
                gameObject.SetActive(false);
            }
        }

        public void UpdateCountdown(float remainingSeconds)
        {
            if (countdownText == null)
            {
                return;
            }

            var displayValue = Mathf.CeilToInt(Mathf.Max(0f, remainingSeconds));
            countdownText.text = displayValue.ToString();
        }

        private void BuildUI()
        {
            gameObject.name = "InactivityResetOverlay";
            transform.SetAsLastSibling();

            var rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = gameObject.AddComponent<RectTransform>();
            }

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
            }

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 2000;

            if (GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }

            var scaler = GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = gameObject.AddComponent<CanvasScaler>();
            }

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            backgroundImage = CreateImageChild("Background", transform, new Color(0f, 0f, 0f, DarkOverlayAlpha));
            backgroundImage.raycastTarget = true;
            var backgroundButton = backgroundImage.gameObject.AddComponent<Button>();
            backgroundButton.transition = Selectable.Transition.None;
            backgroundButton.targetGraphic = backgroundImage;
            backgroundButton.onClick.AddListener(() => manager.CancelCountdown());

            countdownButton = CreateButton("CountdownButton", transform, new Color(0f, 0f, 0f, 0f));
            countdownButton.onClick.AddListener(() => manager.TriggerReset());
            countdownButton.targetGraphic = countdownButton.GetComponent<Image>();

            countdownText = CreateTextChild("CountdownText", countdownButton.transform, "5");
            countdownText.alignment = TextAlignmentOptions.Center;
            countdownText.fontSize = 120;
            countdownText.color = new Color(0.85f, 0.85f, 0.85f, 1f);

            var countdownRect = countdownText.GetComponent<RectTransform>();
            countdownRect.anchorMin = Vector2.zero;
            countdownRect.anchorMax = Vector2.one;
            countdownRect.offsetMin = Vector2.zero;
            countdownRect.offsetMax = Vector2.zero;

            cancelButton = CreateButton("CancelButton", transform, new Color(0f, 0f, 0f, 0f));
            cancelButton.onClick.AddListener(() => manager.CancelCountdown());

            var cancelText = CreateTextChild("CancelText", cancelButton.transform, "X");
            cancelText.alignment = TextAlignmentOptions.Center;
            cancelText.fontSize = 72;
            cancelText.color = new Color(0.85f, 0.85f, 0.85f, 1f);

            var cancelRect = cancelText.GetComponent<RectTransform>();
            cancelRect.anchorMin = new Vector2(0.5f, 0.5f);
            cancelRect.anchorMax = new Vector2(0.5f, 0.5f);
            cancelRect.sizeDelta = new Vector2(60f, 60f);
            cancelRect.anchoredPosition = new Vector2(0f, 0f);

            SetRect(backgroundImage.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            SetRect(countdownButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(220f, 120f));
            SetRect(cancelButton.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-80f, -80f), new Vector2(60f, 60f));
        }

        private static Image CreateImageChild(string name, Transform parent, Color color)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent, false);
            var image = child.AddComponent<Image>();
            image.color = color;
            var rect = child.GetComponent<RectTransform>();
            SetRect(rect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return image;
        }

        private static Button CreateButton(string name, Transform parent, Color color)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent, false);
            var rect = child.AddComponent<RectTransform>();
            var image = child.AddComponent<Image>();
            image.color = color;
            var button = child.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.targetGraphic = image;
            SetRect(rect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return button;
        }

        private static TextMeshProUGUI CreateTextChild(string name, Transform parent, string text)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent, false);
            var rect = child.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var label = child.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.raycastTarget = false;
            return label;
        }

        private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
        }
    }
}
