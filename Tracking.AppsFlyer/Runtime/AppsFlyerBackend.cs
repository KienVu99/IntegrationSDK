using System.Collections.Generic;
using AppsFlyerSDK;
using IntegrationSDK.Core;
using UnityEngine;

namespace IntegrationSDK.Tracking.AppsFlyer
{
    public class AppsFlyerBackend : ITrackingBackend
    {
        public void Initialize(IntegrationSDKConfig config)
        {
            if (!config.appsFlyerEnabled)
            {
                return;
            }

            AppsFlyerSDK.AppsFlyer.setIsDebug(Debug.isDebugBuild);
            AppsFlyerSDK.AppsFlyer.initSDK(config.appsFlyerDevKey, config.appsFlyerIosAppId);
            AppsFlyerSDK.AppsFlyer.startSDK();
        }

        public void LogEvent(string eventName, Dictionary<string, object> parameters)
        {
            var stringParameters = new Dictionary<string, string>();
            foreach (var kvp in parameters)
            {
                stringParameters[kvp.Key] = kvp.Value?.ToString();
            }

            AppsFlyerSDK.AppsFlyer.sendEvent(eventName, stringParameters);
        }

        public void LogAdRevenue(AdRevenueData data)
        {
            var revenueData = new AFAdRevenueData(
                data.AdSource,
                MediationNetwork.ApplovinMax,
                data.Currency,
                data.Value);

            var extraParams = new Dictionary<string, string>
            {
                { "ad_unit_name", data.AdUnitName },
                { "placement", data.Placement },
                { "country_code", data.CountryCode },
                { "ad_format", data.AdFormat }
            };

            AppsFlyerSDK.AppsFlyer.logAdRevenue(revenueData, extraParams);
        }
    }
}
