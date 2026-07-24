using System.Collections.Generic;

namespace IntegrationSDK.Core.Editor
{
    public static class IntegrationSDKConfigValidator
    {
        public static List<string> Validate(IntegrationSDKConfig config)
        {
            var errors = new List<string>();

            if (config == null)
            {
                errors.Add("IntegrationSDKConfig asset not found. Open Tools > Integration SDK > Settings to create one.");
                return errors;
            }

            if (string.IsNullOrEmpty(config.maxSdkKey))
            {
                errors.Add("AppLovin MAX SDK Key is missing.");
            }

            if (string.IsNullOrEmpty(config.androidInterstitialAdUnitId) || string.IsNullOrEmpty(config.iosInterstitialAdUnitId))
            {
                errors.Add("Interstitial Ad Unit ID is missing for Android and/or iOS.");
            }

            if (string.IsNullOrEmpty(config.androidRewardedAdUnitId) || string.IsNullOrEmpty(config.iosRewardedAdUnitId))
            {
                errors.Add("Rewarded Ad Unit ID is missing for Android and/or iOS.");
            }

            if (string.IsNullOrEmpty(config.androidAppOpenAdUnitId) || string.IsNullOrEmpty(config.iosAppOpenAdUnitId))
            {
                errors.Add("App Open Ad Unit ID is missing for Android and/or iOS.");
            }

            if (config.appsFlyerEnabled && string.IsNullOrEmpty(config.appsFlyerDevKey))
            {
                errors.Add("AppsFlyer is enabled but Dev Key is missing.");
            }

            return errors;
        }
    }
}
