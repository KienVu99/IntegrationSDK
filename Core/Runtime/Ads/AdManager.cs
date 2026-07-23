using System;
using UnityEngine;

namespace IntegrationSDK.Core
{
    public static class AdManager
    {
        private static IAdProvider _provider;
        private static bool _warnedNoProvider;

        public static bool HasProvider => _provider != null;

        public static void RegisterProvider(IAdProvider provider)
        {
            if (_provider != null && !ReferenceEquals(_provider, provider))
            {
                Debug.LogWarning("IntegrationSDK: replacing an already-registered ad provider.");
            }

            _provider = provider;
        }

        public static void Initialize(IntegrationSDKConfig config)
        {
            if (!WarnIfNoProvider())
            {
                return;
            }

            _provider.Initialize(config);
        }

        public static void ShowInterstitial(string placement)
        {
            if (!WarnIfNoProvider())
            {
                return;
            }

            _provider.ShowInterstitial(placement);
        }

        public static void ShowRewarded(string placement, Action onRewardEarned)
        {
            if (!WarnIfNoProvider())
            {
                return;
            }

            _provider.ShowRewarded(placement, onRewardEarned);
        }

        public static void ShowAppOpen(string placement)
        {
            if (!WarnIfNoProvider())
            {
                return;
            }

            _provider.ShowAppOpen(placement);
        }

        internal static void ResetForTests()
        {
            _provider = null;
            _warnedNoProvider = false;
        }

        private static bool WarnIfNoProvider()
        {
            if (_provider != null)
            {
                return true;
            }

            if (!_warnedNoProvider)
            {
                Debug.LogWarning("IntegrationSDK: no ad provider registered (add the Ads.AppLovin package).");
                _warnedNoProvider = true;
            }

            return false;
        }
    }
}
