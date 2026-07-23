### Task 1: Scaffold the `Core` package

**Files:**
- Create: `Core/package.json`
- Create: `Core/Runtime/IntegrationSDK.Core.Runtime.asmdef`
- Create: `Core/Editor/IntegrationSDK.Core.Editor.asmdef`
- Create: `Core/Tests/Editor/IntegrationSDK.Core.Tests.Editor.asmdef`
- Create: `Core/Runtime/AssemblyInfo.cs`
- Modify: `Packages/manifest.json` (add local file dependency for dev iteration)

**Interfaces:**
- Produces: package `com.gemmob.integrationsdk.core` resolvable in this dev
  project via `file:../Core`; assembly names `IntegrationSDK.Core.Runtime`,
  `IntegrationSDK.Core.Editor`, `IntegrationSDK.Core.Tests.Editor` that later
  tasks add scripts into.

- [ ] **Step 1: Create the package folder and manifest**

Create `Core/package.json`:

```json
{
  "name": "com.gemmob.integrationsdk.core",
  "version": "0.1.0",
  "displayName": "Integration SDK - Core",
  "description": "Dependency-free core: TrackingManager, AdManager, IntegrationSDKConfig. Integration packages (Ads.AppLovin, Tracking.AppsFlyer, Tracking.Firebase) plug into this without Core ever referencing them.",
  "unity": "2021.3",
  "dependencies": {}
}
```

- [ ] **Step 2: Create the Runtime assembly definition**

Create `Core/Runtime/IntegrationSDK.Core.Runtime.asmdef`:

```json
{
    "name": "IntegrationSDK.Core.Runtime",
    "rootNamespace": "IntegrationSDK.Core",
    "references": [],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

- [ ] **Step 3: Create the Editor assembly definition**

Create `Core/Editor/IntegrationSDK.Core.Editor.asmdef`:

```json
{
    "name": "IntegrationSDK.Core.Editor",
    "rootNamespace": "IntegrationSDK.Core.Editor",
    "references": [
        "IntegrationSDK.Core.Runtime"
    ],
    "includePlatforms": [
        "Editor"
    ],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

- [ ] **Step 4: Create the EditMode test assembly definition**

Create `Core/Tests/Editor/IntegrationSDK.Core.Tests.Editor.asmdef`:

```json
{
    "name": "IntegrationSDK.Core.Tests.Editor",
    "rootNamespace": "IntegrationSDK.Core.Tests",
    "references": [
        "IntegrationSDK.Core.Runtime",
        "IntegrationSDK.Core.Editor",
        "UnityEngine.TestRunner",
        "UnityEditor.TestRunner"
    ],
    "includePlatforms": [
        "Editor"
    ],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": true,
    "precompiledReferences": [
        "nunit.framework.dll"
    ],
    "autoReferenced": true,
    "defineConstraints": [
        "UNITY_INCLUDE_TESTS"
    ],
    "versionDefines": [],
    "noEngineReferences": false
}
```

- [ ] **Step 5: Allow the test assembly to see internal members**

Create `Core/Runtime/AssemblyInfo.cs`:

```csharp
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("IntegrationSDK.Core.Tests.Editor")]
```

- [ ] **Step 6: Register the package as a local dependency in this dev project**

Read `Packages/manifest.json`, then add this entry inside `"dependencies"`:

```json
"com.gemmob.integrationsdk.core": "file:../Core"
```

- [ ] **Step 7: Verify it resolves**

Open Unity Editor (or run in batch mode) and confirm no console errors and that
`Window > Package Manager > In Project` lists `Integration SDK - Core`.

Run: `& "C:\Program Files\Unity\Hub\Editor\<version>\Editor\Unity.exe" -batchmode -quit -projectPath "D:\2026\VibeCode\IntegrationSDK" -logFile -`
Expected: log ends without `error CS` or package resolution errors.

- [ ] **Step 8: Commit**

```bash
git add Core/package.json Core/Runtime/IntegrationSDK.Core.Runtime.asmdef Core/Editor/IntegrationSDK.Core.Editor.asmdef Core/Tests/Editor/IntegrationSDK.Core.Tests.Editor.asmdef Core/Runtime/AssemblyInfo.cs Packages/manifest.json
git commit -m "Scaffold Core UPM package with runtime/editor/test assemblies"
```

---

