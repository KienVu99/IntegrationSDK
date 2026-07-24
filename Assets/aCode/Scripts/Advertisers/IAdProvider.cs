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
        void ShowAppOpen();
        void ShowInterstitial(UnityAction callback);
        void ShowRewardedVideo(UnityAction rewardVideoCallBack);
        void OpenDebugWindow();
        void LoadAdsOnResume();
    }

}