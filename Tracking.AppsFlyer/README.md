# Integration SDK - AppsFlyer Tracking

Implements `ITrackingBackend` from `com.gemmob.integrationsdk.core` using the
AppsFlyer Unity Plugin. Automatically registers with `TrackingManager` at
startup.

## Install

Add these two entries to `Packages/manifest.json` (EDM4U is required by the
AppsFlyer plugin but not bundled by its UPM branch):

```json
"com.gemmob.integrationsdk.tracking.appsflyer": "https://github.com/<org>/IntegrationSDK.git?path=/Tracking.AppsFlyer",
"appsflyer-unity-plugin": "https://github.com/AppsFlyerSDK/appsflyer-unity-plugin.git#upm",
"com.google.external-dependency-manager": "https://github.com/googlesamples/unity-jar-resolver.git?path=upm"
```

Then open `Tools > Integration SDK > Settings`, enable **AppsFlyer**, fill in
Dev Key and iOS App ID, click **Save**.
