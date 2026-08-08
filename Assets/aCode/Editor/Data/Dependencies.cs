using System.Collections.Generic;

namespace aCode.Editor.Data
{
	public class PackageInfo
	{
		public string Name { get; set; }
		public string Version { get; set; }

		public PackageInfo(string name, string version = null)
		{
			Name = name;
			Version = version;
		}
	}
	
	public abstract class Dependencies
	{
		public static readonly List<PackageInfo> BasePackages = new()
		{
			new PackageInfo("com.unity.ads.ios-support", "1.2.0"),
			new PackageInfo("com.google.external-dependency-manager", "1.2.188"),
		};
		
		public static readonly List<PackageInfo> AdmobPackages = new()
		{
			new PackageInfo("com.google.ads.mobile", "9.2.0"),
			new PackageInfo("com.google.ads.mobile.mediation.unity")
		};
		
		public static readonly List<PackageInfo> AdmobMediationPackages = new()
		{
			new PackageInfo("com.google.ads.mobile.mediation.applovin"),
			new PackageInfo("com.google.ads.mobile.mediation.ironsource"),
			new PackageInfo("com.google.ads.mobile.mediation.chartboost"),
			new PackageInfo("com.google.ads.mobile.mediation.inmobi"),
			new PackageInfo("com.google.ads.mobile.mediation.dtexchange"),
			new PackageInfo("com.google.ads.mobile.mediation.liftoffmonetize"),
			// new PackageInfo("com.google.ads.mobile.mediation.mintegral"),
		};


		public static readonly List<PackageInfo> ApplovinPackages = new()
		{
			new PackageInfo("com.applovin.mediation.ads", "8.6.4"),
			// Unity Ads
			new PackageInfo("com.applovin.mediation.adapters.unityads.ios", "4190001.0.0"),
			new PackageInfo("com.applovin.mediation.adapters.unityads.android", "4190001.0.0"),
			
			// IronSource
			new PackageInfo("com.applovin.mediation.adapters.ironsource.ios", "905000000.0.0"),
			new PackageInfo("com.applovin.mediation.adapters.ironsource.android", "905000000.0.0"),
			
			// Fix Google Mobile Ads (Admob)
			new PackageInfo("com.applovin.mediation.adapters.google.android", "25040000.0.0"),
			new PackageInfo("com.applovin.mediation.adapters.google.ios", "13070000.0.0"),

			// Liftoff Monetize (Vungle)
			new PackageInfo("com.applovin.mediation.adapters.vungle.ios", "7070500.0.0"),
			new PackageInfo("com.applovin.mediation.adapters.vungle.android", "7070700.0.0"),

			// Facebook (Meta Audience Network)
			new PackageInfo("com.applovin.mediation.adapters.facebook.ios"),
			new PackageInfo("com.applovin.mediation.adapters.facebook.android"),

			// Mintegral
			new PackageInfo("com.applovin.mediation.adapters.mintegral.ios"),
			new PackageInfo("com.applovin.mediation.adapters.mintegral.android"),
		};
	}
}