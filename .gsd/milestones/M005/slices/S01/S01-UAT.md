# S01: Prefab transition player with LitMotion — UAT

## Prerequisites
- Open the project in Unity Editor
- Open the Boot scene

## Test Steps

### 1. Verify fade-to-black transition
1. Enter Play mode from the Boot scene
2. Click "Play" on the main menu to navigate to InGame
3. **Observe**: A ~0.3s fade-to-black should play during the transition
4. **Observe**: After the fade, the InGame scene should be visible

### 2. Verify fade-in from black
1. From InGame, trigger a win or click back to return to MainMenu
2. **Observe**: The screen fades to black, then fades back in showing the destination scene
3. **Observe**: The fade duration feels smooth (~0.3s each way)

### 3. Verify prefab exists
1. In the Project window, navigate to `Assets/Prefabs/`
2. **Observe**: `TransitionOverlay.prefab` exists
3. Click on it — it should have Canvas, CanvasGroup, Image (black), and UnityTransitionPlayer components

### 4. Verify no visual regression
1. Navigate between all screens (MainMenu → InGame → win/lose → MainMenu, MainMenu → Settings → back)
2. **Observe**: Transitions look identical to before — same fade speed, same black overlay
3. **Observe**: No flickering, no stuck overlays, no input blocking issues

## Expected Results
- All transitions show a smooth 0.3s fade-to-black
- No visual difference from M004 behavior
- Game loop works identically
