using System;
using System.Collections.Generic;
using UnityEngine;

namespace IntegrationSDK.Core
{
    public static class LevelTracker
    {
        private static readonly Dictionary<string, double> _startTimes = new Dictionary<string, double>();

        public static Func<double> NowProvider = () => Time.realtimeSinceStartupAsDouble;

        public static void Start(string level)
        {
            _startTimes[level] = NowProvider();
            TrackingManager.LogEvent("level_start", new Dictionary<string, object>
            {
                { "level", level }
            });
        }

        public static void Passed(string level) => LogEnd(level, "level_passed");

        public static void Failed(string level) => LogEnd(level, "level_failed");

        private static void LogEnd(string level, string eventName)
        {
            var timePlayed = 0d;
            if (_startTimes.TryGetValue(level, out var startTime))
            {
                timePlayed = NowProvider() - startTime;
                _startTimes.Remove(level);
            }

            TrackingManager.LogEvent(eventName, new Dictionary<string, object>
            {
                { "level", level },
                { "time_played", timePlayed }
            });
        }
    }
}
