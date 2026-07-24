using System.Linq;
using UnityEngine;

namespace IntegrationSDK.Core
{
    public static class Bootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            var config = Resources.FindObjectsOfTypeAll<IntegrationSDKConfig>().FirstOrDefault();

            if (config == null)
            {
                Debug.LogWarning("IntegrationSDK: no IntegrationSDKConfig found. Open Tools > Integration SDK > Settings to create one.");
                return;
            }

            AdManager.Initialize(config);
            TrackingManager.InitializeAll(config);
        }
    }
}
