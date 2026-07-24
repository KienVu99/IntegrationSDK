#if UNITY_EDITOR
using System;
using UnityEditor;
using aCode.Editor.Utils;
using System.Threading.Tasks;
using aCode.Configs;
using UnityEngine;

namespace aCode.Editor
{
	public static class AppUpdateFirebase
	{
		private static bool _resolving;
		private static AppSetup _appSetup;
		
		[MenuItem("aCode/03. Update Firebase Package", false, 3)]
		private static async void OnLoad()
		{
			try
			{
				_appSetup = AppSetup.EnsureAsset();
				if(_appSetup == null) return;
				if(!_appSetup.ActiveFirebase) return;
				await RemoveFirebasePackage();
				System.Threading.Thread.Sleep(10000);
				await InstallFirebasePackage();
				System.Threading.Thread.Sleep(10000);
			}
			catch (Exception e)
			{
				Debug.LogError("[aCode] Install Package error : " + e);
			}
		}

		private static Task RemoveFirebasePackage()
		{
			var appManifest = UpmManifest.Load();
			appManifest.RemovePackageDependency("com.google.firebase.app");
			appManifest.RemovePackageDependency("com.google.firebase.analytics");
			appManifest.RemovePackageDependency("com.google.firebase.crashlytics");
			appManifest.RemovePackageDependency("com.google.firebase.messaging");
			appManifest.RemovePackageDependency("com.google.firebase.remote-config");
			appManifest.Save();
			Helper.ResolveUnityPackageManager();
			return Task.CompletedTask;		
		}

		private static Task InstallFirebasePackage()
		{
			var appManifest = UpmManifest.Load();
			if (_appSetup.ActiveFirebase)
			{
				appManifest.AddPackageDependency("com.google.firebase.app", Helper.FirebaseDependencyUrl("com.google.firebase.app"));
				appManifest.AddPackageDependency("com.google.firebase.analytics", Helper.FirebaseDependencyUrl("com.google.firebase.analytics"));
				appManifest.AddPackageDependency("com.google.firebase.crashlytics", Helper.FirebaseDependencyUrl("com.google.firebase.crashlytics"));
				if (_appSetup.ActiveFirebaseMessaging)
				{
					appManifest.AddPackageDependency("com.google.firebase.messaging", Helper.FirebaseDependencyUrl("com.google.firebase.messaging"));
				}
				else
				{
					appManifest.RemovePackageDependency("com.google.firebase.messaging");
				}

				if (_appSetup.ActiveFirebaseRemoteConfig)
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
			appManifest.Save();
			Helper.ResolveUnityPackageManager();
			return Task.CompletedTask;	
		}
		
	}
}
#endif