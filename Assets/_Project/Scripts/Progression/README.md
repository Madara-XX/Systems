# Progression System

Player progression systems for RoombaRampage including XP, leveling, and skill upgrades.

---

## Overview

This folder contains all progression-related systems for the game:

- **XP System** - Experience points and leveling
- **Skill System** - Skill unlocks and upgrades (coming soon)
- **Meta-Progression** - Persistent upgrades between runs (future)

---

## Folder Structure

```
Progression/
├── XP/
│   ├── XPManager.cs          # Core XP system singleton
│   ├── XPGem.cs              # XP pickup component
│   ├── XPGemPool.cs          # Object pooling for gems
│   └── Data/
│       └── XPSettings.cs     # XP configuration ScriptableObject
├── XP_SETUP.md               # Complete XP system setup guide
└── README.md                 # This file
```

---

## Current Systems

### XP & Leveling System

**Status:** ✅ Complete

Experience point system with visual gem pickups, magnetic attraction, and exponential leveling curve.

**Key Features:**
- Exponential XP curve (configurable)
- Magnetic XP gems with smooth attraction
- Object pooling for 100+ simultaneous gems
- Level-up effects (VFX, SFX, healing, time slow)
- Pause system for skill selection
- Full event-driven architecture

**Documentation:** See [XP_SETUP.md](./XP_SETUP.md)

**Quick Start:**
1. Create XPSettings asset
2. Create XP Gem prefab
3. Add XPManager and XPGemPool to scene
4. Configure enemy XP values
5. Play!

---

## Integration Points

### Enemy Death → XP Reward

When enemies die, they automatically spawn XP gems:

```csharp
// In Enemy.cs OnDeath():
if (enemyData.xpValue > 0 && XPGemPool.Instance != null)
{
    XPGemPool.Instance.SpawnGem(transform.position, enemyData.xpValue);
}
```

### Level-Up → Skill Selection (Future)

XP system fires events when player levels up:

```csharp
// In your SkillManager:
XPManager.Instance.OnLevelUp.AddListener(OnPlayerLevelUp);

private void OnPlayerLevelUp(int newLevel)
{
    // Show skill selection UI
    ShowSkillChoices();
}
```

### UI Integration

XPBar automatically connects to XPManager:

```csharp
// In XPBar.cs:
XPManager.Instance.OnXPGained.AddListener(UpdateXPBar);
XPManager.Instance.OnLevelUp.AddListener(OnLevelUp);
```

---

## API Reference

### XPManager

**Singleton Access:**
```csharp
XPManager.Instance
```

**Key Methods:**
```csharp
.AddXP(int amount)              // Award XP
.SetLevel(int level)            // Set level (debug)
.ResetXP()                      // Reset for new run
.CompleteLevelUp()              // Resume game after skill selection
.GetXPProgress()                // Get 0-1 progress for UI
```

**Key Events:**
```csharp
.OnXPGained(int current, int needed)  // XP changed
.OnLevelUp(int newLevel)              // Level-up occurred
.OnMaxLevelReached()                  // Hit level cap
```

**Key Properties:**
```csharp
.CurrentXP                      // Current XP toward next level
.CurrentLevel                   // Current player level
.XPForNextLevel                 // XP needed for next level
.XPProgress                     // 0-1 progress value
.IsLevelUpPending               // Waiting for skill selection?
```

### XPGemPool

**Singleton Access:**
```csharp
XPGemPool.Instance
```

**Key Methods:**
```csharp
.SpawnGem(Vector3 pos, int xpValue)
.SpawnMultipleGems(Vector3 pos, int totalXP, int count)
.ClearAllGems()
```

---

## Configuration

### XPSettings ScriptableObject

Create via: Right-click > Create > RoombaRampage > XP Settings

**Key Settings:**
- **Base XP:** Starting XP requirement (default: 100)
- **Exponent:** Growth curve steepness (default: 1.5)
- **Max Level:** Level cap (default: 30)
- **Magnet Range:** Pull distance for gems (default: 6 units)
- **Magnet Speed:** Pull speed multiplier (default: 8)

**Recommended Settings for 10-Min Runs:**
```
Base XP: 100
Exponent: 1.5
Max Level: 30
Target: 10-15 level-ups per run
```

---

## Balancing Guide

### XP Curve Formula

```
XP Required = BaseXP * (Level ^ Exponent)
```

**Example Curves:**

**Easy (Fast Leveling):**
- Base: 80, Exponent: 1.3 → 15-20 levels in 10 min

**Medium (Recommended):**
- Base: 100, Exponent: 1.5 → 10-15 levels in 10 min

**Hard (Slow Leveling):**
- Base: 120, Exponent: 1.7 → 5-10 levels in 10 min

### Enemy XP Values

Recommended XP rewards:

```
Basic Enemy (100 HP):    25 XP
Fast Enemy (40 HP):      40 XP (high risk)
Tank Enemy (200 HP):     50 XP (high HP)
Boss Enemy (1000 HP):    200-500 XP
```

**Rule of Thumb:** XP correlates with difficulty/health.

---

## Testing & Debug Tools

### Debug GUI (In-Game)

Enable "Show Debug Info" on components to see:

**XPManager (Top-Right):**
- Current Level & XP
- XP Progress
- Level-Up Pending flag
- Buttons: Add XP, Reset

**XPGemPool (Below XPManager):**
- Available/Active gem counts
- Pool statistics
- Buttons: Spawn test gems

### Context Menu Commands

**XPManager:**
- Test: Add 100 XP
- Test: Force Level Up
- Test: Set Level 10

**XPSettings:**
- Print XP Curve (Levels 1-20)
- Estimate Levels in 10-Min Run

### Test Checklist

- [ ] Enemies spawn XP gems on death
- [ ] Gems fly toward player when in range
- [ ] XP bar updates when gems collected
- [ ] Level-up triggers at correct XP threshold
- [ ] Level-up effects play (VFX, sound, heal)
- [ ] Game pauses for skill selection
- [ ] Pool handles 50+ gems without lag

---

## Performance Notes

### Object Pooling

XP gems use object pooling for optimal performance:

- **Initial Pool:** 100 gems (configurable)
- **Auto-Expand:** Grows by 50 if depleted
- **Reuse:** Gems return to pool after collection

**Expected Performance:**
- 100 active gems: 60 FPS
- 200 active gems: 45+ FPS

### Optimization Tips

If experiencing lag with many gems:

1. Increase initial pool size (200-300)
2. Simplify gem mesh (use sphere)
3. Reduce particle effects
4. Increase magnet range (faster collection)
5. Disable gem pulse animation

---

## Future Systems

### Skill System (Next)

**Integration Points Prepared:**
- Level-up pauses game
- IsLevelUpPending flag available
- OnLevelUp event broadcasts new level
- CompleteLevelUp() resumes game

**Required Implementation:**
- Skill selection UI
- Skill data system
- Skill application logic
- Skill tree visualization

### Meta-Progression (Future)

Persistent upgrades between runs:

- Permanent stat boosts
- Starting gear unlocks
- XP multipliers
- Starting level bonuses

---

## Troubleshooting

### Common Issues

**XP Gems Not Spawning:**
- Verify XPGemPool exists in scene
- Check enemy XP values > 0
- Verify gem prefab assigned

**Gems Not Moving Toward Player:**
- Check player has "Player" tag
- Increase magnet range in settings
- Verify XPGem component on prefab

**XP Bar Not Updating:**
- Verify XPBar has Fill Image assigned
- Check OnEnable subscribes to events
- Enable debug info to see values

**Full troubleshooting guide:** See [XP_SETUP.md](./XP_SETUP.md#troubleshooting)

---

## Dependencies

**Unity Systems:**
- Physics (3D) - for gem collection triggers
- UI (Canvas, Image, TextMeshPro) - for XP bar
- Audio - for level-up and collection sounds

**Game Systems:**
- Enemy system (EnemyData, Enemy.cs)
- Player (Player tag, PlayerHealth)
- ScoreManager (similar singleton pattern)

**External Assets:**
- TextMeshPro (included with Unity)
- URP (Universal Render Pipeline)

---

## File Locations

**Scripts:**
```
Assets/_Project/Scripts/Progression/XP/
```

**Data Assets:**
```
Assets/_Project/Data/Progression/
  └── XPSettings.asset
```

**Prefabs:**
```
Assets/_Project/Prefabs/XP/
  └── XPGem.prefab
```

---

## Version History

**v1.0 - 2025-10-30**
- Initial XP & Leveling System
- XPManager singleton
- XPGem visual pickups
- XPGemPool object pooling
- Enemy integration
- XPBar UI integration
- Full documentation

---

## Support

**Documentation:**
- [XP_SETUP.md](./XP_SETUP.md) - Complete setup guide
- [CLAUDE.md](../../../CLAUDE.md) - Project overview

**Debug Commands:**
- Use context menu on components
- Enable "Show Debug Info" for runtime GUI
- Check console for detailed logs

---

**Last Updated:** 2025-10-30
**Unity Version:** 6000.2.6f2
