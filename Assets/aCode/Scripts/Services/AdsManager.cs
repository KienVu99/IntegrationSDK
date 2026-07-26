using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using aCode.Advertisers;
#if UNITY_IOS
#if ACTIVE_MAX_MEDIATION || ACTIVE_ADMOB_MEDIATION
using Unity.Advertisement.IosSupport;
#endif
#endif

namespace aCode.Services
{
	public class AdsManager : MonoBehaviour
    {
        private const string Tag = "MobileAdsManager";
        private static readonly HashSet<int> ThresholdEvent = new() { 3, 5, 7, 10, 15, 20, 30, 40, 50, 60 , 75 , 90, 120, 150, 200, 300, 500, 600, 700, 800, 900, 1000 };
        private static AdsManager _instance;
        private bool _isAuthorizationTrackingChecked;
        
        private UnityAction _initializedAction;
        private bool _initialized, _continueInitialization, _runLogTestDevice;
        private IAdProvider _adManager;
        
        public static AdsManager Instance
        {
            get
            {
                if (_instance != null) return _instance;
                var go = new GameObject{ name = "AdsManager" };
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<AdsManager>();
                return _instance;
            }
        }
        
        public void Initialize(UnityAction completeMethodAction = null)
        {
            if (_adManager == null)
            {
#if USING_MAX_MEDIATION
                _adManager = gameObject.AddComponent<ApplovinImplementation>();
#else
                _adManager = gameObject.AddComponent<AdmobImplementation>();
#endif
            }
            if (_initialized) return;
            if (!_isAuthorizationTrackingChecked)
            {
                UpdateTrackingStatus();
            }
            
            _initializedAction = completeMethodAction;
            StartCoroutine(WaitForConsent(ContinueInitialization));
        }

        private void UpdateTrackingStatus()
        {
            if(_isAuthorizationTrackingChecked) return;
#if UNITY_IOS
#if ACTIVE_MAX_MEDIATION || ACTIVE_ADMOB_MEDIATION
            var status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
            if (status == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
            {
                ATTrackingStatusBinding.RequestAuthorizationTracking();
            }
            else
            {
                _isAuthorizationTrackingChecked = true;
            }
#endif
#endif
        }
        
        private IEnumerator WaitForConsent(UnityAction continueAction)
        {
            yield return null;
            continueAction?.Invoke();
            yield return null;
        }
        
        private void ContinueInitialization()
        {
            _continueInitialization = true;
        }

        private void InitializeAdvertiser()
        {
            _adManager.InitializeAds(OnInitialized);
        }
        
        private void OnInitialized()
        {
            _initialized = true;
            _initializedAction?.Invoke();
        }

        public bool IsInitialized()
        {
            return _initialized;
        }
        
        private void Update()
        {
            if (!_isAuthorizationTrackingChecked)
            {
                UpdateTrackingStatus();
            }
            if (!_continueInitialization) return;
            _continueInitialization = false;
            InitializeAdvertiser();
        }

        public void ShowInterstitial(string placement, UnityAction showSuccessCallback = null)
        {
            if (IsInitialized() && _adManager.IsInterstitialAvailable())
            {
                _adManager.ShowInterstitial(placement, showSuccessCallback);
                var countShowInter = GM.Counter("count_interstitial_ads_key");
                if (ThresholdEvent.Contains(countShowInter))
                {
                    GM.LogEvent($"Show_{countShowInter:D3}_Interstitial");
                }
            }
            else
            {
                GM.Print(Tag, $"Interstitial is NOT available");
                showSuccessCallback?.Invoke();
            }
        }

        public void ShowInterstitial(UnityAction showSuccessCallback)
        {
            ShowInterstitial("default", showSuccessCallback);
        }
        
        public void ShowRewardedVideo(string placement, UnityAction rewardVideoCallBack)
        {
            if (!IsInitialized())
            {
                return;
            }

            if (_adManager.IsRewardedVideoAvailable())
            {
                GM.Print(Tag, $"Rewarded video is available");
                _adManager.ShowRewardedVideo(placement, rewardVideoCallBack);
                var countShowReward = GM.Counter("count_reward_ads_key");
                if (ThresholdEvent.Contains(countShowReward))
                {
                    GM.LogEvent($"Show_{countShowReward:D3}_RewardedVideo");
                }
            }
            else
            {
                GM.Print(Tag, $"Rewarded video is NOT available");
            }
        }

        public void ShowRewardedVideo(UnityAction rewardVideoCallBack)
        {
            ShowRewardedVideo("default", rewardVideoCallBack);
        }
        
        public void ShowBanner(bool collapsible = false)
        {
            if (!IsInitialized()) return;
            GM.Print(Tag, $"Banner loaded from Admob");
            _adManager.ShowBanner(collapsible);
        }

        public void HideBanner()
        {
            if (!IsInitialized()) return;
            GM.Print(Tag, $"Hide banner Admob");
            _adManager.HideBanner();
        }

        public bool IsShowingBanner()
        {
            return IsInitialized() && _adManager.IsShowingBanner();
        }

        public bool IsRewardedVideoAvailable()
        {
            return IsInitialized() && _adManager.IsRewardedVideoAvailable();
        }
        
        public bool IsInterstitialAvailable()
        {
            return IsInitialized() && _adManager.IsInterstitialAvailable();
        }

        internal void OpenDebugWindow()
        {
            _adManager.OpenDebugWindow();
        }

        private void OnApplicationFocus(bool focus)
        {
            if (!focus) return;
            if (_adManager != null)
            {
                _adManager.LoadAdsOnResume();
            }
        }
        
        public void ShowAppOpen(string placement = "default")
        {
            if (!IsInitialized()) return;
            if (_adManager.IsAppOpenAvailable())
            {
                _adManager.ShowAppOpen(placement);
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus || GM.SessionTime() < 25)  return;
            if (IsInitialized() && _adManager.IsAppOpenAvailable())
            {
                GM.Print(Tag, $"Show App Open Ads on application pause");
                _adManager.ShowAppOpen("resume_pause");
            }
            else
            {
                GM.Print(Tag, $"App Open Ads is NOT available");
            }
        }
        
    }
}