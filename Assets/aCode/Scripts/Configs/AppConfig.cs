using System;
using System.Collections.Generic;

namespace aCode.Configs
{
	public static class AppConfig
	{
		private static AppSettings _s;
		private static AppSettings S => _s ??= AppSettings.Load();
		public static bool IsDebugMode => S != null && S.IsDebugMode;
		public static bool TurnOffAds => S != null && S.TurnOffAds;
		public static string ApplovinSdkKey => S?.ApplovinSdkKey ?? string.Empty;
		public static bool ConsentFlowEnabled => S == null || S.ConsentFlowEnabled;
		public static string PrivacyPolicyUrl => S?.PrivacyPolicyUrl ?? string.Empty;
		public static string TermsOfServiceUrl => S?.TermsOfServiceUrl ?? string.Empty;
		public static string BannerID => S?.BannerID ?? string.Empty;
		public static string BannerCollapsibleID => S?.BannerCollapsibleID ?? string.Empty;
		public static string InterstitialID => S?.InterstitialID ?? string.Empty;
		public static string RewardedVideoID => S?.RewardedVideoID ?? string.Empty;
		public static string AdmobAppID => S?.AdmobAppID ?? string.Empty;
		public static string AppOpenID => S?.AppOpenID ?? string.Empty;
		
		public static string BannerIosID => S?.BannerIosID ?? string.Empty;
		public static string BannerCollapsibleIosID => S?.BannerCollapsibleIosID ?? string.Empty;
		public static string InterstitialIosID => S?.InterstitialIosID ?? string.Empty;
		public static string RewardedVideoIosID => S?.RewardedVideoIosID ?? string.Empty;
		public static string AdmobIosAppID => S?.AdmobIosAppID ?? string.Empty;
		public static string AppOpenIosID => S?.AppOpenIosID ?? string.Empty;

		public static bool DirectedForChildren => S != null && S.DirectedForChildren;
		public static bool AutoShowBanner => S != null && S.AutoShowBanner;
		public static BannerPosition BannerPosition => S != null ? S.BannerPosition : BannerPosition.Bottom;

		public static int IntervalShowAds => S != null ? S.IntervalShowAds : 0;
		public static string TestDeviceID => S?.TestDeviceID ?? string.Empty;
		public static string AppleStoreUrl => S?.AppleStoreUrl ?? string.Empty;
		public static string AppsflyerDevKey => S?.AppsflyerDevKey ?? string.Empty;
		public static string AppsflyerAppID => S?.AppsflyerAppID ?? string.Empty;
		
		public static IReadOnlyList<RemoteConfigData> RemoteConfigs => S?.RemoteConfigs ?? Array.Empty<RemoteConfigData>();
	}
	
}