# Task 9 Report: Bootstrapper

## What was done
Created `Core/Runtime/Bootstrapper.cs` with the exact code specified in the brief:
a static `Bootstrapper` class with a `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]`
method that finds the `IntegrationSDKConfig` asset via `Resources.FindObjectsOfTypeAll<IntegrationSDKConfig>().FirstOrDefault()`,
logs a warning and no-ops if none is found, otherwise calls `AdManager.Initialize(config)` and
`TrackingManager.InitializeAll(config)`.

No automated test was added, per the brief — Unity does not provide a way to invoke
`RuntimeInitializeOnLoadMethod` hooks from an EditMode test. This will be verified manually in Task 11.

## Compile check
Ran Unity in batch mode without `-runTests`:
```
Unity.exe -batchmode -quit -projectPath "D:\Unity\2026\IntegrationSDK\IntegrationSDK" -logFile compile-check.log
EXIT_CODE=0
```
`grep -n "error CS" compile-check.log` returned no matches — clean compile.

## EditMode test run
```
Unity.exe -batchmode -runTests -projectPath "D:\Unity\2026\IntegrationSDK\IntegrationSDK" -testPlatform EditMode -testResults test-results.xml -logFile test-run.log
EXIT_CODE=0
```
Result from `test-results.xml`:
```
total="22" passed="22" failed="0"
```
All 22 existing tests still pass, no regressions. Task 9 adds zero new tests, so the count is unchanged
from before (22/22).

## Git status / commit
`git status --porcelain` before commit showed:
```
?? Core/Runtime/Bootstrapper.cs
?? Core/Runtime/Bootstrapper.cs.meta
```
(plus unrelated pre-existing modifications to `.superpowers/sdd/.gitignore`, `.superpowers/sdd/progress.md`,
and `ProjectSettings/ProjectSettings.asset`, which were left untouched/uncommitted as they are outside this task's scope).

Committed both the new script and its `.meta` file:
```
git add Core/Runtime/Bootstrapper.cs Core/Runtime/Bootstrapper.cs.meta
git commit -m "Add Bootstrapper that auto-initializes ad provider and tracking backends on startup"
```

Commit hash: `fcbe96cead8ebc9342b739a53d04b0a27189d14d`
