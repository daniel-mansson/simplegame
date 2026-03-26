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

## Context

- `git mv src dst` automatically moves the `.meta` file too — do not move `.meta` files manually
- On Windows with Git Bash, use forward slashes; `git mv` works the same
- The `Services/` folder should NOT be removed yet — it still contains Economy/Save/Progression/PlayFab files moved in S02
- Namespaces are unchanged (still `SimpleGame.Game.Services` for service files, `SimpleGame.Game.Popup` for popup files) — no source file edits required
- Do NOT commit yet — T02 and T03 complete the slice before committing
