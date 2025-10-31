# HUD Integration Checklist

Quick checklist to ensure HUD is properly integrated with existing game systems.

## Pre-Setup Requirements

### 1. PlayerEvents Asset
- [ ] Create folder: `Assets/_Project/Data/Events/`
- [ ] Right-click → Create → RoombaRampage → Player Events
- [ ] Name: `PlayerEvents`
- [ ] Assign to PlayerHealth component on Player GameObject

### 2. Required GameObjects in Scene
- [ ] Player GameObject with PlayerHealth component
- [ ] ScoreManager GameObject in scene
- [ ] EnemySpawner GameObject in scene

## Canvas Setup

### 3. Create HUD Canvas
- [ ] Create Canvas: Hierarchy → UI → Canvas
- [ ] Rename: `HUD_Canvas`
- [ ] Canvas Render Mode: Screen Space - Overlay
- [ ] Canvas Scaler UI Scale Mode: Scale With Screen Size
- [ ] Canvas Scaler Reference Resolution: 1920 x 1080
- [ ] Canvas Scaler Match: 0.5
- [ ] Canvas Sort Order: 10

### 4. Import TextMeshPro
- [ ] Window → TextMeshPro → Import TMP Essential Resources
- [ ] (Optional) Import TMP Examples & Extras

## HUD Element Creation

### 5. Player Health Bar
- [ ] Create container: TopLeft_Container (anchors: top-left)
- [ ] Create UI Image: Background (dark gray, semi-transparent)
- [ ] Create UI Image: Fill (green, Image Type: Filled, Horizontal)
- [ ] Create TMP Text: Health text ("100/100")
- [ ] Add HealthBar.cs script to background
- [ ] Assign references in Inspector

### 6. Score Display
- [ ] Create container: TopRight_Container (anchors: top-right)
- [ ] Create TMP Text: "Score: 0" (right-aligned, gold color)
- [ ] Add ScoreDisplay.cs script
- [ ] Configure prefix and format

### 7. Wave Display
- [ ] Create container: TopCenter_Container (anchors: top-center)
- [ ] Create TMP Text: "Wave 1 - Enemies: 0" (center-aligned)
- [ ] Add WaveDisplay.cs script
- [ ] Assign EnemySpawner reference

### 8. Kill Counter
- [ ] Under TopRight_Container
- [ ] Create TMP Text: "Kills: 0" (right-aligned, below score)
- [ ] Add KillCounter.cs script
- [ ] Assign PlayerEvents reference

### 9. XP Bar
- [ ] Create container: Bottom_Container (anchors: bottom-stretch)
- [ ] Create UI Image: Background (dark gray, full width)
- [ ] Create UI Image: Fill (cyan, Image Type: Filled, Horizontal)
- [ ] Create TMP Text: "LVL 1" (left-aligned inside bar)
- [ ] Add XPBar.cs script

### 10. Boss Health Bar (Optional)
- [ ] Same as Player Health Bar, but larger
- [ ] Position: Top-center
- [ ] Initially disabled (uncheck GameObject)

## HUDManager Setup

### 11. Add HUDManager
- [ ] Select HUD_Canvas GameObject
- [ ] Add Component → HUDManager
- [ ] Assign all HUD component references:
  - [ ] Player Health Bar
  - [ ] Boss Health Bar (optional)
  - [ ] Score Display
  - [ ] Wave Display
  - [ ] Kill Counter
  - [ ] XP Bar
- [ ] Assign game system references:
  - [ ] Player Health (from scene)
  - [ ] Enemy Spawner (from scene)
  - [ ] Player Events (from project)
- [ ] Check "Show On Start"
- [ ] Check "Auto Initialize"

## Enemy Kill Tracking Integration

### 12. Connect Enemies to Kill Counter

**Option A: Via PlayerEvents (Recommended)**

Add to your Enemy death handling (in `Enemy.cs` or `EnemyHealth.cs`):

```csharp
[SerializeField] private Player.PlayerEvents playerEvents;

private void Die()
{
    // ... existing death code ...

    // Raise kill event
    if (playerEvents != null)
    {
        int scoreValue = enemyData?.scoreValue ?? 10;
        playerEvents.RaiseEnemyKilled(scoreValue);
    }

    // ... rest of death code ...
}
```

**Option B: Via ScoreManager**

Or modify ScoreManager to raise kill events:

```csharp
[SerializeField] private Player.PlayerEvents playerEvents;

public void AddScore(int amount)
{
    currentScore += amount;

    // Update high score
    if (currentScore > highScore)
    {
        highScore = currentScore;
        SaveHighScore();
    }

    // Invoke events
    OnScoreChanged?.Invoke(currentScore);
    OnScoreAdded?.Invoke(amount);

    // Raise enemy killed for kill counter
    if (playerEvents != null)
    {
        playerEvents.RaiseEnemyKilled(amount);
    }
}
```

## Testing

### 13. Test Each Component

**Health Bar:**
- [ ] Play mode
- [ ] Select Player → PlayerHealth component
- [ ] Right-click component → Test: Take 10 Damage
- [ ] Verify health bar updates smoothly
- [ ] Verify color changes with health
- [ ] Verify damage flash effect

**Score Display:**
- [ ] Play mode
- [ ] Open Console: `ScoreManager.Instance.AddScore(100);`
- [ ] Or use ScoreManager debug GUI button
- [ ] Verify score counts up smoothly
- [ ] Verify thousand separators work

**Wave Display:**
- [ ] Play mode
- [ ] Let wave spawn or use EnemySpawner debug button
- [ ] Verify wave number shows
- [ ] Verify enemy count updates

**Kill Counter:**
- [ ] Play mode
- [ ] Kill an enemy
- [ ] Verify kill count increments
- [ ] Test combo (if enabled)

**XP Bar:**
- [ ] Play mode
- [ ] Select XP Bar GameObject
- [ ] Right-click XPBar component → Test: Set 75% XP
- [ ] Verify bar fills smoothly

**HUD Manager:**
- [ ] Play mode
- [ ] Select HUD_Canvas
- [ ] Right-click HUDManager → Test: Hide HUD
- [ ] Right-click HUDManager → Test: Show HUD
- [ ] Verify HUD shows/hides

### 14. Integration Testing

- [ ] Start game from beginning
- [ ] Verify all HUD elements visible
- [ ] Take damage → health bar updates
- [ ] Kill enemies → score and kills update
- [ ] Survive wave → wave counter updates
- [ ] Player dies → HUD resets (if implemented)

## Common Issues

### Health bar doesn't update
✓ Check PlayerEvents asset assigned to both PlayerHealth AND HUDManager
✓ Check HUDManager Auto Initialize is checked
✓ Enable debug mode on HUDManager and HealthBar

### Score doesn't update
✓ Verify ScoreManager exists in scene
✓ Check ScoreManager is not disabled
✓ Enable debug mode on ScoreDisplay

### Kills don't count
✓ Check PlayerEvents assigned to KillCounter
✓ Verify enemies call PlayerEvents.RaiseEnemyKilled() on death
✓ Enable debug mode on KillCounter

### Wave display shows 0
✓ Check EnemySpawner reference assigned
✓ Verify EnemySpawner is spawning
✓ Enable debug mode on WaveDisplay

## Final Verification

### 15. Complete System Test

- [ ] All HUD elements visible and positioned correctly
- [ ] Health bar updates on damage/healing
- [ ] Score displays and counts up smoothly
- [ ] Wave number shows current wave
- [ ] Enemy count updates in real-time
- [ ] Kills increment when enemies die
- [ ] XP bar displays placeholder data
- [ ] No console errors
- [ ] No missing references in Inspector
- [ ] Performance is smooth (60 FPS+)

## Performance Check

- [ ] Open Profiler: Window → Analysis → Profiler
- [ ] Play game for 1 minute
- [ ] Check CPU usage:
  - [ ] HUD components < 0.5ms total
  - [ ] No GC allocations from HUD (except initial)
- [ ] If issues, increase WaveDisplay Update Interval

## Documentation Reference

Stuck? Check these files:
- **HUD_SETUP.md** - Complete setup guide with screenshots
- **README.md** - Component overview and API reference
- **INTEGRATION_CHECKLIST.md** - This file

## Next Steps

After HUD is working:
1. Customize colors and fonts to match game style
2. Add HUD fade in/out for menus
3. Implement screen shake on low health
4. Add boss health bar integration
5. Create XP system and integrate XPBar
6. Add weapon selector UI (future)
7. Add minimap (future)

---

**Setup Time:** 15-30 minutes
**Difficulty:** Beginner-friendly

If you encounter issues not covered here, check HUD_SETUP.md Troubleshooting section.
