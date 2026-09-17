using UnityEngine;

namespace InactivityReset
{
    public static class IosPreferencesBridge
    {
        public static int GetInt(string key, int defaultValue)
        {
#if UNITY_IOS && !UNITY_EDITOR
            return _GetInt(key, defaultValue);
#else
            return defaultValue;
#endif
        }

#if UNITY_IOS && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern int _GetInt(string key, int defaultValue);
#endif
    }
}
