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

