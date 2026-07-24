#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using aCode.Configs;
using aCode.Editor.Data;
using aCode.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace aCode.Editor
{
	public class AppSetupWindow : EditorWindow
	{
        private const string Tag = "aCodeInstall";
		private AppSetup _setup;
        private SerializedObject _so;

        private SerializedProperty _adNetworkProp;
        private SerializedProperty _activeFirebaseProp;
        private SerializedProperty _activeFirebaseMessagingProp;
        private SerializedProperty _activeFirebaseRemoteConfigProp;
        private SerializedProperty _activeAppsflyerProp;
        
        private const string OpenUpmRegistryName = "Google OpenUpm Unity";
        private const string OpenUpmRegistryUrl = "https://package.openupm.com";
        private static readonly List<string> OpenUpmRegistryScopes = new() {"com.google", "com.google.external-dependency-manager", "com.appsflyer"};
        private const string ApplovinRegistryName = "AppLovin MAX Unity";
        private const string ApplovinRegistryUrl = "https://unity.packages.applovin.com/";
        private static readonly List<string> ApplovinRegistryScopes = new() {"com.applovin.mediation.ads", "com.applovin.mediation.adapters", "com.applovin.mediation.dsp" };


        [MenuItem("aCode/01. Install Package", false, 1)]
        public static void Open()
        {
            var win = GetWindow<AppSetupWindow>("aCode App Setup");
            win.minSize = new Vector2(420, 260);
            win.maxSize = new Vector2(420, 260);
            win.Show();
        }

        private void OnEnable()
        {
            _setup = AppSetup.EnsureAsset();
            BindSerialized();
        }

        private void BindSerialized()
        {
            if (_setup == null) return;
            _so = new SerializedObject(_setup);
            _adNetworkProp                 = _so.FindProperty("adNetwork");
            _activeFirebaseProp            = _so.FindProperty("activeFirebase");
            _activeFirebaseMessagingProp   = _so.FindProperty("activeFirebaseMessaging");
            _activeFirebaseRemoteConfigProp= _so.FindProperty("activeFirebaseRemoteConfig");
            _activeAppsflyerProp           = _so.FindProperty("activeAppsflyer");
        }

        private void OnGUI()
        {
            if (_setup == null)
            {
                if (!GUILayout.Button("Create AppSetup.asset", GUILayout.Height(32))) return;
                _setup = AppSetup.EnsureAsset();
                BindSerialized();
                return;
            }

            using (new EditorGUILayout.VerticalScope("box"))
            {
                _so.Update();
                EditorGUILayout.LabelField("Select Ads Network", EditorStyles.boldLabel);
                EditorGUILayout.Space(2);
                EditorGUILayout.PropertyField(_adNetworkProp, new GUIContent("Ad Network"));
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Setup Firebase Tracking", EditorStyles.boldLabel);
                EditorGUILayout.Space(2);
                EditorGUILayout.PropertyField(_activeFirebaseProp, new GUIContent("Active Firebase"));
                EditorGUILayout.PropertyField(_activeFirebaseMessagingProp, new GUIContent("Firebase Messaging"));
                EditorGUILayout.PropertyField(_activeFirebaseRemoteConfigProp, new GUIContent("Firebase Remote Config"));
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Setup AppsFlyer", EditorStyles.boldLabel);
                EditorGUILayout.Space(2);
                EditorGUILayout.PropertyField(_activeAppsflyerProp, new GUIContent("Active AppsFlyer"));
                _so.ApplyModifiedProperties();
            }

            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (!GUILayout.Button("Save and Install", GUILayout.Height(40))) return;
                SaveAsset(_setup);
                RemoveDefinesConfig();
                InstallDependencyPackage(_setup);
                Close();
            }
        }

        private static void SaveAsset(AppSetup asset)
        {
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorGUIUtility.PingObject(asset);
        }

        private static void RemoveDefinesConfig()
        {
            Helper.RemoveDefine("USING_MAX_MEDIATION");
            Helper.RemoveDefine("USING_ADMOB_MEDIATION");
            
            Helper.RemoveDefine("USING_FIREBASE_ANALYTICS");
            Helper.RemoveDefine("USING_FIREBASE_MESSAGING");
            Helper.RemoveDefine("USING_FIREBASE_REMOTE_CONFIG");
            Helper.RemoveDefine("USING_APPSFLYER");
        }
        
        private static async void InstallDependencyPackage(AppSetup appSetup)
        {
            try
            {
                GM.Print(Tag, "Install Base package...");
                var appManifest = UpmManifest.Load();
                appManifest.AddOrUpdateRegistry(OpenUpmRegistryName, OpenUpmRegistryUrl, OpenUpmRegistryScopes);
                if (appSetup.AdNetwork == AdNetwork.Applovin)
                {
                    appManifest.AddOrUpdateRegistry(ApplovinRegistryName, ApplovinRegistryUrl, ApplovinRegistryScopes);
                }
                appManifest.Save();
                Helper.ResolveUnityPackageManager();
                System.Threading.Thread.Sleep(10000);
                
                foreach (var pkg in Dependencies.BasePackages)
                {
                    var version = pkg.Version;
                    if (string.IsNullOrEmpty(version))
                    {
                        version = await Helper.GetPackageVersion(pkg.Name);
                        if (string.IsNullOrEmpty(version))
                        {
                            GM.Print(Tag, $"Cannot find version for package: {pkg.Name}, skipping...");
                            continue;
                        }
                    }
                    appManifest.AddPackageDependency(pkg.Name, version);
                    GM.Print(Tag, $"Added/Updated Base dependency: {pkg.Name} to version {version}");
                }
                
                foreach (var pkg in Dependencies.ApplovinPackages)
                {
                    if (appSetup.AdNetwork == AdNetwork.Applovin)
                    {
                        var version = pkg.Version;
                        if (string.IsNullOrEmpty(version))
                        {
                            version = await Helper.GetPackageVersion(pkg.Name);
                            if (string.IsNullOrEmpty(version))
                            {
                                GM.Print(Tag, $"Cannot find version for package: {pkg.Name}, skipping...");
                                continue;
                            }
                        }
                        appManifest.AddPackageDependency(pkg.Name, version);
                        GM.Print(Tag, $"Added/Updated Applovin dependency: {pkg.Name} to version {version}");
                    }
                    else
                    {
                        appManifest.RemovePackageDependency(pkg.Name);
                        GM.Print(Tag, $"Remove Applovin dependency: {pkg.Name}");    
                    }
                }

                if (appSetup.AdNetwork == AdNetwork.Admob || appSetup.AdNetwork == AdNetwork.AdmobMediation)
                {
                    foreach (var pkg in Dependencies.AdmobPackages)
                    {
                        var version = pkg.Version;
                        if (string.IsNullOrEmpty(version))
                        {
                            version = await Helper.GetPackageVersion(pkg.Name);
                            if (string.IsNullOrEmpty(version))
                            {
                                GM.Print(Tag, $"Cannot find version for package: {pkg.Name}, skipping...");
                                continue;
                            }
                        }
                        appManifest.AddPackageDependency(pkg.Name, version);
                        GM.Print(Tag, $"Added/Updated Admob dependency: {pkg.Name} to version {version}");
                    }
                    
                    foreach (var pkg in Dependencies.AdmobMediationPackages)
                    {
                        if (appSetup.AdNetwork == AdNetwork.AdmobMediation)
                        {
                            var version = pkg.Version;
                            if (string.IsNullOrEmpty(version))
                            {
                                version = await Helper.GetPackageVersion(pkg.Name);
                                if (string.IsNullOrEmpty(version))
                                {
                                    GM.Print(Tag, $"Cannot find version for package: {pkg.Name}, skipping...");
                                    continue;
                                }
                            }
                            appManifest.AddPackageDependency(pkg.Name, version);
                            GM.Print(Tag, $"Added/Updated Admob dependency: {pkg.Name} to version {version}");
                        }
                        else
                        {
                            appManifest.RemovePackageDependency(pkg.Name);
                            GM.Print(Tag, $"Remove Admob dependency: {pkg.Name}");    
                        }
                    }
                }
                else
                {
                    foreach (var pkg in Dependencies.AdmobPackages)
                    {
                        appManifest.RemovePackageDependency(pkg.Name);
                        GM.Print(Tag, $"Remove Admob dependency: {pkg.Name}"); 
                    }
                    foreach (var pkg in Dependencies.AdmobMediationPackages)
                    {
                        appManifest.RemovePackageDependency(pkg.Name);
                        GM.Print(Tag, $"Remove Admob dependency: {pkg.Name}"); 
                    }
                }
                    
                
                if (appSetup.ActiveFirebase)
                {
                    appManifest.AddPackageDependency("com.google.firebase.app", Helper.FirebaseDependencyUrl("com.google.firebase.app"));
                    appManifest.AddPackageDependency("com.google.firebase.analytics", Helper.FirebaseDependencyUrl("com.google.firebase.analytics"));
                    appManifest.AddPackageDependency("com.google.firebase.crashlytics", Helper.FirebaseDependencyUrl("com.google.firebase.crashlytics"));
                    if (appSetup.ActiveFirebaseMessaging)
                    {
                        appManifest.AddPackageDependency("com.google.firebase.messaging", Helper.FirebaseDependencyUrl("com.google.firebase.messaging"));
                    }
                    else
                    {
                        appManifest.RemovePackageDependency("com.google.firebase.messaging");
                    }

                    if (appSetup.ActiveFirebaseRemoteConfig)
                    {
                        appManifest.AddPackageDependency("com.google.firebase.remote-config", Helper.FirebaseDependencyUrl("com.google.firebase.remote-config"));
                    }
                    else
                    {
                        appManifest.RemovePackageDependency("com.google.firebase.remote-config");
                    }
                }
                else
                {
                    appManifest.RemovePackageDependency("com.google.firebase.app");
                    appManifest.RemovePackageDependency("com.google.firebase.analytics");
                    appManifest.RemovePackageDependency("com.google.firebase.crashlytics");
                    appManifest.RemovePackageDependency("com.google.firebase.messaging");
                    appManifest.RemovePackageDependency("com.google.firebase.remote-config");
                }
                
                if (appSetup.ActiveAppsflyer)
                {
                    var version = await Helper.GetPackageVersion("com.appsflyer.unity");
                    if (string.IsNullOrEmpty(version))
                    {
                        version = "6.15.0";
                    }
                    appManifest.AddPackageDependency("com.appsflyer.unity", version);
                    GM.Print(Tag, $"Added/Updated AppsFlyer dependency: com.appsflyer.unity to version {version}");
                }
                else
                {
                    appManifest.RemovePackageDependency("com.appsflyer.unity");
                    GM.Print(Tag, "Remove AppsFlyer dependency: com.appsflyer.unity");
                }

                appManifest.Save();
                Helper.ResolveUnityPackageManager();
                System.Threading.Thread.Sleep(15000);
            }
            catch (Exception e)
            {
                GM.Print(Tag,$"[aCode] Install Max package error: {e.Message}");
            }
        }
        
	}
}
#endif