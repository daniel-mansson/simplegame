# S02: Remove Slot Button Overlay — UAT

**Milestone:** M022
**Slice:** S02

## Human Test Script

These tests are non-blocking. Run when convenient.

### Test 1 — Deck panel shows buttons on level start
1. Open Unity Editor, load InGame scene (or start from Boot)
2. Enter play mode
3. **Expected:** DeckPanel at the bottom of screen shows N blue buttons (one per active slot — typically 3 at level 1)
4. **Expected:** No `SlotButtonCanvas` GameObject in the hierarchy during play mode

### Test 2 — Tap a deck button to place a piece
1. In play mode, tap any deck button
2. **Expected:** The corresponding puzzle piece animates to the board (correct placement) or the hearts counter decrements (incorrect placement)
3. **Expected:** The tapped button disappears from the deck panel

### Test 3 — Deck panel stays in sync as pieces are placed
1. Play through several placements
2. **Expected:** Each time a piece is placed correctly, its deck button disappears and a new button appears for the next piece drawn into that slot
3. **Expected:** When all pieces are placed, no buttons remain in the deck panel

### Test 4 — Win/Lose flow still works
1. Play to a win or lose condition
2. **Expected:** Win/Lose popup appears normally
3. **Expected:** Returning to MainMenu works

### Test 5 — No SlotButtonCanvas in hierarchy
1. During play mode, open the Hierarchy window
2. **Expected:** No `SlotButtonCanvas` GameObject exists anywhere in the hierarchy
