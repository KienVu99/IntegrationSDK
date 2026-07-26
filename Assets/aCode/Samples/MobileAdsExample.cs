using System;
using UnityEngine;
using UnityEngine.UI;

namespace aCode.Samples
{
    public class MobileAdsExample : MonoBehaviour
    {
        private float _deltaTime;
        private int _coins;
        public Text message;
        public Button interstitialButton;
        public Button rewardedButton;
        public Text remoteConfig;
        public Text updateText;
    
        public void ShowBanner()
        {
            GM.ShowBanner();
            message.text = GM.BannerVisible ? "Show Banner Event" : "Show Banner Event Error";
        }
        
        public void ShowCollapsibleBanner()
        {
            GM.ShowBanner(collapsible: true);
            message.text = GM.BannerVisible ? "Show Collapsible Banner Event" : "Show Collapsible Banner Event Error";
        }

        public void HideBanner()
        {
            GM.HideBanner();
            message.text = "Hide Banner Event";
        }
    
        public void ShowInterstitial()
        {
            GM.ShowInterstitial("Demo Ads");
            message.text = "Show Interstitial Event";
        }
    
        public void ShowRewardedVideo()
        {
            GM.ShowRewarded("get_jetpack_button_lv1", OnCompleteRewardVideoAction);
            message.text = "Show Rewarded Video Event";
        }
        
        public void OpenDebugWindows()
        {
            GM.ShowDebugger();
        }

        public void SendTrackingEvent()
        {
            GM.LogEvent("MobileEvent", "TimeClick", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            message.text = "Send Tracking Event";
        }
    
        public void GetRemoteConfig()
        {
            remoteConfig.text = GM.GetRemoteBool("pika_classic") ? "Classic UI" : "New UI";
            message.text = "Get Remote Config Data";
        }

        private void Update()
        {
            interstitialButton.interactable = GM.InterstitialReady;
            rewardedButton.interactable = GM.RewardedReady;
            ShowFpsMeter();
        }
    
        private void ShowFpsMeter()
        {
            updateText.gameObject.SetActive(true);
            _deltaTime += (Time.deltaTime - _deltaTime) * 0.1f;
            var fps = 1.0f / _deltaTime;
            updateText.text = $"{fps:0.} fps";
        }

        private void OnCompleteRewardVideoAction()
        {
            _coins += 200;
            message.text = _coins.ToString();
            Debug.Log("Has earned 200 coins");
        }

        private void Helper()
        {
            GM.LoadScene(1);
            GM.LoadScene("GameScene");
            GM.Print("Hello", GM.GenerateUID());
        }
    }
}