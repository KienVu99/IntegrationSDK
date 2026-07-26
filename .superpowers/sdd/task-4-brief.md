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

