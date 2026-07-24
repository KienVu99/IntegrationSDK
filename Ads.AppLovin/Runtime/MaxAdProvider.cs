using System;
using System.Collections.Generic;
using IntegrationSDK.Core;
using UnityEngine;

namespace IntegrationSDK.Ads.AppLovin
{
    public class MaxAdProvider : IAdProvider
    {
        private IntegrationSDKConfig _config;
        private Action _pendingReward;
        private bool _bannerCreated;

        public void Initialize(IntegrationSDKConfig config)
        {
            _config = config;

            MaxSdkCallbacks.OnSdkInitializedEvent += _ => LoadAllAds();

            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += (adUnitId, adInfo) => LogShowSuccess("show_interstitial_ads_suscces", adInfo);
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent += (adUnitId, adInfo) => LogShowSuccess("show_interstitial_ads_click", adInfo);
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += (adUnitId, adInfo) => MaxSdk.LoadInterstitial(_config.GetInterstitialAdUnitId());
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += (adUnitId, adInfo) => LogAdImpression(adInfo, "interstitial");
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += (adUnitId, error) => LogLoadFailed("interstitial", adUnitId, error);
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += (adUnitId, error, adInfo) => LogLoadFailed("interstitial_display", adUnitId, error);

            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += (adUnitId, adInfo) => LogShowSuccess("show_rewarded_ads_sucess", adInfo);
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent += (adUnitId, adInfo) => LogShowSuccess("show_rewarded_ads_click", adInfo);
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += (adUnitId, reward, adInfo) => _pendingReward?.Invoke();
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += (adUnitId, adInfo) => MaxSdk.LoadRewardedAd(_config.GetRewardedAdUnitId());
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += (adUnitId, adInfo) => LogAdImpression(adInfo, "rewarded");
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += (adUnitId, error) => LogLoadFailed("rewarded", adUnitId, error);
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += (adUnitId, error, adInfo) => LogLoadFailed("rewarded_display", adUnitId, error);

            MaxSdkCallbacks.AppOpen.OnAdDisplayedEvent += (adUnitId, adInfo) => LogShowSuccess("show_aoa_ads_sucess", adInfo);
            MaxSdkCallbacks.AppOpen.OnAdClickedEvent += (adUnitId, adInfo) => LogShowSuccess("show_aoa_ads_click", adInfo);
            MaxSdkCallbacks.AppOpen.OnAdHiddenEvent += (adUnitId, adInfo) => MaxSdk.LoadAppOpenAd(_config.GetAppOpenAdUnitId());
            MaxSdkCallbacks.AppOpen.OnAdRevenuePaidEvent += (adUnitId, adInfo) => LogAdImpression(adInfo, "app_open");
            MaxSdkCallbacks.AppOpen.OnAdLoadFailedEvent += (adUnitId, error) => LogLoadFailed("app_open", adUnitId, error);
            MaxSdkCallbacks.AppOpen.OnAdDisplayFailedEvent += (adUnitId, error, adInfo) => LogLoadFailed("app_open_display", adUnitId, error);

            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += (adUnitId, adInfo) => LogAdImpression(adInfo, "banner");
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += (adUnitId, error) => LogLoadFailed("banner", adUnitId, error);

            // ponytail: verbose native logging left on for on-device debugging while
            // wiring up real ad units - turn off once ads are confirmed working.
            MaxSdk.SetVerboseLogging(true);
            MaxSdk.SetSdkKey(config.maxSdkKey);
            MaxSdk.InitializeSdk();
        }

        public void ShowInterstitial(string placement)
        {
            var adUnitId = _config.GetInterstitialAdUnitId();
            if (string.IsNullOrEmpty(adUnitId))
            {
                return;
            }

            TrackingManager.LogEvent("show_interstitial_ads", new Dictionary<string, object> { { "placement", placement } });
            if (MaxSdk.IsInterstitialReady(adUnitId))
            {
                MaxSdk.ShowInterstitial(adUnitId, placement);
            }
        }

        public void ShowRewarded(string placement, Action onRewardEarned)
        {
            var adUnitId = _config.GetRewardedAdUnitId();
            if (string.IsNullOrEmpty(adUnitId))
            {
                return;
            }

            _pendingReward = onRewardEarned;
            TrackingManager.LogEvent("show_rewarded_ads", new Dictionary<string, object> { { "placement", placement } });
            if (MaxSdk.IsRewardedAdReady(adUnitId))
            {
                MaxSdk.ShowRewardedAd(adUnitId, placement);
            }
        }

        public void ShowAppOpen(string placement)
        {
            var adUnitId = _config.GetAppOpenAdUnitId();
            if (string.IsNullOrEmpty(adUnitId))
            {
                return;
            }

            TrackingManager.LogEvent("show_aoa_ads", new Dictionary<string, object> { { "placement", placement } });
            if (MaxSdk.IsAppOpenAdReady(adUnitId))
            {
                MaxSdk.ShowAppOpenAd(adUnitId, placement);
            }
        }

        public void ShowBanner(string placement)
        {
            var adUnitId = _config.GetBannerAdUnitId();
            if (string.IsNullOrEmpty(adUnitId))
            {
                return;
            }

            if (!_bannerCreated)
            {
                MaxSdk.CreateBanner(adUnitId, new MaxSdkBase.AdViewConfiguration(MaxSdkBase.AdViewPosition.BottomCenter));
                _bannerCreated = true;
            }

            TrackingManager.LogEvent("show_banner_ads", new Dictionary<string, object> { { "placement", placement } });
            MaxSdk.ShowBanner(adUnitId);
        }

        public void HideBanner()
        {
            var adUnitId = _config.GetBannerAdUnitId();
            if (string.IsNullOrEmpty(adUnitId) || !_bannerCreated)
            {
                return;
            }

            MaxSdk.HideBanner(adUnitId);
        }

        private void LoadAllAds()
        {
            if (!string.IsNullOrEmpty(_config.GetInterstitialAdUnitId())) MaxSdk.LoadInterstitial(_config.GetInterstitialAdUnitId());
            if (!string.IsNullOrEmpty(_config.GetRewardedAdUnitId())) MaxSdk.LoadRewardedAd(_config.GetRewardedAdUnitId());
            if (!string.IsNullOrEmpty(_config.GetAppOpenAdUnitId())) MaxSdk.LoadAppOpenAd(_config.GetAppOpenAdUnitId());
        }

        private void LogLoadFailed(string adFormat, string adUnitId, MaxSdkBase.ErrorInfo error)
        {
            Debug.LogWarning($"IntegrationSDK.Debug: {adFormat} failed adUnitId={adUnitId} code={error.Code} message={error.Message} mediatedCode={error.MediatedNetworkErrorCode} mediatedMessage={error.MediatedNetworkErrorMessage}");
        }

        private void LogShowSuccess(string eventName, MaxSdk.AdInfo adInfo)
        {
            TrackingManager.LogEvent(eventName, new Dictionary<string, object> { { "placement", adInfo.Placement } });
        }

        private void LogAdImpression(MaxSdk.AdInfo adInfo, string adFormat)
        {
            TrackingManager.LogAdRevenue(new AdRevenueData
            {
                AdPlatform = "applovin_max",
                AdSource = adInfo.NetworkName,
                AdUnitName = adInfo.AdUnitIdentifier,
                Currency = "USD",
                Value = adInfo.Revenue,
                Placement = adInfo.Placement,
                CountryCode = MaxSdk.GetSdkConfiguration()?.CountryCode,
                AdFormat = adFormat
            });
        }
    }
}
