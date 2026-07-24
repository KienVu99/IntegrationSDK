# Integration SDK - AppLovin MAX Ads

Implements `IAdProvider` from `com.gemmob.integrationsdk.core` using the
AppLovin MAX Unity Plugin. Automatically registers itself with `AdManager` at
startup — no manual wiring needed in game code.

## Install

1. Add this package: `"com.gemmob.integrationsdk.ads.applovin": "https://github.com/<org>/IntegrationSDK.git?path=/Ads.AppLovin"`
2. Import the AppLovin MAX Unity Plugin `.unitypackage` from
   https://github.com/AppLovin/AppLovin-MAX-Unity-Plugin/releases
3. Run `AppLovin > Integration Manager`, enter your AppLovin SDK key, and click
   **Upgrade All Adapters and Migrate to UPM**.
4. Open `Tools > Integration SDK > Settings`, fill in `maxSdkKey` and the
   Interstitial/Rewarded/AppOpen Ad Unit IDs for Android and iOS, click **Save**.

## Usage

```csharp
AdManager.ShowInterstitial("replay_lv1");
AdManager.ShowRewarded("get_jetpack_button_lv1", () => GrantReward());
AdManager.ShowAppOpen("loading_home");
AdManager.ShowBanner("home_screen");
AdManager.HideBanner();
```
