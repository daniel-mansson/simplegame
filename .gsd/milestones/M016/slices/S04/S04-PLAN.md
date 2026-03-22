# S04: Analytics Events

**Goal:** All four analytics event types fire to PlayFab from the correct lifecycle points.

**Demo:** PlayFab Game Manager Event History shows session_start, level_started, level_completed, currency_earned from a Play mode session.

## Must-Haves
- `IAnalyticsService` interface: `TrackSessionStart`, `TrackSessionEnd`, `TrackLevelStarted`, `TrackLevelCompleted`, `TrackLevelFailed`, `TrackCurrencyEarned`, `TrackCurrencySpent`, `TrackPlatformLinked`
- `PlayFabAnalyticsService` — uses `WritePlayerEvent`; no-ops if not logged in
- Session events hooked in `GameBootstrapper` (start after login, end on pause/quit)
- Level events hooked in `InGameSceneController` (started on RunAsync, completed/failed on outcome)
- Currency events hooked in `CoinsService` and `GoldenPieceService` (Earn/TrySpend)
- Platform linked event hooked in `PlatformLinkPresenter` on success
- Edit-mode tests: event dispatch verified via mock, no-op when not logged in
- All existing edit-mode tests continue to pass

## Tasks

- [ ] **T01: IAnalyticsService + PlayFabAnalyticsService**
  Define interface. Implement with WritePlayerEvent. Wire into GameBootstrapper, InGameSceneController.

- [ ] **T02: Currency and platform hooks**
  Add analytics callbacks to CoinsService, GoldenPieceService (earn/spend). Hook PlatformLinkPresenter for platform_account_linked event.

- [ ] **T03: Edit-mode tests**
  Mock IAnalyticsService. Test: events fire at correct lifecycle points, no-op when not logged in.

## Files Likely Touched
- `Assets/Scripts/Game/Services/IAnalyticsService.cs` — new
- `Assets/Scripts/Game/Services/PlayFabAnalyticsService.cs` — new
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — session events
- `Assets/Scripts/Game/InGame/InGameSceneController.cs` — level events
- `Assets/Scripts/Game/Services/CoinsService.cs` — currency events
- `Assets/Scripts/Game/Services/GoldenPieceService.cs` — currency events
- `Assets/Scripts/Game/Popup/PlatformLinkPresenter.cs` — platform_account_linked event
- `Assets/Tests/EditMode/Game/AnalyticsServiceTests.cs` — new
