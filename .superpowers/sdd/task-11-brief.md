### Task 11: `Tracking.AppsFlyer` package + `AppsFlyerBackend`

**Files:**
- Create: `Tracking.AppsFlyer/package.json`
- Create: `Tracking.AppsFlyer/Runtime/IntegrationSDK.Tracking.AppsFlyer.Runtime.asmdef`
- Create: `Tracking.AppsFlyer/Runtime/AppsFlyerBackend.cs`
- Create: `Tracking.AppsFlyer/Runtime/AppsFlyerBackendBootstrap.cs`
- Create: `Tracking.AppsFlyer/README.md`
- Modify: `Packages/manifest.json` (local dev dependency)

**Interfaces:**
- Consumes: `IntegrationSDK.Core.ITrackingBackend`, `TrackingManager`,
  `AdRevenueData`, `IntegrationSDKConfig` (all from Core).
- Produces: `IntegrationSDK.Tracking.AppsFlyer.AppsFlyerBackend : ITrackingBackend`,
  self-registered into `TrackingManager`.

- [ ] **Step 1: Scaffold the package**

Create `Tracking.AppsFlyer/package.json`:

```json
{
  "name": "com.gemmob.integrationsdk.tracking.appsflyer",
  "version": "0.1.0",
  "displayName": "Integration SDK - AppsFlyer Tracking",
  "description": "AppsFlyer ITrackingBackend implementation for IntegrationSDK.",
  "unity": "2021.3",
  "dependencies": {
    "com.gemmob.integrationsdk.core": "0.1.0",
    "appsflyer-unity-plugin": "https://github.com/AppsFlyerSDK/appsflyer-plugin.git#upm",
    "com.google.external-dependency-manager": "https://github.com/googlesamples/unity-jar-resolver.git?path=upm"
  }
}
```

Create `Tracking.AppsFlyer/Runtime/IntegrationSDK.Tracking.AppsFlyer.Runtime.asmdef`:

```json
{
    "name": "IntegrationSDK.Tracking.AppsFlyer.Runtime",
    "rootNamespace": "IntegrationSDK.Tracking.AppsFlyer",
    "references": [
        "IntegrationSDK.Core.Runtime"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

Add to `Packages/manifest.json`:

```json
"com.gemmob.integrationsdk.tracking.appsflyer": "file:../Tracking.AppsFlyer"
```

- [ ] **Step 2: Implement `AppsFlyerBackend`**

Create `Tracking.AppsFlyer/Runtime/AppsFlyerBackend.cs`:

```csharp
using System.Collections.Generic;
using IntegrationSDK.Core;
using UnityEngine;

namespace IntegrationSDK.Tracking.AppsFlyer
{
    public class AppsFlyerBackend : ITrackingBackend
    {
        public void Initialize(IntegrationSDKConfig config)
        {
            if (!config.appsFlyerEnabled)
            {
                return;
            }

            AppsFlyer.setIsDebug(Debug.isDebugBuild);
            AppsFlyer.initSDK(config.appsFlyerDevKey, config.appsFlyerIosAppId);
            AppsFlyer.startSDK();
        }

        public void LogEvent(string eventName, Dictionary<string, object> parameters)
        {
            var stringParameters = new Dictionary<string, string>();
            foreach (var kvp in parameters)
            {
                stringParameters[kvp.Key] = kvp.Value?.ToString();
            }

            global::AppsFlyer.sendEvent(eventName, stringParameters);
        }

        public void LogAdRevenue(AdRevenueData data)
        {
            var revenueData = new AFAdRevenueData(
                data.AdSource,
                MediationNetwork.Applovinmax,
                data.Currency,
                data.Value);

            var extraParams = new Dictionary<string, string>
            {
                { "ad_unit_name", data.AdUnitName },
                { "placement", data.Placement },
                { "country_code", data.CountryCode },
                { "ad_format", data.AdFormat }
            };

            global::AppsFlyer.logAdRevenue(revenueData, extraParams);
        }
    }
}
```

Create `Tracking.AppsFlyer/Runtime/AppsFlyerBackendBootstrap.cs`:

```csharp
using IntegrationSDK.Core;
using UnityEngine;

namespace IntegrationSDK.Tracking.AppsFlyer
{
    internal static class AppsFlyerBackendBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            TrackingManager.RegisterBackend(new AppsFlyerBackend());
        }
    }
}
```

- [ ] **Step 3: Write the README**

Create `Tracking.AppsFlyer/README.md`:

```markdown
# Integration SDK - AppsFlyer Tracking

Implements `ITrackingBackend` from `com.gemmob.integrationsdk.core` using the
AppsFlyer Unity Plugin. Automatically registers with `TrackingManager` at
startup.

## Install

Add these two entries to `Packages/manifest.json` (EDM4U is required by the
AppsFlyer plugin but not bundled by its UPM branch):

```json
"com.gemmob.integrationsdk.tracking.appsflyer": "https://github.com/<org>/IntegrationSDK.git?path=/Tracking.AppsFlyer",
"appsflyer-unity-plugin": "https://github.com/AppsFlyerSDK/appsflyer-plugin.git#upm",
"com.google.external-dependency-manager": "https://github.com/googlesamples/unity-jar-resolver.git?path=upm"
```

Then open `Tools > Integration SDK > Settings`, enable **AppsFlyer**, fill in
Dev Key and iOS App ID, click **Save**.
```

- [ ] **Step 4: Manually verify**

1. In the dev project, confirm `appsflyer-unity-plugin` resolves (Package
   Manager shows no errors) after adding the dependency in Step 1.
2. Enable AppsFlyer + fill Dev Key in the Settings window.
3. Enter Play Mode; confirm no `IntegrationSDK` "no tracking backend" warning
   appears, and AppsFlyer's own SDK init log line appears in the console.

- [ ] **Step 5: Commit**

```bash
git add Tracking.AppsFlyer Packages/manifest.json
git commit -m "Add Tracking.AppsFlyer package with self-registering ITrackingBackend"
```

---

