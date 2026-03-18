# S02: TMP Prefab Kit — UAT

## What to Test

Verify prefabs exist and look reasonable in the Unity Editor.

## Test Steps

1. In the Unity Editor Project window, navigate to `Assets/Prefabs/UI/`
2. Confirm all 8 prefabs are present: BigPopupWindow, SmallPopupWindow, Buttons/PositiveButton, Buttons/DestructiveButton, Buttons/NeutralButton, Text/TitleText, Text/BodyText, Text/ButtonLabel
3. Click BigPopupWindow.prefab — in the Inspector, confirm PopupWindowShell component is present with _canvasGroup and _panel wired
4. Click PositiveButton.prefab — confirm Button + Image (green) + Label child with TextMeshProUGUI
5. Click TitleText.prefab — confirm TextMeshProUGUI with fontSize 36

## Expected Results

- All 8 prefabs present
- Window prefabs have PopupWindowShell with serialized refs wired
- Button prefabs have green/red/grey color respectively
- Text prefabs have correct font sizes

## Re-run

If prefabs are missing or broken, run `Tools/Setup/Create UI Prefab Kit` from the Unity menu.
