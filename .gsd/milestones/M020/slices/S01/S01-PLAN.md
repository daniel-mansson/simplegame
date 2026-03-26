# S01: Move IAP, Ads, and ATT Feature Groups

**Goal:** Create `IAP/`, `Ads/`, and `ATT/` feature folders and move all related files from `Services/` and `Popup/` into them using `git mv`.

**Demo:** `Assets/Scripts/Game/IAP/`, `Ads/`, `ATT/` exist with the correct files; those files are no longer in `Services/` or `Popup/`; Unity compiles; tests pass.

## Must-Haves

- `Assets/Scripts/Game/IAP/` contains exactly 15 files (14 `.cs` + their `.meta` files already tracked)
- `Assets/Scripts/Game/Ads/` contains 7 `.cs` files
- `Assets/Scripts/Game/ATT/` contains 7 `.cs` files  
- None of those 28 files remain in `Services/` or `Popup/`
- `git mv` used for every move (no raw filesystem copy)
- 340 EditMode tests pass after commit

## Tasks

- [x] **T01: Create IAP/ folder and move 15 IAP files**
  Move all IAP-related files from `Services/` (IIAPService, IAPOutcome, IAPResult, IAPProductDefinition, IAPProductInfo, IAPProductCatalog, IAPMockConfig, MockIAPService, UnityIAPService, NullIAPService, PlayFabCatalogService, NullPlayFabCatalogService) and from `Popup/` (IIAPPurchaseView, IAPPurchasePresenter, IAPPurchaseView) into new `Assets/Scripts/Game/IAP/` folder.

- [x] **T02: Create Ads/ folder and move 7 Ads files**
  Move from `Services/` (IAdService, AdResult, UnityAdService, NullAdService) and `Popup/` (IRewardedAdView, RewardedAdPresenter, RewardedAdView) into `Assets/Scripts/Game/Ads/`.

- [x] **T03: Create ATT/ folder and move 7 ATT files**
  Move from `Services/` (IATTService, ATTAuthorizationStatus, UnityATTService, NullATTService) and `Popup/` (IConsentGateView, ConsentGatePresenter, ConsentGateView) into `Assets/Scripts/Game/ATT/`. Run tests; commit.

## Observability / Diagnostics

This slice is a pure filesystem reorganisation. All failure state is inspectable via git and the filesystem:

- **Move integrity:** `git log --diff-filter=R --name-status HEAD` shows renames, not add/delete pairs. If raw `mv` was used, Unity loses GUID links.
- **File count surfaces:** `find Assets/Scripts/Game/IAP -name "*.cs" | wc -l` → 15; `find Assets/Scripts/Game/Ads -name "*.cs" | wc -l` → 7; `find Assets/Scripts/Game/ATT -name "*.cs" | wc -l` → 7
- **Negative check (diagnostic):** `rg "IAdService|IATTService|IIAPService" Assets/Scripts/Game/Services/` → must return empty. Non-empty means a file was missed.
- **Compile health:** Unity console errors after domain reload indicate a `.meta` GUID was broken. `mcp_call(unityMCP, read_console)` surfaces these immediately.
- **Test gate:** `mcporter call unityMCP.run_tests testMode:EditMode` (via stdin pipe — see K006); all 340 EditMode tests must pass after the T03 commit.
- **Redaction:** No secrets involved.

## Files Likely Touched

- `Assets/Scripts/Game/Services/` — source (files removed, but folder remains for S02)
- `Assets/Scripts/Game/Popup/` — source (files removed, remaining popups stay for S03)
- `Assets/Scripts/Game/IAP/` — created
- `Assets/Scripts/Game/Ads/` — created
- `Assets/Scripts/Game/ATT/` — created

## Observability / Diagnostics

This slice is a pure filesystem reorganisation — no runtime behaviour changes. Diagnostic surfaces:

- **Confirm move complete:** `find Assets/Scripts/Game/IAP -name "*.cs" | wc -l` (expect 15), same for Ads (7) and ATT (7)
- **Confirm sources are clean:** `ls Assets/Scripts/Game/Services/*.cs Assets/Scripts/Game/Popup/*.cs` — should show only non-IAP/Ads/ATT files after each task
- **Unity compile status:** Check Unity Editor console after reload; any namespace or reference breakage surfaces as `error CS` lines in `Editor.log`. Use the K011 `python3` snippet to read errors after the last `Starting:` line.
- **Test gate:** `run_tests EditMode` — 340 tests must pass after T03 commits; failure here indicates a `.meta` GUID mismatch or stale Bee dag (see K011).
- **Failure state detection:** If a `git mv` is accidentally omitted, `git status` will show the file as untracked in the old location and missing from the new one. Run `git status --short | grep "^R"` to confirm all expected renames are staged. To verify completeness per task: `find Assets/Scripts/Game/Ads -name "*.cs" | sort` and `find Assets/Scripts/Game/ATT -name "*.cs" | sort` — both must list exactly 7 files. If any file is missing, `git status` will show it as deleted/untracked; re-run the specific `git mv` for that file.
- **No secrets involved** — no redaction constraints apply.

### Structured Failure State

| Failure symptom | How to detect | Recovery |
|---|---|---|
| File not moved (wrong location) | `git status --short` shows `D Assets/.../Services/X.cs` and `?? Assets/.../ATT/X.cs` | Re-run `git mv` for the missing pair; stage and commit |
| `.meta` file left behind | `ls Assets/Scripts/Game/Services/*.meta 2>/dev/null` returns matches | `git rm` the orphaned `.meta` and commit |
| Unity compile error after move | K011 python3 snippet shows `error CS` after last `Starting:` line | Check GUID: `cat Assets/.../ATT/X.cs.meta` and confirm `guid` matches what Unity serialized; if mismatch, restore via `git mv` undo |
| Bee dag stuck on stale content hashes | Same error persists after source file was verified fixed on disk | Delete active dag files per K011, trigger `Assets/Refresh` |
| Test count < 340 after commit | `get_test_job` result shows `failed > 0` or `total < 340` | Check Unity compile status first; if clean, stale Bee dag is most likely cause |
