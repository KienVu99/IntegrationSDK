using System;
using System.Collections.Generic;
using UnityEngine;

namespace aCode.Configs
{
    public enum BannerPosition { Bottom, Top }
    
    public enum RemoteConfigType
    {
        String,
        Number,
        Bool,
    }

    [Serializable]
    public class RemoteConfigData
    {
        public string key;
        public RemoteConfigType type;
        public string defaultValue;
    }

    [CreateAssetMenu(menuName = "aCode/Create App Settings", fileName = "AppSettings")]
    public class AppSettings : ScriptableObject
    {
        
        [Header("Debug & Ads")]
        [SerializeField] private bool isDebugMode;
        [SerializeField] private bool turnOffAds;

        [Header("Applovin Config")]
        // ReSharper disable once StringLiteralTypo
        [SerializeField] private string applovinSdkKey = "";
        [SerializeField] private bool consentFlowEnabled = true;
        [SerializeField] private string privacyPolicyUrl = "";
        [SerializeField] private string termsOfServiceUrl = "";
        [SerializeField] private bool directedForChildren;
        [SerializeField] private bool autoShowBanner = true;
        [SerializeField] private BannerPosition bannerPosition = BannerPosition.Bottom;
        [SerializeField, Min(0)] private int intervalShowAds = 30;
        
        [Header("Android Ads IDs")]
        [SerializeField] private string admobAppID = "";
        [SerializeField] private string appOpenID = "";
        [SerializeField] private string bannerID = "";
        [SerializeField] private string bannerCollapsibleID = "";
        [SerializeField] private string interstitialID = "";
        [SerializeField] private string rewardedVideoID = "";
        
        [Header("IOS Ads IDs")]
        [SerializeField] private string admobIosAppID = "";
        [SerializeField] private string appOpenIosID = "";
        [SerializeField] private string bannerIosID = "";
        [SerializeField] private string bannerCollapsibleIosID = "";
        [SerializeField] private string interstitialIosID = "";
        [SerializeField] private string rewardedVideoIosID = "";
        
        [Header("Misc")]
        [SerializeField] private string testDeviceID = "";
        [SerializeField] private string appleStoreUrl = "";

        [Header("Appsflyer Config")]
        [SerializeField] private string appsflyerDevKey = "";
        [SerializeField] private string appsflyerAppID = "";

        [Header("Remote Config")]
        [SerializeField] private List<RemoteConfigData> remoteConfigs = new List<RemoteConfigData>();
        
        public bool IsDebugMode => isDebugMode;
        public bool TurnOffAds => turnOffAds;
        public string ApplovinSdkKey => applovinSdkKey;
        public bool ConsentFlowEnabled => consentFlowEnabled;
        public string PrivacyPolicyUrl => privacyPolicyUrl;
        public string TermsOfServiceUrl => termsOfServiceUrl;
        public string BannerID => bannerID;
        public string BannerCollapsibleID => bannerCollapsibleID;
        public string BannerIosID => bannerIosID;
        public string BannerCollapsibleIosID => bannerCollapsibleIosID;
        public string InterstitialID => interstitialID;
        public string InterstitialIosID => interstitialIosID;
        public string RewardedVideoID => rewardedVideoID;
        public string RewardedVideoIosID => rewardedVideoIosID;
        public string AppOpenID => appOpenID;
        public string AppOpenIosID => appOpenIosID;
        public string AdmobAppID => admobAppID;
        public string AdmobIosAppID => admobIosAppID;
        public bool DirectedForChildren => directedForChildren;
        public bool AutoShowBanner => autoShowBanner;
        public BannerPosition BannerPosition => bannerPosition;
        public int IntervalShowAds => intervalShowAds;
        public string TestDeviceID => testDeviceID;
        public string AppleStoreUrl => appleStoreUrl;
        public string AppsflyerDevKey => appsflyerDevKey;
        public string AppsflyerAppID => appsflyerAppID;
        public IReadOnlyList<RemoteConfigData> RemoteConfigs => remoteConfigs;

        public static AppSettings Load()
        {
            return Resources.Load<AppSettings>("aCodeSettings");
        }

#if UNITY_EDITOR
        public static AppSettings EnsureAsset()
        {
            var inst = Load();
            if (inst != null) return inst;
            
            if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/Resources")) UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources");
            
            inst = CreateInstance<AppSettings>();
            UnityEditor.AssetDatabase.CreateAsset(inst, $"Assets/Resources/aCodeSettings.asset");
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            return inst;
        }
#endif
    }
}
