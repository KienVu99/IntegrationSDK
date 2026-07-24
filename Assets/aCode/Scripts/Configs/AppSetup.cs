using UnityEngine;

namespace aCode.Configs
{
	public enum AdNetwork {None, Admob, AdmobMediation, Applovin}

	public class AppSetup : ScriptableObject
	{
		[SerializeField] private AdNetwork adNetwork = AdNetwork.None;
		[SerializeField] private bool activeFirebase;
		[SerializeField] private bool activeFirebaseMessaging;
		[SerializeField] private bool activeFirebaseRemoteConfig;
		[SerializeField] private bool activeAppsflyer;
		
		public bool ActiveFirebase => activeFirebase;
		public bool ActiveFirebaseMessaging => activeFirebaseMessaging;
		public bool ActiveFirebaseRemoteConfig => activeFirebaseRemoteConfig;
		public bool ActiveAppsflyer => activeAppsflyer;
		public AdNetwork AdNetwork => adNetwork;

		private static AppSetup Load()
		{
			return Resources.Load<AppSetup>("aCodeSetup");
		}
		
#if UNITY_EDITOR
		public static AppSetup EnsureAsset()
		{
			var inst = Load();
			if (inst != null) return inst;
			
			if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/Resources")) UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources");
			inst = CreateInstance<AppSetup>();
			UnityEditor.AssetDatabase.CreateAsset(inst, $"Assets/Resources/aCodeSetup.asset");
			UnityEditor.AssetDatabase.SaveAssets();
			UnityEditor.AssetDatabase.Refresh();
			return inst;
		}
#endif
	}
}