using IntegrationSDK.Core;
using UnityEngine;

namespace IntegrationSDK.Tracking.AppsFlyer
{
    internal static class AppsFlyerBackendBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            TrackingManager.RegisterBackend(new AppsFlyerBackend());
        }
    }
}
