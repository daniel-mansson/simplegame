## S01 UAT — Ad Service Abstraction & SDK

### What to verify

1. **Package installed:** Open Unity Editor → Window → Package Manager → search for "Advertisement" — confirm "Advertisement Legacy" 4.12.x appears as installed.

2. **Compile clean:** No errors in the Console after Unity reloads the project. The `UnityAdService.cs` file uses `UnityEngine.Advertisements` types — if the package failed to resolve, you'll see CS0246 errors.

3. **Edit-mode tests pass:** Window → General → Test Runner → EditMode → Run All. All 307 tests should pass including the 13 new `AdServiceTests`.

4. **NullAdService behaviour (optional manual check):** Add a temporary script that calls `new NullAdService { SimulateLoaded = false }` and asserts `ShowRewardedAsync()` returns `NotLoaded` — or just trust the automated tests.

### Not yet verifiable

Real ad display (UnityAdService with test game IDs) — verified in S02 after wiring into the game flow.
