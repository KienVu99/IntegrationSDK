# IntegrationSDK Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a modular Unity Package Manager (UPM) suite — a dependency-free `Core`
package plus three independent integration packages (`Ads.AppLovin`,
`Tracking.AppsFlyer`, `Tracking.Firebase`) — that auto-initializes AppLovin MAX ads
and AppsFlyer/Firebase tracking, and auto-fires every event defined in `Tracking.md`.

**Architecture:** `Core` defines `IAdProvider` and `ITrackingBackend` interfaces plus
the `AdManager`/`TrackingManager` static facades game code calls. Each integration
package implements one interface and self-registers via
`[RuntimeInitializeOnLoadMethod]`. Core never references AppLovin/AppsFlyer/Firebase
types, so adding or removing an integration package never touches Core code.

**Tech Stack:** Unity 2021.3+ (project currently targets the installed Editor
version), UPM local file packages for development + Git URL packages for
distribution, Unity Test Framework (EditMode tests, already installed as
`com.unity.test-framework: 1.1.33`), C#.

## Global Constraints

- Source of truth for event names/parameters: `Tracking.md` (verbatim strings, no
  renaming).
- Core package (`com.gemmob.integrationsdk.core`) must have zero dependency on
  AppLovin MAX, AppsFlyer, or Firebase assemblies/types.
- Dependency direction is one-way: integration packages → Core. Core never
  references an integration package.
- Every integration registers with Core only through
  `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]` —
  no reflection-based assembly scanning.
- `IntegrationSDKConfig` is a `ScriptableObject` stored anywhere under `Assets`
  (never required to live in a `Resources` folder) and is made build-safe via
  Unity's Preloaded Assets list.
- Config fields use primitive types only (string/bool) so Core never needs a
  compile-time reference to a third-party SDK type.
- Ad formats in scope: Interstitial, Rewarded, App Open (AOA) only.
- Tracking backends in scope: AppsFlyer, Firebase Analytics only.
- Adapter/backend failures must log a warning and no-op — never throw or crash
  the game.
- AppsFlyer UPM dependency (confirmed working line from AppsFlyer's own docs):
  `"appsflyer-unity-plugin": "https://github.com/AppsFlyerSDK/appsflyer-plugin.git#upm"`.
- EDM4U (External Dependency Manager for Unity) UPM dependency:
  `"com.google.external-dependency-manager": "https://github.com/googlesamples/unity-jar-resolver.git?path=upm"`.
- AppLovin MAX SDK and Firebase Analytics SDK do **not** have a reliable plain
  git-URL UPM line as of this plan (AppLovin ships via its own Integration
  Manager / scoped registry migration flow; Firebase ships via its own scoped
  registry). Both are installed as a **documented one-time manual step per
  consuming project** — this was already agreed in the design spec, not a gap
  introduced here.

---

### Task 1: Scaffold the `Core` package

**Files:**
- Create: `Core/package.json`
- Create: `Core/Runtime/IntegrationSDK.Core.Runtime.asmdef`
- Create: `Core/Editor/IntegrationSDK.Core.Editor.asmdef`
- Create: `Core/Tests/Editor/IntegrationSDK.Core.Tests.Editor.asmdef`
- Create: `Core/Runtime/AssemblyInfo.cs`
- Modify: `Packages/manifest.json` (add local file dependency for dev iteration)

**Interfaces:**
- Produces: package `com.gemmob.integrationsdk.core` resolvable in this dev
  project via `file:../Core`; assembly names `IntegrationSDK.Core.Runtime`,
  `IntegrationSDK.Core.Editor`, `IntegrationSDK.Core.Tests.Editor` that later
  tasks add scripts into.

- [ ] **Step 1: Create the package folder and manifest**

Create `Core/package.json`:

```json
{
  "name": "com.gemmob.integrationsdk.core",
  "version": "0.1.0",
  "displayName": "Integration SDK - Core",
  "description": "Dependency-free core: TrackingManager, AdManager, IntegrationSDKConfig. Integration packages (Ads.AppLovin, Tracking.AppsFlyer, Tracking.Firebase) plug into this without Core ever referencing them.",
  "unity": "2021.3",
  "dependencies": {}
}
```

- [ ] **Step 2: Create the Runtime assembly definition**

Create `Core/Runtime/IntegrationSDK.Core.Runtime.asmdef`:

```json
{
    "name": "IntegrationSDK.Core.Runtime",
    "rootNamespace": "IntegrationSDK.Core",
    "references": [],
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

- [ ] **Step 3: Create the Editor assembly definition**

Create `Core/Editor/IntegrationSDK.Core.Editor.asmdef`:

```json
{
    "name": "IntegrationSDK.Core.Editor",
    "rootNamespace": "IntegrationSDK.Core.Editor",
    "references": [
        "IntegrationSDK.Core.Runtime"
    ],
    "includePlatforms": [
        "Editor"
    ],
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

- [ ] **Step 4: Create the EditMode test assembly definition**

Create `Core/Tests/Editor/IntegrationSDK.Core.Tests.Editor.asmdef`:

```json
{
    "name": "IntegrationSDK.Core.Tests.Editor",
    "rootNamespace": "IntegrationSDK.Core.Tests",
    "references": [
        "IntegrationSDK.Core.Runtime",
        "IntegrationSDK.Core.Editor",
        "UnityEngine.TestRunner",
        "UnityEditor.TestRunner"
    ],
    "includePlatforms": [
        "Editor"
    ],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": true,
    "precompiledReferences": [
        "nunit.framework.dll"
    ],
    "autoReferenced": true,
    "defineConstraints": [
        "UNITY_INCLUDE_TESTS"
    ],
    "versionDefines": [],
    "noEngineReferences": false
}
```

- [ ] **Step 5: Allow the test assembly to see internal members**

Create `Core/Runtime/AssemblyInfo.cs`:

```csharp
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("IntegrationSDK.Core.Tests.Editor")]
```

- [ ] **Step 6: Register the package as a local dependency in this dev project**

Read `Packages/manifest.json`, then add this entry inside `"dependencies"`:

```json
"com.gemmob.integrationsdk.core": "file:../Core"
```

- [ ] **Step 7: Verify it resolves**

Open Unity Editor (or run in batch mode) and confirm no console errors and that
`Window > Package Manager > In Project` lists `Integration SDK - Core`.

Run: `& "C:\Program Files\Unity\Hub\Editor\<version>\Editor\Unity.exe" -batchmode -quit -projectPath "D:\2026\VibeCode\IntegrationSDK" -logFile -`
Expected: log ends without `error CS` or package resolution errors.

- [ ] **Step 8: Commit**

```bash
git add Core/package.json Core/Runtime/IntegrationSDK.Core.Runtime.asmdef Core/Editor/IntegrationSDK.Core.Editor.asmdef Core/Tests/Editor/IntegrationSDK.Core.Tests.Editor.asmdef Core/Runtime/AssemblyInfo.cs Packages/manifest.json
git commit -m "Scaffold Core UPM package with runtime/editor/test assemblies"
```

---

### Task 2: `IntegrationSDKConfig`

**Files:**
- Create: `Core/Runtime/Config/IntegrationSDKConfig.cs`
- Test: `Core/Tests/Editor/IntegrationSDKConfigTests.cs`

**Interfaces:**
- Produces: `IntegrationSDK.Core.IntegrationSDKConfig : ScriptableObject` with public
  fields `maxSdkKey`, `androidInterstitialAdUnitId`, `iosInterstitialAdUnitId`,
  `androidRewardedAdUnitId`, `iosRewardedAdUnitId`, `androidAppOpenAdUnitId`,
  `iosAppOpenAdUnitId`, `appsFlyerEnabled`, `appsFlyerDevKey`, `appsFlyerIosAppId`,
  `firebaseEnabled`; methods `string GetInterstitialAdUnitId()`,
  `string GetRewardedAdUnitId()`, `string GetAppOpenAdUnitId()`.

- [ ] **Step 1: Write the failing test**

Create `Core/Tests/Editor/IntegrationSDKConfigTests.cs`:

```csharp
using IntegrationSDK.Core;
using NUnit.Framework;
using UnityEngine;

public class IntegrationSDKConfigTests
{
    [Test]
    public void GetInterstitialAdUnitId_ReturnsAndroidValue_WhenNotIOS()
    {
        var config = ScriptableObject.CreateInstance<IntegrationSDKConfig>();
        config.androidInterstitialAdUnitId = "android-int-id";
        config.iosInterstitialAdUnitId = "ios-int-id";

        var result = config.GetInterstitialAdUnitIdForPlatform(RuntimePlatform.Android);

        Assert.AreEqual("android-int-id", result);
    }

    [Test]
    public void GetInterstitialAdUnitId_ReturnsIOSValue_WhenIOS()
    {
        var config = ScriptableObject.CreateInstance<IntegrationSDKConfig>();
        config.androidInterstitialAdUnitId = "android-int-id";
        config.iosInterstitialAdUnitId = "ios-int-id";

        var result = config.GetInterstitialAdUnitIdForPlatform(RuntimePlatform.IPhonePlayer);

        Assert.AreEqual("ios-int-id", result);
    }
}
```

Note: the test calls a platform-parameterized overload
(`GetInterstitialAdUnitIdForPlatform(RuntimePlatform)`) instead of the
zero-arg `GetInterstitialAdUnitId()` so the test is deterministic in the Editor
(where `Application.platform` is always the Editor platform, not
Android/iOS). The zero-arg method is a thin wrapper calling the
parameterized one with `Application.platform`.

- [ ] **Step 2: Run test to verify it fails**

Run: `Unity Test Runner > EditMode > Run All` (or
`-runTests -testPlatform EditMode -testResults results.xml` in batch mode)
Expected: FAIL — `IntegrationSDKConfig` does not exist.

- [ ] **Step 3: Write the implementation**

Create `Core/Runtime/Config/IntegrationSDKConfig.cs`:

```csharp
using UnityEngine;

namespace IntegrationSDK.Core
{
    [CreateAssetMenu(fileName = "IntegrationSDKConfig", menuName = "Integration SDK/Config")]
    public class IntegrationSDKConfig : ScriptableObject
    {
        [Header("AppLovin MAX")]
        public string maxSdkKey;
        public string androidInterstitialAdUnitId;
        public string iosInterstitialAdUnitId;
        public string androidRewardedAdUnitId;
        public string iosRewardedAdUnitId;
        public string androidAppOpenAdUnitId;
        public string iosAppOpenAdUnitId;

        [Header("AppsFlyer")]
        public bool appsFlyerEnabled;
        public string appsFlyerDevKey;
        public string appsFlyerIosAppId;

        [Header("Firebase")]
        public bool firebaseEnabled;

        public string GetInterstitialAdUnitId() => GetInterstitialAdUnitIdForPlatform(Application.platform);
        public string GetRewardedAdUnitId() => GetRewardedAdUnitIdForPlatform(Application.platform);
        public string GetAppOpenAdUnitId() => GetAppOpenAdUnitIdForPlatform(Application.platform);

        public string GetInterstitialAdUnitIdForPlatform(RuntimePlatform platform) =>
            platform == RuntimePlatform.IPhonePlayer ? iosInterstitialAdUnitId : androidInterstitialAdUnitId;

        public string GetRewardedAdUnitIdForPlatform(RuntimePlatform platform) =>
            platform == RuntimePlatform.IPhonePlayer ? iosRewardedAdUnitId : androidRewardedAdUnitId;

        public string GetAppOpenAdUnitIdForPlatform(RuntimePlatform platform) =>
            platform == RuntimePlatform.IPhonePlayer ? iosAppOpenAdUnitId : androidAppOpenAdUnitId;
    }
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `Unity Test Runner > EditMode > Run All`
Expected: PASS (2/2).

- [ ] **Step 5: Commit**

```bash
git add Core/Runtime/Config/IntegrationSDKConfig.cs Core/Tests/Editor/IntegrationSDKConfigTests.cs
git commit -m "Add IntegrationSDKConfig ScriptableObject"
```

---

### Task 3: `ITrackingBackend`, `AdRevenueData`, `TrackingManager`

**Files:**
- Create: `Core/Runtime/Tracking/AdRevenueData.cs`
- Create: `Core/Runtime/Tracking/ITrackingBackend.cs`
- Create: `Core/Runtime/Tracking/TrackingManager.cs`
- Test: `Core/Tests/Editor/TrackingManagerTests.cs`

**Interfaces:**
- Consumes: `IntegrationSDKConfig` (Task 2).
- Produces:
  - `IntegrationSDK.Core.AdRevenueData` with fields `AdPlatform, AdSource,
    AdUnitName, Currency, Value (double), Placement, CountryCode, AdFormat`
    (all `string` except `Value`).
  - `IntegrationSDK.Core.ITrackingBackend` with
    `void Initialize(IntegrationSDKConfig config)`,
    `void LogEvent(string eventName, Dictionary<string, object> parameters)`,
    `void LogAdRevenue(AdRevenueData data)`.
  - `IntegrationSDK.Core.TrackingManager` static class with
    `RegisterBackend(ITrackingBackend)`, `UnregisterBackend(ITrackingBackend)`,
    `InitializeAll(IntegrationSDKConfig config)`,
    `LogEvent(string eventName, Dictionary<string, object> parameters = null)`,
    `LogAdRevenue(AdRevenueData data)`. Internal `ResetForTests()` for test
    isolation (visible to `IntegrationSDK.Core.Tests.Editor` via
    `InternalsVisibleTo` from Task 1).

- [ ] **Step 1: Write the failing test**

Create `Core/Tests/Editor/TrackingManagerTests.cs`:

```csharp
using System.Collections.Generic;
using IntegrationSDK.Core;
using NUnit.Framework;

public class TrackingManagerTests
{
    private class FakeBackend : ITrackingBackend
    {
        public bool Initialized;
        public readonly List<(string name, Dictionary<string, object> parameters)> LoggedEvents = new List<(string, Dictionary<string, object>)>();
        public readonly List<AdRevenueData> LoggedRevenue = new List<AdRevenueData>();

        public void Initialize(IntegrationSDKConfig config) => Initialized = true;
        public void LogEvent(string eventName, Dictionary<string, object> parameters) => LoggedEvents.Add((eventName, parameters));
        public void LogAdRevenue(AdRevenueData data) => LoggedRevenue.Add(data);
    }

    private class ThrowingBackend : ITrackingBackend
    {
        public void Initialize(IntegrationSDKConfig config) { }
        public void LogEvent(string eventName, Dictionary<string, object> parameters) => throw new System.Exception("boom");
        public void LogAdRevenue(AdRevenueData data) => throw new System.Exception("boom");
    }

    [SetUp]
    public void SetUp() => TrackingManager.ResetForTests();

    [TearDown]
    public void TearDown() => TrackingManager.ResetForTests();

    [Test]
    public void LogEvent_FansOutToAllRegisteredBackends()
    {
        var backendA = new FakeBackend();
        var backendB = new FakeBackend();
        TrackingManager.RegisterBackend(backendA);
        TrackingManager.RegisterBackend(backendB);

        TrackingManager.LogEvent("level_start", new Dictionary<string, object> { { "level", 1 } });

        Assert.AreEqual(1, backendA.LoggedEvents.Count);
        Assert.AreEqual(1, backendB.LoggedEvents.Count);
        Assert.AreEqual("level_start", backendA.LoggedEvents[0].name);
    }

    [Test]
    public void LogEvent_DoesNotThrow_WhenNoBackendsRegistered()
    {
        Assert.DoesNotThrow(() => TrackingManager.LogEvent("show_interstitial_ads", null));
    }

    [Test]
    public void LogEvent_DoesNotThrow_WhenABackendThrows()
    {
        TrackingManager.RegisterBackend(new ThrowingBackend());
        var backendB = new FakeBackend();
        TrackingManager.RegisterBackend(backendB);

        Assert.DoesNotThrow(() => TrackingManager.LogEvent("ad_impression", new Dictionary<string, object>()));
        Assert.AreEqual(1, backendB.LoggedEvents.Count);
    }

    [Test]
    public void UnregisterBackend_StopsReceivingEvents()
    {
        var backend = new FakeBackend();
        TrackingManager.RegisterBackend(backend);
        TrackingManager.UnregisterBackend(backend);

        TrackingManager.LogEvent("level_passed", new Dictionary<string, object>());

        Assert.AreEqual(0, backend.LoggedEvents.Count);
    }

    [Test]
    public void LogAdRevenue_FansOutToAllRegisteredBackends()
    {
        var backend = new FakeBackend();
        TrackingManager.RegisterBackend(backend);
        var data = new AdRevenueData { AdPlatform = "applovin_max", Value = 0.01 };

        TrackingManager.LogAdRevenue(data);

        Assert.AreEqual(1, backend.LoggedRevenue.Count);
        Assert.AreEqual("applovin_max", backend.LoggedRevenue[0].AdPlatform);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `Unity Test Runner > EditMode > Run All`
Expected: FAIL — `TrackingManager`, `ITrackingBackend`, `AdRevenueData` do not exist.

- [ ] **Step 3: Write the implementation**

Create `Core/Runtime/Tracking/AdRevenueData.cs`:

```csharp
namespace IntegrationSDK.Core
{
    public class AdRevenueData
    {
        public string AdPlatform;
        public string AdSource;
        public string AdUnitName;
        public string Currency;
        public double Value;
        public string Placement;
        public string CountryCode;
        public string AdFormat;
    }
}
```

Create `Core/Runtime/Tracking/ITrackingBackend.cs`:

```csharp
using System.Collections.Generic;

namespace IntegrationSDK.Core
{
    public interface ITrackingBackend
    {
        void Initialize(IntegrationSDKConfig config);
        void LogEvent(string eventName, Dictionary<string, object> parameters);
        void LogAdRevenue(AdRevenueData data);
    }
}
```

Create `Core/Runtime/Tracking/TrackingManager.cs`:

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

namespace IntegrationSDK.Core
{
    public static class TrackingManager
    {
        private static readonly List<ITrackingBackend> _backends = new List<ITrackingBackend>();
        private static bool _warnedNoBackends;

        public static void RegisterBackend(ITrackingBackend backend)
        {
            if (backend != null && !_backends.Contains(backend))
            {
                _backends.Add(backend);
            }
        }

        public static void UnregisterBackend(ITrackingBackend backend)
        {
            _backends.Remove(backend);
        }

        public static void InitializeAll(IntegrationSDKConfig config)
        {
            foreach (var backend in _backends)
            {
                SafeInvoke(() => backend.Initialize(config));
            }
        }

        public static void LogEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            parameters ??= new Dictionary<string, object>();

            if (!WarnIfNoBackends())
            {
                return;
            }

            foreach (var backend in _backends)
            {
                SafeInvoke(() => backend.LogEvent(eventName, parameters));
            }
        }

        public static void LogAdRevenue(AdRevenueData data)
        {
            if (!WarnIfNoBackends())
            {
                return;
            }

            foreach (var backend in _backends)
            {
                SafeInvoke(() => backend.LogAdRevenue(data));
            }
        }

        internal static void ResetForTests()
        {
            _backends.Clear();
            _warnedNoBackends = false;
        }

        private static bool WarnIfNoBackends()
        {
            if (_backends.Count > 0)
            {
                return true;
            }

            if (!_warnedNoBackends)
            {
                Debug.LogWarning("IntegrationSDK: no tracking backend registered (add Tracking.AppsFlyer and/or Tracking.Firebase package).");
                _warnedNoBackends = true;
            }

            return false;
        }

        private static void SafeInvoke(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"IntegrationSDK: tracking backend threw an exception: {e}");
            }
        }
    }
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `Unity Test Runner > EditMode > Run All`
Expected: PASS (all `TrackingManagerTests`).

- [ ] **Step 5: Commit**

```bash
git add Core/Runtime/Tracking Core/Tests/Editor/TrackingManagerTests.cs
git commit -m "Add ITrackingBackend, AdRevenueData, and TrackingManager fan-out facade"
```

---

### Task 4: `IAdProvider` and `AdManager`

**Files:**
- Create: `Core/Runtime/Ads/IAdProvider.cs`
- Create: `Core/Runtime/Ads/AdManager.cs`
- Test: `Core/Tests/Editor/AdManagerTests.cs`

**Interfaces:**
- Consumes: `IntegrationSDKConfig` (Task 2).
- Produces:
  - `IntegrationSDK.Core.IAdProvider` with
    `void Initialize(IntegrationSDKConfig config)`,
    `void ShowInterstitial(string placement)`,
    `void ShowRewarded(string placement, Action onRewardEarned)`,
    `void ShowAppOpen(string placement)`.
  - `IntegrationSDK.Core.AdManager` static class with
    `RegisterProvider(IAdProvider provider)`, `bool HasProvider`,
    `Initialize(IntegrationSDKConfig config)`, `ShowInterstitial(string placement)`,
    `ShowRewarded(string placement, Action onRewardEarned)`,
    `ShowAppOpen(string placement)`. Internal `ResetForTests()`.

- [ ] **Step 1: Write the failing test**

Create `Core/Tests/Editor/AdManagerTests.cs`:

```csharp
using System;
using IntegrationSDK.Core;
using NUnit.Framework;

public class AdManagerTests
{
    private class FakeAdProvider : IAdProvider
    {
        public bool Initialized;
        public string LastInterstitialPlacement;
        public string LastRewardedPlacement;
        public string LastAppOpenPlacement;
        public Action PendingReward;

        public void Initialize(IntegrationSDKConfig config) => Initialized = true;
        public void ShowInterstitial(string placement) => LastInterstitialPlacement = placement;
        public void ShowRewarded(string placement, Action onRewardEarned)
        {
            LastRewardedPlacement = placement;
            PendingReward = onRewardEarned;
        }
        public void ShowAppOpen(string placement) => LastAppOpenPlacement = placement;
    }

    [SetUp]
    public void SetUp() => AdManager.ResetForTests();

    [TearDown]
    public void TearDown() => AdManager.ResetForTests();

    [Test]
    public void ShowInterstitial_ForwardsToRegisteredProvider()
    {
        var provider = new FakeAdProvider();
        AdManager.RegisterProvider(provider);

        AdManager.ShowInterstitial("replay_lv1");

        Assert.AreEqual("replay_lv1", provider.LastInterstitialPlacement);
    }

    [Test]
    public void ShowRewarded_InvokesCallback_WhenProviderCallsIt()
    {
        var provider = new FakeAdProvider();
        AdManager.RegisterProvider(provider);
        var rewardGranted = false;

        AdManager.ShowRewarded("get_jetpack_button_lv1", () => rewardGranted = true);
        provider.PendingReward.Invoke();

        Assert.IsTrue(rewardGranted);
    }

    [Test]
    public void ShowInterstitial_DoesNotThrow_WhenNoProviderRegistered()
    {
        Assert.DoesNotThrow(() => AdManager.ShowInterstitial("replay_lv1"));
        Assert.IsFalse(AdManager.HasProvider);
    }

    [Test]
    public void RegisterProvider_ReplacesPreviousProvider()
    {
        var providerA = new FakeAdProvider();
        var providerB = new FakeAdProvider();
        AdManager.RegisterProvider(providerA);
        AdManager.RegisterProvider(providerB);

        AdManager.ShowAppOpen("loading_home");

        Assert.IsNull(providerA.LastAppOpenPlacement);
        Assert.AreEqual("loading_home", providerB.LastAppOpenPlacement);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `Unity Test Runner > EditMode > Run All`
Expected: FAIL — `IAdProvider`, `AdManager` do not exist.

- [ ] **Step 3: Write the implementation**

Create `Core/Runtime/Ads/IAdProvider.cs`:

```csharp
using System;

namespace IntegrationSDK.Core
{
    public interface IAdProvider
    {
        void Initialize(IntegrationSDKConfig config);
        void ShowInterstitial(string placement);
        void ShowRewarded(string placement, Action onRewardEarned);
        void ShowAppOpen(string placement);
    }
}
```

Create `Core/Runtime/Ads/AdManager.cs`:

```csharp
using System;
using UnityEngine;

namespace IntegrationSDK.Core
{
    public static class AdManager
    {
        private static IAdProvider _provider;
        private static bool _warnedNoProvider;

        public static bool HasProvider => _provider != null;

        public static void RegisterProvider(IAdProvider provider)
        {
            if (_provider != null && !ReferenceEquals(_provider, provider))
            {
                Debug.LogWarning("IntegrationSDK: replacing an already-registered ad provider.");
            }

            _provider = provider;
        }

        public static void Initialize(IntegrationSDKConfig config)
        {
            if (!WarnIfNoProvider())
            {
                return;
            }

            _provider.Initialize(config);
        }

        public static void ShowInterstitial(string placement)
        {
            if (!WarnIfNoProvider())
            {
                return;
            }

            _provider.ShowInterstitial(placement);
        }

        public static void ShowRewarded(string placement, Action onRewardEarned)
        {
            if (!WarnIfNoProvider())
            {
                return;
            }

            _provider.ShowRewarded(placement, onRewardEarned);
        }

        public static void ShowAppOpen(string placement)
        {
            if (!WarnIfNoProvider())
            {
                return;
            }

            _provider.ShowAppOpen(placement);
        }

        internal static void ResetForTests()
        {
            _provider = null;
            _warnedNoProvider = false;
        }

        private static bool WarnIfNoProvider()
        {
            if (_provider != null)
            {
                return true;
            }

            if (!_warnedNoProvider)
            {
                Debug.LogWarning("IntegrationSDK: no ad provider registered (add the Ads.AppLovin package).");
                _warnedNoProvider = true;
            }

            return false;
        }
    }
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `Unity Test Runner > EditMode > Run All`
Expected: PASS (all `AdManagerTests`).

- [ ] **Step 5: Commit**

```bash
git add Core/Runtime/Ads Core/Tests/Editor/AdManagerTests.cs
git commit -m "Add IAdProvider interface and AdManager forwarding facade"
```

---

### Task 5: `LevelTracker`

**Files:**
- Create: `Core/Runtime/LevelTracker.cs`
- Test: `Core/Tests/Editor/LevelTrackerTests.cs`

**Interfaces:**
- Consumes: `TrackingManager.LogEvent(string, Dictionary<string, object>)` (Task 3).
- Produces: `IntegrationSDK.Core.LevelTracker` static class with
  `Func<double> NowProvider` (settable, defaults to
  `() => Time.realtimeSinceStartupAsDouble`), `Start(string level)`,
  `Passed(string level)`, `Failed(string level)`.

- [ ] **Step 1: Write the failing test**

Create `Core/Tests/Editor/LevelTrackerTests.cs`:

```csharp
using System.Collections.Generic;
using IntegrationSDK.Core;
using NUnit.Framework;

public class LevelTrackerTests
{
    private class FakeBackend : ITrackingBackend
    {
        public readonly List<(string name, Dictionary<string, object> parameters)> LoggedEvents = new List<(string, Dictionary<string, object>)>();
        public void Initialize(IntegrationSDKConfig config) { }
        public void LogEvent(string eventName, Dictionary<string, object> parameters) => LoggedEvents.Add((eventName, parameters));
        public void LogAdRevenue(AdRevenueData data) { }
    }

    private FakeBackend _backend;
    private double _fakeNow;

    [SetUp]
    public void SetUp()
    {
        TrackingManager.ResetForTests();
        _backend = new FakeBackend();
        TrackingManager.RegisterBackend(_backend);
        _fakeNow = 0d;
        LevelTracker.NowProvider = () => _fakeNow;
    }

    [TearDown]
    public void TearDown()
    {
        TrackingManager.ResetForTests();
        LevelTracker.NowProvider = null;
    }

    [Test]
    public void Start_LogsLevelStart()
    {
        LevelTracker.Start("lv1");

        Assert.AreEqual("level_start", _backend.LoggedEvents[0].name);
        Assert.AreEqual("lv1", _backend.LoggedEvents[0].parameters["level"]);
    }

    [Test]
    public void Passed_LogsLevelPassedWithElapsedTimePlayed()
    {
        LevelTracker.Start("lv1");
        _fakeNow = 12.5d;

        LevelTracker.Passed("lv1");

        var (name, parameters) = _backend.LoggedEvents[1];
        Assert.AreEqual("level_passed", name);
        Assert.AreEqual("lv1", parameters["level"]);
        Assert.AreEqual(12.5d, (double)parameters["time_played"], 0.0001);
    }

    [Test]
    public void Failed_LogsLevelFailedWithElapsedTimePlayed()
    {
        LevelTracker.Start("lv2");
        _fakeNow = 3.0d;

        LevelTracker.Failed("lv2");

        var (name, parameters) = _backend.LoggedEvents[1];
        Assert.AreEqual("level_failed", name);
        Assert.AreEqual(3.0d, (double)parameters["time_played"], 0.0001);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `Unity Test Runner > EditMode > Run All`
Expected: FAIL — `LevelTracker` does not exist.

- [ ] **Step 3: Write the implementation**

Create `Core/Runtime/LevelTracker.cs`:

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

namespace IntegrationSDK.Core
{
    public static class LevelTracker
    {
        private static readonly Dictionary<string, double> _startTimes = new Dictionary<string, double>();

        public static Func<double> NowProvider = () => Time.realtimeSinceStartupAsDouble;

        public static void Start(string level)
        {
            _startTimes[level] = NowProvider();
            TrackingManager.LogEvent("level_start", new Dictionary<string, object>
            {
                { "level", level }
            });
        }

        public static void Passed(string level) => LogEnd(level, "level_passed");

        public static void Failed(string level) => LogEnd(level, "level_failed");

        private static void LogEnd(string level, string eventName)
        {
            var timePlayed = 0d;
            if (_startTimes.TryGetValue(level, out var startTime))
            {
                timePlayed = NowProvider() - startTime;
                _startTimes.Remove(level);
            }

            TrackingManager.LogEvent(eventName, new Dictionary<string, object>
            {
                { "level", level },
                { "time_played", timePlayed }
            });
        }
    }
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `Unity Test Runner > EditMode > Run All`
Expected: PASS (all `LevelTrackerTests`).

- [ ] **Step 5: Commit**

```bash
git add Core/Runtime/LevelTracker.cs Core/Tests/Editor/LevelTrackerTests.cs
git commit -m "Add LevelTracker with injectable time provider for level_start/passed/failed"
```

---

### Task 6: Preloaded Assets helper (config without `Resources`)

**Files:**
- Create: `Core/Editor/PreloadedAssetsHelper.cs`
- Test: `Core/Tests/Editor/PreloadedAssetsHelperTests.cs`

**Interfaces:**
- Produces: `IntegrationSDK.Core.Editor.PreloadedAssetsHelper` static class with
  `static UnityEngine.Object[] AddAssetToList(UnityEngine.Object[] existing, UnityEngine.Object asset)`
  (pure, dedups by reference, ignores `null` asset) and
  `static void RegisterInPlayerSettings(UnityEngine.Object asset)` (thin wrapper
  calling `PlayerSettings.GetPreloadedAssets()` /
  `PlayerSettings.SetPreloadedAssets()` with the pure function above).

- [ ] **Step 1: Write the failing test**

Create `Core/Tests/Editor/PreloadedAssetsHelperTests.cs`:

```csharp
using IntegrationSDK.Core.Editor;
using NUnit.Framework;
using UnityEngine;

public class PreloadedAssetsHelperTests
{
    [Test]
    public void AddAssetToList_AppendsAsset_WhenListEmpty()
    {
        var asset = ScriptableObject.CreateInstance<IntegrationSDK.Core.IntegrationSDKConfig>();

        var result = PreloadedAssetsHelper.AddAssetToList(new Object[0], asset);

        Assert.AreEqual(1, result.Length);
        Assert.AreSame(asset, result[0]);
    }

    [Test]
    public void AddAssetToList_DoesNotDuplicate_WhenAssetAlreadyPresent()
    {
        var asset = ScriptableObject.CreateInstance<IntegrationSDK.Core.IntegrationSDKConfig>();
        var existing = new Object[] { asset };

        var result = PreloadedAssetsHelper.AddAssetToList(existing, asset);

        Assert.AreEqual(1, result.Length);
    }

    [Test]
    public void AddAssetToList_PreservesOtherEntries()
    {
        var other = ScriptableObject.CreateInstance<IntegrationSDK.Core.IntegrationSDKConfig>();
        var asset = ScriptableObject.CreateInstance<IntegrationSDK.Core.IntegrationSDKConfig>();

        var result = PreloadedAssetsHelper.AddAssetToList(new Object[] { other }, asset);

        Assert.AreEqual(2, result.Length);
        CollectionAssert.Contains(result, other);
        CollectionAssert.Contains(result, asset);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `Unity Test Runner > EditMode > Run All`
Expected: FAIL — `PreloadedAssetsHelper` does not exist.

- [ ] **Step 3: Write the implementation**

Create `Core/Editor/PreloadedAssetsHelper.cs`:

```csharp
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace IntegrationSDK.Core.Editor
{
    public static class PreloadedAssetsHelper
    {
        public static Object[] AddAssetToList(Object[] existing, Object asset)
        {
            if (asset == null)
            {
                return existing;
            }

            if (existing.Any(o => o == asset))
            {
                return existing;
            }

            var result = new Object[existing.Length + 1];
            existing.CopyTo(result, 0);
            result[existing.Length] = asset;
            return result;
        }

        public static void RegisterInPlayerSettings(Object asset)
        {
            var updated = AddAssetToList(PlayerSettings.GetPreloadedAssets(), asset);
            PlayerSettings.SetPreloadedAssets(updated);
        }
    }
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `Unity Test Runner > EditMode > Run All`
Expected: PASS (all `PreloadedAssetsHelperTests`).

- [ ] **Step 5: Commit**

```bash
git add Core/Editor/PreloadedAssetsHelper.cs Core/Tests/Editor/PreloadedAssetsHelperTests.cs
git commit -m "Add PreloadedAssetsHelper so config assets ship without living in Resources"
```

---

### Task 7: Config validator + build guard

**Files:**
- Create: `Core/Editor/IntegrationSDKConfigValidator.cs`
- Create: `Core/Editor/IntegrationSDKBuildValidator.cs`
- Test: `Core/Tests/Editor/IntegrationSDKConfigValidatorTests.cs`

**Interfaces:**
- Consumes: `IntegrationSDKConfig` (Task 2).
- Produces: `IntegrationSDK.Core.Editor.IntegrationSDKConfigValidator` static class
  with `static List<string> Validate(IntegrationSDKConfig config)` (pure — returns
  human-readable error strings, empty list if valid). `IntegrationSDKBuildValidator`
  implements `IPreprocessBuildWithReport`, calls `Validate` on the found config and
  throws `BuildFailedException` if errors exist (not unit tested — thin wiring
  around already-tested `Validate`).

- [ ] **Step 1: Write the failing test**

Create `Core/Tests/Editor/IntegrationSDKConfigValidatorTests.cs`:

```csharp
using IntegrationSDK.Core;
using IntegrationSDK.Core.Editor;
using NUnit.Framework;
using UnityEngine;

public class IntegrationSDKConfigValidatorTests
{
    [Test]
    public void Validate_ReturnsNoErrors_ForFullyConfiguredConfig()
    {
        var config = ScriptableObject.CreateInstance<IntegrationSDKConfig>();
        config.maxSdkKey = "sdk-key";
        config.androidInterstitialAdUnitId = "and-int";
        config.iosInterstitialAdUnitId = "ios-int";
        config.androidRewardedAdUnitId = "and-rew";
        config.iosRewardedAdUnitId = "ios-rew";
        config.androidAppOpenAdUnitId = "and-aoa";
        config.iosAppOpenAdUnitId = "ios-aoa";

        var errors = IntegrationSDKConfigValidator.Validate(config);

        Assert.IsEmpty(errors);
    }

    [Test]
    public void Validate_ReturnsError_WhenMaxSdkKeyMissing()
    {
        var config = ScriptableObject.CreateInstance<IntegrationSDKConfig>();

        var errors = IntegrationSDKConfigValidator.Validate(config);

        CollectionAssert.Contains(errors, "AppLovin MAX SDK Key is missing.");
    }

    [Test]
    public void Validate_ReturnsError_WhenAppsFlyerEnabledButDevKeyMissing()
    {
        var config = ScriptableObject.CreateInstance<IntegrationSDKConfig>();
        config.maxSdkKey = "sdk-key";
        config.androidInterstitialAdUnitId = "x";
        config.iosInterstitialAdUnitId = "x";
        config.androidRewardedAdUnitId = "x";
        config.iosRewardedAdUnitId = "x";
        config.androidAppOpenAdUnitId = "x";
        config.iosAppOpenAdUnitId = "x";
        config.appsFlyerEnabled = true;

        var errors = IntegrationSDKConfigValidator.Validate(config);

        CollectionAssert.Contains(errors, "AppsFlyer is enabled but Dev Key is missing.");
    }

    [Test]
    public void Validate_ReturnsNull_ConfigError_WhenConfigMissing()
    {
        var errors = IntegrationSDKConfigValidator.Validate(null);

        CollectionAssert.Contains(errors, "IntegrationSDKConfig asset not found. Open Tools > Integration SDK > Settings to create one.");
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `Unity Test Runner > EditMode > Run All`
Expected: FAIL — `IntegrationSDKConfigValidator` does not exist.

- [ ] **Step 3: Write the implementation**

Create `Core/Editor/IntegrationSDKConfigValidator.cs`:

```csharp
using System.Collections.Generic;

namespace IntegrationSDK.Core.Editor
{
    public static class IntegrationSDKConfigValidator
    {
        public static List<string> Validate(IntegrationSDKConfig config)
        {
            var errors = new List<string>();

            if (config == null)
            {
                errors.Add("IntegrationSDKConfig asset not found. Open Tools > Integration SDK > Settings to create one.");
                return errors;
            }

            if (string.IsNullOrEmpty(config.maxSdkKey))
            {
                errors.Add("AppLovin MAX SDK Key is missing.");
            }

            if (string.IsNullOrEmpty(config.androidInterstitialAdUnitId) || string.IsNullOrEmpty(config.iosInterstitialAdUnitId))
            {
                errors.Add("Interstitial Ad Unit ID is missing for Android and/or iOS.");
            }

            if (string.IsNullOrEmpty(config.androidRewardedAdUnitId) || string.IsNullOrEmpty(config.iosRewardedAdUnitId))
            {
                errors.Add("Rewarded Ad Unit ID is missing for Android and/or iOS.");
            }

            if (string.IsNullOrEmpty(config.androidAppOpenAdUnitId) || string.IsNullOrEmpty(config.iosAppOpenAdUnitId))
            {
                errors.Add("App Open Ad Unit ID is missing for Android and/or iOS.");
            }

            if (config.appsFlyerEnabled && string.IsNullOrEmpty(config.appsFlyerDevKey))
            {
                errors.Add("AppsFlyer is enabled but Dev Key is missing.");
            }

            return errors;
        }
    }
}
```

Create `Core/Editor/IntegrationSDKBuildValidator.cs`:

```csharp
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace IntegrationSDK.Core.Editor
{
    public class IntegrationSDKBuildValidator : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            var config = Resources.FindObjectsOfTypeAll<IntegrationSDKConfig>().FirstOrDefault();
            var errors = IntegrationSDKConfigValidator.Validate(config);

            if (errors.Count > 0)
            {
                throw new BuildFailedException("IntegrationSDK config errors:\n- " + string.Join("\n- ", errors));
            }
        }
    }
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `Unity Test Runner > EditMode > Run All`
Expected: PASS (all `IntegrationSDKConfigValidatorTests`).

- [ ] **Step 5: Commit**

```bash
git add Core/Editor/IntegrationSDKConfigValidator.cs Core/Editor/IntegrationSDKBuildValidator.cs Core/Tests/Editor/IntegrationSDKConfigValidatorTests.cs
git commit -m "Add config validator and build-time guard against missing keys"
```

---

### Task 8: Settings window (menu bar UI)

**Files:**
- Create: `Core/Editor/IntegrationSDKSettingsWindow.cs`

**Interfaces:**
- Consumes: `IntegrationSDKConfig` (Task 2),
  `PreloadedAssetsHelper.RegisterInPlayerSettings` (Task 6).
- Produces: menu item `Tools > Integration SDK > Settings` opening an
  `EditorWindow` that creates/loads the config asset and saves + registers it.

This task has no automated test — Unity `EditorWindow` GUI code is not
meaningfully unit-testable, and the two pieces of logic it depends on
(`PreloadedAssetsHelper`, `IntegrationSDKConfigValidator`) are already covered
by Tasks 6 and 7. Verification is manual, listed in Step 3.

- [ ] **Step 1: Write the settings window**

Create `Core/Editor/IntegrationSDKSettingsWindow.cs`:

```csharp
using UnityEditor;
using UnityEngine;

namespace IntegrationSDK.Core.Editor
{
    public class IntegrationSDKSettingsWindow : EditorWindow
    {
        private const string ConfigPathPrefKey = "IntegrationSDK.ConfigAssetPath";

        private IntegrationSDKConfig _config;
        private SerializedObject _serializedConfig;

        [MenuItem("Tools/Integration SDK/Settings")]
        public static void Open()
        {
            GetWindow<IntegrationSDKSettingsWindow>("Integration SDK").LoadOrCreateConfig();
        }

        private void OnEnable() => LoadOrCreateConfig();

        private void LoadOrCreateConfig()
        {
            var savedPath = EditorPrefs.GetString(ConfigPathPrefKey, string.Empty);
            _config = !string.IsNullOrEmpty(savedPath)
                ? AssetDatabase.LoadAssetAtPath<IntegrationSDKConfig>(savedPath)
                : null;

            if (_config == null)
            {
                var chosenPath = EditorUtility.SaveFilePanelInProject(
                    "Create Integration SDK Config",
                    "IntegrationSDKConfig",
                    "asset",
                    "Choose where to save the Integration SDK config asset.");

                if (string.IsNullOrEmpty(chosenPath))
                {
                    return;
                }

                _config = CreateInstance<IntegrationSDKConfig>();
                AssetDatabase.CreateAsset(_config, chosenPath);
                AssetDatabase.SaveAssets();
                EditorPrefs.SetString(ConfigPathPrefKey, chosenPath);
            }

            _serializedConfig = new SerializedObject(_config);
        }

        private void OnGUI()
        {
            if (_config == null || _serializedConfig == null)
            {
                EditorGUILayout.HelpBox("No config loaded.", MessageType.Warning);
                return;
            }

            _serializedConfig.Update();

            var iterator = _serializedConfig.GetIterator();
            iterator.NextVisible(true);
            while (iterator.NextVisible(false))
            {
                EditorGUILayout.PropertyField(iterator, true);
            }

            _serializedConfig.ApplyModifiedProperties();

            var errors = IntegrationSDKConfigValidator.Validate(_config);
            foreach (var error in errors)
            {
                EditorGUILayout.HelpBox(error, MessageType.Warning);
            }

            if (GUILayout.Button("Save"))
            {
                EditorUtility.SetDirty(_config);
                AssetDatabase.SaveAssets();
                PreloadedAssetsHelper.RegisterInPlayerSettings(_config);
            }
        }
    }
}
```

- [ ] **Step 2: Manually verify in the Editor**

1. Open Unity Editor on this project.
2. `Tools > Integration SDK > Settings` — confirm a save-file dialog appears the
   first time, and the asset gets created at the chosen path.
3. Fill in `maxSdkKey` and all Ad Unit ID fields, click **Save**.
4. Confirm no warnings remain in the window.
5. Open `Edit > Project Settings > Player > Preloaded Assets` and confirm the
   config asset is listed exactly once.
6. Close and reopen the window — confirm it loads the same asset (no duplicate
   save dialog).

- [ ] **Step 3: Commit**

```bash
git add Core/Editor/IntegrationSDKSettingsWindow.cs
git commit -m "Add Integration SDK Settings window under Tools menu"
```

---

### Task 9: `Bootstrapper`

**Files:**
- Create: `Core/Runtime/Bootstrapper.cs`

**Interfaces:**
- Consumes: `IntegrationSDKConfig` (Task 2), `AdManager.Initialize` (Task 4),
  `TrackingManager.InitializeAll` (Task 3).
- Produces: automatic startup — no public API for game code to call.

No automated test: this method's entire body is Unity lifecycle glue
(`RuntimeInitializeOnLoadMethod` + `Resources.FindObjectsOfTypeAll`) around
already-tested `AdManager`/`TrackingManager` calls; Unity does not provide a
way to invoke `RuntimeInitializeOnLoadMethod` hooks from an EditMode test.
Verified manually in Task 11 once a real ad provider exists end-to-end.

- [ ] **Step 1: Write the implementation**

Create `Core/Runtime/Bootstrapper.cs`:

```csharp
using System.Linq;
using UnityEngine;

namespace IntegrationSDK.Core
{
    public static class Bootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            var config = Resources.FindObjectsOfTypeAll<IntegrationSDKConfig>().FirstOrDefault();

            if (config == null)
            {
                Debug.LogWarning("IntegrationSDK: no IntegrationSDKConfig found. Open Tools > Integration SDK > Settings to create one.");
                return;
            }

            AdManager.Initialize(config);
            TrackingManager.InitializeAll(config);
        }
    }
}
```

- [ ] **Step 2: Commit**

```bash
git add Core/Runtime/Bootstrapper.cs
git commit -m "Add Bootstrapper that auto-initializes ad provider and tracking backends on startup"
```

---

### Task 10: `Ads.AppLovin` package + `MaxAdProvider`

**Files:**
- Create: `Ads.AppLovin/package.json`
- Create: `Ads.AppLovin/Runtime/IntegrationSDK.Ads.AppLovin.Runtime.asmdef`
- Create: `Ads.AppLovin/Runtime/MaxAdProvider.cs`
- Create: `Ads.AppLovin/Runtime/MaxAdProviderBootstrap.cs`
- Create: `Ads.AppLovin/README.md`
- Modify: `Packages/manifest.json` (local dev dependency)

**Interfaces:**
- Consumes: `IntegrationSDK.Core.IAdProvider`, `IntegrationSDK.Core.AdManager`,
  `IntegrationSDK.Core.TrackingManager`, `IntegrationSDK.Core.AdRevenueData`,
  `IntegrationSDK.Core.IntegrationSDKConfig` (all from Core).
- Produces: `IntegrationSDK.Ads.AppLovin.MaxAdProvider : IAdProvider`,
  self-registered into `AdManager` via `RuntimeInitializeOnLoadMethod`.

This task requires the real AppLovin MAX Unity Plugin to compile
(`MaxSdk`, `MaxSdkCallbacks` types). Installing it is a manual, documented
one-time step (see Step 1) — not something a git-URL package dependency can
do reliably today (AppLovin's own Integration Manager handles the
registries/adapters). Because of that, this task's code cannot be
build-verified in this environment without actually running the Integration
Manager first; Step 5 documents the exact manual verification the engineer
must run once MAX is installed.

- [ ] **Step 1: Install the real AppLovin MAX SDK in this dev project (manual, one-time)**

1. Download the latest `AppLovinMAX-Unity-Plugin-x.y.z.unitypackage` from
   https://github.com/AppLovin/AppLovin-MAX-Unity-Plugin/releases and import it
   into this project.
2. Open `AppLovin > Integration Manager`, enter the AppLovin SDK key for this
   project, and click **Upgrade All Adapters and Migrate to UPM** (this adds the
   required scoped registry + adapter packages to `Packages/manifest.json`
   automatically).
3. Confirm `MaxSdk` and `MaxSdkCallbacks` types are available (e.g. Unity
   auto-completes `MaxSdk.` in a scratch script) before continuing.

- [ ] **Step 2: Scaffold the package**

Create `Ads.AppLovin/package.json`:

```json
{
  "name": "com.gemmob.integrationsdk.ads.applovin",
  "version": "0.1.0",
  "displayName": "Integration SDK - AppLovin MAX Ads",
  "description": "AppLovin MAX IAdProvider implementation for IntegrationSDK. Requires the AppLovin MAX Unity Plugin to be installed separately via AppLovin's Integration Manager.",
  "unity": "2021.3",
  "dependencies": {
    "com.gemmob.integrationsdk.core": "0.1.0"
  }
}
```

Create `Ads.AppLovin/Runtime/IntegrationSDK.Ads.AppLovin.Runtime.asmdef`:

```json
{
    "name": "IntegrationSDK.Ads.AppLovin.Runtime",
    "rootNamespace": "IntegrationSDK.Ads.AppLovin",
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
"com.gemmob.integrationsdk.ads.applovin": "file:../Ads.AppLovin"
```

- [ ] **Step 3: Implement `MaxAdProvider`**

Create `Ads.AppLovin/Runtime/MaxAdProvider.cs`:

```csharp
using System;
using System.Collections.Generic;
using IntegrationSDK.Core;
using UnityEngine;

namespace IntegrationSDK.Ads.AppLovin
{
    public class MaxAdProvider : IAdProvider
    {
        private IntegrationSDKConfig _config;
        private Action _pendingReward;

        public void Initialize(IntegrationSDKConfig config)
        {
            _config = config;

            MaxSdkCallbacks.OnSdkInitializedEvent += _ => LoadAllAds();

            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += (adUnitId, adInfo) => LogShowSuccess("show_interstitial_ads_suscces", adInfo);
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent += (adUnitId, adInfo) => LogShowSuccess("show_interstitial_ads_click", adInfo);
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += (adUnitId, adInfo) => MaxSdk.LoadInterstitial(_config.GetInterstitialAdUnitId());
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += (adUnitId, adInfo) => LogAdImpression(adInfo, "interstitial");

            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += (adUnitId, adInfo) => LogShowSuccess("show_rewarded_ads_sucess", adInfo);
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent += (adUnitId, adInfo) => LogShowSuccess("show_rewarded_ads_click", adInfo);
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += (adUnitId, reward, adInfo) => _pendingReward?.Invoke();
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += (adUnitId, adInfo) => MaxSdk.LoadRewardedAd(_config.GetRewardedAdUnitId());
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += (adUnitId, adInfo) => LogAdImpression(adInfo, "rewarded");

            MaxSdkCallbacks.AppOpen.OnAdDisplayedEvent += (adUnitId, adInfo) => LogShowSuccess("show_aoa_ads_sucess", adInfo);
            MaxSdkCallbacks.AppOpen.OnAdClickedEvent += (adUnitId, adInfo) => LogShowSuccess("show_aoa_ads_click", adInfo);
            MaxSdkCallbacks.AppOpen.OnAdHiddenEvent += (adUnitId, adInfo) => MaxSdk.LoadAppOpenAd(_config.GetAppOpenAdUnitId());
            MaxSdkCallbacks.AppOpen.OnAdRevenuePaidEvent += (adUnitId, adInfo) => LogAdImpression(adInfo, "app_open");

            MaxSdk.SetSdkKey(config.maxSdkKey);
            MaxSdk.InitializeSdk();
        }

        public void ShowInterstitial(string placement)
        {
            TrackingManager.LogEvent("show_interstitial_ads", new Dictionary<string, object> { { "placement", placement } });
            if (MaxSdk.IsInterstitialReady(_config.GetInterstitialAdUnitId()))
            {
                MaxSdk.ShowInterstitial(_config.GetInterstitialAdUnitId(), placement);
            }
        }

        public void ShowRewarded(string placement, Action onRewardEarned)
        {
            _pendingReward = onRewardEarned;
            TrackingManager.LogEvent("show_rewarded_ads", new Dictionary<string, object> { { "placement", placement } });
            if (MaxSdk.IsRewardedAdReady(_config.GetRewardedAdUnitId()))
            {
                MaxSdk.ShowRewardedAd(_config.GetRewardedAdUnitId(), placement);
            }
        }

        public void ShowAppOpen(string placement)
        {
            TrackingManager.LogEvent("show_aoa_ads", new Dictionary<string, object> { { "placement", placement } });
            if (MaxSdk.IsAppOpenAdReady(_config.GetAppOpenAdUnitId()))
            {
                MaxSdk.ShowAppOpenAd(_config.GetAppOpenAdUnitId(), placement);
            }
        }

        private void LoadAllAds()
        {
            MaxSdk.LoadInterstitial(_config.GetInterstitialAdUnitId());
            MaxSdk.LoadRewardedAd(_config.GetRewardedAdUnitId());
            MaxSdk.LoadAppOpenAd(_config.GetAppOpenAdUnitId());
        }

        private void LogShowSuccess(string eventName, MaxSdk.AdInfo adInfo)
        {
            TrackingManager.LogEvent(eventName, new Dictionary<string, object> { { "placement", adInfo.Placement } });
        }

        private void LogAdImpression(MaxSdk.AdInfo adInfo, string adFormat)
        {
            TrackingManager.LogAdRevenue(new AdRevenueData
            {
                AdPlatform = "applovin_max",
                AdSource = adInfo.NetworkName,
                AdUnitName = adInfo.AdUnitIdentifier,
                Currency = "USD",
                Value = adInfo.Revenue,
                Placement = adInfo.Placement,
                CountryCode = MaxSdk.GetSdkConfiguration()?.CountryCode,
                AdFormat = adFormat
            });
        }
    }
}
```

Create `Ads.AppLovin/Runtime/MaxAdProviderBootstrap.cs`:

```csharp
using IntegrationSDK.Core;
using UnityEngine;

namespace IntegrationSDK.Ads.AppLovin
{
    internal static class MaxAdProviderBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            AdManager.RegisterProvider(new MaxAdProvider());
        }
    }
}
```

- [ ] **Step 4: Write the README**

Create `Ads.AppLovin/README.md`:

```markdown
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
```
```

- [ ] **Step 5: Manually verify (requires Step 1 completed)**

1. Fill in AppLovin test Ad Unit IDs in the Settings window (Task 8) and enter
   Play Mode.
2. Confirm the console shows MAX SDK initialization succeed with no
   `IntegrationSDK` warnings about a missing provider.
3. Call `AdManager.ShowInterstitial("test_placement")` from a test script or
   the Inspector debug button; confirm `show_interstitial_ads` (and, once a
   tracking module from Task 11/12 is installed, `..._suscces`/`ad_impression`)
   appear in the console/logging you wire up for manual inspection.

- [ ] **Step 6: Commit**

```bash
git add Ads.AppLovin Packages/manifest.json
git commit -m "Add Ads.AppLovin package with MaxAdProvider self-registering IAdProvider"
```

---

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

### Task 12: `Tracking.Firebase` package + `FirebaseBackend`

**Files:**
- Create: `Tracking.Firebase/package.json`
- Create: `Tracking.Firebase/Runtime/IntegrationSDK.Tracking.Firebase.Runtime.asmdef`
- Create: `Tracking.Firebase/Runtime/FirebaseBackend.cs`
- Create: `Tracking.Firebase/Runtime/FirebaseBackendBootstrap.cs`
- Create: `Tracking.Firebase/README.md`

**Interfaces:**
- Consumes: `IntegrationSDK.Core.ITrackingBackend`, `TrackingManager`,
  `AdRevenueData`, `IntegrationSDKConfig` (all from Core).
- Produces: `IntegrationSDK.Tracking.Firebase.FirebaseBackend : ITrackingBackend`,
  self-registered into `TrackingManager`.

Firebase Analytics has no reliable plain git-URL UPM dependency (it ships via
its own scoped registry / `.tgz` files per Firebase's official Unity setup
docs) — this package therefore depends on Core only; the Firebase SDK itself
is a manual one-time install per consuming project, same as already agreed
for AppLovin MAX in Task 10.

- [ ] **Step 1: Install the real Firebase Analytics SDK in this dev project (manual, one-time)**

1. Follow https://firebase.google.com/docs/unity/setup to add the Firebase
   Unity SDK to this project and import `FirebaseAnalytics.unitypackage` (or
   the equivalent `.tgz` per current Firebase docs).
2. Confirm `Firebase.FirebaseApp` and `Firebase.Analytics.FirebaseAnalytics`
   types are available before continuing.

- [ ] **Step 2: Scaffold the package**

Create `Tracking.Firebase/package.json`:

```json
{
  "name": "com.gemmob.integrationsdk.tracking.firebase",
  "version": "0.1.0",
  "displayName": "Integration SDK - Firebase Tracking",
  "description": "Firebase Analytics ITrackingBackend implementation for IntegrationSDK. Requires the Firebase Unity SDK to be installed separately per Firebase's official setup docs.",
  "unity": "2021.3",
  "dependencies": {
    "com.gemmob.integrationsdk.core": "0.1.0"
  }
}
```

Create `Tracking.Firebase/Runtime/IntegrationSDK.Tracking.Firebase.Runtime.asmdef`:

```json
{
    "name": "IntegrationSDK.Tracking.Firebase.Runtime",
    "rootNamespace": "IntegrationSDK.Tracking.Firebase",
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
"com.gemmob.integrationsdk.tracking.firebase": "file:../Tracking.Firebase"
```

- [ ] **Step 3: Implement `FirebaseBackend`**

Create `Tracking.Firebase/Runtime/FirebaseBackend.cs`:

```csharp
using System.Collections.Generic;
using Firebase.Analytics;
using Firebase.Extensions;
using IntegrationSDK.Core;

namespace IntegrationSDK.Tracking.Firebase
{
    public class FirebaseBackend : ITrackingBackend
    {
        public void Initialize(IntegrationSDKConfig config)
        {
            if (!config.firebaseEnabled)
            {
                return;
            }

            global::Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                if (task.Result == global::Firebase.DependencyStatus.Available)
                {
                    FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                }
            });
        }

        public void LogEvent(string eventName, Dictionary<string, object> parameters)
        {
            var firebaseParameters = new List<Parameter>();
            foreach (var kvp in parameters)
            {
                firebaseParameters.Add(new Parameter(kvp.Key, kvp.Value?.ToString()));
            }

            FirebaseAnalytics.LogEvent(eventName, firebaseParameters.ToArray());
        }

        public void LogAdRevenue(AdRevenueData data)
        {
            FirebaseAnalytics.LogEvent(
                FirebaseAnalytics.EventAdImpression,
                new Parameter(FirebaseAnalytics.ParameterAdPlatform, data.AdPlatform),
                new Parameter(FirebaseAnalytics.ParameterAdSource, data.AdSource),
                new Parameter(FirebaseAnalytics.ParameterAdUnitName, data.AdUnitName),
                new Parameter(FirebaseAnalytics.ParameterCurrency, data.Currency),
                new Parameter(FirebaseAnalytics.ParameterValue, data.Value),
                new Parameter(FirebaseAnalytics.ParameterAdFormat, data.AdFormat));
        }
    }
}
```

Create `Tracking.Firebase/Runtime/FirebaseBackendBootstrap.cs`:

```csharp
using IntegrationSDK.Core;
using UnityEngine;

namespace IntegrationSDK.Tracking.Firebase
{
    internal static class FirebaseBackendBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            TrackingManager.RegisterBackend(new FirebaseBackend());
        }
    }
}
```

- [ ] **Step 4: Write the README**

Create `Tracking.Firebase/README.md`:

```markdown
# Integration SDK - Firebase Tracking

Implements `ITrackingBackend` from `com.gemmob.integrationsdk.core` using
Firebase Analytics. Automatically registers with `TrackingManager` at startup.

## Install

1. Follow https://firebase.google.com/docs/unity/setup to add the Firebase
   Unity SDK + Analytics to your project (Firebase does not ship via a plain
   git-URL UPM dependency, so this is a manual one-time step per project).
2. Add `"com.gemmob.integrationsdk.tracking.firebase": "https://github.com/<org>/IntegrationSDK.git?path=/Tracking.Firebase"`
   to `Packages/manifest.json`.
3. Open `Tools > Integration SDK > Settings`, enable **Firebase**, click
   **Save**.
```

- [ ] **Step 5: Manually verify (requires Step 1 completed)**

1. Enable Firebase in the Settings window.
2. Enter Play Mode; confirm Firebase's own init log line appears and no
   `IntegrationSDK` "no tracking backend" warning appears.
3. Call `TrackingManager.LogEvent("level_start", ...)` from a test script;
   confirm the event appears in the Firebase DebugView console (requires
   `adb shell setprop debug.firebase.analytics.app <bundle id>` on Android, per
   Firebase's own debug instructions).

- [ ] **Step 6: Commit**

```bash
git add Tracking.Firebase Packages/manifest.json
git commit -m "Add Tracking.Firebase package with self-registering ITrackingBackend"
```

---

## Self-Review Notes

- **Spec coverage:** Core config/settings window/build validation → Tasks 2, 6,
  7, 8. Auto-init on startup → Task 9. Auto-wire ad revenue → `ad_impression` →
  Task 10 (`LogAdImpression`). Show/success/click events per format → Task 10.
  `level_start/passed/failed` with `time_played` → Task 5. AppsFlyer/Firebase
  fan-out toggle → Tasks 3, 11, 12. Module add/remove independence → Tasks 1,
  10, 11, 12 each being a separate package with one-way dependency on Core.
- **Consent/GDPR/ATT automation** from the spec is delegated to
  `MaxSdk.InitializeSdk()` in Task 10, which AppLovin's own SDK handles
  natively (CMP/UMP + ATT) once the AppLovin dashboard is configured — this
  is called out in Task 10's Step 1/README rather than reimplemented, since
  building a second consent flow on top of MAX's built-in one would conflict
  with it.
- **No placeholders:** every task has concrete file paths and complete code;
  the two SDKs without a reliable UPM git line (AppLovin MAX, Firebase) are
  handled as explicit documented manual steps, not TBDs.
- **Type consistency checked:** `IAdProvider`/`ITrackingBackend` signatures in
  Tasks 3/4 match usage in Tasks 5, 9, 10, 11, 12. `AdRevenueData` field names
  match between Task 3's definition and Tasks 10/11/12's construction sites.
