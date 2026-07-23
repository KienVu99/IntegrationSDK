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

