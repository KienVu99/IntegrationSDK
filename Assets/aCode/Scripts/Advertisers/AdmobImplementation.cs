using System;
using aCode.Configs;
#if USING_ADMOB_MEDIATION
using GoogleMobileAds.Api;
#if USING_FIREBASE_ANALYTICS
using Firebase.Analytics;
#endif
#endif
using UnityEngine;
using UnityEngine.Events;

namespace aCode.Advertisers
{
	public class AdmobImplementation : MonoBehaviour, IAdProvider
	{
        private const string Tag = "Admob";
        private bool _initialized;
#if USING_ADMOB_MEDIATION
        private string _bannerAdUnitId, _bannerCollapsibleAdUnitId, _interstitialAdUnitId, _rewardedAdUnitId, _appOpenAdUnitId;
        private bool _isBannerShowing;
        private int _interstitialRetryAttempt, _rewardedRetryAttempt, _appOpenRetryAttempt;
        private const int MaxRetryCount = 10;
        private UnityAction _initializedAction, _onRewardVideoCallBack, _onAppOpenShowCallBac, _onInterstitialClosedCallback;
        
        private TimeSpan _intervalShowAds;
        private DateTime _timeAdsCanShow;
        private string _currentInterstitialPlacement = "default";
        private string _currentRewardedPlacement = "default";
        private string _currentAppOpenPlacement = "default";
        
        private AppOpenAd _appOpen;
        private DateTime _appOpenExpireTime;
        private BannerView _bannerView, _bannerCollapsibleView;
        private InterstitialAd _interstitialAd;
        private RewardedAd _rewardedVideo;
        private AdPosition _bannerPosition;
        
        private UnityAction _onRewardedVideoClosed;
        private bool _rewardedVideoWatched;
        private bool _activeCollapsibleBanner;
        
        public void InitializeAds(UnityAction initializedAds)
        {
            if (_initialized) return;
            LogData( "Start Initialization");
            _initializedAction = initializedAds;
            
#if UNITY_ANDROID
            _bannerAdUnitId = AppConfig.BannerID;
            _bannerCollapsibleAdUnitId = AppConfig.BannerCollapsibleID;
            _interstitialAdUnitId = AppConfig.InterstitialID;
            _rewardedAdUnitId = AppConfig.RewardedVideoID;
            _appOpenAdUnitId = AppConfig.AppOpenID;
#else
            _bannerAdUnitId = AppConfig.BannerIosID;
            _bannerCollapsibleAdUnitId = AppConfig.BannerCollapsibleIosID;
            _interstitialAdUnitId = AppConfig.InterstitialIosID;
            _rewardedAdUnitId = AppConfig.RewardedVideoIosID;
            _appOpenAdUnitId = AppConfig.AppOpenIosID;
#endif
            
            if (string.IsNullOrEmpty(_bannerCollapsibleAdUnitId))
            {
                _bannerCollapsibleAdUnitId = _bannerAdUnitId;
            }
            
            _bannerPosition = AppConfig.BannerPosition == BannerPosition.Bottom ? AdPosition.Bottom : AdPosition.Top;
            _intervalShowAds = TimeSpan.FromSeconds(AppConfig.IntervalShowAds);
            
            if ( Screen.height > 0 && (float) Screen.width / Screen.height < 0.6f)
            {
                _activeCollapsibleBanner = true;
            }
            
            TagForChildDirectedTreatment tagForChildren;
            TagForUnderAgeOfConsent tagForUnderAge;
            MaxAdContentRating contentRating;

            if (AppConfig.DirectedForChildren)
            {
                tagForChildren = TagForChildDirectedTreatment.True;
                tagForUnderAge = TagForUnderAgeOfConsent.True;
                contentRating = MaxAdContentRating.G;
            }
            else
            {
                tagForChildren = TagForChildDirectedTreatment.Unspecified;
                tagForUnderAge = TagForUnderAgeOfConsent.Unspecified;
                contentRating = MaxAdContentRating.Unspecified;
            }
            var requestConfiguration = new RequestConfiguration
            {
                TagForChildDirectedTreatment = tagForChildren,
                MaxAdContentRating = contentRating,
                TagForUnderAgeOfConsent = tagForUnderAge,
            };

            requestConfiguration.TestDeviceIds.Add(AdRequest.TestDeviceSimulator);

            LogData( $"Admob ID : BannerId : {_bannerAdUnitId} - interstitialId : {_interstitialAdUnitId} - rewardedVideoId : {_rewardedAdUnitId} - appOpenId : {_appOpenAdUnitId}");
            if (GM.DebugMode)
            {
                if (!string.IsNullOrEmpty(_bannerAdUnitId))
                {
#if UNITY_IOS
                    _bannerAdUnitId = "ca-app-pub-3940256099942544/2934735716";
#else
                    _bannerAdUnitId = "ca-app-pub-3940256099942544/6300978111";
#endif
                }
                
                if (!string.IsNullOrEmpty(_bannerCollapsibleAdUnitId))
                {
#if UNITY_IOS
                    _bannerCollapsibleAdUnitId = "ca-app-pub-3940256099942544/2435281174";
#else
                    _bannerCollapsibleAdUnitId = "ca-app-pub-3940256099942544/9214589741";
#endif
                }

                if (!string.IsNullOrEmpty(_interstitialAdUnitId))
                {
#if UNITY_IOS
                    _interstitialAdUnitId = "ca-app-pub-3940256099942544/4411468910";
#else
                    _interstitialAdUnitId = "ca-app-pub-3940256099942544/1033173712";
#endif
                }

                if (!string.IsNullOrEmpty(_rewardedAdUnitId))
                {
#if UNITY_IOS
                    _rewardedAdUnitId = "ca-app-pub-3940256099942544/1712485313";
#else
                    _rewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917";
#endif
                }

                if (!string.IsNullOrEmpty(_appOpenAdUnitId))
                {
#if UNITY_IOS
                    _appOpenAdUnitId = "ca-app-pub-3940256099942544/5662855259";
#else
                    _appOpenAdUnitId = "ca-app-pub-3940256099942544/3419835294";
#endif
                }
            }
            MobileAds.SetiOSAppPauseOnBackground(true);
            MobileAds.RaiseAdEventsOnUnityMainThread = true;
            MobileAds.SetRequestConfiguration(requestConfiguration);
            MobileAds.Initialize(InitAdmobComplete);
        }

        private void InitAdmobComplete(InitializationStatus status)
        {
            LogData( "Initialization complete: ");
            var adapterState = status.getAdapterStatusMap();
            LogData( "Adapter status: ");
            foreach (var adapter in adapterState)
            {
                LogData( adapter.Key + " " + adapter.Value.InitializationState + " " + adapter.Value.Description);
            }
            if (!string.IsNullOrEmpty(_bannerAdUnitId))
            {
                LoadBannerAds();
            }
            if (!string.IsNullOrEmpty(_interstitialAdUnitId))
            {
                LoadInterstitial();
            }
            if (!string.IsNullOrEmpty(_rewardedAdUnitId))
            {
                LoadRewardedVideo();
            }
            if (!string.IsNullOrEmpty(_appOpenAdUnitId))
            {
                LoadAppOpen();
            }
            _initialized = true;
            _initializedAction?.Invoke();
        }

        public bool IsShowingBanner()
        {
            return _isBannerShowing;
        }
        public void ShowBanner(bool collapsible = false)
        {
            if (collapsible && _activeCollapsibleBanner)
            {
                if (string.IsNullOrEmpty(_bannerCollapsibleAdUnitId)) return;
                LoadBannerAds(true);
            }
            else
            {
                if (string.IsNullOrEmpty(_bannerAdUnitId)) return;
                if(_isBannerShowing) return;
                if (_bannerView == null)
                { 
                    LoadBannerAds();
                }
                if (_bannerView == null) return;
                _bannerView.Show();
            }
            _isBannerShowing = true;
        }

        public float BannerHeight()
        {
            if (_bannerCollapsibleView != null) return _bannerCollapsibleView.GetHeightInPixels();
            return _bannerView?.GetHeightInPixels() ?? 0f;
        }
        
        public void HideBanner()
        {
            _isBannerShowing = false;
            // Ẩn banner thường và xóa bỏ banner collapsible
            DestroyBannerCollapsible();
            _bannerView?.Hide();
            _bannerCollapsibleView?.Hide();
            if(_bannerCollapsibleView != null)
            {
                GM.Print(Tag, "Destroy Collapsible Banner ad.");
                _bannerCollapsibleView.Destroy();
                _bannerCollapsibleView = null;
            }
            GM.Print(Tag, "Hide Banner ad.");
        }
        
        private void LoadBannerAds(bool collapsible = false)
        {
            LogData("Start Loading Banner");
            var adRequest = new AdRequest();
            if (collapsible && _activeCollapsibleBanner)
            {
                if (string.IsNullOrEmpty(_bannerCollapsibleAdUnitId)) return;
                _bannerCollapsibleView?.Destroy();
                _bannerCollapsibleView = new BannerView(_bannerCollapsibleAdUnitId, AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth), _bannerPosition);
                // listen to events the banner may raise.
                _bannerCollapsibleView.OnBannerAdLoaded += BannerCollapsibleLoadSuccess;
                _bannerCollapsibleView.OnBannerAdLoadFailed += BannerCollapsibleLoadFailed;
                _bannerCollapsibleView.OnAdPaid += BannerCollapsibleAdPaid;
                _bannerCollapsibleView.OnAdImpressionRecorded += BannerCollapsibleImpressionRecorded;
                _bannerCollapsibleView.OnAdClicked += BannerCollapsibleClicked;
                _bannerCollapsibleView.OnAdFullScreenContentOpened += BannerCollapsibleFullScreenOpened;
                _bannerCollapsibleView.OnAdFullScreenContentClosed += BannerCollapsibleFullScreenClose;
                var collapsiblePosition = _bannerPosition == AdPosition.Bottom ? "bottom" : "top";
                adRequest.Extras.Add("collapsible", collapsiblePosition);
                adRequest.Extras.Add("collapsible_request_id", Guid.NewGuid().ToString());
                _bannerCollapsibleView.LoadAd(adRequest);
                _bannerCollapsibleView.Show();
            }
            else
            {
                if (string.IsNullOrEmpty(_bannerAdUnitId)) return;
                _bannerView?.Destroy();
                _bannerView = new BannerView(_bannerAdUnitId, AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth), _bannerPosition);
                _bannerView.OnBannerAdLoaded += BannerLoadSuccess;
                _bannerView.OnBannerAdLoadFailed += BannerLoadFailed;
                _bannerView.OnAdPaid += BannerAdPaid;
                _bannerView.OnAdImpressionRecorded += BannerImpressionRecorded;
                _bannerView.OnAdClicked += BannerClicked;
                _bannerView.OnAdFullScreenContentOpened += BannerFullScreenOpened;
                _bannerView.OnAdFullScreenContentClosed += BannerFullScreenClose;
                _bannerView.LoadAd(adRequest);
                _bannerView.Hide();
            }
        }
        
        private void DestroyBanner()
        {
            LogData("Destroy Banner");
            if (_bannerView != null)
            {
                _bannerView.OnBannerAdLoaded -= BannerLoadSuccess;
                _bannerView.OnBannerAdLoadFailed -= BannerLoadFailed;
                _bannerView.OnAdPaid -= BannerAdPaid;
                _bannerView.OnAdImpressionRecorded -= BannerImpressionRecorded;
                _bannerView.OnAdClicked -= BannerClicked;
                _bannerView.OnAdFullScreenContentOpened -= BannerFullScreenOpened;
                _bannerView.OnAdFullScreenContentClosed -= BannerFullScreenClose;
                _bannerView.Destroy();
            }
            _bannerView = null;
        }
        
        private void DestroyBannerCollapsible()
        {
            if (_bannerCollapsibleView != null)
            {
                _bannerCollapsibleView.OnBannerAdLoaded -= BannerCollapsibleLoadSuccess;
                _bannerCollapsibleView.OnBannerAdLoadFailed -= BannerCollapsibleLoadFailed;
                _bannerCollapsibleView.OnAdPaid -= BannerCollapsibleAdPaid;
                _bannerCollapsibleView.OnAdImpressionRecorded -= BannerCollapsibleImpressionRecorded;
                _bannerCollapsibleView.OnAdClicked -= BannerCollapsibleClicked;
                _bannerCollapsibleView.OnAdFullScreenContentOpened -= BannerCollapsibleFullScreenOpened;
                _bannerCollapsibleView.OnAdFullScreenContentClosed -= BannerCollapsibleFullScreenClose;
                _bannerCollapsibleView.Destroy();
            }
            _bannerCollapsibleView = null;
        }

        
        private void BannerLoadSuccess()
        {
            LogData("Banner Loaded");
        }
        
        private void BannerCollapsibleLoadSuccess()
        {
            LogData("Collapsible Banner Loaded");
        }
        
        
        private void BannerLoadFailed(LoadAdError loadAdError)
        {
            LogData($"Admob Banner Failed To Load : {loadAdError}" );
            DestroyBanner();
        }
        
        private void BannerCollapsibleLoadFailed(LoadAdError loadAdError)
        {
            LogData($"Admob Banner Failed To Load : {loadAdError}" );
            DestroyBannerCollapsible();
        }
        
        private void BannerAdPaid(AdValue adValue)
        {
            LogData($"Banner ad received a paid event. currency: {adValue.CurrencyCode}, value: {adValue.Value}");
            SendAdPaidEvent(adValue, _bannerAdUnitId, _bannerView.GetResponseInfo().GetMediationAdapterClassName());
        }
        
        private void BannerCollapsibleAdPaid(AdValue adValue)
        {
            LogData($"Banner Collapsible ad received a paid event. currency: {adValue.CurrencyCode}, value: {adValue.Value}");
            SendAdPaidEvent(adValue, _bannerCollapsibleAdUnitId, _bannerCollapsibleView.GetResponseInfo().GetMediationAdapterClassName());
        }
        
        private void BannerImpressionRecorded()
        {
            LogData("Banner view recorded an impression.");
        }

        private void BannerCollapsibleImpressionRecorded()
        {
            LogData("Banner Collapsible view recorded an impression.");
        }
        
        private void BannerClicked()
        {
            LogData("Banner view was clicked.");
        }

        private void BannerCollapsibleClicked()
        {
            LogData("Banner Collapsible view was clicked.");
        }
        
        private void BannerFullScreenOpened()
        {
            LogData("Banner view full screen content opened.");
        }

        private void BannerCollapsibleFullScreenOpened()
        {
            LogData("Banner Collapsible view full screen content opened.");
        }
        private void BannerFullScreenClose()
        {
            LogData("Banner view full screen content closed.");
        }
        
        private void BannerCollapsibleFullScreenClose()
        {
            LogData("Banner Collapsible view full screen content closed.");
        }
        
        public bool IsInterstitialAvailable()
        {
            return !string.IsNullOrEmpty(_interstitialAdUnitId) && DateTime.Now > _timeAdsCanShow && _interstitialAd != null && _interstitialAd.CanShowAd();
        }
        
        public void ShowInterstitial(string placement, UnityAction callback)
        {
            _currentInterstitialPlacement = string.IsNullOrEmpty(placement) ? "default" : placement;
            if (IsInterstitialAvailable())
            {
                GM.LogEvent("show_interstitial_ads", "placement", _currentInterstitialPlacement);
                _onInterstitialClosedCallback = callback;
                _interstitialAd.Show();
                _timeAdsCanShow = DateTime.Now + _intervalShowAds;
            }
            else
            {
                LogData("Interstitial ad cannot be shown.");
                callback?.Invoke();
            }
        }

        public void ShowInterstitial(UnityAction callback)
        {
            ShowInterstitial("default", callback);
        }

        private void LoadInterstitial()
        {
            LogData( "Start Loading Interstitial");
            if (string.IsNullOrEmpty(_interstitialAdUnitId)) return;
            if (_interstitialAd != null)
            {
                _interstitialAd.OnAdPaid -= InterstitialAdPaid;
                _interstitialAd.OnAdImpressionRecorded -= ImpressionRecorded;
                _interstitialAd.OnAdClicked -= AdClicked;
                _interstitialAd.OnAdFullScreenContentOpened -= AdFullScreenOpened;
                _interstitialAd.OnAdFullScreenContentClosed -= AdFullScreenClosed;
                _interstitialAd.OnAdFullScreenContentFailed -= AdFullScreenFailed;
                _interstitialAd.Destroy();
                _interstitialAd = null;
            }
            InterstitialAd.Load(_interstitialAdUnitId, new AdRequest(), InterstitialLoadCallback);
        }
        
        private void InterstitialLoadCallback(InterstitialAd ad, LoadAdError loadAdError)
        {
            if (loadAdError != null || ad == null)
            {
                InterstitialFailed(loadAdError);
            }
            else
            {
                InterstitialLoaded(ad);
            }
        }

        private void InterstitialLoaded(InterstitialAd ad)
        {
            _interstitialAd = ad;
            LogData( "Interstitial Loaded");
            _interstitialAd.OnAdPaid += InterstitialAdPaid;
            _interstitialAd.OnAdImpressionRecorded += ImpressionRecorded;
            _interstitialAd.OnAdClicked += AdClicked;
            _interstitialAd.OnAdFullScreenContentOpened += AdFullScreenOpened;
            _interstitialAd.OnAdFullScreenContentClosed += AdFullScreenClosed;
            _interstitialAd.OnAdFullScreenContentFailed += AdFullScreenFailed;
            _interstitialRetryAttempt = 0;
        }

        private void InterstitialFailed(LoadAdError e)
        {
            _interstitialRetryAttempt++;
            var retryDelay = Math.Pow(2, Math.Min(6, _interstitialRetryAttempt));
            LogData("Interstitial failed to load with error code: " + e.GetResponseInfo());
            Invoke(nameof(LoadInterstitial), (float) retryDelay);
        }
        
        private void InterstitialClosed()
        {
            LogData("Reload Interstitial");
            LoadInterstitial();
            var callback = _onInterstitialClosedCallback;
            _onInterstitialClosedCallback = null;
            callback?.Invoke();
        }
        
        private void InterstitialAdPaid(AdValue adValue)
        {
            LogData($"Interstitial ad paid {adValue.Value} {adValue.CurrencyCode}");
            SendAdPaidEvent(adValue, _interstitialAdUnitId, _interstitialAd.GetResponseInfo().GetMediationAdapterClassName());
        }
      
        private void ImpressionRecorded()
        {
            LogData("Interstitial ad recorded an impression.");
        }
        
        private void AdClicked()
        {
            LogData("Interstitial ad was clicked.");
            GM.LogEvent("show_interstitial_ads_click", "placement", _currentInterstitialPlacement);
        }
        
        private void AdFullScreenOpened()
        {
            LogData("Interstitial ad full screen content opened.");
        }
        
        private void AdFullScreenClosed()
        {
            LogData("Interstitial ad full screen content closed.");
            GM.LogEvent("show_interstitial_ads_suscces", "placement", _currentInterstitialPlacement);
            InterstitialClosed();
        }

        private void AdFullScreenFailed(AdError error)
        {
            LogData($"Interstitial ad failed to open full screen content. Error: {error}");
            InterstitialClosed();
        }

        public bool IsAppOpenAvailable()
        {
            if (_appOpen != null)
            {
                return _appOpen.CanShowAd() && DateTime.Now < _appOpenExpireTime;
            }
            return false;
        }

        public void ShowAppOpen(string placement = "default")
        {
            _currentAppOpenPlacement = string.IsNullOrEmpty(placement) ? "default" : placement;
            if (IsAppOpenAvailable())
            {
                GM.LogEvent("show_aoa_ads", "placement", _currentAppOpenPlacement);
                _appOpen.Show();
                _timeAdsCanShow = DateTime.Now + _intervalShowAds;
            }
            else
            {
                LogData( "AppOpen ad cannot be shown.");
                if (!_initialized) return;
                if (DateTime.Now > _appOpenExpireTime)
                {
                    LoadAppOpen();
                }
            }
        }

        private void LoadAppOpen()
        {
            LogData( "Start Loading App Open");
            if (string.IsNullOrEmpty(_appOpenAdUnitId)) return;
            if (_appOpen != null)
            {
                _appOpen.OnAdPaid -= AppOpenAdPaid;
                _appOpen.OnAdImpressionRecorded -= AppOpenImpressionRecorded;
                _appOpen.OnAdClicked -= AppOpenAdClicked;
                _appOpen.OnAdFullScreenContentOpened -= AppOpenAdFullScreenOpened;
                _appOpen.OnAdFullScreenContentClosed -= AppOpenAdFullScreenClosed;
                _appOpen.OnAdFullScreenContentFailed -= AppOpenAdFullScreenFailed;
                _appOpen.Destroy();
                _appOpen = null;
            }
            AppOpenAd.Load(_appOpenAdUnitId, new AdRequest(), AppOpenLoadCallback);
        }

        private void AppOpenAdFullScreenFailed(AdError error)
        {
            LogData($"App open ad failed to open full screen content. Error: {error}");
            AppOpenClosed();
        }

        private void AppOpenAdFullScreenClosed()
        {
            LogData("App open ad full screen content closed.");
            AppOpenClosed();
        }

        private void AppOpenClosed()
        {
            LogData("Reload App Open");
            LoadAppOpen();
        }
        private void AppOpenAdFullScreenOpened()
        {
            LogData("Open app ad full screen content opened.");
            GM.LogEvent("show_aoa_ads_sucess", "placement", _currentAppOpenPlacement);
        }

        private void AppOpenAdClicked()
        {
            LogData("Open app ad was clicked.");
            GM.LogEvent("show_aoa_ads_click", "placement", _currentAppOpenPlacement);
        }

        private void AppOpenImpressionRecorded()
        {
            LogData( "Open app ad recorded an impression.");
        }

        private void AppOpenAdPaid(AdValue adValue)
        {
            LogData( $"Open app ad paid {adValue.Value} {adValue.CurrencyCode}");
        }

        private void AppOpenLoadCallback(AppOpenAd ad, LoadAdError loadAdError)
        {
            if (loadAdError != null || ad == null)
            {
                AppOpenFailed(loadAdError);
            }
            else
            {
                AppOpenLoaded(ad);
            }
        }

        private void AppOpenLoaded(AppOpenAd ad)
        {
            _appOpenExpireTime = DateTime.Now + TimeSpan.FromHours(4);
            _appOpen = ad;
            LogData("App Open Loaded");
            _appOpen.OnAdPaid += AppOpenAdPaid;
            _appOpen.OnAdImpressionRecorded += AppOpenImpressionRecorded;
            _appOpen.OnAdClicked += AppOpenAdClicked;
            _appOpen.OnAdFullScreenContentOpened += AppOpenAdFullScreenOpened;
            _appOpen.OnAdFullScreenContentClosed += AppOpenAdFullScreenClosed;
            _appOpen.OnAdFullScreenContentFailed += AppOpenAdFullScreenFailed;
            _appOpenRetryAttempt = 0;
        }

        private void AppOpenFailed(LoadAdError loadAdError)
        {
            LogData( $"App open ad failed to load error :{loadAdError}");
            _appOpenRetryAttempt++;
            var retryDelay = Math.Pow(2, Math.Min(6, _appOpenRetryAttempt));
            Invoke(nameof(LoadAppOpen), (float) retryDelay);
        }

        public bool IsRewardedVideoAvailable()
        {
            return _rewardedVideo != null && _rewardedVideo.CanShowAd();
        }
        
        public void ShowRewardedVideo(string placement, UnityAction rewardVideoCallBack)
        {
            if (IsRewardedVideoAvailable())
            {
                _currentRewardedPlacement = string.IsNullOrEmpty(placement) ? "default" : placement;
                GM.LogEvent("show_rewarded_ads", "placement", _currentRewardedPlacement);
                _onRewardedVideoClosed = rewardVideoCallBack;
                _rewardedVideoWatched = false;
                _rewardedVideo.Show(RewardedVideoWatched);
                _timeAdsCanShow = DateTime.Now + _intervalShowAds;
            }
            else
            {
                LogData( "Rewarded video cannot be shown.");
            }
        }

        public void ShowRewardedVideo(UnityAction rewardVideoCallBack)
        {
            ShowRewardedVideo("default", rewardVideoCallBack);
        }
        
        private void RewardedVideoWatched(Reward reward)
        {
            LogData($"Rewarded Video Watched -> Reward amount: {reward.Amount} Reward type: {reward.Type}");
            GM.LogEvent("show_rewarded_ads_sucess", "placement", _currentRewardedPlacement);
            _rewardedVideoWatched = true;
        }
        
        private void LoadRewardedVideo()
        {
            LogData( "Start Loading Rewarded Video");
            if (string.IsNullOrEmpty(_rewardedAdUnitId)) return;
            if (_rewardedVideo != null)
            {
                _rewardedVideo.OnAdPaid -= RewardedPaid;
                _rewardedVideo.OnAdImpressionRecorded -= RewardedImpressionRec;
                _rewardedVideo.OnAdClicked -= RewardedAdClicked;
                _rewardedVideo.OnAdFullScreenContentOpened -= RewardedFullScreenOpened;
                _rewardedVideo.OnAdFullScreenContentClosed -= RewardedFullScreenClosed;
                _rewardedVideo.OnAdFullScreenContentFailed -= RewardedFullScreenFailed;
                _rewardedVideo.Destroy();
                _rewardedVideo = null;
            }
            RewardedAd.Load(_rewardedAdUnitId, new AdRequest(), LoadRewardedVideoCallback);
        }
        
        private void LoadRewardedVideoCallback(RewardedAd ad, LoadAdError loadAdError)
        {
            if (loadAdError != null || ad == null)
            {
                RewardedVideoFailed(loadAdError);
            }
            else
            {
                RewardedVideoLoaded(ad);
            }
        }
        
        private void RewardedVideoLoaded(RewardedAd ad)
        {
            _rewardedVideo = ad;
            LogData( "Rewarded Video Loaded.");

            _rewardedVideo.OnAdPaid += RewardedPaid;
            _rewardedVideo.OnAdImpressionRecorded += RewardedImpressionRec;
            _rewardedVideo.OnAdClicked += RewardedAdClicked;
            _rewardedVideo.OnAdFullScreenContentOpened += RewardedFullScreenOpened;
            _rewardedVideo.OnAdFullScreenContentClosed += RewardedFullScreenClosed;
            _rewardedVideo.OnAdFullScreenContentFailed += RewardedFullScreenFailed;
            _rewardedRetryAttempt = 0;
        }

        private void RewardedVideoFailed(LoadAdError e)
        {
            LogData($"Rewarded Video Failed: {e.GetMessage()}, Rewarded Video Response info: {e.GetResponseInfo()}");
            _rewardedRetryAttempt++;
            var retryDelay = Math.Pow(2, Math.Min(6, _rewardedRetryAttempt));
            Invoke(nameof(LoadRewardedVideo), (float) retryDelay);
        }
        
        private void RewardedImpressionRec()
        {
            LogData("Rewarded ad recorded an impression.");
        }

        private void RewardedPaid(AdValue adValue)
        {
            LogData($"Rewarded ad paid {adValue.Value} {adValue.CurrencyCode}");
        }

        private void RewardedAdClicked()
        {
            LogData("Rewarded ad was clicked.");
            GM.LogEvent("show_rewarded_ads_click", "placement", _currentRewardedPlacement);
        }
        
        private void RewardedFullScreenOpened()
        {
            LogData("Rewarded ad full screen content opened.");
        }

        private void RewardedFullScreenClosed()
        {
            LogData("Rewarded ad full screen content closed.");
            LoadRewardedVideo();
        }

        private void RewardedFullScreenFailed(AdError error)
        {
            LogData($"Rewarded ad failed to open full screen content. Error: {error}");
            LoadRewardedVideo();
        }
        
        public void LoadAdsOnResume()
        {
            if (!_initialized) return;
            if (!IsInterstitialAvailable() && _interstitialRetryAttempt == MaxRetryCount)
            {
                LoadInterstitial();
            }
            if (!IsRewardedVideoAvailable() && _rewardedRetryAttempt == MaxRetryCount)
            {
                LoadRewardedVideo();
            }
            if (!IsAppOpenAvailable() && _appOpenRetryAttempt == MaxRetryCount)
            {
                LoadAppOpen();
            }
            LogData("Run Load Ads On Resume");
        }
        
        public void OpenDebugWindow()
        {
            MobileAds.OpenAdInspector(error =>
            {
                if (error != null)
                {
                    LogData( $"Ad Inspector failed to open with error: {error}");
                    return;
                }

                Debug.Log("Ad Inspector opened successfully.");
            });
        }
        
        
        private void Update()
        {
            if (_rewardedVideoWatched)
            {
                _rewardedVideoWatched = false;
                if (_onRewardedVideoClosed != null)
                {
                    LogData("Rewarded Video Reward Callback");
                    _onRewardedVideoClosed();
                    _onRewardedVideoClosed = null;
                }
                LoadRewardedVideo();
            }
        }
        
        private void SendAdPaidEvent(AdValue adValue, string adUnitId, string network)
        {
#if USING_FIREBASE_ANALYTICS
            Parameter[] ltvData = {
                new (FirebaseAnalytics.ParameterAdPlatform, "Admob"),
                new (FirebaseAnalytics.ParameterAdSource, network ?? "unknown"),
                new (FirebaseAnalytics.ParameterAdUnitName, adUnitId ?? "unknown"),
                new (FirebaseAnalytics.ParameterAdFormat, "unknown"),
                new (FirebaseAnalytics.ParameterCurrency, adValue.CurrencyCode),
                new (FirebaseAnalytics.ParameterValue, adValue.Value / 1000000f),
                new ("value_micro", adValue.Value),
                new ("ad_unit_id", adUnitId ?? "unknown"),
                new ("precision", (int) adValue.Precision),
                new ("placement", "default"),
                new ("country_code", "unknown"),
                new ("network", network ?? "unknown")
            };
            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventAdImpression, ltvData);
#endif
            LogData( $"HandleAdPaidEvent received with ad value (in micros): {adValue.Value}, using adUnitId : {adUnitId},  precision: {adValue.Precision}, currency:{adValue.CurrencyCode} from ad network adapter {network}");
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