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

