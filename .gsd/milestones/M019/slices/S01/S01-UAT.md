# S01 UAT: IIAPService Abstraction and Mock

**Test these manually when convenient. Non-blocking — agent has moved on.**

## 1. Mock outcomes in Editor

Open `Assets/Resources/IAPMockConfig.asset` in the Inspector.

| Step | Action | Expected |
|------|--------|----------|
| 1 | Set MockOutcome = Success, CoinsGranted = 500. Enter Play Mode. Open Shop. | Pack buttons show "500 Coins", "1200 Coins", "2500 Coins" |
| 2 | Tap first pack. | Status shows "Purchase complete! Your balance: 500 coins" |
| 3 | Set MockOutcome = Cancelled. Tap a pack. | Status shows "Purchase cancelled." |
| 4 | Set MockOutcome = PaymentFailed. Tap a pack. | Status shows "Purchase failed. Please try again." |
| 5 | Set MockOutcome = ValidationFailed. Tap a pack. | Status shows "Purchase could not be verified. Please try again." |

## 2. IAPProductCatalog asset

Open `Assets/Resources/IAPProductCatalog.asset`. Confirm:
- Products array has 3 entries
- ProductIds: `com.simplegame.coins.500`, `com.simplegame.coins.1200`, `com.simplegame.coins.2500`
- CoinsAmounts: 500, 1200, 2500
