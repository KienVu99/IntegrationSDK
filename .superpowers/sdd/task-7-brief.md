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

