# Integration SDK - AppLovin MAX Ads

Implements `IAdProvider` from `com.gemmob.integrationsdk.core` using the
AppLovin MAX Unity Plugin. Automatically registers itself with `AdManager` at
startup — no manual wiring needed in game code.

## Install

Add this to your project's `Packages/manifest.json` (merge with your existing
`scopedRegistries`/`dependencies` if you have them):

```json
{
  "scopedRegistries": [
    {
      "name": "AppLovin MAX Unity",
      "url": "https://unity.packages.applovin.com/",
      "scopes": ["com.applovin.mediation.ads", "com.applovin.mediation.adapters", "com.applovin.mediation.dsp"]
    },
    {
      "name": "package.openupm.com",
      "url": "https://package.openupm.com",
      "scopes": ["com.google.external-dependency-manager"]
    }
  ],
  "dependencies": {
    "com.applovin.mediation.ads": "8.6.4",
    "com.google.external-dependency-manager": "1.2.186",
    "com.gemmob.integrationsdk.core": "https://github.com/KienVu99/IntegrationSDK.git?path=/Core#main",
    "com.gemmob.integrationsdk.ads.applovin": "https://github.com/KienVu99/IntegrationSDK.git?path=/Ads.AppLovin#main"
  }
}
```

Then in the Editor:

1. Open Unity — `Core`'s `AndroidGradleTemplateSetup` runs automatically on
   load and enables Custom Gradle Templates + copies the template files
   needed for AppLovin/AppsFlyer's Android dependencies to resolve. If it
   logs that it changed `ProjectSettings.asset`, **restart the Editor once**
   so Unity picks up the change (there is no public scripting API for this
   setting, so it has to be patched on disk).
2. Open `Tools > Integration SDK > Settings`, fill in `maxSdkKey` and the
   Interstitial/Rewarded/AppOpen/Banner Ad Unit IDs for Android and iOS
   (leave both platforms blank for any format you don't use), click **Save**.

If you later add other mediated ad networks (AdMob, Unity Ads, ironSource,
...), you'll need their adapter UPM packages too — for that, run
`AppLovin > Integration Manager` once to install the exact adapter package
names/versions AppLovin currently recommends, then it'll show up in this
same scoped-registry `dependencies` block for next time.

## Usage

```csharp
AdManager.ShowInterstitial("replay_lv1");
AdManager.ShowRewarded("get_jetpack_button_lv1", () => GrantReward());
AdManager.ShowAppOpen("loading_home");
AdManager.ShowBanner("home_screen");
AdManager.HideBanner();
```
