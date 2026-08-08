#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using aCode.Configs;
using UnityEditor;
using UnityEngine;

namespace aCode.Editor.Utils
{
	internal static class  UpdateSettingData
	{

        public static void UpdateAdmobConfig()
        {
            var admobAndroid = AppConfig.AdmobAppID ?? string.Empty;
            var admobIos     = AppConfig.AdmobIosAppID ?? string.Empty;
            
            var type = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return Array.Empty<Type>(); } })
                .FirstOrDefault(t => t.Name == "GoogleMobileAdsSettings");

            if (type == null)
            {
                Debug.LogWarning("[aCode] GoogleMobileAdsSettings type not found.");
                return;
            }
            
            UnityEngine.Object settingsObj = null;
            var instProp = type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            if (instProp != null) settingsObj = instProp.GetValue(null, null) as UnityEngine.Object;

            if (settingsObj == null)
            {
                var guids = AssetDatabase.FindAssets("t:ScriptableObject GoogleMobileAdsSettings");
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                    if (obj == null || obj.GetType().Name != "GoogleMobileAdsSettings") continue;
                    settingsObj = obj;
                    break;
                }
            }

            if (settingsObj == null)
            {
                Debug.LogWarning("[aCode] GoogleMobileAdsSettings asset not found.");
                return;
            }
            
            var so = new SerializedObject(settingsObj);
            var anySet = false;
            
            anySet |= TrySetSo(so, new[] { "GoogleMobileAdsAndroidAppId", "googleMobileAdsAndroidAppId" }, admobAndroid);
            anySet |= TrySetSo(so, new[] { "GoogleMobileAdsIOSAppId", "googleMobileAdsIOSAppId" }, admobIos);

            if (anySet)
            {
                so.ApplyModifiedProperties();
            }
            else
            {
                TrySetProp(type, settingsObj, "GoogleMobileAdsAndroidAppId", admobAndroid);
                TrySetProp(type, settingsObj, "GoogleMobileAdsIOSAppId", admobIos);

                TrySetField(type, settingsObj, "googleMobileAdsAndroidAppId", admobAndroid);
                TrySetField(type, settingsObj, "googleMobileAdsIOSAppId", admobIos);
            }

            EditorUtility.SetDirty(settingsObj);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[aCode] GoogleMobileAdsSettings updated.");
        }
        
		public static void UpdateApplovinConfig()
        {
            var sdkKey = AppConfig.ApplovinSdkKey ?? string.Empty;
            var admobAndroid = AppConfig.AdmobAppID ?? string.Empty;
            var admobIos     = AppConfig.AdmobIosAppID ?? string.Empty;

            var type = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return Array.Empty<Type>(); } })
                .FirstOrDefault(t => t.Name == "AppLovinSettings");

            if (type == null)
            {
                Debug.LogWarning("[aCode] ApplovinSettings type not found.");
                return;
            }
            
            UnityEngine.Object settingsObj = null;
            var instProp = type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            if (instProp != null) settingsObj = instProp.GetValue(null, null) as UnityEngine.Object;

            if (settingsObj == null)
            {
                var guids = AssetDatabase.FindAssets("t:ScriptableObject AppLovinSettings");
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                    if (obj == null || obj.GetType().Name != "AppLovinSettings") continue;
                    settingsObj = obj;
                    break;
                }
            }

            if (settingsObj == null)
            {
                Debug.LogWarning("[aCode] ApplovinSettings asset not found.");
                return;
            }
            
            var so = new SerializedObject(settingsObj);
            var anySet = false;

            anySet |= TrySetSo(so, new[] { "SdkKey", "sdkKey" }, sdkKey);
            anySet |= TrySetSo(so, new[] { "AdMobAndroidAppId", "adMobAndroidAppId", "admobAndroidAppId" }, admobAndroid);
            anySet |= TrySetSo(so, new[] { "AdMobIosAppId", "adMobIosAppId", "admobIosAppId" }, admobIos);

            if (anySet)
            {
                so.ApplyModifiedProperties();
            }
            else
            {
                TrySetProp(type, settingsObj, "SdkKey", sdkKey);
                TrySetProp(type, settingsObj, "AdMobAndroidAppId", admobAndroid);
                TrySetProp(type, settingsObj, "AdMobIosAppId", admobIos);

                TrySetField(type, settingsObj, "sdkKey", sdkKey);
                TrySetField(type, settingsObj, "adMobAndroidAppId", admobAndroid);
                TrySetField(type, settingsObj, "adMobIosAppId", admobIos);
            }

            EditorUtility.SetDirty(settingsObj);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[aCode] AppLovinSettings updated.");
            UpdateApplovinConsentFlowConfig();
        }

        public static void UpdateApplovinConsentFlowConfig()
        {
            try
            {
                var dir = Application.dataPath + "/MaxSdk/Resources";
                if (!System.IO.Directory.Exists(dir))
                {
                    System.IO.Directory.CreateDirectory(dir);
                }
                var jsonPath = System.IO.Path.Combine(dir, "AppLovinInternalSettings.json");
                var consentSettings = new AppLovinConsentSettings
                {
                    consentFlowEnabled = AppConfig.ConsentFlowEnabled,
                    privacyPolicyUrl = AppConfig.PrivacyPolicyUrl ?? string.Empty,
                    termsOfServiceUrl = AppConfig.TermsOfServiceUrl ?? string.Empty
                };
                var json = JsonUtility.ToJson(consentSettings, true);
                System.IO.File.WriteAllText(jsonPath, json);
                AssetDatabase.Refresh();
                Debug.Log("[aCode] AppLovinInternalSettings.json updated.");
            }
            catch (Exception e)
            {
                Debug.LogError("[aCode] Failed to update AppLovinInternalSettings.json: " + e.Message);
            }
        }

        [Serializable]
        private class AppLovinConsentSettings
        {
            public bool consentFlowEnabled;
            public string privacyPolicyUrl;
            public string termsOfServiceUrl;
        }
        
        private static bool TrySetSo(SerializedObject so, string[] names, string value)
        {
            foreach (var n in names)
            {
                var p = so.FindProperty(n);
                if (p is not { propertyType: SerializedPropertyType.String }) continue;
                p.stringValue = value ?? string.Empty;
                return true;
            }
            return false;
        }

        private static void TrySetProp(Type t, object target, string name, string val)
        {
            var p = t.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (p != null && p.CanWrite && p.PropertyType == typeof(string)) p.SetValue(target, val ?? string.Empty);
        }

        private static void TrySetField(Type t, object target, string name, string val)
        {
            var f = t.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (f != null && f.FieldType == typeof(string)) f.SetValue(target, val ?? string.Empty);
        }
	}
}
#endif