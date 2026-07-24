using System;

namespace IntegrationSDK.Core
{
    public interface IAdProvider
    {
        void Initialize(IntegrationSDKConfig config);
        void ShowInterstitial(string placement);
        void ShowRewarded(string placement, Action onRewardEarned);
        void ShowAppOpen(string placement);
        void ShowBanner(string placement);
        void HideBanner();
    }
}
