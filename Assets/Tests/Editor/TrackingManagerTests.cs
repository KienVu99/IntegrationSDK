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
