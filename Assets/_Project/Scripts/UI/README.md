# UI System - RoombaRampage

Complete UI system for the 3D top-down bullet hell driving game.

## Structure

```
UI/
├── HUD/                    # Heads-Up Display components
│   ├── HealthBar.cs        # Reusable health bar with animations
│   ├── ScoreDisplay.cs     # Score counter with count-up effect
│   ├── WaveDisplay.cs      # Wave number and enemy count
│   ├── KillCounter.cs      # Kill tracking with combo system
│   ├── XPBar.cs            # XP bar (placeholder for future)
│   └── HUDManager.cs       # Central HUD controller
├── HUD_SETUP.md            # Complete setup guide
└── README.md               # This file
```

## Quick Start

1. **Read the setup guide:** `HUD_SETUP.md` (< 400 lines, comprehensive)
2. **Create Canvas:** Screen Space Overlay, 1920x1080 reference
3. **Add HUD elements:** Follow the layout diagram in HUD_SETUP.md
4. **Add scripts:** Assign to corresponding GameObjects
5. **Configure HUDManager:** Assign all references on Canvas
6. **Test:** Use context menus on each component

## Component Overview

### HealthBar
- **Purpose:** Display health with smooth animations and color gradients
- **Features:** Smooth fill lerp, damage flash, green→yellow→red gradient
- **Reusable:** Works for player, boss, or any health display
- **Events:** Auto-connects via PlayerEvents ScriptableObject

### ScoreDisplay
- **Purpose:** Display player score with count-up animation
- **Features:** Smooth count-up, thousand separators, configurable format
- **Integration:** Auto-connects to ScoreManager.Instance
- **Performance:** Coroutine-based animation, no Update() overhead

### WaveDisplay
- **Purpose:** Display current wave and active enemy count
- **Features:** Wave number, enemy count, optional wave announcements
- **Integration:** Connects to EnemySpawner
- **Performance:** Periodic updates (0.5s), not every frame

### KillCounter
- **Purpose:** Track and display enemy kills
- **Features:** Total kills, combo system, configurable format
- **Integration:** Connects to PlayerEvents.OnEnemyKilled
- **Optional:** Combo counter for kills in quick succession

### XPBar
- **Purpose:** Display XP progress (placeholder for future XP system)
- **Features:** Smooth fill animation, level display, visual match to health bar
- **Status:** Placeholder - ready for XP system integration
- **Future:** Will connect to XP Manager when implemented

### HUDManager
- **Purpose:** Central controller for all HUD elements
- **Features:** Auto-initialization, show/hide, boss health bar control
- **Singleton:** Access via `HUDManager.Instance`
- **API:** Public methods for controlling HUD from other systems

## Integration Points

### Player Health
```csharp
// PlayerHealth raises events via PlayerEvents ScriptableObject
// HUDManager subscribes to these events:
playerEvents.OnPlayerDamaged += UpdateHealthBar;
playerEvents.OnPlayerHealed += UpdateHealthBar;
playerEvents.OnPlayerRespawned += ResetHUD;
```

### Score System
```csharp
// ScoreDisplay auto-subscribes to ScoreManager
ScoreManager.Instance.OnScoreChanged += UpdateScore;
ScoreManager.Instance.OnScoreAdded += ShowScoreIncrement;
```

### Wave System
```csharp
// WaveDisplay polls EnemySpawner periodically
int wave = enemySpawner.GetCurrentWave();
int count = enemySpawner.GetActiveEnemyCount();
```

### Kill Tracking
```csharp
// Enemies raise kill event via PlayerEvents
playerEvents.RaiseEnemyKilled(scoreValue);
// KillCounter listens and increments
```

## Usage Examples

### Show/Hide HUD
```csharp
HUDManager.Instance.ShowHUD(false); // Hide
HUDManager.Instance.ShowHUD(true);  // Show
```

### Boss Health Bar
```csharp
// Show boss health
HUDManager.Instance.ShowBossHealthBar("Mega Boss", 5000, 5000);

// Update boss health
HUDManager.Instance.UpdateBossHealth(4200, 5000);

// Hide boss health
HUDManager.Instance.HideBossHealthBar();
```

### Manual Health Update
```csharp
// If you need to manually update (not using events)
HUDManager.Instance.UpdatePlayerHealth(75, 100);
```

### Reset HUD
```csharp
// Reset all HUD elements (on game restart)
HUDManager.Instance.ResetHUD();
```

## Testing

All components have context menu test commands:

**HealthBar:**
- Test: Set 100% Health
- Test: Set 50% Health
- Test: Set 25% Health
- Test: Trigger Damage Flash

**ScoreDisplay:**
- Test: Add 100 Score
- Test: Add 1000 Score
- Test: Reset Score

**WaveDisplay:**
- Test: Force Update
- Test: Show Wave Complete

**KillCounter:**
- Test: Add Kill
- Test: Add 5 Kills
- Test: Reset Kills

**XPBar:**
- Test: Set 75% XP
- Test: Level Up
- Test: Set Level 10

**HUDManager:**
- Test: Show HUD
- Test: Hide HUD
- Test: Show Boss Health Bar
- Test: Reset HUD

## Visual Guidelines

### Colors
- **Health High:** RGB(50, 255, 50) - Green
- **Health Med:** RGB(255, 255, 0) - Yellow
- **Health Low:** RGB(255, 50, 50) - Red
- **XP Bar:** RGB(76, 204, 255) - Cyan
- **Score/Kills:** RGB(255, 215, 0) - Gold
- **Background:** RGBA(20, 20, 20, 180) - Semi-transparent dark

### Fonts
Recommended sci-fi fonts (free):
- Orbitron (Google Fonts)
- Rajdhani (Google Fonts)
- Exo 2 (Google Fonts)

### Text Sizes
- Health/XP bars: 16-18pt
- Score/Kills: 20-24pt
- Wave display: 18-20pt
- Announcements: 28-32pt

## Performance Notes

- **WaveDisplay:** Updates every 0.5s, not every frame
- **ScoreDisplay:** Coroutine-based count-up, no Update()
- **HealthBar:** Only animates when health changes
- **KillCounter:** Event-driven, minimal overhead
- **No expensive operations in Update()** loops

## Troubleshooting

### HUD not visible
- Check Canvas Render Mode: Screen Space - Overlay
- Check Canvas Sort Order: 10+
- Verify HUD_Canvas GameObject is active

### Health bar not updating
- Check PlayerEvents asset is assigned to PlayerHealth
- Verify HUDManager has correct references
- Enable debug mode to see event calls

### Score/Kills not updating
- Check ScoreManager exists in scene
- Verify PlayerEvents.RaiseEnemyKilled() is called on enemy death
- Enable debug mode on components

### TextMeshPro errors
- Import TMP Essential Resources: Window → TextMeshPro → Import...

See **HUD_SETUP.md** for complete troubleshooting guide.

## Future Enhancements

### Planned Features
- [ ] Screen shake on low health
- [ ] HUD fade in/out animations
- [ ] Minimap (separate component)
- [ ] Weapon selector UI
- [ ] Power-up indicator
- [ ] Damage numbers (floating text)
- [ ] Boss intro animation

### XP System Integration
When XP system is implemented:
1. Create XPManager singleton
2. Add OnXPChanged and OnLevelUp events
3. Uncomment integration code in XPBar.cs
4. Connect to HUDManager initialization

## Dependencies

**Required:**
- Unity 6 (6000.2.6f2)
- TextMeshPro package
- URP (Universal Render Pipeline)

**Game Systems:**
- PlayerHealth.cs (Player namespace)
- PlayerEvents.cs (Player namespace, ScriptableObject)
- ScoreManager.cs (Managers namespace)
- EnemySpawner.cs (Enemies namespace)

**Optional:**
- Enemy.cs / EnemyHealth.cs (for kill tracking)

## Architecture

### Event Flow
```
[Player Takes Damage]
    ↓
PlayerHealth.TakeDamage()
    ↓
PlayerEvents.RaiseDamaged(current, max)
    ↓
HUDManager.OnPlayerHealthChanged()
    ↓
HealthBar.UpdateHealthSmooth()
```

### Score Flow
```
[Enemy Dies]
    ↓
ScoreManager.AddScore(points)
    ↓
ScoreManager.OnScoreChanged.Invoke(score)
    ↓
ScoreDisplay.OnScoreChanged()
    ↓
ScoreDisplay.CountUpCoroutine()
```

### Kill Flow
```
[Enemy Dies]
    ↓
PlayerEvents.RaiseEnemyKilled(scoreValue)
    ↓
KillCounter.OnEnemyKilled()
    ↓
KillCounter.UpdateKillDisplay()
```

## Notes

- All components are **event-driven** for loose coupling
- **No direct references** between HUD and game systems (uses events)
- **Reusable components** (HealthBar works for any health display)
- **Performance-optimized** (no expensive Update() operations)
- **Responsive layout** (anchors for different resolutions)
- **Debug-friendly** (context menus and debug modes)

---

**Created for RoombaRampage - Unity 6 with URP**
