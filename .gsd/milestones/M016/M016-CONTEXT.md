# M016: PlayFab Integration — Accounts, Cloud Save & Analytics

**Gathered:** 2026-03-20
**Status:** Ready for planning

## Project Description

SimpleGame is a Unity mobile jigsaw puzzle game. Core gameplay is complete and stable. M016 adds PlayFab as the backend layer: every player gets an anonymous account on first launch, progress is synced to the cloud, platform accounts (Game Center/Google Play Games) can be linked for cross-device recovery, and analytics events are sent throughout the session.

## Why This Milestone

The game is distribution-ready (M015 done). Before launch, player identity and progress persistence need to be real — PlayerPrefs-only save data is lost on reinstall. PlayFab provides the backend without requiring a custom server.

## User-Visible Outcome

### When this milestone is complete, the user can:

- Launch the game on a new device and see progress restored automatically after reinstalling (cloud save pull at boot)
- Link their Game Center or Google Play Games account from Settings or the first-launch prompt
- Recover progress on a new device by signing into the same platform account

### Entry point / environment

- Entry point: Unity Editor Play mode + device build (iOS/Android)
- Environment: local dev (Play mode for logic), device build for platform linking verification
- Live dependencies: PlayFab title (requires a configured Title ID in `PlayFabSharedSettings`)

## Completion Class

- Contract complete means: Unit tests cover merge logic, login flow, and event dispatch. Boot sequence compiles and logs correct PlayFab IDs in Play mode.
- Integration complete means: Cloud save round-trips correctly between two Play mode sessions. Analytics events visible in PlayFab Game Manager.
- Operational complete means: Platform linking works on a real device (at least one platform verified by hand).

## Final Integrated Acceptance

To call this milestone complete, we must prove:

- Fresh install: game creates an anonymous PlayFab account and logs a valid Player ID
- Reinstall: game fetches cloud data at boot and local state reflects the cloud values (take-max applied)
- Settings screen shows link status and link/unlink buttons for Game Center and Google Play Games
- PlayFab Game Manager Event History shows at least one session_start, level_started, level_completed, currency_earned event from a test session

## Risks and Unknowns

- **Google Play Games Unity plugin install** — separate SDK with App ID config, gradle dependencies, and possible version conflicts with Unity 6. This is the riskiest dependency and must be proven in S01/S02 before S03 commits to it.
- **PlayFab SDK install method** — no UPM package available; must be installed as a `.unitypackage` manually into `Assets/PlayFabSDK/`. This is a one-time manual step before auto-mode can run S01.
- **`IMetaSaveService` stays synchronous** — explicitly decided. Cloud sync is a separate async layer in `GameBootstrapper.Start()`, not wired into individual service `Save()` calls.
- **Take-max merge is correct only for monotonically increasing fields** — coins, golden pieces, and object step counts all increase in normal play. If a future feature requires subtracting and persisting (e.g. spending coins offline), the merge strategy needs revisiting.

## Existing Codebase / Prior Art

- `Assets/Scripts/Game/Services/IMetaSaveService.cs` — synchronous save interface; stays unchanged
- `Assets/Scripts/Game/Services/PlayerPrefsMetaSaveService.cs` — current local implementation; becomes the local cache layer beneath cloud sync
- `Assets/Scripts/Game/Services/MetaSaveData.cs` — already `[Serializable]` with `JsonUtility` path; add a `savedAt` long field for timestamp tracking
- `Assets/Scripts/Game/Boot/GameBootstrapper.cs` — cloud pull and PlayFab login happen in `Start()` before the navigation loop
- `Assets/Scripts/Game/Settings/SettingsSceneController.cs` + `ISettingsView.cs` + `SettingsPresenter.cs` — extend for link/unlink UI
- `Assets/Scripts/Game/Services/CoinsService.cs` + `GoldenPieceService.cs` — analytics hooks added here for earn/spend events

> See `.gsd/DECISIONS.md` for all architectural and pattern decisions — it is an append-only register; read it during planning, append to it during execution.

## Relevant Requirements

- R137 — Anonymous PlayFab account on first launch
- R138 — Entity token and Player ID persisted locally
- R139 — Cloud save push
- R140 — Cloud save pull with take-max merge
- R141 — Game Center linking
- R142 — Google Play Games linking (highest risk)
- R143 — First-launch link prompt
- R144 — Link/unlink in Settings
- R145–R148 — Analytics events (session, level, currency, linking)

## Scope

### In Scope

- PlayFab SDK install (`Assets/PlayFabSDK/` via `.unitypackage`)
- Anonymous login (`LoginWithCustomID`, device ID, `CreateAccount: true`)
- Player ID persistence in PlayerPrefs
- Cloud save: `UpdateUserData` / `GetUserData` with take-max merge
- Cloud sync lifecycle: pull at boot, push at session end checkpoints
- Game Center linking and unlinking (iOS)
- Google Play Games linking and unlinking (Android)
- First-launch skippable link prompt (uses existing popup stack)
- Settings screen extended with link status and link/unlink buttons
- Analytics events: session_start, session_end, level_started, level_completed, level_failed, currency_earned, currency_spent, platform_account_linked

### Out of Scope / Non-Goals

- Email/password authentication
- Leaderboards, friends, multiplayer
- PlayFab Economy / Virtual Currency (coins remain local, tracked by analytics only)
- Push notifications
- Server-side Cloud Script / Azure Functions

## Technical Constraints

- PlayFab SDK has no UPM package — must be installed manually as `.unitypackage` before S01 can run. A `PlayFabSharedSettings` ScriptableObject with the project Title ID must be configured before the build compiles.
- Google Play Games Unity plugin requires a separate manual install step and `google-services.json` / App ID configuration. This is a prerequisite for S03 Android linking.
- `IMetaSaveService` must remain synchronous — all callers (`CoinsService`, `GoldenPieceService`, `MetaProgressionService`) depend on synchronous load/save. Cloud sync is an explicit async layer only in `GameBootstrapper`.
- PlayFab client API is callback-based. All calls must be wrapped in UniTask adapters (TaskCompletionSource pattern) to integrate with the existing `async UniTaskVoid Start()` boot flow.

## Integration Points

- `GameBootstrapper.Start()` — PlayFab login and cloud pull inserted before navigation loop
- `IMetaSaveService` / `PlayerPrefsMetaSaveService` — local cache; cloud pull result is merged then written back via this interface
- `SettingsSceneController` / `ISettingsView` — extended with link status and link/unlink actions
- `CoinsService.Earn()` / `TrySpend()` — analytics hooks for currency events
- `GoldenPieceService.Earn()` / `TrySpend()` — analytics hooks for currency events
- `InGameSceneController` — analytics hooks for level events
- Existing popup stack (`PopupManager<PopupId>`) — first-launch prompt uses a new `PopupId.PlatformLink` entry

## Open Questions

- None — all decisions locked during discussion.
