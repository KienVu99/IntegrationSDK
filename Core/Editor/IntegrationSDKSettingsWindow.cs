using UnityEditor;
using UnityEngine;

namespace IntegrationSDK.Core.Editor
{
    public class IntegrationSDKSettingsWindow : EditorWindow
    {
        private const string ConfigPathPrefKey = "IntegrationSDK.ConfigAssetPath";

        private IntegrationSDKConfig _config;
        private SerializedObject _serializedConfig;

        [MenuItem("Tools/Integration SDK/Settings")]
        public static void Open()
        {
            GetWindow<IntegrationSDKSettingsWindow>("Integration SDK").LoadOrCreateConfig();
        }

        private void OnEnable() => LoadOrCreateConfig();

        private void LoadOrCreateConfig()
        {
            var savedPath = EditorPrefs.GetString(ConfigPathPrefKey, string.Empty);
            _config = !string.IsNullOrEmpty(savedPath)
                ? AssetDatabase.LoadAssetAtPath<IntegrationSDKConfig>(savedPath)
                : null;

            if (_config == null)
            {
                var chosenPath = EditorUtility.SaveFilePanelInProject(
                    "Create Integration SDK Config",
                    "IntegrationSDKConfig",
                    "asset",
                    "Choose where to save the Integration SDK config asset.");

                if (string.IsNullOrEmpty(chosenPath))
                {
                    return;
                }

                _config = CreateInstance<IntegrationSDKConfig>();
                AssetDatabase.CreateAsset(_config, chosenPath);
                AssetDatabase.SaveAssets();
                EditorPrefs.SetString(ConfigPathPrefKey, chosenPath);
            }

            _serializedConfig = new SerializedObject(_config);
        }

        private void OnGUI()
        {
            if (_config == null || _serializedConfig == null)
            {
                EditorGUILayout.HelpBox("No config loaded.", MessageType.Warning);
                return;
            }

            _serializedConfig.Update();

            var iterator = _serializedConfig.GetIterator();
            iterator.NextVisible(true);
            while (iterator.NextVisible(false))
            {
                EditorGUILayout.PropertyField(iterator, true);
            }

            _serializedConfig.ApplyModifiedProperties();

            var errors = IntegrationSDKConfigValidator.Validate(_config);
            foreach (var error in errors)
            {
                EditorGUILayout.HelpBox(error, MessageType.Warning);
            }

            if (GUILayout.Button("Save"))
            {
                EditorUtility.SetDirty(_config);
                AssetDatabase.SaveAssets();
                PreloadedAssetsHelper.RegisterInPlayerSettings(_config);
            }
        }
    }
}
