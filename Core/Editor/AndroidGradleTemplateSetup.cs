using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace IntegrationSDK.Core.Editor
{
    // ponytail: AppLovin/AppsFlyer need EDM4U to patch dependencies into
    // mainTemplate.gradle instead of copying AARs (the AAR-copy path breaks on
    // newer JDKs). Doing this by hand in every consuming project is exactly the
    // kind of one-time setup step that should just happen automatically.
    //
    // There is no public scripting API for the "Custom Main/Base/Properties
    // Gradle Template" checkboxes (PlayerSettings.Android has no such
    // properties in Unity 2021/2022) - they only exist as serialized fields on
    // ProjectSettings/ProjectSettings.asset, so this patches that file's YAML
    // directly instead.
    [InitializeOnLoad]
    public static class AndroidGradleTemplateSetup
    {
        private const string TargetDir = "Assets/Plugins/Android";
        private const string ProjectSettingsPath = "ProjectSettings/ProjectSettings.asset";

        private static readonly string[] FlagsToEnable =
        {
            "useCustomMainGradleTemplate",
            "useCustomBaseGradleTemplate",
            "useCustomGradlePropertiesTemplate"
        };

        static AndroidGradleTemplateSetup()
        {
            if (!File.Exists(ProjectSettingsPath))
            {
                return;
            }

            var text = File.ReadAllText(ProjectSettingsPath);
            var changed = false;

            foreach (var flag in FlagsToEnable)
            {
                var pattern = $@"(?m)^(\s*{flag}:\s*)0\s*$";
                var replaced = Regex.Replace(text, pattern, "${1}1");
                if (replaced != text)
                {
                    text = replaced;
                    changed = true;
                }
            }

            CopyTemplateIfMissing("mainTemplate.gradle");
            CopyTemplateIfMissing("baseProjectTemplate.gradle");
            CopyTemplateIfMissing("gradleTemplate.properties");

            if (!changed)
            {
                return;
            }

            File.WriteAllText(ProjectSettingsPath, text);
            Debug.Log("IntegrationSDK: enabled custom Android Gradle templates (required for AppLovin/AppsFlyer dependency resolution). Restart the Unity Editor for this to take effect.");
        }

        private static void CopyTemplateIfMissing(string fileName)
        {
            var targetPath = Path.Combine(TargetDir, fileName);
            if (File.Exists(targetPath))
            {
                return;
            }

            var sourcePath = Path.Combine(EditorApplication.applicationContentsPath,
                "PlaybackEngines", "AndroidPlayer", "Tools", "GradleTemplates", fileName);

            if (!File.Exists(sourcePath))
            {
                Debug.LogWarning($"IntegrationSDK: could not find Gradle template source '{sourcePath}' - copy it manually into {TargetDir}.");
                return;
            }

            Directory.CreateDirectory(TargetDir);
            File.Copy(sourcePath, targetPath);
            AssetDatabase.Refresh();
        }
    }
}
