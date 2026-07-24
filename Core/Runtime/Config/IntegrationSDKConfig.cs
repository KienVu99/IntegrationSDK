using UnityEngine;

namespace IntegrationSDK.Core
{
    [CreateAssetMenu(fileName = "IntegrationSDKConfig", menuName = "Integration SDK/Config")]
    public class IntegrationSDKConfig : ScriptableObject
    {
        [Header("AppLovin MAX")]
        public string maxSdkKey;
        public string androidInterstitialAdUnitId;
        public string iosInterstitialAdUnitId;
        public string androidRewardedAdUnitId;
        public string iosRewardedAdUnitId;
        public string androidAppOpenAdUnitId;
        public string iosAppOpenAdUnitId;
        public string androidBannerAdUnitId;
        public string iosBannerAdUnitId;

        [Header("AppsFlyer")]
        public bool appsFlyerEnabled;
        public string appsFlyerDevKey;
        public string appsFlyerIosAppId;

        [Header("Firebase")]
        public bool firebaseEnabled;

        public string GetInterstitialAdUnitId() => GetInterstitialAdUnitIdForPlatform(Application.platform);
        public string GetRewardedAdUnitId() => GetRewardedAdUnitIdForPlatform(Application.platform);
        public string GetAppOpenAdUnitId() => GetAppOpenAdUnitIdForPlatform(Application.platform);

        public string GetInterstitialAdUnitIdForPlatform(RuntimePlatform platform) =>
            platform == RuntimePlatform.IPhonePlayer ? iosInterstitialAdUnitId : androidInterstitialAdUnitId;

        public string GetRewardedAdUnitIdForPlatform(RuntimePlatform platform) =>
            platform == RuntimePlatform.IPhonePlayer ? iosRewardedAdUnitId : androidRewardedAdUnitId;

        public string GetAppOpenAdUnitIdForPlatform(RuntimePlatform platform) =>
            platform == RuntimePlatform.IPhonePlayer ? iosAppOpenAdUnitId : androidAppOpenAdUnitId;

        public string GetBannerAdUnitId() => GetBannerAdUnitIdForPlatform(Application.platform);

        public string GetBannerAdUnitIdForPlatform(RuntimePlatform platform) =>
            platform == RuntimePlatform.IPhonePlayer ? iosBannerAdUnitId : androidBannerAdUnitId;
    }
}
