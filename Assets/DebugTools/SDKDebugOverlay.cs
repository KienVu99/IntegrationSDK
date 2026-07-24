using IntegrationSDK.Core;
using UnityEngine;

namespace IntegrationSDK.DebugTools
{
    // ponytail: OnGUI keeps this to one file with no scene/prefab wiring needed -
    // works on any machine that just clones the repo and hits Play. Swap for a
    // real UI if this needs to look nicer than debug buttons.
    internal class SDKDebugOverlay : MonoBehaviour
    {
        private Vector2 _scroll;
        private bool _bannerVisible;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Create()
        {
            var go = new GameObject("[IntegrationSDK Debug Overlay]");
            go.AddComponent<SDKDebugOverlay>();
            Object.DontDestroyOnLoad(go);
        }

        private void OnGUI()
        {
            var scale = Screen.dpi > 0 ? Screen.dpi / 160f : 2f;
            GUIUtility.ScaleAroundPivot(new Vector2(scale, scale), Vector2.zero);

            var areaWidth = Screen.width / scale - 20;
            var areaHeight = Screen.height / scale - 20;
            GUILayout.BeginArea(new Rect(10, 10, areaWidth, areaHeight), GUI.skin.box);
            GUILayout.Label("IntegrationSDK Debug");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Interstitial", GUILayout.Height(60))) AdManager.ShowInterstitial("debug_overlay");
            if (GUILayout.Button("Rewarded", GUILayout.Height(60))) AdManager.ShowRewarded("debug_overlay", () => Debug.Log("IntegrationSDK.Debug: reward granted"));
            if (GUILayout.Button("App Open", GUILayout.Height(60))) AdManager.ShowAppOpen("debug_overlay");
            if (GUILayout.Button(_bannerVisible ? "Hide Banner" : "Show Banner", GUILayout.Height(60)))
            {
                if (_bannerVisible) AdManager.HideBanner(); else AdManager.ShowBanner("debug_overlay");
                _bannerVisible = !_bannerVisible;
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            GUILayout.Label("Tracking log (Initialize / LogEvent / LogAdRevenue):");
            _scroll = GUILayout.BeginScrollView(_scroll, GUI.skin.box);
            foreach (var line in SDKDebugTrackingBackend.Log)
            {
                GUILayout.Label(line);
            }
            GUILayout.EndScrollView();

            GUILayout.EndArea();
        }
    }
}
