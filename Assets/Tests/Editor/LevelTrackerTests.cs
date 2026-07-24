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
