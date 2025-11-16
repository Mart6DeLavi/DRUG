Game Speed Ramping (Scroll)

How it works
- `Assets/Scripts/GameSpeedController.cs` provides a global speed multiplier that ramps up over time. It starts at `startMultiplier` and increases up to `maxMultiplier` using `accelerationPerSecond`.
- `PlayerMovement` multiplies its horizontal speed by this global multiplier in addition to existing bonuses/debuffs from `GameManager`.

Setup
1. In your gameplay scene, add an empty GameObject (e.g. `GameControllers`).
2. Add the `GameSpeedController` component to it.
3. Configure in the Inspector:
   - `Start Multiplier` (default 1)
   - `Max Multiplier` (e.g. 2.5)
   - `Acceleration Per Second` (e.g. 0.05)
   - Optional `Custom Curve` to shape progression over time (seconds on X axis).

Notes
- Pausing the game with `Time.timeScale = 0` also pauses the ramp.
- On scene reload (e.g., after death), the multiplier resets to the start value.
- Temporary effects (`GameManager.playerSpeedMultiplier`) still apply; final speed = base `PlayerMovement.speed` × `GameManager.playerSpeedMultiplier` × `GameSpeedController.CurrentMultiplier`.
