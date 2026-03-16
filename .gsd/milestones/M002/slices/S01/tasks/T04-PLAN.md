---
estimated_steps: 4
estimated_files: 3
---

# T04: Remove ISampleView, SamplePresenter, and GameService from Core runtime

**Slice:** S01 — Core Assembly Restructure + Generic Managers
**Milestone:** M002

## Description

`ISampleView`, `SamplePresenter`, and `GameService` are not framework code. `ISampleView` and `SamplePresenter` are test fixtures that move to the Core test assembly in S03. `GameService` is game-specific domain logic that moves to Game in S02. Remove all three from Core runtime sources now.

## Steps

1. `git rm "Assets/Scripts/Core/MVP/ISampleView.cs"` (and confirm `.meta` is also removed by git)
2. `git rm "Assets/Scripts/Core/MVP/SamplePresenter.cs"` (and `.meta`)
3. `git rm "Assets/Scripts/Core/Services/GameService.cs"` (and `.meta`)
4. Verify no other Core files import these types — `grep -r "ISampleView\|SamplePresenter\|GameService" Assets/Scripts/Core/` should return only the UIFactory.cs and game-specific presenter files (which are deleted in T05)

## Must-Haves

- [ ] `ISampleView.cs` no longer exists in `Assets/Scripts/Core/MVP/`
- [ ] `SamplePresenter.cs` no longer exists in `Assets/Scripts/Core/MVP/`
- [ ] `GameService.cs` no longer exists in `Assets/Scripts/Core/Services/`

## Verification

- `find Assets/Scripts/Core -name "ISampleView.cs" -o -name "SamplePresenter.cs" -o -name "GameService.cs"` returns empty

## Inputs

- `Assets/Scripts/Core/MVP/ISampleView.cs` — test fixture to remove
- `Assets/Scripts/Core/MVP/SamplePresenter.cs` — test fixture to remove
- `Assets/Scripts/Core/Services/GameService.cs` — game-specific service to remove

## Expected Output

- All three files deleted from Core runtime sources
