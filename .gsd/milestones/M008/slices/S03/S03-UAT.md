# S03: Wire All Popups to Prefabs — UAT

## What to Test

Verify popups show and animate correctly in play mode, and that the Boot scene loaded without errors.

## Test Steps

1. Open Boot scene in Unity Editor
2. In the Hierarchy, expand PopupCanvas — confirm 6 popup GameObjects are present with their view components
3. Click any popup GO (e.g. ConfirmDialogPopup) — in Inspector, confirm ConfirmDialogView component has _canvasGroup, _panel, _messageText (TMP_Text), _confirmButton, _cancelButton all wired (not None)
4. Enter Play mode from Boot scene
5. Navigate to InGame, complete or lose a level to trigger a popup
6. Observe: popup bounces up from below, dim overlay fades in behind it
7. Dismiss: popup scales down and fades, dim overlay fades out, input immediately usable

## Expected Results

- All fields wired (no None in Inspector)
- Popup enter animation: slide up + OutBounce
- Popup exit animation: scale down + fade out
- Dim overlay: fades in with open, fades out with close
- Input unblocked before fade-out completes (can tap UI immediately after clicking dismiss)
