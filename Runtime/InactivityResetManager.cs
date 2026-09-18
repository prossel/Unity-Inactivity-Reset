using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace InactivityReset
{
    public sealed class InactivityResetManager : MonoBehaviour
    {
        public enum State
        {
            Monitoring,
            Countdown,
            Resetting
        }

        public static InactivityResetManager Instance { get; private set; }

        [Header("Defaults")]
        [SerializeField] private float defaultIdleTimeoutSeconds = 60f;
        [SerializeField] private float defaultCountdownDurationSeconds = 5f;

        [Header("Reset Target")]
        [SerializeField] private string resetSceneOverride = string.Empty;

        [Header("Runtime")]
        [SerializeField] private bool autoCreateOverlay = true;

        [Header("Events")]
        [SerializeField] private UnityEvent onBeforeReset = new UnityEvent();
        [SerializeField] private UnityEvent onAfterReset = new UnityEvent();

        public UnityEvent OnBeforeReset => onBeforeReset;
        public UnityEvent OnAfterReset => onAfterReset;

        private State currentState = State.Monitoring;
        private ActivityMonitor activityMonitor;
        private FourCornerGestureDetector fourCornerGestureDetector;
        private InactivityResetOverlay overlay;
        private float idleTimer;
        private float countdownRemaining;
        private string installSceneName;

        public float IdleTimeoutSeconds { get; private set; } = 60f;
        public float CountdownDurationSeconds { get; private set; } = 5f;
        public State CurrentState => currentState;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            installSceneName = SceneManager.GetActiveScene().name;
            if (string.IsNullOrEmpty(resetSceneOverride))
            {
                resetSceneOverride = installSceneName;
            }

            ResolveConfiguredDurations();
            InitializeRuntimeComponents();

            currentState = State.Monitoring;
            idleTimer = 0f;
            countdownRemaining = CountdownDurationSeconds;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;

            if (activityMonitor != null)
            {
                activityMonitor.OnActivity -= HandleActivity;
            }

            if (fourCornerGestureDetector != null)
            {
                fourCornerGestureDetector.OnFourCornerGesture -= HandleFourCornerGesture;
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                ResolveConfiguredDurations();
                ResetIdleTimer();
            }
        }

        private void Update()
        {
            if (currentState == State.Monitoring)
            {
                idleTimer += Time.unscaledDeltaTime;
                if (idleTimer >= IdleTimeoutSeconds)
                {
                    StartCountdown();
                }
                return;
            }

            if (currentState == State.Countdown)
            {
                countdownRemaining -= Time.unscaledDeltaTime;
                if (countdownRemaining <= 0f)
                {
                    TriggerReset();
                    return;
                }

                if (overlay != null)
                {
                    overlay.UpdateCountdown(countdownRemaining);
                }
            }
        }

        public void TriggerReset()
        {
            if (currentState == State.Resetting)
            {
                return;
            }

            currentState = State.Resetting;
            if (overlay != null)
            {
                overlay.Hide();
            }

            onBeforeReset?.Invoke();
            StartCoroutine(LoadResetSceneAfterCleanup());
        }

        // Gives OnBeforeReset listeners (e.g. cancelling in-flight async UI work) a couple of
        // frames to finish before the scene load destroys the objects they were operating on.
        private System.Collections.IEnumerator LoadResetSceneAfterCleanup()
        {
            yield return null;
            yield return null;
            SceneManager.LoadScene(GetResolvedSceneName());
        }

        public void CancelCountdown()
        {
            if (currentState != State.Countdown)
            {
                return;
            }

            currentState = State.Monitoring;
            idleTimer = 0f;
            if (overlay != null)
            {
                overlay.Hide();
            }
        }

        public void ResetIdleTimer()
        {
            idleTimer = 0f;
        }

        private void HandleActivity()
        {
            if (currentState == State.Monitoring)
            {
                ResetIdleTimer();
            }
        }

        private void HandleFourCornerGesture()
        {
            StartCountdown();
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (currentState == State.Resetting)
            {
                currentState = State.Monitoring;
                idleTimer = 0f;
                countdownRemaining = CountdownDurationSeconds;
                onAfterReset?.Invoke();
            }
            else
            {
                idleTimer = 0f;
            }

            if (overlay != null)
            {
                overlay.Hide();
            }
        }

        private void StartCountdown()
        {
            if (currentState != State.Monitoring)
            {
                return;
            }

            currentState = State.Countdown;
            countdownRemaining = CountdownDurationSeconds;
            idleTimer = 0f;

            if (overlay != null)
            {
                overlay.Show(countdownRemaining);
            }
        }

        private void InitializeRuntimeComponents()
        {
            activityMonitor = GetComponent<ActivityMonitor>();
            if (activityMonitor == null)
            {
                activityMonitor = gameObject.AddComponent<ActivityMonitor>();
            }

            activityMonitor.OnActivity += HandleActivity;

            fourCornerGestureDetector = GetComponent<FourCornerGestureDetector>();
            if (fourCornerGestureDetector == null)
            {
                fourCornerGestureDetector = gameObject.AddComponent<FourCornerGestureDetector>();
            }

            fourCornerGestureDetector.OnFourCornerGesture += HandleFourCornerGesture;

            if (autoCreateOverlay)
            {
                var overlayObject = gameObject.transform.Find("InactivityResetOverlay")?.gameObject;
                if (overlayObject == null)
                {
                    overlayObject = new GameObject("InactivityResetOverlay");
                    overlayObject.transform.SetParent(transform, false);
                    overlayObject.transform.SetAsLastSibling();
                }

                overlay = overlayObject.GetComponent<InactivityResetOverlay>();
                if (overlay == null)
                {
                    overlay = overlayObject.AddComponent<InactivityResetOverlay>();
                }

                overlay.Initialize(this);
            }
        }

        private void ResolveConfiguredDurations()
        {
            var timeoutFallback = Mathf.RoundToInt(defaultIdleTimeoutSeconds);
            var countdownFallback = Mathf.RoundToInt(defaultCountdownDurationSeconds);

#if UNITY_IOS && !UNITY_EDITOR
            var timeoutFromIos = IosPreferencesBridge.GetInt("inactivity_timeout_seconds", timeoutFallback);
            var countdownFromIos = IosPreferencesBridge.GetInt("inactivity_countdown_seconds", countdownFallback);
            IdleTimeoutSeconds = Mathf.Max(1f, timeoutFromIos);
            CountdownDurationSeconds = Mathf.Max(1f, countdownFromIos);
#else
            IdleTimeoutSeconds = Mathf.Max(1f, timeoutFallback);
            CountdownDurationSeconds = Mathf.Max(1f, countdownFallback);
#endif
        }

        private string GetResolvedSceneName()
        {
            return string.IsNullOrEmpty(resetSceneOverride) ? installSceneName : resetSceneOverride;
        }
    }
}
