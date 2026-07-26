# Task 11 Report: Tracking.AppsFlyer package + AppsFlyerBackend

## Status: DONE_WITH_CONCERNS (blocked on Unity verification step only)

## What was done

1. Created `Tracking.AppsFlyer/package.json` with the corrected AppsFlyer git
   URL (`appsflyer-unity-plugin.git#upm`, not the brief's typo'd
   `appsflyer-plugin.git#upm`).
2. Created `Tracking.AppsFlyer/Runtime/IntegrationSDK.Tracking.AppsFlyer.Runtime.asmdef`
   per the brief, **plus one addition beyond the brief**: added an `"AppsFlyer"`
   assembly reference (in addition to `IntegrationSDK.Core.Runtime`). The
   brief's asmdef only referenced Core, but `AppsFlyerBackend.cs` uses
   `AppsFlyer`, `AFAdRevenueData`, and `MediationNetwork` types that live in
   the AppsFlyer plugin's own `AppsFlyer.asmdef` (confirmed at
   `Library/PackageCache/appsflyer-unity-plugin@d26fcb5d8c/AppsFlyer.asmdef`,
   name `"AppsFlyer"`). Without this reference the code would not compile
   (Unity assembly definitions require explicit references between asmdef-based
   assemblies).
3. Added `"com.gemmob.integrationsdk.tracking.appsflyer": "file:../Tracking.AppsFlyer"`
   to `Packages/manifest.json`, alongside (not replacing) the existing
   `appsflyer-unity-plugin` / `com.google.external-dependency-manager` entries.
4. Created `Tracking.AppsFlyer/Runtime/AppsFlyerBackend.cs` and
   `AppsFlyerBackendBootstrap.cs` using the exact code from the brief
   (verbatim - no typo issue in the C#).
5. Created `Tracking.AppsFlyer/README.md` per the brief, with the corrected
   URL in the install snippet.

## URL typo correction

Per the task instructions, the brief's package.json/README snippets used
`https://github.com/AppsFlyerSDK/appsflyer-plugin.git#upm` (wrong repo name).
All occurrences were written using the corrected, already-verified-resolving
URL: `https://github.com/AppsFlyerSDK/appsflyer-unity-plugin.git#upm`. This
matches what's already in the root `Packages/manifest.json` (untouched by
this task except for the one new line added).

## Blocker: could not run batch-mode Unity verification

Per the task's explicit safety instruction, before running Unity in batch
mode I checked for a running GUI instance:

```
tasklist //FI "IMAGENAME eq Unity.exe"
```

This found two live Unity.exe processes, one of which is this exact
project:

```
"C:\Program Files\Unity\Hub\Editor\2022.3.62f2\Editor\Unity.exe"
  -projectpath D:\Unity\2026\IntegrationSDK\IntegrationSDK ...
```

(the other is an unrelated GUI instance for a different project,
`D:\Unity\2026\GameVibe\Solitaire`).

Because a GUI Editor instance already has `D:\Unity\2026\IntegrationSDK\IntegrationSDK`
open, launching a second batch-mode Unity process against the same project
would very likely fail with a lock-file conflict ("another Unity instance is
running") or race with the open Editor. Per the explicit instruction in this
task, I stopped here rather than kill the user's Unity process or risk a
conflicting concurrent launch.

No compile-check log and no EditMode test run were produced in this
session. Steps 5 and 6 (compile check, EditMode test suite) are outstanding
and require the human to either:
- close the open Unity Editor instance for this project so batch mode can
  run, or
- let the already-open Editor recompile and report back whether the
  Console shows 0 errors / test results.

Side evidence found while investigating: `Packages/packages-lock.json` has
uncommitted changes reflecting AppsFlyer + EDM4U resolution (pre-existing,
made by the user per task context), confirming the open Editor instance had
already resolved the AppsFlyer plugin package successfully in this project
before this task started. That diff is unrelated to the new
`Tracking.AppsFlyer` package (which the open Editor has not yet re-resolved
against, since the manifest.json edit happened after/concurrent with the
running Editor session).

## Files not committed

Per the task's instruction to only commit after successful verification, and
since verification (steps 5-6) could not be run, no commit was made. Files
are left as untracked/modified working tree so the human can inspect, and
either:
- close the GUI Unity instance and ask for a re-run to finish verification
  and commit, or
- verify manually in the already-open Editor and give the go-ahead to commit
  as-is.

Current `git status --porcelain` (relevant lines):

```
 M Packages/manifest.json
 M Packages/packages-lock.json
?? Tracking.AppsFlyer/
```

(`.superpowers/sdd/.gitignore`, `.superpowers/sdd/progress.md`,
`ProjectSettings/ProjectSettings.asset`, and
`ProjectSettings/GvhProjectSettings.xml` are pre-existing/unrelated changes
not touched by this task.)

## Outstanding manual steps (brief's Step 4, same pattern as prior tasks)

1. Confirm 0 `error CS` in the Unity Console after this Editor recompiles
   with the new `Tracking.AppsFlyer` package.
2. Enable AppsFlyer + fill Dev Key/iOS App ID in
   `Tools > Integration SDK > Settings`.
3. Enter Play Mode; confirm no "no tracking backend" warning and that
   AppsFlyer's own SDK init log line appears (requires human interaction,
   not headlessly automatable).

## Next steps once Unity is free

1. Close (or otherwise free) the GUI Unity instance on this project.
2. Run: `Unity.exe -batchmode -quit -projectPath "D:\Unity\2026\IntegrationSDK\IntegrationSDK" -logFile <path>` and confirm 0 `error CS`.
3. Run: `Unity.exe -batchmode -runTests -testPlatform EditMode -testResults <path>.xml -logFile -` and confirm the existing 22 Core tests still pass.
4. `git add Tracking.AppsFlyer Packages/manifest.json && git commit -m "Add Tracking.AppsFlyer package with self-registering ITrackingBackend"`.
