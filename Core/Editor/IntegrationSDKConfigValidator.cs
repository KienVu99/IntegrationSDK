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

            var usesAnyAdFormat =
                !string.IsNullOrEmpty(config.androidInterstitialAdUnitId) || !string.IsNullOrEmpty(config.iosInterstitialAdUnitId) ||
                !string.IsNullOrEmpty(config.androidRewardedAdUnitId) || !string.IsNullOrEmpty(config.iosRewardedAdUnitId) ||
                !string.IsNullOrEmpty(config.androidAppOpenAdUnitId) || !string.IsNullOrEmpty(config.iosAppOpenAdUnitId) ||
                !string.IsNullOrEmpty(config.androidBannerAdUnitId) || !string.IsNullOrEmpty(config.iosBannerAdUnitId);

            if (usesAnyAdFormat && string.IsNullOrEmpty(config.maxSdkKey))
            {
                errors.Add("AppLovin MAX SDK Key is missing.");
            }

            ValidateAdFormat(config.androidInterstitialAdUnitId, config.iosInterstitialAdUnitId, "Interstitial", errors);
            ValidateAdFormat(config.androidRewardedAdUnitId, config.iosRewardedAdUnitId, "Rewarded", errors);
            ValidateAdFormat(config.androidAppOpenAdUnitId, config.iosAppOpenAdUnitId, "App Open", errors);
            ValidateAdFormat(config.androidBannerAdUnitId, config.iosBannerAdUnitId, "Banner", errors);

            if (config.appsFlyerEnabled && string.IsNullOrEmpty(config.appsFlyerDevKey))
            {
                errors.Add("AppsFlyer is enabled but Dev Key is missing.");
            }

            return errors;
        }

        private static void ValidateAdFormat(string androidId, string iosId, string formatName, List<string> errors)
        {
            var hasAndroid = !string.IsNullOrEmpty(androidId);
            var hasIos = !string.IsNullOrEmpty(iosId);

            if (hasAndroid != hasIos)
            {
                errors.Add($"{formatName} Ad Unit ID is set for one platform but not the other (Android/iOS must both be set, or both left blank to disable this ad format).");
            }
        }
    }
}
