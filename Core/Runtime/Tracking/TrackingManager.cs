using System;
using System.Collections.Generic;
using UnityEngine;

namespace IntegrationSDK.Core
{
    public static class TrackingManager
    {
        private static readonly List<ITrackingBackend> _backends = new List<ITrackingBackend>();
        private static bool _warnedNoBackends;

        public static void RegisterBackend(ITrackingBackend backend)
        {
            if (backend != null && !_backends.Contains(backend))
            {
                _backends.Add(backend);
            }
        }

        public static void UnregisterBackend(ITrackingBackend backend)
        {
            _backends.Remove(backend);
        }

        public static void InitializeAll(IntegrationSDKConfig config)
        {
            foreach (var backend in _backends)
            {
                SafeInvoke(() => backend.Initialize(config));
            }
        }

        public static void LogEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            parameters ??= new Dictionary<string, object>();

            if (!WarnIfNoBackends())
            {
                return;
            }

            foreach (var backend in _backends)
            {
                SafeInvoke(() => backend.LogEvent(eventName, parameters));
            }
        }

        public static void LogAdRevenue(AdRevenueData data)
        {
            if (!WarnIfNoBackends())
            {
                return;
            }

            foreach (var backend in _backends)
            {
                SafeInvoke(() => backend.LogAdRevenue(data));
            }
        }

        internal static void ResetForTests()
        {
            _backends.Clear();
            _warnedNoBackends = false;
        }

        private static bool WarnIfNoBackends()
        {
            if (_backends.Count > 0)
            {
                return true;
            }

            if (!_warnedNoBackends)
            {
                Debug.LogWarning("IntegrationSDK: no tracking backend registered (add Tracking.AppsFlyer and/or Tracking.Firebase package).");
                _warnedNoBackends = true;
            }

            return false;
        }

        private static void SafeInvoke(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"IntegrationSDK: tracking backend threw an exception: {e}");
            }
        }
    }
}
