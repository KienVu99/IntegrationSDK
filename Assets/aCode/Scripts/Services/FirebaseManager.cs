using UnityEngine;
using System.Threading.Tasks;
using System.Collections;

#if USING_FIREBASE_ANALYTICS
using Firebase;
using Firebase.Analytics;
using Firebase.Extensions;
using Firebase.Crashlytics;
#if USING_FIREBASE_REMOTE_CONFIG
using System;
using System.Collections.Generic;
using Firebase.RemoteConfig;
using aCode.Configs;
#endif
#endif

#if UNITY_IOS
#if USING_ADMOB_MEDIATION || USING_MAX_MEDIATION
using Unity.Advertisement.IosSupport;
#endif
#endif

namespace aCode.Tracking
{
    public class FirebaseManager: MonoBehaviour
    {
        
        private const string Tag = "FirebaseManager";
        private static FirebaseManager _instance;
        private static bool _isQuitting;
        
#if USING_FIREBASE_ANALYTICS
        private DependencyStatus _dependencyStatus = DependencyStatus.UnavailableOther;
#endif
#if USING_FIREBASE_MESSAGING
        private const string FirebaseTopic = "GameNotify";
#endif
        
        private bool _isFirebaseInitialized;
        private bool _isRemoteConfigReady;
        private int _totalMessageReceived;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _instance = null;
            _isQuitting = false;
        }
        
        public static FirebaseManager Instance
        {
            get
            {
                if (_instance != null) return _instance;
                if (!Application.isPlaying || _isQuitting) return null;
                var existed = FindFirstObjectByType<FirebaseManager>();
                if (existed != null)
                {
                    _instance = existed;
                    return _instance;
                }
                var go = new GameObject{ name = "FirebaseManager" };
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<FirebaseManager>();
                return _instance;
            }
        }
        
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        public void Initialize()
        {
            if (_isFirebaseInitialized) return;
            GM.Print(Tag, "Init Firebase Manager");
            StartCoroutine(InitFirebaseManager());
        }

        private IEnumerator InitFirebaseManager()
        {
#if USING_FIREBASE_ANALYTICS
            // Initialize Firebase
            var task = FirebaseApp.CheckAndFixDependenciesAsync();
            while (!task.IsCompleted) yield return null;
            _dependencyStatus = task.Result;
            if (_dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
                _isFirebaseInitialized = true;
            }
            else
            {
                GM.Print(Tag, $"Could not resolve all Firebase dependencies: {_dependencyStatus}");
            }
#else
        _isFirebaseInitialized = true;
        yield break;
#endif
            
        }
        
#if USING_FIREBASE_ANALYTICS
        
        private void InitializeFirebase()
        {
            _isFirebaseInitialized = true;
            Crashlytics.ReportUncaughtExceptionsAsFatal = true;
#if USING_FIREBASE_MESSAGING
            Firebase.Messaging.FirebaseMessaging.SubscribeAsync(FirebaseTopic).ContinueWithOnMainThread(
                task => { LogFirebaseMessageTaskCompletion(task, "SubscribeAsync"); }
            );
            GM.Print(Tag,"Firebase Messaging Initialized");
            Firebase.Messaging.FirebaseMessaging.RequestPermissionAsync().ContinueWithOnMainThread(
                task => { LogFirebaseMessageTaskCompletion(task, "RequestPermissionAsync");}
            );
#endif
#if USING_FIREBASE_REMOTE_CONFIG
            var remoteConfigData = AppConfig.RemoteConfigs;
            
            if (remoteConfigData.Count > 0)
            {
                var defaultsRemoteConfig = new Dictionary<string, object>(remoteConfigData.Count);
                foreach (var t in remoteConfigData)
                {
                    var remoteKey = t.key.Trim();
                    var remoteValue = t.defaultValue.Trim();
                    switch (t.type)
                    {
                        case RemoteConfigType.Bool:
                            defaultsRemoteConfig.Add(remoteKey, bool.Parse(remoteValue));
                            break;
                        case RemoteConfigType.Number:
                            defaultsRemoteConfig.Add(remoteKey, int.Parse(remoteValue));
                            break;
                        case RemoteConfigType.String:
                        default:
                            defaultsRemoteConfig.Add(remoteKey, remoteValue);
                            break;
                    }
                }
                FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaultsRemoteConfig).ContinueWithOnMainThread(_ => { GetRemoteConfigData(); });
            }
            else
            {
                GM.Print(Tag, "Set default config to using remote config");
            }
#endif
            
        }
        
#if USING_FIREBASE_REMOTE_CONFIG
        private void GetRemoteConfigData()
        {
            GM.Print("Firebase", "RemoteConfig configured and ready!");
            var fetchTask = FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.Zero);
            fetchTask.ContinueWithOnMainThread(FetchRemoteConfigDataOnComplete);
        }

        private void FetchRemoteConfigDataOnComplete(Task fetchTask)
        {
            if (fetchTask.IsCanceled)
            {
                GM.Print(Tag,"Fetch canceled.");
                _isRemoteConfigReady = true; // Mark as ready with defaults
            }
            else if (fetchTask.IsFaulted)
            {
                GM.Print(Tag,"Fetch encountered an error.");
                _isRemoteConfigReady = true; // Mark as ready with defaults
            }
            else if (fetchTask.IsCompleted)
            {
                GM.Print(Tag,"Fetch completed successfully!");
            }

            var info = FirebaseRemoteConfig.DefaultInstance.Info;
            GM.Print(Tag, "Fetch completed at: " + info.FetchTime + " status: " + info.LastFetchStatus);
            switch (info.LastFetchStatus)
            {
                case LastFetchStatus.Success:
                    FirebaseRemoteConfig.DefaultInstance.ActivateAsync().ContinueWithOnMainThread(_ => {
                        GM.Print(Tag, $"Remote data loaded and ready (last fetch time {info.FetchTime}).");
                        _isRemoteConfigReady = true; // Remote Config is now ready!
                    });
                    break;
                case LastFetchStatus.Failure:
                    switch (info.LastFetchFailureReason)
                    {
                        case FetchFailureReason.Error:
                            GM.Print(Tag,"Fetch failed for unknown reason");
                            break;
                        case FetchFailureReason.Throttled:
                            GM.Print(Tag,"Fetch throttled until " + info.ThrottledEndTime);
                            break;
                        case FetchFailureReason.Invalid:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    _isRemoteConfigReady = true; // Mark as ready with defaults on failure
                    break;
                case LastFetchStatus.Pending:
                    GM.Print(Tag,"Latest Fetch call still pending.");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
#endif
        
#if USING_FIREBASE_MESSAGING
        private void OnFirebaseTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token) {
            GM.Print(Tag, "Received Registration Token: " + token.Token);
            _isFirebaseInitialized = true;
        }

        private void OnFirebaseMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e)
        {
            _totalMessageReceived++;
            if (!string.IsNullOrEmpty(e.Message?.From))
                GM.Print("Firebase", "Received a new message from: " + e.Message.From);
            
            if (e.Message?.Link != null) {
                GM.Print("Firebase","Open Link: " + e.Message.Link);
                Application.OpenURL(e.Message.Link.ToString());
                return;
            }
            var messageData = e.Message?.Data;
            
            if (messageData != null && messageData.Count > 0) {
                GM.Print("Firebase","Data:");
                foreach (var iter in messageData) {
                    switch (iter.Key)
                    {
                        case "url":
                            Application.OpenURL(iter.Value);
                            return;
                        case "ads":
                            GM.ShowInterstitial("push-notify");
                            return;
                        default:
                            GM.Print("Firebase", "  " + iter.Key + ": " + iter.Value);
                            break;
                    }
                }
            }
            GM.Print("Firebase","Firebase Message Received");
        }
        
        private void LogFirebaseMessageTaskCompletion(Task task, string operation) {
            if (task.IsCanceled) {
                GM.Print("Firebase",operation + " canceled.");
            } else if (task.IsFaulted)
            {
                GM.Print("Firebase",operation + " uncounted an error.");
                if (task.Exception == null) return;
                foreach (var exception in task.Exception.Flatten().InnerExceptions)
                {
                    var errorCode = "";
                    if (exception is FirebaseException firebaseEx)
                    {
                        errorCode = $"Error.{((Firebase.Messaging.Error)firebaseEx.ErrorCode).ToString()}: ";
                    }

                    GM.Print("Firebase", errorCode + exception);
                }
            } else if (task.IsCompleted) {
                GM.Print("Firebase",operation + " completed");
            }

            if (_totalMessageReceived > 0)
            {
                GM.Print("Firebase",$"Total message received: {_totalMessageReceived}");
            }
        }
#endif
        
        public void TrackScreen(string screen)
        {
            if (!_isFirebaseInitialized || string.IsNullOrWhiteSpace(screen)) return;
            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventScreenView, FirebaseAnalytics.ParameterScreenName, screen);
        }

        public void SetUserProperty(string propertyName, string propertyValue)
        {
            if (!_isFirebaseInitialized)
                return;
            FirebaseAnalytics.SetUserProperty(propertyName, propertyValue);
        }
        public void TrackEvent(string eventName)
        {
            if (!_isFirebaseInitialized)
                return;
            FirebaseAnalytics.LogEvent(eventName);
        }

        public void TrackEvent(string eventName, params object[] parameterList)
        {
            if (!_isFirebaseInitialized || string.IsNullOrWhiteSpace(eventName)) return;
            if (parameterList == null || parameterList.Length < 2) {
                FirebaseAnalytics.LogEvent(eventName);
                return;
            }
            var count = parameterList.Length / 2;
            var ps = new Parameter[count];
            for (int i = 0, j = 0; j < count; i += 2, j++)
            {
                var key = parameterList[i]?.ToString() ?? "";
                var val = (i + 1) < parameterList.Length ? parameterList[i + 1] : null;
                if (string.IsNullOrEmpty(key)) { GM.PrintError(Tag, $"LogEvent empty key @ {i}"); ps[j] = new Parameter("_invalid_", ""); continue; }

                ps[j] = val switch
                {
                    null => new Parameter(key, ""),
                    string s => new Parameter(key, s),
                    int n => new Parameter(key, n),
                    long l => new Parameter(key, l),
                    float f => new Parameter(key, f),
                    double d => new Parameter(key, d),
                    bool b => new Parameter(key, b ? 1 : 0),
                    _ => new Parameter(key, val.ToString())
                };
            }
            FirebaseAnalytics.LogEvent(eventName, ps);
        }

        public void TrackUserProperty(string propertyName, string propertyValue)
        {
            if (!_isFirebaseInitialized)
                return;
            FirebaseAnalytics.SetUserProperty(propertyName, propertyValue);
        }
        
        public string GetStringRemoteConfig(string key, string defaultValue = "")
        {
#if USING_FIREBASE_REMOTE_CONFIG
            return _isFirebaseInitialized ? FirebaseRemoteConfig.DefaultInstance.GetValue(key).StringValue : defaultValue;
#else         
            return defaultValue;
#endif
        }
        
        public bool GetBoolRemoteConfig(string key, bool defaultValue = false)
        {
#if USING_FIREBASE_REMOTE_CONFIG
            return _isFirebaseInitialized ? FirebaseRemoteConfig.DefaultInstance.GetValue(key).BooleanValue : defaultValue;
#else         
            return defaultValue;
#endif
        }
        
        public int GetIntRemoteConfig(string key, int defaultValue = 0)
        {
#if USING_FIREBASE_REMOTE_CONFIG
            if (!_isFirebaseInitialized) return defaultValue;
            return (int) FirebaseRemoteConfig.DefaultInstance.GetValue(key).LongValue;
#else         
            return defaultValue;
#endif
        }

        public bool IsInitialized()
        {
            return _isFirebaseInitialized;
        }
        
        /// <summary>
        /// Check if Remote Config data has been fetched and activated
        /// </summary>
        public bool IsRemoteConfigReady()
        {
#if USING_FIREBASE_REMOTE_CONFIG
            return _isRemoteConfigReady;
#else
            return _isFirebaseInitialized; // If no Remote Config, just check Firebase init
#endif
        }
        
        private void OnApplicationQuit()
        {
            _isQuitting = true;
#if USING_FIREBASE_ANALYTICS || USING_FIREBASE_MESSAGING || USING_FIREBASE_REMOTE_CONFIG
            try { FirebaseApp.DefaultInstance?.Dispose(); } catch { /* ignore */ }
#endif
        }

        private void OnEnable()
        {
#if USING_FIREBASE_MESSAGING
            if (!_isFirebaseInitialized) return;
            Firebase.Messaging.FirebaseMessaging.TokenReceived   += OnFirebaseTokenReceived;
            Firebase.Messaging.FirebaseMessaging.MessageReceived += OnFirebaseMessageReceived;
#endif
        }

        private void OnDisable()
        {
#if USING_FIREBASE_MESSAGING
            try
            {
                Firebase.Messaging.FirebaseMessaging.TokenReceived   -= OnFirebaseTokenReceived;
                Firebase.Messaging.FirebaseMessaging.MessageReceived -= OnFirebaseMessageReceived;
            }
            catch { /* ignore */ }
#endif
        }
        
        private void OnDestroy()
        {
#if USING_FIREBASE_MESSAGING
            try
            {
                Firebase.Messaging.FirebaseMessaging.MessageReceived -= OnFirebaseMessageReceived;
                Firebase.Messaging.FirebaseMessaging.TokenReceived   -= OnFirebaseTokenReceived;
            }
            catch { /* ignore */ }
#endif
        }
        
#endif
    }
}