using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Events;
#if USING_MAX_MEDIATION
using System;
using aCode.Configs;
#endif
#if USING_FIREBASE_ANALYTICS
using Firebase.Analytics;
#endif

namespace aCode.Advertisers
{
    [SuppressMessage("ReSharper", "AccessToStaticMemberViaDerivedType")]
	public class ApplovinImplementation : MonoBehaviour, IAdProvider
	{
        private const string Tag = "Applovin";
        private bool _initialized;
#if USING_MAX_MEDIATION
        
        private string _interstitialAdUnitId, _rewardedAdUnitId, _bannerAdUnitId, _appOpenAdUnitId;
        private BannerPosition _bannerPosition;
        
        private bool _isBannerShowing;
        private int _interstitialRetryAttempt, _rewardedRetryAttempt, _appOpenRetryAttempt;
        private const int MaxRetryCount = 10;
        private UnityAction _initializedAction, _onRewardVideoCallBack, _onAppOpenShowCallBac, _onInterstitialClosedCallback;
        
        private TimeSpan _intervalShowAds;
        private DateTime _timeAdsCanShow;
        private string _currentInterstitialPlacement = "default";
        private string _currentRewardedPlacement = "default";
        private string _currentAppOpenPlacement = "default";

        public void InitializeAds(UnityAction initializedAds)
        {
            if (_initialized) return;
            _initializedAction = initializedAds;
#if UNITY_ANDROID
            _bannerAdUnitId = AppConfig.BannerID;
            _interstitialAdUnitId = AppConfig.InterstitialID;
            _rewardedAdUnitId = AppConfig.RewardedVideoID;
            _appOpenAdUnitId = AppConfig.AppOpenID;
#else
            _bannerAdUnitId = AppConfig.BannerIosID;
            _interstitialAdUnitId = AppConfig.InterstitialIosID;
            _rewardedAdUnitId = AppConfig.RewardedVideoIosID;
            _appOpenAdUnitId = AppConfig.AppOpenIosID;
#endif
            _bannerPosition = AppConfig.BannerPosition;
            _intervalShowAds = TimeSpan.FromSeconds(AppConfig.IntervalShowAds);
            
            LogData("Start Initialization");
            MaxSdkCallbacks.OnSdkInitializedEvent += _ =>
            {
                GM.Print(Tag,"MAX SDK Initialized");
                if (!string.IsNullOrEmpty(_bannerAdUnitId))
                {
                    InitializeBannerAds();
                }
                if (!string.IsNullOrEmpty(_appOpenAdUnitId))
                {
                    InitializeAppOpenAds();
                }
                if (!string.IsNullOrEmpty(_interstitialAdUnitId))
                {
                    InitializeInterstitialAds();
                }
                if (!string.IsNullOrEmpty(_rewardedAdUnitId))
                {
                    InitializeRewardedAds();
                }
                _initializedAction?.Invoke();
            };
            MaxSdk.InitializeSdk();
        }

        public bool IsShowingBanner()
        {
            return !string.IsNullOrEmpty(_bannerAdUnitId) && _isBannerShowing;
        }

        public bool IsInterstitialAvailable()
        {
            return !string.IsNullOrEmpty(_interstitialAdUnitId) && DateTime.Now > _timeAdsCanShow && MaxSdk.IsInterstitialReady(_interstitialAdUnitId);
        }

        public bool IsRewardedVideoAvailable()
        {
            return !string.IsNullOrEmpty(_rewardedAdUnitId) && MaxSdk.IsRewardedAdReady(_rewardedAdUnitId);
        }

        public void ShowBanner(bool collapsible = false)
        {
            if(string.IsNullOrEmpty(_bannerAdUnitId)) return;
            MaxSdk.ShowBanner(_bannerAdUnitId);
        }

        public void HideBanner()
        {
            if(string.IsNullOrEmpty(_bannerAdUnitId)) return;
            MaxSdk.HideBanner(_bannerAdUnitId);
        }
        
        public void ShowInterstitial(string placement, UnityAction callback)
        {
            _currentInterstitialPlacement = string.IsNullOrEmpty(placement) ? "default" : placement;
            if (IsInterstitialAvailable())
            {
                GM.LogEvent("show_interstitial_ads", "placement", _currentInterstitialPlacement);
                _onInterstitialClosedCallback = callback;
                _timeAdsCanShow = DateTime.Now + _intervalShowAds;
                MaxSdk.ShowInterstitial(_interstitialAdUnitId);
            }
            else
            {
                callback?.Invoke();
            }
        }

        public void ShowInterstitial(UnityAction callback)
        {
            ShowInterstitial("default", callback);
        }
        
        public void ShowRewardedVideo(string placement, UnityAction rewardVideoCallBack)
        {
            if (!IsRewardedVideoAvailable()) return;
            _currentRewardedPlacement = string.IsNullOrEmpty(placement) ? "default" : placement;
            GM.LogEvent("show_rewarded_ads", "placement", _currentRewardedPlacement);
            _timeAdsCanShow = DateTime.Now + _intervalShowAds;
            _onRewardVideoCallBack = rewardVideoCallBack;
            MaxSdk.ShowRewardedAd(_rewardedAdUnitId);
        }

        public void ShowRewardedVideo(UnityAction rewardVideoCallBack)
        {
            ShowRewardedVideo("default", rewardVideoCallBack);
        }
        
        public bool IsAppOpenAvailable()
        {
            return !string.IsNullOrEmpty(_appOpenAdUnitId) && DateTime.Now > _timeAdsCanShow && MaxSdk.IsAppOpenAdReady(_appOpenAdUnitId);
        }
        
        public void ShowAppOpen(string placement = "default")
        {
            if (!IsAppOpenAvailable()) return;
            _currentAppOpenPlacement = string.IsNullOrEmpty(placement) ? "default" : placement;
            GM.LogEvent("show_aoa_ads", "placement", _currentAppOpenPlacement);
            MaxSdk.ShowAppOpenAd(_appOpenAdUnitId);
            _timeAdsCanShow = DateTime.Now + TimeSpan.FromSeconds(25);
        }
        
        public void OpenDebugWindow()
        {
            MaxSdk.ShowMediationDebugger();
        }
        
        private void InitializeInterstitialAds()
        {
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += InterstitialFailedToDisplayEvent;
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialClickedEvent;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialDismissedEvent;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += SendAdPaidEvent;
            LoadInterstitial();
        }

        private void LoadInterstitial()
        {
            if(string.IsNullOrEmpty(_interstitialAdUnitId)) return;
            MaxSdk.LoadInterstitial(_interstitialAdUnitId);
        }
        
        private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            LogData("Interstitial loaded");
            _interstitialRetryAttempt = 0;
        }

        private void OnInterstitialFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            _interstitialRetryAttempt++;
            var retryDelay = Math.Pow(2, Math.Min(6, _interstitialRetryAttempt));
            LogData("Interstitial failed to load with error code: " + errorInfo.Code);
            Invoke(nameof(LoadInterstitial), (float) retryDelay);
        }

        private void InterstitialClosed()
        {
            var callback = _onInterstitialClosedCallback;
            _onInterstitialClosedCallback = null;
            callback?.Invoke();
        }

        private void InterstitialFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            LogData("Interstitial failed to display with error code: " + errorInfo.Code);
            LoadInterstitial();
            InterstitialClosed();
        }

        private void OnInterstitialClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            LogData("Interstitial clicked");
            GM.LogEvent("show_interstitial_ads_click", "placement", _currentInterstitialPlacement);
        }

        private void OnInterstitialDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            LogData("Interstitial dismissed");
            GM.LogEvent("show_interstitial_ads_suscces", "placement", _currentInterstitialPlacement);
            _timeAdsCanShow = DateTime.Now + _intervalShowAds;
            LoadInterstitial();
            InterstitialClosed();
        }
        
        private void InitializeRewardedAds()
        {
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdDismissedEvent;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += SendAdPaidEvent;
            LoadRewardedAd();
        }

        private void LoadRewardedAd()
        {
            if(string.IsNullOrEmpty(_rewardedAdUnitId)) return;
            MaxSdk.LoadRewardedAd(_rewardedAdUnitId);
        }
        
        private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            LogData("Rewarded ad loaded");
            _rewardedRetryAttempt = 0;
        }

        private void OnRewardedAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            _rewardedRetryAttempt++;
            var retryDelay = Math.Pow(2, Math.Min(6, _rewardedRetryAttempt));
            LogData("Rewarded ad failed to load with error code: " + errorInfo.Code);
            Invoke(nameof(LoadRewardedAd), (float) retryDelay);
        }

        private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            LogData("Rewarded ad failed to display with error code: " + errorInfo.Code);
            LoadRewardedAd();
        }

        private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            LogData("Rewarded ad displayed");
        }

        private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            LogData("Rewarded ad clicked");
            GM.LogEvent("show_rewarded_ads_click", "placement", _currentRewardedPlacement);
        }

        private void OnRewardedAdDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            LogData("Rewarded ad dismissed");
            _timeAdsCanShow = DateTime.Now + _intervalShowAds;
            LoadRewardedAd();
            _onRewardVideoCallBack = null;
        }

        private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
        {
            GM.LogEvent("show_rewarded_ads_sucess", "placement", _currentRewardedPlacement);
            _timeAdsCanShow = DateTime.Now + _intervalShowAds;
            var callback = _onRewardVideoCallBack;
            _onRewardVideoCallBack = null;
            callback?.Invoke();
            LogData("Rewarded ad received reward");
        }

        private void InitializeBannerAds()
        {
            if(string.IsNullOrEmpty(_bannerAdUnitId)) return;
            MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerAdLoadedEvent;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerAdFailedEvent;
            MaxSdkCallbacks.Banner.OnAdClickedEvent += OnBannerAdClickedEvent;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += SendAdPaidEvent;
            var bannerConfiguration = new MaxSdkBase.AdViewConfiguration(_bannerPosition == BannerPosition.Top ? MaxSdkBase.AdViewPosition.TopCenter : MaxSdkBase.AdViewPosition.BottomCenter);
            MaxSdk.CreateBanner(_bannerAdUnitId, bannerConfiguration);
            MaxSdk.SetBannerBackgroundColor(_bannerAdUnitId, Color.black);
        }
        
        private void OnBannerAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            LogData("Banner ad loaded");
        }

        private void OnBannerAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            LogData("Banner ad failed to load with error code: " + errorInfo.Code);
        }

        private void OnBannerAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            LogData("Banner ad clicked");
        }

        private void InitializeAppOpenAds()
        {
            MaxSdkCallbacks.AppOpen.OnAdDisplayedEvent += OnAppOpenDisplayedEvent;
            MaxSdkCallbacks.AppOpen.OnAdDisplayFailedEvent += OnAppOpenDisplayFailedEvent;
            MaxSdkCallbacks.AppOpen.OnAdHiddenEvent += OnAppOpenHiddenEvent;
            MaxSdkCallbacks.AppOpen.OnAdLoadedEvent += OnAppOpenLoadedEvent;
            MaxSdkCallbacks.AppOpen.OnAdLoadFailedEvent += OnAppOpenLoadFailedEvent;
            MaxSdkCallbacks.AppOpen.OnAdRevenuePaidEvent += OnAppOpenRevenuePaidEvent;
            MaxSdkCallbacks.AppOpen.OnAdClickedEvent += OnAppOpenClickEvent;
            MaxSdk.LoadAppOpenAd(_appOpenAdUnitId);
        }
        
        private void OnAppOpenDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            LogData( "App Open ad displayed");
            GM.LogEvent("show_aoa_ads_sucess", "placement", _currentAppOpenPlacement);
            _appOpenRetryAttempt = 0;
        }
        
        private void OnAppOpenLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            LogData( "App Open ad loaded");
            _appOpenRetryAttempt = 0;
        }
        
        private void OnAppOpenLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            LogData("App Open ad failed to load with error code: " + errorInfo.Code);
            _appOpenRetryAttempt++;
            var retryDelay = Math.Pow(2, Math.Min(6, _appOpenRetryAttempt));
            Invoke(nameof(LoadAppOpenAd), (float) retryDelay);
        }

        private void OnAppOpenDisplayFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            LogData("App Open ad failed to display with error code: " + errorInfo.Code);
            _appOpenRetryAttempt++;
            var retryDelay = Math.Pow(2, Math.Min(6, _appOpenRetryAttempt));
            Invoke(nameof(LoadAppOpenAd), (float) retryDelay);
        }

        private void LoadAppOpenAd()
        {
            if(string.IsNullOrEmpty(_appOpenAdUnitId)) return;
            MaxSdk.LoadAppOpenAd(_appOpenAdUnitId);
        }

        private void OnAppOpenRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            SendAdPaidEvent(adUnitId, adInfo);
            _appOpenRetryAttempt = 0;
        }

        private void OnAppOpenHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            LogData("App Open ad hidden");
            LoadAppOpenAd();
            _onRewardVideoCallBack?.Invoke();
            _onRewardVideoCallBack = null;
            _timeAdsCanShow = DateTime.Now + TimeSpan.FromSeconds(25);
            _appOpenRetryAttempt = 0;
        }

        private void OnAppOpenClickEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            LogData("App Open ad clicked");
            GM.LogEvent("show_aoa_ads_click", "placement", _currentAppOpenPlacement);
            _appOpenRetryAttempt = 0;
        }
        
        private void SendAdPaidEvent(string adUnitName, MaxSdkBase.AdInfo adInfo)
        {
            if (adInfo == null) return;
            const string adPlatform = "Applovin_Max";
            const string currency = "USD";
            var revenue = adInfo.Revenue;
            
            var countryCode = MaxSdk.GetSdkConfiguration().CountryCode;
            var adNetwork = adInfo.NetworkName;
            var adFormat     = adInfo.AdFormat;
            var adUnitIdentifier = adInfo.AdUnitIdentifier;
            var adPlacement = adInfo.Placement;
            var networkPlacement = adInfo.NetworkPlacement;
            var precision = 0;
            var revenuePrecision  = adInfo.RevenuePrecision;
            if (!string.IsNullOrEmpty(revenuePrecision))
            {
                var s = revenuePrecision.Trim().ToLowerInvariant();
                if (s.Contains("estimate"))
                {
                    precision = 1;
                }else if (s.Contains("percentage"))
                {
                    precision = 2;
                }else if (s.Contains("precise") || s.Contains("exact"))
                {
                    precision = 3;
                }
            }
            
#if USING_FIREBASE_ANALYTICS
            Parameter[] ltvData = {
                new(FirebaseAnalytics.ParameterAdPlatform, adPlatform),
                new (FirebaseAnalytics.ParameterAdSource, adNetwork ?? "unknown"),
                new (FirebaseAnalytics.ParameterAdUnitName, adUnitIdentifier ?? adUnitName),
                new (FirebaseAnalytics.ParameterAdFormat, adFormat ?? "unknown"),
                new (FirebaseAnalytics.ParameterCurrency, currency),
                new (FirebaseAnalytics.ParameterValue, revenue),
                new ("precision", precision),
                new ("placement", adPlacement ?? "default"),
                new ("network_placement", networkPlacement ?? "unknown"),
                new ("country_code", countryCode ?? "unknown")
            };

            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventAdImpression, ltvData);
#endif
            LogData( $"[Firebase] ad_impression | Platform={adPlatform}, Source={adNetwork}, " + $"Format={adFormat}, Unit={adUnitIdentifier}, Revenue={revenue} {currency}, " + $"Country={countryCode}, Placement={adPlacement}, NetPlacement={networkPlacement}");
        }

        public void LoadAdsOnResume()
        {
            if (!_initialized) return;
            LogData("Run Load Ads On Resume");
            if (!IsInterstitialAvailable())
            {
                if (_interstitialRetryAttempt == MaxRetryCount)
                {
                    LoadInterstitial();
                }
            }

            if (!IsRewardedVideoAvailable())
            {
                if (_rewardedRetryAttempt == MaxRetryCount)
                {
                    LoadRewardedAd();
                }
            }
            LogData("Finish Load Ads On Resume");
        }
#else
		
        public void InitializeAds(UnityAction initializedAds)
        {
            LogData("InitializeAds called, but Ads SDK is not integrated.");
            _initialized = true;
            initializedAds?.Invoke();
        }

        public bool IsShowingBanner()
        {
            LogData("IsShowingBanner called, but Ads SDK is not integrated.");
            return false;
        }

        public bool IsInterstitialAvailable()
        {
            LogData("IsInterstitialAvailable called, but Ads SDK is not integrated.");
            return true;
        }
        
        public bool IsAppOpenAvailable()
        {
            LogData("IsAppOpenAvailable called, but Ads SDK is not integrated.");
            return false;
        }
        
        public bool IsRewardedVideoAvailable()
        {
            LogData("IsRewardedVideoAvailable called, but Ads SDK is not integrated.");
            return true;
        }

        public void ShowBanner(bool collapsible = false)
        {
            LogData("ShowBanner called, but Ads SDK is not integrated.");
        }

        public void HideBanner()
        {
            LogData("HideBanner called, but Ads SDK is not integrated.");
        }
        
        public void ShowAppOpen(string placement = "default")
        {
            LogData("ShowAppOpen called, but Ads SDK is not integrated.");
        }

        public void ShowInterstitial(string placement, UnityAction callback)
        {
            LogData("ShowInterstitial called, but Ads SDK is not integrated.");
            callback?.Invoke();
        }

        public void ShowInterstitial(UnityAction callback)
        {
            ShowInterstitial("default", callback);
        }
        
        public void ShowRewardedVideo(string placement, UnityAction rewardVideoCallBack)
        {
            LogData("ShowRewardedVideo called, but Ads SDK is not integrated.");
            rewardVideoCallBack?.Invoke();
        }

        public void ShowRewardedVideo(UnityAction rewardVideoCallBack)
        {
            ShowRewardedVideo("default", rewardVideoCallBack);
        }

        public void OpenDebugWindow()
        {
            LogData("OpenDebugWindow called, but Ads SDK is not integrated.");
        }

        public void LoadAdsOnResume()
        {
            LogData("LoadAdsOnResume called, but Ads SDK is not integrated.");
        }
#endif
        private void LogData(string message)
        {
            if(_initialized) GM.Print(Tag, message);
        }
	}
}