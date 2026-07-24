using IntegrationSDK.Core;
using UnityEngine;

namespace IntegrationSDK.Ads.AppLovin
{
    internal static class MaxAdProviderBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            AdManager.RegisterProvider(new MaxAdProvider());
        }
    }
}
