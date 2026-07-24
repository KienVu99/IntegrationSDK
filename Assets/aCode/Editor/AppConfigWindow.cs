#if UNITY_EDITOR
using System;
using aCode.Configs;
using System.IO;
using aCode.Editor.Data;
using aCode.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

namespace aCode.Editor
{
    public class AppConfigWindow : EditorWindow
    {
        private const string Title = "Game Config";

        private AppSettings _settings;
        private SerializedObject _so;
        private Vector2 _scroll;
        private ReorderableList _rcList;
        
        [MenuItem("aCode/02. Game Config", false, 2)]
        public static void Open()
        {
            var settings = AppSettings.EnsureAsset();
            var win = GetWindow<AppConfigWindow>(Title);
            win._Init(settings);
            win.minSize = new Vector2(640, 500);
            win.Show();
        }

        private void _Init(AppSettings settings)
        {
            _settings = settings;
            _so = new SerializedObject(_settings);
            BuildRemoteList();
        }

        private void OnEnable()
        {
            if (_settings == null)
                _settings = AppSettings.EnsureAsset();
            _so = new SerializedObject(_settings);
            BuildRemoteList();
        }

        private void BuildRemoteList()
        {
            var rcProp = _so.FindProperty("remoteConfigs");
            _rcList = new ReorderableList(_so, rcProp, true, true, true, true)
            {
                drawHeaderCallback = rect =>
                {
                    var r1 = new Rect(rect.x + 6, rect.y, rect.width * 0.42f, rect.height);
                    var r2 = new Rect(rect.x + rect.width * 0.45f, rect.y, rect.width * 0.18f, rect.height);
                    var r3 = new Rect(rect.x + rect.width * 0.65f, rect.y, rect.width * 0.33f - 8, rect.height);
                    EditorGUI.LabelField(r1, "Key", EditorStyles.miniBoldLabel);
                    EditorGUI.LabelField(r2, "Type", EditorStyles.miniBoldLabel);
                    EditorGUI.LabelField(r3, "Default Value", EditorStyles.miniBoldLabel);
                }
            };

            _rcList.drawElementCallback = (rect, index, active, focused) =>
            {
                var listProp = _rcList.serializedProperty;
                var el = listProp.GetArrayElementAtIndex(index);

                var keyProp   = el.FindPropertyRelative("key");
                var typeProp  = el.FindPropertyRelative("type");
                var valueProp = el.FindPropertyRelative("defaultValue");

                const float pad = 2f;
                var lineH = EditorGUIUtility.singleLineHeight;
                rect.height = lineH;

                var rKey  = new Rect(rect.x + 6, rect.y + pad, rect.width * 0.42f - 8, lineH);
                var rType = new Rect(rect.x + rect.width * 0.45f, rect.y + pad, rect.width * 0.18f - 8, lineH);
                var rVal  = new Rect(rect.x + rect.width * 0.65f, rect.y + pad, rect.width * 0.33f - 10, lineH);

                EditorGUI.PropertyField(rKey,  keyProp,  GUIContent.none);
                EditorGUI.PropertyField(rType, typeProp, GUIContent.none);
                EditorGUI.PropertyField(rVal,  valueProp, GUIContent.none);
                
                var type = (RemoteConfigType)typeProp.enumValueIndex;
                var warn = false;
                var msg = "";

                switch (type)
                {
                    case RemoteConfigType.Number:
                    {
                        var s = valueProp.stringValue;
                        warn = !string.IsNullOrEmpty(s) && !double.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out _);
                        if (warn) msg = "↳ Expect a number (e.g. 3.14)";
                        break;
                    }
                    case RemoteConfigType.Bool:
                    {
                        var s = valueProp.stringValue?.Trim().ToLowerInvariant();
                        warn = s is not ("true" or "false");
                        if (warn) msg = "↳ Expect true/false";
                        break;
                    }
                    case RemoteConfigType.String:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (!warn) return;
                var warnRect = new Rect(rVal.x, rVal.y + lineH + 2, rVal.width, lineH);
                EditorGUI.LabelField(warnRect, msg, EditorStyles.miniLabel);
            };

            _rcList.elementHeightCallback = index =>
            {
                var listProp = _rcList.serializedProperty;
                var el = listProp.GetArrayElementAtIndex(index);
                var typeProp = el.FindPropertyRelative("type");
                var valueProp = el.FindPropertyRelative("defaultValue");

                var baseH = EditorGUIUtility.singleLineHeight + 6f;
                var type = (RemoteConfigType)typeProp.enumValueIndex;

                switch (type)
                {
                    case RemoteConfigType.Number:
                    {
                        var s = valueProp.stringValue;
                        var bad = !string.IsNullOrEmpty(s) && !double.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out _);
                        return bad ? baseH + EditorGUIUtility.singleLineHeight : baseH;
                    }
                    case RemoteConfigType.Bool:
                    {
                        var s = valueProp.stringValue?.Trim().ToLowerInvariant();
                        var bad = s is not ("true" or "false");
                        return bad ? baseH + EditorGUIUtility.singleLineHeight : baseH;
                    }
                    case RemoteConfigType.String:
                    default:
                        return baseH;
                }
            };

            _rcList.onAddCallback = list =>
            {
                var i = list.serializedProperty.arraySize;
                list.serializedProperty.InsertArrayElementAtIndex(i);
                var el = list.serializedProperty.GetArrayElementAtIndex(i);
                el.FindPropertyRelative("key").stringValue = "new_key";
                el.FindPropertyRelative("type").enumValueIndex = (int)RemoteConfigType.String;
                el.FindPropertyRelative("defaultValue").stringValue = "";
            };

            _rcList.onRemoveCallback = list =>
            {
                if (EditorUtility.DisplayDialog("Remove Remote Config", "Remove this remote config?", "Remove", "Cancel"))
                {
                    ReorderableList.defaultBehaviours.DoRemoveButton(list);
                }
            };
        }

        private void OnGUI()
        {
            if (_settings == null)
            {
                EditorGUILayout.HelpBox("Settings asset is missing. Click 'Create/Locate Settings' to generate it.", MessageType.Warning);
                if (!GUILayout.Button("Create/Locate Settings")) return;
                _settings = AppSettings.EnsureAsset();
                _so = new SerializedObject(_settings);
                BuildRemoteList();
                return;
            }

            _so.UpdateIfRequiredOrScript();
            EditorGUI.BeginChangeCheck();

            using (new EditorGUILayout.VerticalScope())
            {
                _scroll = EditorGUILayout.BeginScrollView(_scroll);
                EditorGUILayout.Space(4);
                DrawBlock("Debug & Ads Switches", () =>
                {
                    PropertyField("isDebugMode", "Is Debug Mode");
                    PropertyField("turnOffAds", "Turn Off Ads");
                });

                EditorGUILayout.Space(4);
                DrawBlock("App Config", () =>
                {
                    PropertyField("applovinSdkKey", "Applovin SDK Key");
                    PropertyField("directedForChildren", "Directed For Children");
                    PropertyField("intervalShowAds", "Interval Show Ads");
                    PropertyField("autoShowBanner", "Auto Show Banner");
                    PropertyField("bannerPosition", "Banner Position");
                });
                
                DrawBlock("Android Config", () =>
                {
                    PropertyField("admobAppID", "Admob App ID");
                    PropertyField("appOpenID", "App Open ID");
                    PropertyField("bannerID", "Banner ID");
                    PropertyField("bannerCollapsibleID", "Banner Collapsible");
                    PropertyField("interstitialID", "Interstitial ID");
                    PropertyField("rewardedVideoID", "Rewarded Video ID");
                });
                DrawBlock("IOS Config", () =>
                {
                    PropertyField("admobIosAppID", "Admob IOS App ID");
                    PropertyField("appOpenIosID", "App Open ID IOS");
                    PropertyField("bannerIosID", "Banner ID IOS");
                    PropertyField("bannerCollapsibleIosID", "Banner Collapsible IOS");
                    PropertyField("interstitialIosID", "Interstitial ID IOS");
                    PropertyField("rewardedVideoIosID", "Rewarded Video ID IOS");
                });

                EditorGUILayout.Space(4);
                DrawBlock("Appsflyer Config", () =>
                {
                    PropertyField("appsflyerDevKey", "Appsflyer Dev Key");
                    PropertyField("appsflyerAppID", "Appsflyer App ID (iOS)");
                });

                EditorGUILayout.Space(4);
                DrawBlock("Misc", () =>
                {
                    PropertyField("testDeviceID", "Test Device ID");
                    PropertyField("appleStoreUrl", "Apple Store Url");
                });

                EditorGUILayout.Space(8);
                DrawBlock("Firebase Remote Config", () =>
                {
                    _rcList.DoLayoutList();
                });

                EditorGUILayout.EndScrollView();

                EditorGUILayout.Space(10);
                DrawFooterButtons();
            }

            if (!EditorGUI.EndChangeCheck()) return;
            Undo.RecordObject(_settings, "Edit Game Config");
            if (!EditorGUIUtility.editingTextField) GUI.FocusControl(null);
            _so.ApplyModifiedProperties();
            EditorUtility.SetDirty(_settings);
        }

        private static void DrawBlock(string label, Action body)
        {
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField(label, EditorStyles.miniBoldLabel);
                EditorGUI.indentLevel++;
                body?.Invoke();
                EditorGUI.indentLevel--;
            }
        }
        
        private void DrawFooterButtons()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (!GUILayout.Button("Save & Close", GUILayout.Height(44), GUILayout.ExpandWidth(true))) return;
                Undo.RecordObject(_settings, "Save Game Config");
                GUI.FocusControl(null);
                _so.ApplyModifiedProperties();
                EditorUtility.SetDirty(_settings);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                UpdatePackageDefine();
                UpdateProguardData();
                Close();
                Debug.unityLogger.Log("GUI", "Saved Game Config");
            }
        }

        private void PropertyField(string fieldName, string label)
        {
            var p = _so.FindProperty(fieldName);
            if (p == null)
            {
                EditorGUILayout.HelpBox($"Missing serialized field '{fieldName}' on AppSettings.", MessageType.Error);
                return;
            }
            EditorGUILayout.PropertyField(p, new GUIContent(label));
        }
        
        private static void UpdatePackageDefine()
        {
            var appSetup = AppSetup.EnsureAsset();
            if(appSetup == null) return;
            switch (appSetup.AdNetwork)
            {
                case AdNetwork.Applovin:
                    UpdateSettingData.UpdateApplovinConfig();
                    Helper.AddDefine("USING_MAX_MEDIATION");
                    Helper.RemoveDefine("USING_ADMOB_MEDIATION");
                    break;
                case AdNetwork.Admob:
                    UpdateSettingData.UpdateAdmobConfig();
                    Helper.RemoveDefine("USING_MAX_MEDIATION");
                    Helper.AddDefine("USING_ADMOB_MEDIATION");
                    break;
                case AdNetwork.None:
                default:
                    Helper.RemoveDefine("USING_MAX_MEDIATION");
                    Helper.RemoveDefine("USING_ADMOB_MEDIATION");
                    break;
            }

            if (appSetup.ActiveFirebase)
            {
                Helper.AddDefine("USING_FIREBASE_ANALYTICS");
                if (appSetup.ActiveFirebaseMessaging)
                {
                    Helper.AddDefine("USING_FIREBASE_MESSAGING");
                }
                else
                {
                    Helper.RemoveDefine("USING_FIREBASE_MESSAGING");
                }
                if (appSetup.ActiveFirebaseRemoteConfig)
                {
                    Helper.AddDefine("USING_FIREBASE_REMOTE_CONFIG");
                }
                else
                {
                    Helper.RemoveDefine("USING_FIREBASE_REMOTE_CONFIG");
                }
            }
            else
            {
                Helper.RemoveDefine("USING_FIREBASE_ANALYTICS");
                Helper.RemoveDefine("USING_FIREBASE_MESSAGING");
                Helper.RemoveDefine("USING_FIREBASE_REMOTE_CONFIG");
            }

            if (appSetup.ActiveAppsflyer)
            {
                Helper.AddDefine("USING_APPSFLYER");
            }
            else
            {
                Helper.RemoveDefine("USING_APPSFLYER");
            }
        }
        
        private static void UpdateProguardData()
        {
            const string proguardFilePath = "Assets/Plugins/Android/proguard-user.txt";
            var proguardFile = Application.dataPath + "/Plugins/Android/proguard-user.txt";
            var proguardFileDisabled = Application.dataPath + "/Plugins/Android/proguard-user.txt.DISABLED";
            if (File.Exists(proguardFile) || File.Exists(proguardFileDisabled)) return;
            var dir = Path.GetDirectoryName(proguardFilePath);
            if (!Directory.Exists(dir))
            {
                if (dir != null) Directory.CreateDirectory(dir);
            }
            File.WriteAllText(proguardFile, ProguardData.Content);
            Debug.unityLogger.Log("Proguard", "Content copied to file: " + proguardFile);
        }
    }
}
#endif
