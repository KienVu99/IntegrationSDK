using System;
using System.Collections.Generic;
using System.Linq;
using IntegrationSDK.Core;
using UnityEngine;

namespace IntegrationSDK.DebugTools
{
    // ponytail: mirror backend so the on-screen overlay can show every
    // Initialize/LogEvent/LogAdRevenue call TrackingManager broadcasts, alongside
    // whatever real backends (AppsFlyer, Firebase) are registered. Remove
    // Assets/DebugTools before shipping a real release build.
    internal class SDKDebugTrackingBackend : ITrackingBackend
    {
        private const int MaxEntries = 60;
        private static readonly List<string> _log = new List<string>();

        public static IReadOnlyList<string> Log => _log;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            TrackingManager.RegisterBackend(new SDKDebugTrackingBackend());
        }

        public void Initialize(IntegrationSDKConfig config)
        {
            Append($"Initialize appsFlyerEnabled={config.appsFlyerEnabled} devKey={Mask(config.appsFlyerDevKey)} firebaseEnabled={config.firebaseEnabled}");
        }

        public void LogEvent(string eventName, Dictionary<string, object> parameters)
        {
            var paramText = parameters == null || parameters.Count == 0
                ? "{}"
                : string.Join(", ", parameters.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            Append($"LogEvent {eventName} [{paramText}]");
        }

        public void LogAdRevenue(AdRevenueData data)
        {
            Append($"LogAdRevenue source={data.AdSource} format={data.AdFormat} value={data.Value} {data.Currency} placement={data.Placement}");
        }

        private static string Mask(string value) =>
            string.IsNullOrEmpty(value) ? "<empty>" : value.Length <= 4 ? value : value.Substring(0, 4) + "...";

        private static void Append(string line)
        {
            var entry = $"[{DateTime.Now:HH:mm:ss}] {line}";
            Debug.Log($"IntegrationSDK.Debug: {entry}");
            _log.Add(entry);
            if (_log.Count > MaxEntries)
            {
                _log.RemoveAt(0);
            }
        }
    }
}
