using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace IntegrationSDK.Core.Editor
{
    // ponytail: manifest.json has no supported C# API for adding scoped
    // registries, and git-URL package entries are easiest to add the same
    // way - so this patches the file's JSON text directly rather than
    // building a full JSON object model just for this one screen.
    public class IntegrationSDKPackageSetupWindow : EditorWindow
    {
        private const string RepoUrl = "https://github.com/KienVu99/IntegrationSDK.git";
        private const string ManifestPath = "Packages/manifest.json";

        // Manifest.json dependency key prefixes for other ad/analytics SDKs that
        // would conflict with (or duplicate) what this SDK installs.
        private static readonly (string label, string keyPrefix)[] KnownManifestPackages =
        {
            ("Firebase (any module)", "com.google.firebase"),
            ("Google Mobile Ads (AdMob)", "com.google.ads.mobile"),
            ("Unity Ads", "com.unity.ads"),
            ("Facebook SDK", "com.facebook.unity"),
        };

        // Known top-level Assets/ folders these SDKs drop in when imported as
        // raw .unitypackage assets instead of UPM packages.
        private static readonly string[] KnownAssetFolders =
        {
            "Firebase", "GoogleMobileAds", "IronSource", "FacebookSDK", "Facebook",
        };

        private bool _includeAds = true;
        private bool _includeAppsFlyer = true;

        private List<string> _foundManifestKeys = new List<string>();
        private List<string> _foundAssetFolders = new List<string>();
        private readonly HashSet<string> _selectedForRemoval = new HashSet<string>();

        [MenuItem("Tools/Integration SDK/Package Setup")]
        public static void Open() => GetWindow<IntegrationSDKPackageSetupWindow>("Integration SDK Packages");

        private void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "Choose which Integration SDK modules to add, then click Save. " +
                "This edits Packages/manifest.json and lets Unity resolve the packages.",
                MessageType.Info);

            EditorGUILayout.LabelField("Core", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Always included (required by every module).");

            EditorGUILayout.Space();
            _includeAds = EditorGUILayout.ToggleLeft("Ads.AppLovin (AppLovin MAX ads)", _includeAds);
            _includeAppsFlyer = EditorGUILayout.ToggleLeft("Tracking.AppsFlyer (AppsFlyer analytics)", _includeAppsFlyer);

            EditorGUILayout.Space();
            if (GUILayout.Button("Save"))
            {
                ApplyToManifest();
            }

            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("Remove existing ad/analytics SDKs", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Scan for other ad/Firebase SDKs already in this project before adding a new one, " +
                "to avoid duplicate/conflicting native dependencies. Commit or back up the project " +
                "first - removed Assets folders go to the OS Recycle Bin, but manifest.json edits are not undoable from here.",
                MessageType.Warning);

            if (GUILayout.Button("Scan for existing ad/analytics SDKs"))
            {
                Scan();
            }

            if (_foundManifestKeys.Count > 0 || _foundAssetFolders.Count > 0)
            {
                EditorGUILayout.LabelField("manifest.json packages:");
                foreach (var key in _foundManifestKeys)
                {
                    DrawRemovalToggle(key);
                }

                EditorGUILayout.LabelField("Assets/ folders:");
                foreach (var folder in _foundAssetFolders)
                {
                    DrawRemovalToggle($"Assets/{folder}");
                }

                EditorGUILayout.Space();
                using (new EditorGUI.DisabledScope(_selectedForRemoval.Count == 0))
                {
                    if (GUILayout.Button($"Remove Selected ({_selectedForRemoval.Count})"))
                    {
                        RemoveSelected();
                    }
                }
            }
            else if (_hasScanned)
            {
                EditorGUILayout.LabelField("No known ad/Firebase SDKs found.");
            }
        }

        private bool _hasScanned;

        private void DrawRemovalToggle(string id)
        {
            var isSelected = _selectedForRemoval.Contains(id);
            var toggled = EditorGUILayout.ToggleLeft(id, isSelected);
            if (toggled == isSelected)
            {
                return;
            }

            if (toggled) _selectedForRemoval.Add(id);
            else _selectedForRemoval.Remove(id);
        }

        private void Scan()
        {
            _hasScanned = true;
            _selectedForRemoval.Clear();
            _foundManifestKeys = new List<string>();
            _foundAssetFolders = new List<string>();

            if (File.Exists(ManifestPath))
            {
                var text = File.ReadAllText(ManifestPath);
                foreach (var (_, keyPrefix) in KnownManifestPackages)
                {
                    foreach (Match match in Regex.Matches(text, "\"(" + Regex.Escape(keyPrefix) + "[a-zA-Z0-9._-]*)\"\\s*:"))
                    {
                        var key = match.Groups[1].Value;
                        if (!_foundManifestKeys.Contains(key))
                        {
                            _foundManifestKeys.Add(key);
                        }
                    }
                }
            }

            foreach (var folder in KnownAssetFolders)
            {
                if (Directory.Exists($"Assets/{folder}"))
                {
                    _foundAssetFolders.Add(folder);
                }
            }
        }

        private void RemoveSelected()
        {
            if (!EditorUtility.DisplayDialog(
                    "Remove selected SDKs?",
                    $"This will remove {_selectedForRemoval.Count} item(s):\n\n" +
                    string.Join("\n", _selectedForRemoval) +
                    "\n\nAssets/ folders go to the OS Recycle Bin. manifest.json changes are written immediately. Continue?",
                    "Remove", "Cancel"))
            {
                return;
            }

            var manifestKeysToRemove = _selectedForRemoval.Where(s => !s.StartsWith("Assets/")).ToList();
            var assetFoldersToRemove = _selectedForRemoval.Where(s => s.StartsWith("Assets/")).ToList();

            if (manifestKeysToRemove.Count > 0 && File.Exists(ManifestPath))
            {
                var text = File.ReadAllText(ManifestPath);
                foreach (var key in manifestKeysToRemove)
                {
                    text = Regex.Replace(text, $"\\s*\"{Regex.Escape(key)}\"\\s*:\\s*\"[^\"]*\",?\\n", "\n");
                }
                File.WriteAllText(ManifestPath, text);
            }

            foreach (var path in assetFoldersToRemove)
            {
                if (AssetDatabase.IsValidFolder(path))
                {
                    AssetDatabase.MoveAssetToTrash(path);
                }
            }

            Debug.Log($"IntegrationSDK: removed {_selectedForRemoval.Count} item(s). Resolving packages...");
            _selectedForRemoval.Clear();
            Scan();

            Client.Resolve();
            AssetDatabase.Refresh();
        }

        private void ApplyToManifest()
        {
            if (!File.Exists(ManifestPath))
            {
                Debug.LogError($"IntegrationSDK: {ManifestPath} not found.");
                return;
            }

            var text = File.ReadAllText(ManifestPath);

            var dependencies = new List<(string key, string value)>
            {
                ("com.gemmob.integrationsdk.core", $"{RepoUrl}?path=/Core#main")
            };
            var registries = new List<(string name, string url, string[] scopes)>();

            if (_includeAds)
            {
                dependencies.Add(("com.gemmob.integrationsdk.ads.applovin", $"{RepoUrl}?path=/Ads.AppLovin#main"));
                dependencies.Add(("com.applovin.mediation.ads", "8.6.4"));
                dependencies.Add(("com.google.external-dependency-manager", "1.2.186"));
                registries.Add(("AppLovin MAX Unity", "https://unity.packages.applovin.com/",
                    new[] { "com.applovin.mediation.ads", "com.applovin.mediation.adapters", "com.applovin.mediation.dsp" }));
                registries.Add(("package.openupm.com", "https://package.openupm.com",
                    new[] { "com.google.external-dependency-manager" }));
            }

            if (_includeAppsFlyer)
            {
                dependencies.Add(("com.gemmob.integrationsdk.tracking.appsflyer", $"{RepoUrl}?path=/Tracking.AppsFlyer#main"));
                dependencies.Add(("appsflyer-unity-plugin", "https://github.com/AppsFlyerSDK/appsflyer-unity-plugin.git#upm"));
                dependencies.Add(("com.google.external-dependency-manager", "1.2.186"));
            }

            text = AddMissingRegistries(text, registries);
            text = AddMissingDependencies(text, dependencies);

            File.WriteAllText(ManifestPath, text);
            Debug.Log("IntegrationSDK: updated Packages/manifest.json. Resolving packages...");

            Client.Resolve();
            AssetDatabase.Refresh();
        }

        private static string AddMissingRegistries(string manifestText, List<(string name, string url, string[] scopes)> registries)
        {
            var toAdd = registries.FindAll(r => !manifestText.Contains($"\"{r.url}\""));
            if (toAdd.Count == 0)
            {
                return manifestText;
            }

            var entries = "";
            foreach (var (name, url, scopes) in toAdd)
            {
                var scopesJson = string.Join(", ", scopes.Select(s => $"\"{s}\""));
                entries += $"    {{\n      \"name\": \"{name}\",\n      \"url\": \"{url}\",\n      \"scopes\": [{scopesJson}]\n    }},\n";
            }

            var scopedRegistriesMatch = Regex.Match(manifestText, "\"scopedRegistries\"\\s*:\\s*\\[");
            if (scopedRegistriesMatch.Success)
            {
                var insertAt = scopedRegistriesMatch.Index + scopedRegistriesMatch.Length;
                return manifestText.Insert(insertAt, "\n" + entries.TrimEnd('\n', ','));
            }

            return manifestText.TrimStart().Insert(1, $"\n  \"scopedRegistries\": [\n{entries.TrimEnd('\n', ',')}\n  ],");
        }

        private static string AddMissingDependencies(string manifestText, List<(string key, string value)> dependencies)
        {
            var seen = new HashSet<string>();
            var entries = "";
            foreach (var (key, value) in dependencies)
            {
                if (!seen.Add(key) || manifestText.Contains($"\"{key}\""))
                {
                    continue;
                }

                entries += $"    \"{key}\": \"{value}\",\n";
            }

            if (entries.Length == 0)
            {
                return manifestText;
            }

            var dependenciesMatch = Regex.Match(manifestText, "\"dependencies\"\\s*:\\s*\\{");
            if (!dependenciesMatch.Success)
            {
                Debug.LogError("IntegrationSDK: could not find a \"dependencies\" object in manifest.json.");
                return manifestText;
            }

            var insertAt = dependenciesMatch.Index + dependenciesMatch.Length;
            return manifestText.Insert(insertAt, "\n" + entries);
        }
    }
}
