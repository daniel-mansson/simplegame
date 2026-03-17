# S01: IViewResolver + Container Refactor — UAT

## Prerequisites
- Unity Editor open with the project

## Test Steps

1. **Verify IViewResolver exists**
   - Open `Assets/Scripts/Core/PopupManagement/IViewResolver.cs`
   - Confirm it has `T Get<T>() where T : class` method

2. **Verify container renamed**
   - Open `Assets/Scripts/Game/Popup/`
   - Confirm `UnityViewContainer.cs` exists (no `UnityPopupContainer.cs`)
   - Confirm class implements both `IPopupContainer<PopupId>` and `IViewResolver`

3. **Verify no old references**
   - Search project for "UnityPopupContainer" — should find zero results in .cs files

4. **Run tests**
   - Run Edit Mode tests in Unity Test Runner
   - All 169 tests should pass
   - ViewContainerTests suite should have 5 green tests

## Expected Result
All checks pass. Container renamed, IViewResolver in Core, all tests green.
