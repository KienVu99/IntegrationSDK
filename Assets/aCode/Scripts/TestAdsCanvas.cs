using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace aCode
{
    /// <summary>
    /// Test Canvas component for testing Ads (Banner, Interstitial, Rewarded, App Open) and Level Tracking
    /// </summary>
    public class TestAdsCanvas : MonoBehaviour
    {
        [Header("Status UI")]
        public Text statusText;
        public Text interBtnText;
        public Text rewardBtnText;
        public Button interBtn;
        public Button rewardBtn;

        private void Awake()
        {
            EnsureEventSystem();
        }

        private void Start()
        {
            EnsureEventSystem();
            if (statusText == null)
            {
                CreateDefaultUI();
            }
            SetStatus("Test Canvas Ready - Click buttons below to test real ads!");
        }

        private void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            }
        }

        private void Update()
        {
            if (interBtnText != null)
            {
                bool isReady = GM.InterstitialReady;
                interBtnText.text = isReady ? "SHOW INTERSTITIAL [READY]" : "SHOW INTERSTITIAL [LOADING...]";
                if (interBtn != null) interBtn.interactable = isReady;
            }

            if (rewardBtnText != null)
            {
                bool isReady = GM.RewardedReady;
                rewardBtnText.text = isReady ? "SHOW REWARDED VIDEO [READY]" : "SHOW REWARDED VIDEO [LOADING...]";
                if (rewardBtn != null) rewardBtn.interactable = isReady;
            }
        }

        public void SetStatus(string msg)
        {
            if (statusText != null)
            {
                statusText.text = $"[{DateTime.Now:HH:mm:ss}] {msg}";
            }
            Debug.Log($"[TestAdsCanvas] {msg}");
        }

        #region Ad Actions

        public void ShowBanner()
        {
            GM.ShowBanner();
            SetStatus("Show Banner called -> GM.ShowBanner()");
        }

        public void ShowCollapsibleBanner()
        {
            GM.ShowBanner(collapsible: true);
            SetStatus("Show Collapsible Banner called -> GM.ShowBanner(true)");
        }

        public void HideBanner()
        {
            GM.HideBanner();
            SetStatus("Hide Banner called -> GM.HideBanner()");
        }

        public void ShowInterstitial()
        {
            SetStatus("Requesting Interstitial Ad (placement: test_inter_placement)...");
            GM.ShowInterstitial("test_inter_placement", () =>
            {
                SetStatus("Interstitial Ad Closed / Completed Callback Invoked!");
            });
        }

        public void ShowRewarded()
        {
            SetStatus("Requesting Rewarded Video Ad (placement: test_reward_placement)...");
            GM.ShowRewarded("test_reward_placement", () =>
            {
                SetStatus("Rewarded Video Watched! Reward Granted (+200 Coins).");
            });
        }

        public void ShowAppOpen()
        {
            SetStatus("Requesting App Open Ad (placement: test_aoa_placement)...");
            GM.ShowAppOpen("test_aoa_placement");
        }

        public void OpenDebugger()
        {
            GM.ShowDebugger();
            SetStatus("Open Mediation Debugger called -> GM.ShowDebugger()");
        }

        #endregion

        #region Level Tracking Actions

        public void TestLevelStart()
        {
            GM.LevelStart("level_01");
            SetStatus("Logged event: level_start (level: level_01)");
        }

        public void TestLevelPassed()
        {
            GM.LevelComplete("level_01", 120);
            SetStatus("Logged event: level_passed (level: level_01, time_played: 120)");
        }

        public void TestLevelFailed()
        {
            GM.LevelFail("level_01", 45);
            SetStatus("Logged event: level_failed (level: level_01, time_played: 45)");
        }

        #endregion

        #region Dynamic UI Builder

        private void CreateDefaultUI()
        {
            var canvas = gameObject.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
            }
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            var scaler = gameObject.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);

            if (gameObject.GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }

            var panelObj = new GameObject("TestPanel", typeof(RectTransform), typeof(Image));
            panelObj.transform.SetParent(transform, false);
            var panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.05f, 0.05f);
            panelRect.anchorMax = new Vector2(0.95f, 0.95f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            panelObj.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f, 0.92f);

            var layout = panelObj.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(30, 30, 30, 30);
            layout.spacing = 15;
            layout.childControlWidth = true;
            layout.childControlHeight = true;

            // Status Text
            var statusObj = new GameObject("StatusText", typeof(RectTransform), typeof(Text));
            statusObj.transform.SetParent(panelObj.transform, false);
            statusText = statusObj.GetComponent<Text>();
            statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Font.CreateDynamicFontFromOSFont("Arial", 22);
            statusText.fontSize = 26;
            statusText.alignment = TextAnchor.MiddleCenter;
            statusText.color = Color.yellow;
            var statusElement = statusObj.AddComponent<LayoutElement>();
            statusElement.minHeight = 80;

            // Buttons
            CreateButton(panelObj.transform, "SHOW BANNER", ShowBanner, new Color(0.2f, 0.4f, 0.8f));
            CreateButton(panelObj.transform, "HIDE BANNER", HideBanner, new Color(0.5f, 0.5f, 0.5f));
            
            interBtn = CreateButton(panelObj.transform, "SHOW INTERSTITIAL", ShowInterstitial, new Color(0.9f, 0.5f, 0.1f), out interBtnText);
            rewardBtn = CreateButton(panelObj.transform, "SHOW REWARDED VIDEO", ShowRewarded, new Color(0.1f, 0.7f, 0.3f), out rewardBtnText);
            
            CreateButton(panelObj.transform, "SHOW APP OPEN (AOA)", ShowAppOpen, new Color(0.7f, 0.2f, 0.7f));
            CreateButton(panelObj.transform, "OPEN MEDIATION DEBUGGER", OpenDebugger, new Color(0.4f, 0.4f, 0.4f));
            CreateButton(panelObj.transform, "TEST LEVEL START", TestLevelStart, new Color(0.2f, 0.6f, 0.9f));
            CreateButton(panelObj.transform, "TEST LEVEL PASSED", TestLevelPassed, new Color(0.3f, 0.8f, 0.4f));
            CreateButton(panelObj.transform, "TEST LEVEL FAILED", TestLevelFailed, new Color(0.9f, 0.3f, 0.3f));
        }

        private Button CreateButton(Transform parent, string label, UnityEngine.Events.UnityAction onClick, Color btnColor)
        {
            Text textComp;
            return CreateButton(parent, label, onClick, btnColor, out textComp);
        }

        private Button CreateButton(Transform parent, string label, UnityEngine.Events.UnityAction onClick, Color btnColor, out Text textComp)
        {
            var btnObj = new GameObject($"Btn_{label.Replace(" ", "_")}", typeof(RectTransform), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(parent, false);
            btnObj.GetComponent<Image>().color = btnColor;
            var button = btnObj.GetComponent<Button>();
            button.onClick.AddListener(onClick);

            var element = btnObj.AddComponent<LayoutElement>();
            element.minHeight = 70;

            var txtObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
            txtObj.transform.SetParent(btnObj.transform, false);
            var txtRect = txtObj.GetComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.offsetMin = Vector2.zero;
            txtRect.offsetMax = Vector2.zero;

            textComp = txtObj.GetComponent<Text>();
            textComp.text = label;
            textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Font.CreateDynamicFontFromOSFont("Arial", 18);
            textComp.fontSize = 22;
            textComp.fontStyle = FontStyle.Bold;
            textComp.alignment = TextAnchor.MiddleCenter;
            textComp.color = Color.white;

            return button;
        }

        #endregion
    }
}
