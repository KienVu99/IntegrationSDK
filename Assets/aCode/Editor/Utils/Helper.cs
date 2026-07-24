#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.PackageManager;
using UnityEngine.Events;

namespace aCode.Editor.Utils
{
    public static class Helper
    {
        
        public static string FirebaseDependencyUrl(string packageName)
        {
            return $"git+https://coder:0067d515873e78000325192059b08ef213ec7c6c@git.h2c.us/upm/firebase-{packageName.Replace("com.google.firebase.", "")}.git?path=/package#main";
        }
        
        public static bool HasDefine(string def)
        {
            var group   = EditorUserBuildSettings.selectedBuildTargetGroup;
            var symbols = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(group));
            return symbols.Split(';').Any(s => s.Trim() == def);
        }
        
        public static void UpdateDefine(string symbol, bool update = true)
        {
            if (update)
            {
                GM.Print("GlobalDefineUtils", $"Adding define symbol: {symbol}");
                AddDefine(symbol);
            }
            else
            {
                GM.Print("GlobalDefineUtils", $"Removing define symbol: {symbol}");
                RemoveDefine(symbol);
            }
        }
        
        public static void AddDefine(string symbol)
        {
            foreach (var targetGroup in EditorUtils.GetWorkingBuildTargetGroups())
            {
                var target = NamedBuildTarget.FromBuildTargetGroup(targetGroup);
                SDS_AddDefine(symbol, target);
            }
        }
        public static void AddDefines(string[] symbols)
        {
            foreach (var targetGroup in EditorUtils.GetWorkingBuildTargetGroups())
            {
                var target = NamedBuildTarget.FromBuildTargetGroup(targetGroup);
                SDS_AddDefines(symbols, target);
                
            }     
        }

        public static void RemoveDefine(string symbol)
        {
            foreach (var targetGroup in EditorUtils.GetWorkingBuildTargetGroups())
            {
                var target = NamedBuildTarget.FromBuildTargetGroup(targetGroup);
                SDS_RemoveDefine(symbol, target);
            }
        }
        public static void RemoveDefines(string[] symbols)
        {
            foreach (var targetGroup in EditorUtils.GetWorkingBuildTargetGroups())
            {
                var target = NamedBuildTarget.FromBuildTargetGroup(targetGroup);
                SDS_RemoveDefines(symbols, target);
            }
        }

        public static bool SDS_IsDefined(string symbol, NamedBuildTarget namedBuildTarget)
        {
            var symbolStr = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
            var symbols = new List<string>(symbolStr.Split(';'));
            return symbols.Contains(symbol);
        }
        
        private static void SDS_AddDefines(IEnumerable<string> symbols, NamedBuildTarget namedBuildTarget)
        {
            var symbolStr = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
            var currentSymbols = new List<string>(symbolStr.Split(';'));
            var added = 0;

            foreach (var symbol in symbols)
            {
                if (currentSymbols.Contains(symbol)) continue;
                currentSymbols.Add(symbol);
                added++;
            }

            if (added <= 0) return;
            var sb = new StringBuilder();

            for (var i = 0; i < currentSymbols.Count; i++)
            {
                sb.Append(currentSymbols[i]);
                if (i < currentSymbols.Count - 1) sb.Append(";");
            }

            PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, sb.ToString());
        }

        private static void SDS_AddDefine(string symbol, NamedBuildTarget namedBuildTarget)
        {
            var symbolStr = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
            var symbols = new List<string>(symbolStr.Split(';'));
            if (symbols.Contains(symbol)) return;
            symbols.Add(symbol);

            var sb = new StringBuilder();

            for (var i = 0; i < symbols.Count; i++)
            {
                sb.Append(symbols[i]);
                if (i < symbols.Count - 1) sb.Append(";");
            }
            PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, sb.ToString());
        }

        private static void SDS_RemoveDefines(string[] symbols, NamedBuildTarget namedBuildTarget)
        {
            var symbolStr = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
            var currentSymbols = new List<string>(symbolStr.Split(';'));
            var removed = 0;

            foreach (var symbol in symbols)
            {
                if (!currentSymbols.Contains(symbol)) continue;
                currentSymbols.Remove(symbol);
                removed++;
            }

            if (removed <= 0) return;
            var sb = new StringBuilder();

            for (var i = 0; i < currentSymbols.Count; i++)
            {
                sb.Append(currentSymbols[i]);
                if (i < currentSymbols.Count - 1) sb.Append(";");
            }

            PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, sb.ToString());
        }

        private static void SDS_RemoveDefine(string symbol, NamedBuildTarget namedBuildTarget)
        {
            var symbolStr = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
            var symbols = new List<string>(symbolStr.Split(';'));

            if (!symbols.Contains(symbol)) return;
            symbols.Remove(symbol);

            var settings = new StringBuilder();

            for (var i = 0; i < symbols.Count; i++)
            {
                settings.Append(symbols[i]);
                if (i < symbols.Count - 1) settings.Append(";");
            }
            PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, settings.ToString());
        }
        
        
        public static void ResolveUnityPackageManager()
        {
#if UNITY_2020_1_OR_NEWER
            Client.Resolve();
#else
            var packageManagerClientType = typeof(Client);
            var packageManagerResolveMethod = packageManagerClientType.GetMethod("Resolve", BindingFlags.NonPublic | BindingFlags.Static);

            if (packageManagerResolveMethod != null)
            {
                packageManagerResolveMethod.Invoke(null, null);
            }
#endif
        }
        
        public static async Task<string> GetPackageVersion(string packageName)
        {
            var request = Client.Search(packageName);
            while (!request.IsCompleted)
            {
                await Task.Delay(100);
            }
            return request.Status != StatusCode.Success ? "" : request.Result.First().versions.latestCompatible;
        }

        public static void OpenUnityMenu(string menuName, UnityAction callback = null)
        {
            var open = EditorApplication.ExecuteMenuItem(menuName);
            if (open) return;
            GM.Print("OpenUnityMenu",$"[aCode] ExecuteMenuItem '{menuName}' failed, trying to open directly...");
            callback?.Invoke();
        }
        
        public static string SafeProjectPath(string relative)
        {
            var p = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, relative));
            return p.Replace('\\', '/');
        }
        
        public static void DeleteDir(string path)
        {
            if (!Directory.Exists(path)) return;
            foreach (var file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
            {
                var attr = File.GetAttributes(file);
                if ((attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    File.SetAttributes(file, attr & ~FileAttributes.ReadOnly);
            }
            FileUtil.DeleteFileOrDirectory(path);
            FileUtil.DeleteFileOrDirectory(path + "~");
        }

        
    }
}
#endif