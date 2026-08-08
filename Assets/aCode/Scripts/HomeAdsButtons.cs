using UnityEngine;
using aCode;

public class HomeAdsButtons : MonoBehaviour
{
    public void OnBannerButton() => GM.ShowBanner();

    public void OnInterstitialButton() => GM.ShowInterstitial("home_button");

    public void OnRewardedButton()
    {
        if (GM.RewardedReady)
            GM.ShowRewarded("home_button", () => Debug.Log("Reward granted"));
        else
            Debug.Log("Rewarded not ready");
    }

    public void OnAppOpenButton() => GM.ShowAppOpen("home_button");
}
