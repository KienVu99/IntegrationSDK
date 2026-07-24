#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using aCode.Editor.Utils;

namespace aCode.Editor
{
	public class UpmManifest
	{
		private const string KeyUrl = "url";
        private const string KeyName = "name";
        private const string KeyScopes = "scopes";
        private const string KeyScopedRegistry = "scopedRegistries";
        
        private Dictionary<string, object> _manifest;
        
        private static string ManifestPath => Path.Combine(Directory.GetCurrentDirectory(), "Packages/manifest.json");

        private UpmManifest() { }
        
        public static UpmManifest Load()
        {
            return new UpmManifest { _manifest = GetManifest() };
        }
        
        public void AddOrUpdateRegistry(string name, string url, List<string> scopes)
        {
            var registry = GetRegistry(name);
            if (registry == null)
            {
                var registries = GetRegistries();
                registries?.Add(new Dictionary<string, object> {
                    {KeyName, name},
                    {KeyUrl, url},
                    {KeyScopes, scopes}
                });
                return;
            }
            UpdateRegistry(registry, scopes);
        }
        public void Save()
        {
            var content = JsonUtils.Serialize(_manifest, true);
            File.WriteAllText(ManifestPath, content);
        }
        
        public void AddPackageDependency(string packageName, string version)
        {
            var manifestDependencies = GetDependencies();
            manifestDependencies[packageName] = version;
        }
        
        public void UpdatePackageDependency(string packageName, string version, bool status = true)
        {
            if (status)
            {
                AddPackageDependency(packageName, version);
            }
            else
            {
                RemovePackageDependency(packageName);
            }
        }
        
        public void RemovePackageDependency(string packageName)
        {
            var manifestDependencies = GetDependencies();
            manifestDependencies.Remove(packageName);
        }
        
        private static Dictionary<string, object> GetManifest()
        {
            if (!File.Exists(ManifestPath))
            {
                throw new Exception("Manifest not Found!");
            }
            
            var manifestJson = File.ReadAllText(ManifestPath);
            if (string.IsNullOrEmpty(manifestJson))
            {
                throw new Exception("Manifest is empty!");
            }
            
            var deserializedManifest = JsonUtils.Deserialize(manifestJson) as Dictionary<string, object>;
            return deserializedManifest ?? throw new Exception("Failed to deserialize manifest");
        }
        
        private Dictionary<string, object> GetDependencies()
        {
            var dependencies = _manifest["dependencies"] as Dictionary<string, object>;
            return dependencies ?? throw new Exception("No dependencies found in manifest.");
        }
        
        private List<object> GetRegistries()
        {
            EnsureScopedRegistryExists();
            return _manifest[KeyScopedRegistry] as List<object>;
        }
        
        private Dictionary<string, object> GetRegistry(string name)
        {
            var registries = GetRegistries();
            return registries?.OfType<Dictionary<string, object>>().FirstOrDefault(registry => GetStringFromDictionary(registry, KeyName).Equals(name));
        }
        
        private void EnsureScopedRegistryExists()
        {
            if (_manifest.ContainsKey(KeyScopedRegistry)) return;
            _manifest.Add(KeyScopedRegistry, new List<object>());
        }
        
        private static void UpdateRegistry(Dictionary<string, object> registry, List<string> newScopes)
        {
            var scopes = GetListFromDictionary(registry, KeyScopes);
            if (scopes == null)
            {
                registry[KeyScopes] = new List<string>(newScopes);
                return;
            }
            var uniqueNewScopes = newScopes.Where(scope => !scopes.Contains(scope)).ToList();
            scopes.AddRange(uniqueNewScopes);
        }

        private static string GetStringFromDictionary(IDictionary<string, object> dictionary, string key, string defaultValue = "")
        {
            if (dictionary == null) return defaultValue;
            if (dictionary.TryGetValue(key, out var value) && value != null)
            {
                return value.ToString();
            }
            
            return defaultValue;
        }

        private static List<object> GetListFromDictionary(IDictionary<string, object> dictionary, string key, List<object> defaultValue = null)
        {
            if (dictionary == null) return defaultValue;

            if (dictionary.TryGetValue(key, out var value) && value is List<object> list)
            {
                return list;
            }
            
            return defaultValue;
        }
        
	}
}
#endif