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
			new PackageInfo("com.unity.ads.ios-support"),
			new PackageInfo("com.google.external-dependency-manager"),
		};
		
		public static readonly List<PackageInfo> AdmobPackages = new()
		{
			new PackageInfo("com.google.ads.mobile"),
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
			new PackageInfo("com.applovin.mediation.ads"),
			// Unity Ads
			new PackageInfo("com.applovin.mediation.adapters.unityads.ios"),
			new PackageInfo("com.applovin.mediation.adapters.unityads.android"),
			
			// IronSource
			new PackageInfo("com.applovin.mediation.adapters.ironsource.ios"),
			new PackageInfo("com.applovin.mediation.adapters.ironsource.android"),
			
			// Fix Google Mobile Ads
			// new PackageInfo("com.applovin.mediation.adapters.google.android", "24050001.0.0"),
			new PackageInfo("com.applovin.mediation.adapters.google.android"),
			// new PackageInfo("com.applovin.mediation.adapters.googleadmanager.android", "24050000.0.0"),
			new PackageInfo("com.applovin.mediation.adapters.googleadmanager.android"),
			new PackageInfo("com.applovin.mediation.adapters.google.ios"),
			new PackageInfo("com.applovin.mediation.adapters.googleadmanager.ios"),
			
			// Liftoff
			new PackageInfo("com.applovin.mediation.adapters.vungle.ios"),
			new PackageInfo("com.applovin.mediation.adapters.vungle.android"),
			
			// Chartboost
			new PackageInfo("com.applovin.mediation.adapters.chartboost.ios"),
			new PackageInfo("com.applovin.mediation.adapters.chartboost.android"),
			
			// InMobi
			new PackageInfo("com.applovin.mediation.adapters.inmobi.ios"),
			new PackageInfo("com.applovin.mediation.adapters.inmobi.android"),
			
			// DT Exchange
			new PackageInfo("com.applovin.mediation.adapters.fyber.ios"),
			new PackageInfo("com.applovin.mediation.adapters.fyber.android"),
			
			// Mintegral
			// new PackageInfo("com.applovin.mediation.adapters.mintegral.ios"),
			// new PackageInfo("com.applovin.mediation.adapters.mintegral.android"),
		};
	}
}