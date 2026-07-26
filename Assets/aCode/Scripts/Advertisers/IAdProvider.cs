using UnityEngine.Events;

namespace aCode.Advertisers
{
    public interface IAdProvider
    {
        void InitializeAds(UnityAction initializedAds);
        void ShowBanner(bool collapsible);
        void HideBanner();
        bool IsShowingBanner();
        bool IsAppOpenAvailable();
        bool IsInterstitialAvailable();
        bool IsRewardedVideoAvailable();
        void ShowAppOpen(string placement = "default");
        void ShowInterstitial(string placement = "default", UnityAction callback = null);
        void ShowInterstitial(UnityAction callback);
        void ShowRewardedVideo(string placement = "default", UnityAction rewardVideoCallBack = null);
        void ShowRewardedVideo(UnityAction rewardVideoCallBack);
        void OpenDebugWindow();
        void LoadAdsOnResume();
    }

}