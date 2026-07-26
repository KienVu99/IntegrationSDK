using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using aCode.Configs;
using aCode.Services;
using aCode.Tracking;
using UnityEngine.Events;
#if USING_APPSFLYER
using AppsFlyerSDK;
#endif


namespace aCode
{
    /// <summary>
    /// GemAPI - Game Manager API
    /// Provides unified access to ads, analytics, remote config and utilities
    /// </summary>
#if USING_APPSFLYER
    public class GM : MonoBehaviour, IAppsFlyerConversionData
#else
    public class GM : MonoBehaviour
#endif
    {
        private static GM _instance;
        private const int TargetFrameRate = 60;
        private float _startTime;
        private bool _isShowingBanner;
        private float _timer;
        private bool _isDebugMode;
        
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            Print("GM", "Start GemAPI");
            Application.targetFrameRate = TargetFrameRate;
            _startTime = Time.time;
            _isDebugMode = Debug.isDebugBuild || AppConfig.IsDebugMode;
            AdsManager.Instance?.Initialize();
            FirebaseManager.Instance?.Initialize();
#if USING_APPSFLYER
            InitializeAppsflyer();
#endif
            
            // Start background coroutines
            StartCoroutine(AutoShowBannerRoutine());
            StartCoroutine(TrackOnlineTimeRoutine());
        }
        
        private static readonly int[] Thresholds = { 5, 10, 15, 20, 30, 45, 60, 90, 120, 180, 240, 300, 360, 420, 480, 540, 600 };
        private int _nextThresholdIndex;
        
        /// <summary>
        /// Auto show banner when interstitial is ready
        /// </summary>
        private IEnumerator AutoShowBannerRoutine()
        {
            if (!AppConfig.AutoShowBanner) yield break;
            
            var waitInterval = new WaitForSeconds(2f);
            while (!_isShowingBanner)
            {
                yield return waitInterval;
                if (!InterstitialReady) continue;
                ShowBanner();
                _isShowingBanner = true;
            }
        }
        
        /// <summary>
        /// Track user online time milestones
        /// </summary>
        private IEnumerator TrackOnlineTimeRoutine()
        {
            while (_nextThresholdIndex < Thresholds.Length)
            {
                var thresholdMinutes = Thresholds[_nextThresholdIndex];
                var targetTime = thresholdMinutes * 60f;
                var waitTime = targetTime - (Time.time - _startTime);
                
                if (waitTime > 0)
                {
                    yield return new WaitForSeconds(waitTime);
                }
                
                LogEvent($"User_Online_{thresholdMinutes:D3}_Minutes");
                _nextThresholdIndex++;
            }
        }

        #region Timer & Session
        
        /// <summary>
        /// Start or reset the timer
        /// </summary>
        public static void StartTimer()
        {
	        if (_instance != null)
	        {
		        _instance._timer = Time.time;   
	        }
        }
        
        /// <summary>
        /// Get elapsed time since StartTimer() was called (in seconds)
        /// </summary>
        public static int ElapsedTime()
		{
			if (_instance == null) return 0;
			return (int)(Time.time - _instance._timer);
		}
        
        /// <summary>
        /// Get total session time in seconds
        /// </summary>
        public static int SessionTime()
        {
            if (_instance == null) return 0;
            return (int)(Time.time - _instance._startTime);
        }
        
        #endregion

        #region PlayerPrefs - Storage
        
        /// <summary>
        /// Save an integer value
        /// </summary>
        public static void SaveInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Load an integer value
        /// </summary>
        public static int LoadInt(string key, int defaultValue = 0)
        {
            return PlayerPrefs.GetInt(key, defaultValue);
        }
        
        /// <summary>
        /// Save a float value
        /// </summary>
        public static void SaveFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Load a float value
        /// </summary>
        public static float LoadFloat(string key, float defaultValue = 0f)
        {
            return PlayerPrefs.GetFloat(key, defaultValue);
        }
        
        /// <summary>
        /// Save a string value
        /// </summary>
        public static void SaveString(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Load a string value
        /// </summary>
        public static string LoadString(string key, string defaultValue = "")
        {
            return PlayerPrefs.GetString(key, defaultValue);
        }
        
        /// <summary>
        /// Save a boolean value
        /// </summary>
        public static void SaveBool(string key, bool value)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Load a boolean value
        /// </summary>
        public static bool LoadBool(string key, bool defaultValue = false)
        {
            return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
        }
        
        /// <summary>
        /// Check if a key exists
        /// </summary>
        public static bool HasKey(string key)
        {
            return PlayerPrefs.HasKey(key);
        }
        
        /// <summary>
        /// Delete a key
        /// </summary>
        public static void DeleteKey(string key)
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// Delete all PlayerPrefs data
        /// </summary>
        public static void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Increment and return a counter value
        /// </summary>
        public static int Counter(string key)
        {
            var total = PlayerPrefs.GetInt("gem-counter-" + key, 0);
            total++;
            PlayerPrefs.SetInt("gem-counter-" + key, total);
            PlayerPrefs.Save();
            return total;
        }
        
        /// <summary>
        /// Get current counter value without incrementing
        /// </summary>
        public static int GetCounter(string key)
        {
            return PlayerPrefs.GetInt("gem-counter-" + key, 0);
        }
        
        /// <summary>
        /// Reset counter to zero
        /// </summary>
        public static void ResetCounter(string key)
        {
            PlayerPrefs.SetInt("gem-counter-" + key, 0);
            PlayerPrefs.Save();
        }
        
        #endregion

        #region Debug & Logging
        
        /// <summary>
        /// Check if running in debug mode
        /// </summary>
        public static bool DebugMode => _instance != null && _instance._isDebugMode;

        private static bool AdsDisabled => _instance != null && _instance._isDebugMode && AppConfig.TurnOffAds;
        
        /// <summary>
        /// Print a log message with tag
        /// </summary>
        public static void Print(string tag, object message)
        {
            Debug.unityLogger.Log(tag, message);
        }

        /// <summary>
        /// Print a game log message
        /// </summary>
        public static void PrintLog(object message)
        {
            Debug.unityLogger.Log("GameLog", message);
        }
        
        /// <summary>
        /// Print an error message with tag
        /// </summary>
        public static void PrintError(string tag, object message)
        {
            Debug.unityLogger.LogError(tag, message);
        }
        
        /// <summary>
        /// Print a warning message with tag
        /// </summary>
        public static void PrintWarning(string tag, object message)
        {
            Debug.unityLogger.LogWarning(tag, message);
        }
        
        #endregion

        #region Scene Management
        
        /// <summary>
        /// Load scene by name
        /// </summary>
        public static void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
        
        /// <summary>
        /// Load scene by index
        /// </summary>
        public static void LoadScene(int sceneIndex)
        {
            SceneManager.LoadScene(sceneIndex);
        }
        
        /// <summary>
        /// Load scene async with progress callback
        /// </summary>
        public static void LoadSceneAsync(string sceneName, Action<float> onProgress = null, Action onComplete = null)
        {
            if (_instance == null)
            {
                LoadScene(sceneName);
                onComplete?.Invoke();
                return;
            }
            _instance.StartCoroutine(LoadSceneAsyncCoroutine(sceneName, onProgress, onComplete));
        }
        
        private static IEnumerator LoadSceneAsyncCoroutine(string sceneName, Action<float> onProgress, Action onComplete)
        {
            var asyncOp = SceneManager.LoadSceneAsync(sceneName);
            if (asyncOp != null)
            {
                asyncOp.allowSceneActivation = false;

                while (asyncOp.progress < 0.9f)
                {
                    onProgress?.Invoke(asyncOp.progress);
                    yield return null;
                }

                onProgress?.Invoke(1f);
                asyncOp.allowSceneActivation = true;

                while (!asyncOp.isDone)
                {
                    yield return null;
                }
            }

            onComplete?.Invoke();
        }
        
        /// <summary>
        /// Get current scene name
        /// </summary>
        public static string CurrentScene => SceneManager.GetActiveScene().name;
        
        /// <summary>
        /// Get current scene index
        /// </summary>
        public static int CurrentSceneIndex => SceneManager.GetActiveScene().buildIndex;
        
        /// <summary>
        /// Reload current scene
        /// </summary>
        public static void ReloadScene()
        {
            LoadScene(CurrentSceneIndex);
        }
        
        #endregion

        #region Ads - Banner
        
        /// <summary>
        /// Check if banner is currently visible
        /// </summary>
        public static bool BannerVisible
        {
            get
            {
                if (AdsDisabled) return true;
                return AdsReady && AdsManager.Instance.IsShowingBanner();
            }
        }

        /// <summary>
        /// Show banner ad
        /// </summary>
        public static void ShowBanner(bool collapsible = false)
        {
            if (AdsDisabled) return;
            AdsManager.Instance?.ShowBanner(collapsible);
        }
        
        /// <summary>
        /// Hide banner ad
        /// </summary>
        public static void HideBanner()
        {
            if (AdsDisabled) return;
            AdsManager.Instance?.HideBanner();
        }
        
        #endregion

        #region Ads - Interstitial
        
        /// <summary>
        /// Check if interstitial ad is ready
        /// </summary>
        public static bool InterstitialReady
        {
            get
            {
                if (AdsDisabled) return true;
                return AdsReady && AdsManager.Instance.IsInterstitialAvailable();
            }
        }
        
        /// <summary>
        /// Show interstitial ad
        /// </summary>
        public static void ShowInterstitial(string placement, UnityAction showSuccessCallback = null)
        {
            if (AdsDisabled)
            {
                showSuccessCallback?.Invoke();
                return;
            }
            AdsManager.Instance?.ShowInterstitial(placement, showSuccessCallback);
        }

        /// <summary>
        /// Show interstitial ad with default placement
        /// </summary>
        public static void ShowInterstitial(UnityAction showSuccessCallback)
        {
            ShowInterstitial("default", showSuccessCallback);
        }
        
        #endregion

        #region Ads - Rewarded Video
        
        /// <summary>
        /// Check if rewarded video is ready
        /// </summary>
        public static bool RewardedReady
        {
            get
            {
                if (AdsDisabled) return true;
                return AdsReady && AdsManager.Instance.IsRewardedVideoAvailable();
            }
        }
        
        /// <summary>
        /// Show rewarded video ad
        /// </summary>
        public static void ShowRewarded(string placement, UnityAction onRewardCallback)
        {
            if (AdsDisabled)
            {
                onRewardCallback?.Invoke();
                return;
            }
            AdsManager.Instance?.ShowRewardedVideo(placement, onRewardCallback);
        }

        /// <summary>
        /// Show rewarded video ad with default placement
        /// </summary>
        public static void ShowRewarded(UnityAction onRewardCallback)
        {
            ShowRewarded("default", onRewardCallback);
        }

        /// <summary>
        /// Show App Open ad
        /// </summary>
        public static void ShowAppOpen(string placement = "default")
        {
            if (AdsDisabled) return;
            AdsManager.Instance?.ShowAppOpen(placement);
        }
        
        /// <summary>
        /// Open ads mediation debugger
        /// </summary>
        public static void ShowDebugger()
        {
            AdsManager.Instance?.OpenDebugWindow();
        }
        
        #endregion

        #region Analytics - Events
        
        /// <summary>
        /// Set user property for analytics
        /// </summary>
        public static void SetUserProperty(string name, string value)
        {
            Print("UserProperty", $"[User - Property]; name: {name} ; value: {value}");
#if USING_FIREBASE_ANALYTICS
            FirebaseManager.Instance?.SetUserProperty(name, value);      
#endif
        }
        
        /// <summary>
        /// Track screen view
        /// </summary>
        public static void LogScreen(string screenName)
        {
#if USING_FIREBASE_ANALYTICS
            FirebaseManager.Instance?.TrackScreen(screenName);
#endif
        }

        /// <summary>
        /// Track event
        /// </summary>
        public static void LogEvent(string eventName)
        {
#if USING_FIREBASE_ANALYTICS
            FirebaseManager.Instance?.TrackEvent(eventName);
#endif
#if USING_APPSFLYER
            AppsFlyer.sendEvent(eventName, null);
#endif
        }
        
        /// <summary>
        /// Track event with parameters
        /// </summary>
        public static void LogEvent(string eventName, params object[] parameters)
        {
#if USING_FIREBASE_ANALYTICS
            Debug.unityLogger.Log("Firebase", $"Event: {eventName} ; Parameters: {string.Join(", ", parameters)}");
            FirebaseManager.Instance?.TrackEvent(eventName, parameters);
#endif
#if USING_APPSFLYER
            System.Collections.Generic.Dictionary<string, string> afParams = null;
            if (parameters != null && parameters.Length >= 2)
            {
                afParams = new System.Collections.Generic.Dictionary<string, string>();
                var count = parameters.Length / 2;
                for (int i = 0; i < count * 2; i += 2)
                {
                    var key = parameters[i]?.ToString();
                    var val = (i + 1) < parameters.Length ? parameters[i + 1]?.ToString() : "";
                    if (!string.IsNullOrEmpty(key))
                    {
                        afParams[key] = val;
                    }
                }
            }
            AppsFlyer.sendEvent(eventName, afParams);
#endif
        }

        #endregion

        #region Analytics - Level Tracking

        /// <summary>
        /// Track level started
        /// </summary>
        public static void LevelStart(string level)
        {
            LogEvent("level_start", "level", level);
        }
        
        /// <summary>
        /// Track level started
        /// </summary>
        public static void LevelStart(int level)
        {
            LogEvent("level_start", "level", level.ToString());
        }
        
        /// <summary>
        /// Track level replay
        /// </summary>
        public static void LevelReplay(string level)
        {
            LogEvent("level_replay", "level", level);
        }
        
        /// <summary>
        /// Track level replay
        /// </summary>
        public static void LevelReplay(int level)
        {
            LogEvent("level_replay", "level", level.ToString());
        }
        
        /// <summary>
        /// Track level completed
        /// </summary>
        public static void LevelComplete(string level, string playTime)
        {
            LogEvent("level_passed", "level", level, "time_played", playTime);
        }
        
        /// <summary>
        /// Track level completed
        /// </summary>
        public static void LevelComplete(int level, int playTime)
        {
            LogEvent("level_passed", "level", level.ToString(), "time_played", playTime);
        }

        /// <summary>
        /// Track level completed
        /// </summary>
        public static void LevelComplete(int level, float playTime)
        {
            LogEvent("level_passed", "level", level.ToString(), "time_played", playTime);
        }
        
        /// <summary>
        /// Track level failed
        /// </summary>
        public static void LevelFail(string level, string playTime)
        {
            LogEvent("level_failed", "level", level, "time_played", playTime);
        }
        
        /// <summary>
        /// Track level failed
        /// </summary>
        public static void LevelFail(int level, int playTime)
        {
            LogEvent("level_failed", "level", level.ToString(), "time_played", playTime);
        }
        
        /// <summary>
        /// Track level failed
        /// </summary>
        public static void LevelFail(int level, float playTime)
        {
            LogEvent("level_failed", "level", level.ToString(), "time_played", playTime);
        }
        
        #endregion

        #region Remote Config
        
        /// <summary>
        /// Get string value from Remote Config
        /// </summary>
        public static string GetRemoteString(string key, string defaultValue = "")
        {
#if USING_FIREBASE_ANALYTICS
            return !FirebaseReady ? defaultValue : FirebaseManager.Instance.GetStringRemoteConfig(key, defaultValue);
#else
	        return defaultValue;
#endif
        }
        
        /// <summary>
        /// Get integer value from Remote Config
        /// </summary>
        public static int GetRemoteInt(string key, int defaultValue = 0)
        {
#if USING_FIREBASE_ANALYTICS
            return !FirebaseReady ? defaultValue : FirebaseManager.Instance.GetIntRemoteConfig(key, defaultValue);
#else
	        return defaultValue;
#endif
        }
        
        /// <summary>
        /// Get boolean value from Remote Config
        /// </summary>
        public static bool GetRemoteBool(string key, bool defaultValue = false)
        {
#if USING_FIREBASE_ANALYTICS
            return !FirebaseReady ? defaultValue : FirebaseManager.Instance.GetBoolRemoteConfig(key, defaultValue);
#else
            return defaultValue;
#endif
        }

        /// <summary>
        /// Check if Remote Config data is ready
        /// </summary>
        public static bool RemoteReady
        {
            get
            {
#if USING_FIREBASE_ANALYTICS
                var fm = FirebaseManager.Instance;
                return fm != null && fm.IsRemoteConfigReady();
#else
                return true;
#endif
            }
        }
        
        #endregion

        

        #region Utilities - App & Store
        
        /// <summary>
        /// Open app store for rating
        /// </summary>
        public static void OpenRateApp()
        {
#if UNITY_IOS
            if (!string.IsNullOrEmpty(AppConfig.AppleStoreUrl)){
                Application.OpenURL(AppConfig.AppleStoreUrl);
            }
#else
            Application.OpenURL("https://play.google.com/store/apps/details?id=" + Application.identifier);
#endif
        }
        
        /// <summary>
        /// Open URL in browser
        /// </summary>
        public static void OpenURL(string url)
        {
            Application.OpenURL(url);
        }
        
        /// <summary>
        /// Share text (simple share)
        /// </summary>
        public static void Share(string text, string subject = "")
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            using var intentClass = new AndroidJavaClass("android.content.Intent");
            using var intentObject = new AndroidJavaObject("android.content.Intent");
            intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
            intentObject.Call<AndroidJavaObject>("setType", "text/plain");
            intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_SUBJECT"), subject);
            intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), text);
            using var unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");
            using var chooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intentObject, "Share");
            currentActivity.Call("startActivity", chooser);
#elif UNITY_IOS && !UNITY_EDITOR
            // iOS requires native plugin for sharing
            Print("GM", "Share on iOS requires native plugin implementation");
#else
            Print("GM", $"Share: {text}");
#endif
        }
        
        /// <summary>
        /// Quit application
        /// </summary>
        public static void QuitApp()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        
        /// <summary>
        /// Get app version
        /// </summary>
        public static string AppVersion => Application.version;
        
        /// <summary>
        /// Get bundle identifier
        /// </summary>
        public static string BundleId => Application.identifier;
        
        #endregion

        #region Utilities - Device & Platform
        
        /// <summary>
        /// Check if running on mobile
        /// </summary>
        public static bool IsMobile => Application.isMobilePlatform;
        
        /// <summary>
        /// Check if running on Android
        /// </summary>
        public static bool IsAndroid => Application.platform == RuntimePlatform.Android;
        
        /// <summary>
        /// Check if running on iOS
        /// </summary>
        public static bool IsIOS => Application.platform == RuntimePlatform.IPhonePlayer;
        
        /// <summary>
        /// Check if running in Unity Editor
        /// </summary>
        public static bool IsEditor => Application.isEditor;
        
        /// <summary>
        /// Get screen width
        /// </summary>
        public static int ScreenWidth => Screen.width;
        
        /// <summary>
        /// Get screen height
        /// </summary>
        public static int ScreenHeight => Screen.height;
        
        /// <summary>
        /// Get screen aspect ratio
        /// </summary>
        public static float ScreenAspect => (float)Screen.width / Screen.height;
        
        /// <summary>
        /// Check internet connectivity
        /// </summary>
        public static bool HasInternet => Application.internetReachability != NetworkReachability.NotReachable;
        
        /// <summary>
        /// Check if connected via WiFi
        /// </summary>
        public static bool IsWifi => Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
        
        #endregion

        #region Utilities - Delay & Coroutines
        
        /// <summary>
        /// Delay execution of an action
        /// </summary>
        public static void Delay(float seconds, Action action)
        {
            if (_instance == null)
            {
                action?.Invoke();
                return;
            }
            _instance.StartCoroutine(DelayCoroutine(seconds, action));
        }
        
        private static IEnumerator DelayCoroutine(float seconds, Action action)
        {
            yield return new WaitForSeconds(seconds);
            action?.Invoke();
        }
        
        /// <summary>
        /// Delay execution using unscaled time (works when game is paused)
        /// </summary>
        public static void DelayRealtime(float seconds, Action action)
        {
            if (_instance == null)
            {
                action?.Invoke();
                return;
            }
            _instance.StartCoroutine(DelayRealtimeCoroutine(seconds, action));
        }
        
        private static IEnumerator DelayRealtimeCoroutine(float seconds, Action action)
        {
            yield return new WaitForSecondsRealtime(seconds);
            action?.Invoke();
        }
        
        /// <summary>
        /// Run coroutine
        /// </summary>
        public static Coroutine Run(IEnumerator routine)
        {
            return _instance != null ? _instance.StartCoroutine(routine) : null;
        }
        
        /// <summary>
        /// Stop coroutine
        /// </summary>
        public static void Stop(Coroutine routine)
        {
            if (_instance != null && routine != null)
            {
                _instance.StopCoroutine(routine);
            }
        }
        
        #endregion

        #region Utilities - Random & Math
        
        /// <summary>
        /// Get random integer between min (inclusive) and max (exclusive)
        /// </summary>
        public static int RandomInt(int min, int max)
        {
            return UnityEngine.Random.Range(min, max);
        }
        
        /// <summary>
        /// Get random float between min and max
        /// </summary>
        public static float RandomFloat(float min, float max)
        {
            return UnityEngine.Random.Range(min, max);
        }
        
        /// <summary>
        /// Get random boolean
        /// </summary>
        public static bool RandomBool()
        {
            return UnityEngine.Random.value > 0.5f;
        }
        
        /// <summary>
        /// Get random element from array
        /// </summary>
        public static T RandomElement<T>(T[] array)
        {
            if (array == null || array.Length == 0) return default;
            return array[UnityEngine.Random.Range(0, array.Length)];
        }
        
        #endregion

        #region Utilities - ID Generation
        
        /// <summary>
        /// Generate a new unique user ID
        /// </summary>
        public static string GenerateUID()
        {
            return MD5Hash(Guid.NewGuid().ToString() + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        }
        
        /// <summary>
        /// Get or create persistent device ID
        /// </summary>
        public static string DeviceID
        {
            get
            {
                const string key = "gem_device_id";
                if (HasKey(key))
                {
                    return LoadString(key);
                }
                var newId = GenerateUID();
                SaveString(key, newId);
                return newId;
            }
        }
        
        #endregion

        #region Private Helpers
        
        private static bool FirebaseReady
        {
            get
            {
#if USING_FIREBASE_ANALYTICS
                var fm = FirebaseManager.Instance;
                return fm != null && fm.IsInitialized();
#else
                return false;
#endif
            }
        }

        private static bool AdsReady
        {
            get
            {
#if USING_ADMOB_MEDIATION || USING_MAX_MEDIATION
                var ads = AdsManager.Instance;
                return ads != null && ads.IsInitialized();
#else
                return false;
#endif
            }
        }

        private static string MD5Hash(string source)
        {
            var md5 = System.Security.Cryptography.MD5.Create();
            var converted = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(source));
            md5.Clear();
            return BitConverter.ToString(converted).Replace("-", string.Empty).ToLower();
        }
        
#if USING_APPSFLYER
        private void InitializeAppsflyer()
        {
            var devKey = AppConfig.AppsflyerDevKey;
            var appId = AppConfig.AppsflyerAppID;
            
            Print("Appsflyer", $"Initializing Appsflyer. DevKey: {devKey}, AppID: {appId}");
            
            AppsFlyer.setIsDebug(_isDebugMode);
            AppsFlyer.initSDK(devKey, appId, this);
            AppsFlyer.startSDK();
        }

        public void onConversionDataSuccess(string conversionData)
        {
            Print("Appsflyer", "onConversionDataSuccess: " + conversionData);
        }

        public void onConversionDataFail(string error)
        {
            PrintError("Appsflyer", "onConversionDataFail: " + error);
        }

        public void onAppOpenAttribution(string attributionData)
        {
            Print("Appsflyer", "onAppOpenAttribution: " + attributionData);
        }

        public void onAppOpenAttributionFailure(string error)
        {
            PrintError("Appsflyer", "onAppOpenAttributionFailure: " + error);
        }
#endif
        
        #endregion
    }
}
