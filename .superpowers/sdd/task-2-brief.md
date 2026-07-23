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

