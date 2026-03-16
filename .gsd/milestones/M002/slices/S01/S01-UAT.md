# S01 UAT — Core Assembly Restructure

**Milestone:** M002
**Slice:** S01

These checks can be run at any time to verify S01's outcomes hold.

## Shell Checks (run from project root)

```bash
# 1. No game-specific types in Core sources
grep -r "IMainMenuView\|ISettingsView\|IConfirmDialogView\|GameService\|UIFactory\|ISampleView\|SamplePresenter\|ScreenId\|PopupId" Assets/Scripts/Core/
# Expected: no output

# 2. Generic ScreenManager
grep "class ScreenManager<TScreenId>" Assets/Scripts/Core/ScreenManagement/ScreenManager.cs
# Expected: match

# 3. Generic PopupManager
grep "class PopupManager<TPopupId>" Assets/Scripts/Core/PopupManagement/PopupManager.cs
# Expected: match

# 4. Generic IPopupContainer
grep "interface IPopupContainer<" Assets/Scripts/Core/PopupManagement/IPopupContainer.cs
# Expected: match

# 5. Core/MVP contains only framework base types
find Assets/Scripts/Core/MVP -name "*.cs" | sort
# Expected: IPopupView.cs, IView.cs, Presenter.cs only

# 6. Unity impls in Core/Unity/
find Assets/Scripts/Core/Unity -name "*.cs" | sort
# Expected: UnityInputBlocker.cs, UnitySceneLoader.cs, UnityTransitionPlayer.cs

# 7. No old Runtime asmdef
find Assets/Scripts -name "SimpleGame.Runtime.asmdef"
# Expected: no output

# 8. Static guard
grep -r "static " --include="*.cs" Assets/Scripts/Core/ | grep -v "static void\|static class\|static readonly\|static async\|static UniTask"
# Expected: no output
```

## Note

After S01, the project has known compile errors (test assembly and Editor assembly reference deleted types). This is expected — S02 and S03 resolve it.
