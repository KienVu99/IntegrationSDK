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

