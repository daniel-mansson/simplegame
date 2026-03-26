# T01: Create IAP/ Folder and Move 15 IAP Files

**Slice:** S01
**Milestone:** M020

## Goal

Move all IAP-related source files from `Services/` and `Popup/` into a new `Assets/Scripts/Game/IAP/` folder using `git mv`.

## Must-Haves

### Artifacts
- `Assets/Scripts/Game/IAP/` exists
- These 15 files are in `IAP/` (not in `Services/` or `Popup/`):
  - From `Services/`: `IIAPService.cs`, `IAPOutcome.cs`, `IAPResult.cs`, `IAPProductDefinition.cs`, `IAPProductInfo.cs`, `IAPProductCatalog.cs`, `IAPMockConfig.cs`, `MockIAPService.cs`, `UnityIAPService.cs`, `NullIAPService.cs`, `PlayFabCatalogService.cs`, `NullPlayFabCatalogService.cs`
  - From `Popup/`: `IIAPPurchaseView.cs`, `IAPPurchasePresenter.cs`, `IAPPurchaseView.cs`
- All corresponding `.meta` files moved (git mv handles this automatically)
- `Services/` and `Popup/` do NOT contain any of the above files

### Truths
- `find Assets/Scripts/Game/IAP -name "*.cs" | wc -l` returns 15 (on Windows: `(Get-ChildItem -Path Assets/Scripts/Game/IAP -Filter "*.cs" | Measure-Object).Count`)
- `rg "IAPService\|IAPOutcome\|IAPResult\|IAPProduct\|IAPMock\|IAPPurchase" Assets/Scripts/Game/Services/ Assets/Scripts/Game/Popup/` returns no matches

## Steps

1. `mkdir -p Assets/Scripts/Game/IAP` (or create via `git mv` which creates parent dirs)
2. `git mv` each of the 12 `Services/IAP*.cs` and `Services/*IAP*.cs` files to `IAP/`
3. `git mv` the 3 popup files (`IIAPPurchaseView.cs`, `IAPPurchasePresenter.cs`, `IAPPurchaseView.cs`) from `Popup/` to `IAP/`
4. `git status` — confirm 15 renames staged, no unexpected changes
5. Verify no IAP files remain in `Services/` or `Popup/`

## Observability Impact

This task is a pure filesystem reorganisation — no runtime behaviour changes. Failure state is entirely visible via git:

- **Success signal:** `git status` shows exactly 30 renames staged (15 `.cs` + 15 `.meta`), no other changes
- **Failure signal:** `git status` shows unexpected deletions (without corresponding additions) — indicates a raw `mv` was used instead of `git mv`, which would break Unity GUID tracking
- **Inspection command:** `find Assets/Scripts/Game/IAP -name "*.cs" | wc -l` → must return 15
- **Negative check:** `ls Assets/Scripts/Game/Services/ | grep -i iap` → must return empty
- **Redaction:** No secrets involved; this task touches only source file paths

## Context

- `git mv src dst` automatically moves the `.meta` file too — do not move `.meta` files manually
- On Windows with Git Bash, use forward slashes; `git mv` works the same
- The `Services/` folder should NOT be removed yet — it still contains Economy/Save/Progression/PlayFab files moved in S02
- Namespaces are unchanged (still `SimpleGame.Game.Services` for service files, `SimpleGame.Game.Popup` for popup files) — no source file edits required
- Do NOT commit yet — T02 and T03 complete the slice before committing

## Observability Impact

This task is a pure `git mv` reorganisation — no runtime code changes. Inspection surfaces after T01:

- **File count check:** `find Assets/Scripts/Game/IAP -name "*.cs" | wc -l` → 15. Deviation means a file was missed.
- **Source directories clean:** `ls Assets/Scripts/Game/Services/*.cs | grep -iE "IAP|Purchase"` and same for `Popup/` — both should return no matches.
- **Git staging:** `git status --short | grep "^R" | wc -l` → 30 (15 `.cs` + 15 `.meta` renames). If fewer, a file or its meta was not moved.
- **Failure state:** Any missed `git mv` shows as `?? Assets/Scripts/Game/IAP/<file>.cs` (untracked new) + `D  Assets/Scripts/Game/Services/<file>.cs` (deleted old) in `git status`. Recoverable by running the missed `git mv` before T02 begins.
- **Unity compile:** No compile errors expected since namespaces are not changed. If Unity shows `error CS0246` (type not found), a `.meta` GUID was lost — check `git status` for any non-rename entries on the affected file.
