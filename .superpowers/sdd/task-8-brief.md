### Task 8: Settings window (menu bar UI)

**Files:**
- Create: `Core/Editor/IntegrationSDKSettingsWindow.cs`

**Interfaces:**
- Consumes: `IntegrationSDKConfig` (Task 2),
  `PreloadedAssetsHelper.RegisterInPlayerSettings` (Task 6).
- Produces: menu item `Tools > Integration SDK > Settings` opening an
  `EditorWindow` that creates/loads the config asset and saves + registers it.

This task has no automated test — Unity `EditorWindow` GUI code is not
meaningfully unit-testable, and the two pieces of logic it depends on
(`PreloadedAssetsHelper`, `IntegrationSDKConfigValidator`) are already covered
by Tasks 6 and 7. Verification is manual, listed in Step 3.

- [ ] **Step 1: Write the settings window**

Create `Core/Editor/IntegrationSDKSettingsWindow.cs`:

```csharp
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
```

- [ ] **Step 2: Manually verify in the Editor**

1. Open Unity Editor on this project.
2. `Tools > Integration SDK > Settings` — confirm a save-file dialog appears the
   first time, and the asset gets created at the chosen path.
3. Fill in `maxSdkKey` and all Ad Unit ID fields, click **Save**.
4. Confirm no warnings remain in the window.
5. Open `Edit > Project Settings > Player > Preloaded Assets` and confirm the
   config asset is listed exactly once.
6. Close and reopen the window — confirm it loads the same asset (no duplicate
   save dialog).

- [ ] **Step 3: Commit**

```bash
git add Core/Editor/IntegrationSDKSettingsWindow.cs
git commit -m "Add Integration SDK Settings window under Tools menu"
```

---

