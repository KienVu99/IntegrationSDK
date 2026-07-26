# Task 7 Report: Config validator + build guard

## What was done
- Created `Core/Editor/IntegrationSDKConfigValidator.cs`: static class `IntegrationSDK.Core.Editor.IntegrationSDKConfigValidator` with `static List<string> Validate(IntegrationSDKConfig config)`, exactly per brief.
- Created `Core/Editor/IntegrationSDKBuildValidator.cs`: `IPreprocessBuildWithReport` implementation that finds the config via `Resources.FindObjectsOfTypeAll<IntegrationSDKConfig>()`, calls `Validate`, and throws `BuildFailedException` on errors. No automated test (per brief, thin Unity build-pipeline glue).
- Created `Core/Tests/Editor/IntegrationSDKConfigValidatorTests.cs` with the 4 exact tests specified in the brief (fully configured -> no errors, missing MAX SDK key -> error, AppsFlyer enabled without dev key -> error, null config -> error).
- Followed TDD: wrote test file first, ran EditMode tests (confirmed compile failure because `IntegrationSDKConfigValidator` didn't exist yet), then implemented both classes, then reran tests (all green).
- Added the three new `.cs` files and their Unity-generated `.meta` files to git and committed.

## Test run commands and output

Unity version used (from `ProjectSettings/ProjectVersion.txt`): 2022.3.62f2

**Failing run (before implementation):**
```
"C:\Program Files\Unity\Hub\Editor\2022.3.62f2\Editor\Unity.exe" -batchmode -projectPath "D:/Unity/2026/IntegrationSDK/IntegrationSDK" -runTests -testPlatform EditMode -testResults <results-fail.xml> -logFile -
```
Result: Compile error confirmed (as expected, no `-quit` flag used):
```
Core\Tests\Editor\IntegrationSDKConfigValidatorTests.cs(20,22): error CS0103: The name 'IntegrationSDKConfigValidator' does not exist in the current context
... (repeated for all 4 test methods referencing the type)
Aborting batchmode due to failure: Scripts have compiler errors.
```

**Passing run (after implementation):**
```
"C:\Program Files\Unity\Hub\Editor\2022.3.62f2\Editor\Unity.exe" -batchmode -projectPath "D:/Unity/2026/IntegrationSDK/IntegrationSDK" -runTests -testPlatform EditMode -testResults <results-pass.xml> -logFile -
```
Result (from results XML `test-run` summary):
```
testcasecount="22" result="Passed" total="22" passed="22" failed="0" inconclusive="0" skipped="0"
```
22/22 tests passed — the 4 new `IntegrationSDKConfigValidatorTests` plus no regressions in `AdManagerTests`, `TrackingManagerTests`, `IntegrationSDKConfigTests`, `LevelTrackerTests`, `PreloadedAssetsHelperTests`. This second (passing) run also confirms `IntegrationSDKBuildValidator.cs` compiles cleanly — the run would have aborted on any `error CS` (as seen in the failing run) had there been a compile problem in that file, and it did not.

## Git

`git status --porcelain` before commit showed the 3 new `.cs` files and their `.meta` files as untracked; both were added and committed together.

Commit hash: `12b2a4642ac7afcbbda2301814e7e2084c3d8c61`
Commit message: "Add config validator and build-time guard against missing keys"

## Files
- D:\Unity\2026\IntegrationSDK\IntegrationSDK\Core\Editor\IntegrationSDKConfigValidator.cs
- D:\Unity\2026\IntegrationSDK\IntegrationSDK\Core\Editor\IntegrationSDKBuildValidator.cs
- D:\Unity\2026\IntegrationSDK\IntegrationSDK\Core\Tests\Editor\IntegrationSDKConfigValidatorTests.cs
- Corresponding `.meta` files for all three, tracked in the same commit.

## Concerns
None. Implementation and tests match the brief verbatim.
