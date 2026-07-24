# Integration SDK

Modular Unity SDK for AppLovin MAX ads and AppsFlyer tracking. Each module is
a self-registering package — install it, fill in your keys, call the static
`AdManager`/`TrackingManager` APIs from your game code. No manual wiring.

## Packages

| Package | What it does |
|---|---|
| `com.gemmob.integrationsdk.core` | `IAdProvider`/`AdManager`, `ITrackingBackend`/`TrackingManager`, config asset, build-time validation. Required by every other package. |
| `com.gemmob.integrationsdk.ads.applovin` | AppLovin MAX ads: interstitial, rewarded, app open, banner. |
| `com.gemmob.integrationsdk.tracking.appsflyer` | AppsFlyer event + ad revenue tracking. |

## Quick start (recommended)

1. Add **Core** to your project's `Packages/manifest.json`:
   ```json
   "com.gemmob.integrationsdk.core": "https://github.com/KienVu99/IntegrationSDK.git?path=/Core#main"
   ```
2. Open Unity and let it resolve. Console will log that Custom Android Gradle
   Templates were enabled automatically — if it says to restart the Editor,
   do that once (there's no scripting API for that setting, so it's patched
   into `ProjectSettings.asset` on disk).
3. Open **`Tools > Integration SDK > Package Setup`**:
   - If migrating an existing project, click **Scan for existing
     ad/analytics SDKs** first and remove anything that would conflict
     (Firebase, AdMob, Unity Ads, Facebook SDK). Review the list before
     confirming — Assets folders go to the OS Recycle Bin, but
     `manifest.json` edits are immediate. Commit your project first.
   - Tick the modules you want (Ads.AppLovin / Tracking.AppsFlyer) and click
     **Save**. This writes the scoped registries + package entries for you
     and triggers Unity to resolve them — no manual JSON editing.
4. Open **`Tools > Integration SDK > Settings`**: fill in the AppLovin SDK
   Key, Ad Unit IDs (Android + iOS) for whichever formats you use, and the
   AppsFlyer Dev Key if tracking is enabled. Leave both platforms blank for
   an ad format you don't use. Click **Save**.

## Manual install (if you'd rather edit manifest.json yourself)

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
    "com.gemmob.integrationsdk.ads.applovin": "https://github.com/KienVu99/IntegrationSDK.git?path=/Ads.AppLovin#main",
    "com.gemmob.integrationsdk.tracking.appsflyer": "https://github.com/KienVu99/IntegrationSDK.git?path=/Tracking.AppsFlyer#main",
    "appsflyer-unity-plugin": "https://github.com/AppsFlyerSDK/appsflyer-unity-plugin.git#upm"
  }
}
```

AppLovin MAX itself installs via its own scoped registry above — you do
**not** need to run `AppLovin > Integration Manager` unless you add extra
mediated ad networks later (AdMob, Unity Ads, ironSource, ...), in which
case run it once to get the exact adapter package names/versions.

## Usage

```csharp
using IntegrationSDK.Core;

AdManager.ShowInterstitial("replay_lv1");
AdManager.ShowRewarded("get_jetpack_button_lv1", () => GrantReward());
AdManager.ShowAppOpen("loading_home");
AdManager.ShowBanner("home_screen");
AdManager.HideBanner();

TrackingManager.LogEvent("level_complete", new Dictionary<string, object> { { "level", 5 } });
```

Ad provider and tracking backend register themselves at startup
(`RuntimeInitializeOnLoadMethod`) — there's nothing else to call.

## Building

The Android build resolves AppLovin/AppsFlyer's native dependencies by
patching them into `mainTemplate.gradle` instead of copying AARs (the AAR
copy path breaks on newer JDKs). If dependencies don't resolve after adding
a package, run `Assets > External Dependency Manager > Android Resolver >
Force Resolve`.

## Debugging

`Assets/DebugTools` (this demo project only — not pulled in via git URL
package installs) adds an on-screen overlay with buttons for each ad format
and a live log of every `Initialize`/`LogEvent`/`LogAdRevenue` call, useful
for confirming ads and tracking fire correctly on a real device.

## Troubleshooting

- **"No ad provider registered" warning on device but works in Editor**:
  make sure `Assets/link.xml` (from the Core package) is present — IL2CPP
  can strip assemblies that are only reached via
  `[RuntimeInitializeOnLoadMethod]` if nothing else references them.
- **"Unsupported class file major version 61" during Gradle resolve**:
  Gradle 5.1.1 (EDM4U's internal dependency-download build) isn't
  compatible with JDK 17+. Enabling Custom Gradle Templates (done
  automatically by this SDK) works around it entirely by letting your real
  Android Gradle build resolve dependencies instead.
- **NO FILL / "Ad Unit ID is invalid or disabled"**: the Ad Unit ID doesn't
  belong to the app registered under your SDK Key + package name in the
  AppLovin dashboard. Double check the package name matches exactly.
