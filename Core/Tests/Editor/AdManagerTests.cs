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
