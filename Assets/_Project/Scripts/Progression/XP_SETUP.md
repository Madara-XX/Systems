# XP & Leveling System Setup Guide

Complete setup guide for RoombaRampage's XP and Leveling System.

---

## Table of Contents

1. [System Overview](#system-overview)
2. [Quick Start (5 Minutes)](#quick-start-5-minutes)
3. [Component Reference](#component-reference)
4. [Creating XP Settings Asset](#creating-xp-settings-asset)
5. [Creating XP Gem Prefab](#creating-xp-gem-prefab)
6. [Scene Setup](#scene-setup)
7. [Enemy Integration](#enemy-integration)
8. [XP Curve Balancing](#xp-curve-balancing)
9. [Skill System Integration](#skill-system-integration)
10. [Testing & Validation](#testing--validation)
11. [Troubleshooting](#troubleshooting)
12. [Performance Optimization](#performance-optimization)

---

## System Overview

### Architecture

The XP system consists of 4 core components:

1. **XPManager** - Singleton that manages XP accumulation, leveling, and events
2. **XPSettings** - ScriptableObject configuration for XP curve and visuals
3. **XPGem** - Visual pickup that flies toward player
4. **XPGemPool** - Object pooling for performance optimization

### Features

- Exponential XP curve (configurable)
- Magnetic XP gems that fly to player
- Object pooling for 100+ simultaneous gems
- Level-up effects (VFX, SFX, time slow, healing)
- Pause game for skill selection on level-up
- Save/load support via PlayerPrefs
- Maximum level capping
- Full event system for UI integration

### Data Flow

```
Enemy Dies → Spawns XP Gem → Player Collects → XPManager.AddXP() → Level-Up Check → Events → UI Update
```

---

## Quick Start (5 Minutes)

### Step 1: Create XP Settings Asset

1. Right-click in `Assets/_Project/Data/` folder
2. Create > RoombaRampage > **XP Settings**
3. Name it `XPSettings`
4. Configure:
   - Base XP: **100**
   - Exponent: **1.5**
   - Max Level: **30**
   - Magnet Range: **6**
   - Magnet Speed: **8**

### Step 2: Create XP Gem Prefab

1. Create empty GameObject in scene: `XPGem`
2. Add 3D mesh (Sphere or custom gem model)
   - Scale: `(0.3, 0.3, 0.3)`
3. Add **Sphere Collider**:
   - Is Trigger: **Enabled**
   - Radius: **0.5**
4. Add **XPGem** component
5. Optional: Add Particle System for sparkles
6. Drag to `Assets/_Project/Prefabs/XP/` to create prefab
7. Delete from scene

### Step 3: Setup Scene Managers

1. Create empty GameObject: `XPManager`
2. Add **XPManager** component
3. Assign **XP Settings** asset
4. Optional: Assign Player Health reference

5. Create empty GameObject: `XPGemPool`
6. Add **XPGemPool** component
7. Assign **XP Settings** asset
8. Assign **XP Gem Prefab**

### Step 4: Configure Enemy XP Values

1. Open existing EnemyData assets in `Assets/_Project/Data/Enemies/`
2. Set XP Values:
   - Basic Enemy: **25 XP**
   - Fast Enemy: **40 XP**
   - Tank Enemy: **50 XP** (future)
3. Save assets

### Step 5: Test!

1. Press Play
2. Kill enemies - XP gems should spawn
3. Walk near gems - they should fly to player
4. XP bar should fill
5. On level-up: VFX, pause, skill selection (coming soon)

**System is now fully functional!**

---

## Component Reference

### XPManager.cs

**Purpose:** Core XP system singleton. Manages XP accumulation, level progression, and events.

**Key Methods:**
```csharp
XPManager.Instance.AddXP(int amount)           // Award XP
XPManager.Instance.SetLevel(int level)         // Set level (debug)
XPManager.Instance.ResetXP()                   // Reset for new run
XPManager.Instance.CompleteLevelUp()           // Resume after skill selection
XPManager.Instance.GetXPProgress()             // Get 0-1 progress
```

**Key Events:**
```csharp
OnXPGained(int currentXP, int xpNeeded)        // XP changed
OnLevelUp(int newLevel)                        // Level-up occurred
OnMaxLevelReached()                            // Hit level cap
```

**Inspector Fields:**
- XP Settings (required)
- Player Health (optional - for heal on level-up)
- Show Debug Info

---

### XPSettings.cs

**Purpose:** Configuration ScriptableObject for all XP system parameters.

**XP Curve Formula:**
```
XP Required = BaseXP * (Level ^ Exponent)
Example: Level 5 = 100 * (5 ^ 1.5) = 1118 XP
```

**Key Settings:**

**XP Curve:**
- Base XP: Starting XP requirement (default: 100)
- Exponent: Growth rate (default: 1.5)
- Max Level: Level cap (default: 30)

**XP Gem Configuration:**
- Magnet Range: Pull distance (default: 6 units)
- Magnet Speed: Pull speed multiplier (default: 8)
- Magnet Acceleration Curve: Speed-up curve as gem approaches
- Gem Lifetime: Auto-collect timeout (0 = infinite)
- Gem Spawn Height: Y offset when spawned (default: 0.5)
- Gem Spawn Spread: Random XZ scatter (default: 0.5)

**Object Pooling:**
- Initial Pool Size: Pre-spawned gems (default: 100)
- Auto Expand Pool: Create more if depleted (default: true)
- Pool Expand Amount: How many to add (default: 50)

**Level-Up Effects:**
- Level-Up VFX Prefab: Particle effect at player
- Level-Up Sound: Audio clip
- XP Collect Sound: Gem collection sound
- Enable Time Slow: Slow-mo effect (default: true)
- Level-Up Time Scale: Slow amount (default: 0.5 = half speed)
- Heal On Level-Up: Full health restore (default: true)
- Pause On Level-Up: For skill selection (default: true)
- Pause Delay: Wait before pause (default: 0.3s)

**Debug Methods:**
- Print XP Curve (Levels 1-20)
- Estimate Levels in 10-Min Run

---

### XPGem.cs

**Purpose:** Individual XP pickup that flies toward player.

**Features:**
- Magnetic attraction when in range
- Smooth movement with acceleration curve
- Visual rotation and pulse animation
- Auto-collection on timeout
- Poolable (returns to pool after collection)

**Inspector Fields:**
- XP Value: Amount awarded (default: 25)
- Rotation Speed: Visual spin (default: 180 deg/s)
- Enable Pulse Animation: Scale breathing effect
- Pulse Scale Range: Min/max scale (default: 0.9-1.1)
- Pulse Speed: Animation speed (default: 2)
- Sparkle Particles: Optional particle system

**Requirements:**
- Collider (trigger) - for player detection
- MeshRenderer or SpriteRenderer - for visuals

---

### XPGemPool.cs

**Purpose:** Object pool for XP gems. Optimizes performance by reusing gems.

**Features:**
- Pre-spawns pool at start
- Auto-expands if depleted
- Tracks active/available gems
- Clean hierarchy organization

**Key Methods:**
```csharp
XPGemPool.Instance.SpawnGem(Vector3 pos, int xpValue)
XPGemPool.Instance.SpawnMultipleGems(Vector3 pos, int totalXP, int count)
XPGemPool.Instance.ReturnGem(XPGem gem)
XPGemPool.Instance.ClearAllGems()
```

**Inspector Fields:**
- XP Settings (required)
- Gem Prefab (required)
- Pool Parent (optional - auto-created)

---

## Creating XP Settings Asset

### Step-by-Step

1. Navigate to `Assets/_Project/Data/Progression/`
2. Right-click > Create > RoombaRampage > **XP Settings**
3. Name: `XPSettings`

### Recommended Settings for 10-Minute Runs

**Goal:** 10-15 level-ups in a 10-minute run

```
XP Curve:
  Base XP: 100
  Exponent: 1.5
  Max Level: 30

XP Gem Configuration:
  Magnet Range: 6 units
  Magnet Speed: 8
  Gem Lifetime: 30 seconds (or 0 for infinite)
  Gem Spawn Height: 0.5
  Gem Spawn Spread: 0.5

Object Pooling:
  Initial Pool Size: 100
  Auto Expand Pool: ✓
  Pool Expand Amount: 50

Level-Up Effects:
  Enable Time Slow: ✓
  Level-Up Time Scale: 0.5 (half speed)
  Heal On Level-Up: ✓
  Pause On Level-Up: ✓
  Pause Delay: 0.3 seconds
```

### Validating XP Curve

Use built-in context menu tools:

1. Select XPSettings asset
2. Right-click > **Print XP Curve (Levels 1-20)**
   - Prints XP requirements to console
3. Right-click > **Estimate Levels in 10-Min Run**
   - Calculates expected level-ups

**Expected Output:**
```
Level 1→2: 100 XP
Level 2→3: 212 XP
Level 3→4: 349 XP
Level 5→6: 671 XP
Level 10→11: 1995 XP
Level 15→16: 3711 XP

Estimated Final Level: 12-15
```

---

## Creating XP Gem Prefab

### Visual Style Options

#### Option A: Simple Sphere (Fastest)

1. Create GameObject: `XPGem`
2. Add **Sphere** mesh (3D Object > Sphere)
   - Position: (0, 0, 0)
   - Scale: (0.3, 0.3, 0.3)
3. Material: Create glowing material
   - Shader: URP/Lit or URP/Unlit
   - Base Color: Cyan (0.3, 0.8, 1.0)
   - Emission: Enabled, cyan, intensity 2
4. Add **Sphere Collider**:
   - Is Trigger: ✓
   - Radius: 0.5
5. Add **XPGem** component
6. Save as prefab

#### Option B: Custom Gem Model (Better Visuals)

1. Import gem 3D model (e.g., from Asset Store)
2. Create GameObject: `XPGem`
3. Add imported model as child
   - Scale to ~0.5 units tall
4. Add **Sphere Collider** to root:
   - Is Trigger: ✓
   - Radius: 0.5
5. Add **XPGem** component to root
6. Configure material with emission
7. Save as prefab

#### Option C: Sprite-Based (2D Style)

1. Create GameObject: `XPGem`
2. Add **Sprite Renderer**
   - Sprite: Gem sprite texture
   - Material: Sprites-Default or custom lit sprite shader
3. Add **Sphere Collider**:
   - Is Trigger: ✓
   - Radius: 0.5
4. Add **XPGem** component
5. Set Billboard mode (face camera) if desired
6. Save as prefab

### Adding Particle Effects

1. Add child GameObject: `Sparkles`
2. Add **Particle System** component
3. Configure:
   - Start Lifetime: 0.5-1.0
   - Start Speed: 0.5
   - Start Size: 0.05-0.15
   - Start Color: White with alpha fade
   - Emission Rate: 10-20
   - Shape: Sphere, Radius 0.3
   - Color Over Lifetime: Fade out
   - Looping: ✓
4. Assign to XPGem component's Sparkle Particles field

### Material Setup (URP)

**Glowing XP Gem Material:**

1. Create material: `XPGem_Material`
2. Shader: **URP/Lit** or **URP/Unlit**
3. Settings:
   - Surface Type: Opaque
   - Base Map: None (or gem texture)
   - Base Color: Cyan (0.3, 0.8, 1.0, 1.0)
   - Emission: **Enabled**
   - Emission Map: None
   - Emission Color: Cyan (0.3, 0.8, 1.0)
   - Emission Intensity: **2.0** (HDR)
4. Apply to XP gem mesh

---

## Scene Setup

### Required GameObjects

Create these GameObjects in your main game scene:

#### 1. XPManager GameObject

```
GameObject: "XPManager"
  Components:
    - XPManager
      - XP Settings: [Assign XPSettings asset]
      - Player Health: [Optional - auto-finds player]
      - Show Debug Info: ✓ (for testing)
```

**Important:** XPManager is **DontDestroyOnLoad** - only needs to exist once!

#### 2. XPGemPool GameObject

```
GameObject: "XPGemPool"
  Components:
    - XPGemPool
      - XP Settings: [Assign XPSettings asset]
      - Gem Prefab: [Assign XPGem prefab]
      - Pool Parent: [Leave empty - auto-created]
      - Show Debug Info: ✓ (for testing)
```

**Important:** XPGemPool is **DontDestroyOnLoad** - only needs to exist once!

### Scene Hierarchy

```
Scene: GameScene
├── Player
│   └── (Existing player setup)
├── HUD Canvas
│   ├── HealthBar
│   ├── XPBar [Already exists]
│   └── ...
├── Managers
│   ├── GameManager
│   ├── ScoreManager
│   ├── XPManager ← NEW
│   └── XPGemPool ← NEW
└── ...
```

### Testing Scene Setup

1. Create test scene or use existing gameplay scene
2. Add XPManager and XPGemPool GameObjects
3. Ensure Player exists with Player tag
4. Ensure HUD has XPBar component
5. Press Play - should see no errors

**Debug GUI:** If "Show Debug Info" enabled:
- Top-right: XP Manager stats
- Below that: XP Gem Pool stats
- Buttons to spawn test gems

---

## Enemy Integration

### Adding XP Rewards to Enemies

**All enemy prefabs automatically spawn XP gems on death!** Just set XP values in EnemyData assets.

#### Step 1: Update EnemyData Assets

1. Navigate to `Assets/_Project/Data/Enemies/`
2. Open each EnemyData asset
3. Set **XP Value** field (new field added)

**Recommended XP Values:**

```
Basic Enemy (100 HP):
  Score Value: 10
  XP Value: 25

Fast Enemy (40 HP):
  Score Value: 15
  XP Value: 40 (higher risk = more reward)

Tank Enemy (200 HP):
  Score Value: 20
  XP Value: 50 (high HP = high XP)

Boss Enemy (1000 HP):
  Score Value: 100
  XP Value: 200-500 (massive XP boost)
```

**Balancing Rule:** XP should roughly correlate with enemy difficulty/health.

#### Step 2: Verify Enemy.cs Integration

The `Enemy.cs` OnDeath() method already spawns XP gems:

```csharp
// Spawn XP gem
if (enemyData != null && enemyData.xpValue > 0 && XPGemPool.Instance != null)
{
    XPGemPool.Instance.SpawnGem(transform.position, enemyData.xpValue);
}
```

**No additional code needed!**

#### Advanced: Multiple XP Gems for Large Enemies

For boss enemies or high-value targets, spawn multiple smaller gems:

```csharp
// In Enemy.cs OnDeath() - replace single gem spawn with:
if (enemyData.xpValue > 100 && XPGemPool.Instance != null)
{
    // Spawn 5 gems for large enemies
    XPGemPool.Instance.SpawnMultipleGems(transform.position, enemyData.xpValue, 5);
}
else if (enemyData.xpValue > 0 && XPGemPool.Instance != null)
{
    // Single gem for normal enemies
    XPGemPool.Instance.SpawnGem(transform.position, enemyData.xpValue);
}
```

---

## XP Curve Balancing

### Understanding the XP Formula

```
XP Required for Level N = BaseXP * (N ^ Exponent)
```

**Parameters:**
- **BaseXP:** Starting difficulty (level 1→2)
- **Exponent:** Growth rate (1.0 = linear, 1.5 = moderate curve, 2.0 = steep curve)

### Example Curves

#### Easy Curve (Fast Leveling)
```
Base XP: 80
Exponent: 1.3
Max Level: 25

Level 1→2: 80 XP
Level 5→6: 334 XP
Level 10→11: 948 XP
Level 20→21: 2892 XP

Result: ~15-20 level-ups in 10 minutes
```

#### Medium Curve (Recommended)
```
Base XP: 100
Exponent: 1.5
Max Level: 30

Level 1→2: 100 XP
Level 5→6: 671 XP
Level 10→11: 1995 XP
Level 20→21: 7416 XP

Result: ~10-15 level-ups in 10 minutes
```

#### Hard Curve (Slow Leveling)
```
Base XP: 120
Exponent: 1.7
Max Level: 40

Level 1→2: 120 XP
Level 5→6: 992 XP
Level 10→11: 3590 XP
Level 20→21: 16,429 XP

Result: ~5-10 level-ups in 10 minutes
```

### Balancing for 10-Minute Runs

**Target Assumptions:**
- 10-minute run duration
- 25 enemies killed per minute (average)
- 250 total enemies killed
- Average XP per enemy: 30 XP
- **Total XP Available: ~7,500 XP**

**Using XPSettings Debug Tool:**

1. Select XPSettings asset
2. Right-click > **Estimate Levels in 10-Min Run**
3. Adjust Base XP and Exponent until output shows **10-15 levels**

### Fine-Tuning Tips

**If players level too fast:**
- Increase Base XP (+10-20)
- Increase Exponent (+0.1-0.2)
- Reduce enemy XP values (-5 per enemy)

**If players level too slow:**
- Decrease Base XP (-10-20)
- Decrease Exponent (-0.1-0.2)
- Increase enemy XP values (+5 per enemy)

**Monitor during playtests:**
- Track: Average level reached at 5 minutes
- Target: Level 6-8 at 5-minute mark
- Adjust curve accordingly

---

## Skill System Integration

The XP system is designed to integrate seamlessly with a future Skill System.

### Level-Up Flow

When a player levels up:

1. **XP Manager:** Detects level-up
2. **Effects Triggered:** VFX, SFX, time slow
3. **Pause Game:** Time.timeScale = 0
4. **Set Flag:** `XPManager.IsLevelUpPending = true`
5. **Skill System:** Detects flag, shows skill selection UI
6. **Player Chooses Skill**
7. **Skill System:** Calls `XPManager.Instance.CompleteLevelUp()`
8. **Game Resumes:** Time.timeScale = 1

### Integration Points

#### Detecting Level-Up in Skill System

```csharp
// In your SkillManager.cs or similar:
private void OnEnable()
{
    if (XPManager.Instance != null)
    {
        XPManager.Instance.OnLevelUp.AddListener(OnPlayerLevelUp);
    }
}

private void OnPlayerLevelUp(int newLevel)
{
    // Show skill selection UI
    ShowSkillSelectionUI(newLevel);
}
```

#### Resuming Game After Skill Selection

```csharp
// After player selects a skill:
private void OnSkillSelected(Skill selectedSkill)
{
    // Apply skill upgrade
    ApplySkill(selectedSkill);

    // Hide UI
    HideSkillSelectionUI();

    // Resume game
    XPManager.Instance.CompleteLevelUp();
}
```

#### Checking for Pending Level-Up

```csharp
// Useful for UI state management:
private void Update()
{
    if (XPManager.Instance.IsLevelUpPending && !skillUIVisible)
    {
        ShowSkillSelectionUI(XPManager.Instance.CurrentLevel);
    }
}
```

### XPSettings Configuration

Control pause behavior in XPSettings:

- **Pause On Level-Up:** Enable/disable pause for skill selection
- **Pause Delay:** Wait for VFX to play before pausing (default: 0.3s)

For testing without Skill System, disable "Pause On Level-Up" in XPSettings.

---

## Testing & Validation

### Test Checklist

#### Basic Functionality
- [ ] XPManager exists in scene
- [ ] XPGemPool exists in scene
- [ ] XPSettings asset assigned
- [ ] XP Gem prefab assigned
- [ ] Player has "Player" tag
- [ ] XPBar UI exists and updates
- [ ] No console errors on Play

#### XP Gem Spawning
- [ ] Kill enemy → XP gem spawns at death position
- [ ] Gem has visual (mesh/sprite visible)
- [ ] Gem rotates/animates
- [ ] Multiple enemies spawn multiple gems

#### Magnet & Collection
- [ ] Walk near gem → gem starts moving toward player
- [ ] Gem accelerates as it gets closer
- [ ] Touching gem collects it
- [ ] XP bar updates when collected
- [ ] Gem disappears after collection
- [ ] Collection sound plays (if assigned)

#### Leveling System
- [ ] Collect enough XP → level-up occurs
- [ ] Level-up VFX spawns at player (if assigned)
- [ ] Level-up sound plays (if assigned)
- [ ] XP bar resets to 0
- [ ] Level number increases
- [ ] Player heals to full (if enabled)
- [ ] Time slows briefly (if enabled)
- [ ] Game pauses for skill selection (if enabled)

#### Performance
- [ ] Spawn 50+ gems → no lag
- [ ] Pool auto-expands if needed
- [ ] Gems return to pool correctly
- [ ] No memory leaks after multiple runs

#### Edge Cases
- [ ] Level-up at max level → XP gain stops
- [ ] Multiple level-ups at once → all trigger correctly
- [ ] Gems auto-collect after lifetime expires
- [ ] System works after scene reload

### Debug Tools

**XPManager Debug GUI (Top-Right):**
- Current Level & XP
- XP Progress bar
- Level-Up Pending flag
- Buttons: Add XP, Reset XP

**XPGemPool Debug GUI (Below XPManager):**
- Available gems in pool
- Active gems in scene
- Total gems created
- Peak active count
- Buttons: Spawn test gems, Clear gems

**Context Menu Commands:**

**XPManager:**
- Test: Add 100 XP
- Test: Force Level Up
- Test: Set Level 10
- Test: Reset XP

**XPGemPool:**
- Test: Spawn 50 Gems
- Test: Clear All Gems
- Print Pool Stats

**XPSettings:**
- Print XP Curve (Levels 1-20)
- Estimate Levels in 10-Min Run

### Manual Test Procedure

1. **Setup Scene:**
   - Enable debug info on XPManager and XPGemPool
   - Start game

2. **Test XP Gain:**
   - Kill 5 enemies
   - Verify gems spawn and fly to player
   - Check XP bar fills correctly

3. **Test Level-Up:**
   - Use debug button "Add 500 XP" to force level-up
   - Verify all level-up effects trigger
   - Verify game pauses (if enabled)

4. **Test Multi-Level:**
   - Use debug button "Add 5000 XP"
   - Verify multiple level-ups work

5. **Test Pool Performance:**
   - Use debug button "Spawn 50 Gems"
   - Verify no lag
   - Check pool stats

6. **Test Max Level:**
   - Use debug button "Set Level 29"
   - Gain XP to hit level 30
   - Verify XP gain stops at max level

---

## Troubleshooting

### XP Gems Not Spawning

**Problem:** Enemies die but no gems appear.

**Solutions:**
- Verify XPGemPool exists in scene
- Verify Gem Prefab assigned in XPGemPool
- Verify EnemyData has xpValue > 0
- Check Enemy.cs has updated OnDeath() code
- Check console for pool errors

**Debug:**
```csharp
// Add to Enemy.cs OnDeath():
Debug.Log($"Spawning XP gem with value: {enemyData.xpValue}");
```

---

### XP Gems Not Moving Toward Player

**Problem:** Gems spawn but don't fly to player.

**Solutions:**
- Verify player has "Player" tag (case-sensitive)
- Check Magnet Range in XPSettings (increase to 10+ for testing)
- Verify XPGem component on prefab
- Check gem's collider is trigger
- Ensure gems are on XZ plane (Y = 0.5-1.0)

**Debug:**
- Enable "Show Debug Info" on XPGem prefab
- Watch for green gizmo line connecting gem to player

---

### XP Gems Not Collecting

**Problem:** Gems reach player but don't get collected.

**Solutions:**
- Verify player has Collider component
- Verify player has "Player" tag
- Check XPGem's collider is trigger
- Verify OnTriggerEnter fires (add debug log)

**Debug:**
```csharp
// Add to XPGem.cs OnTriggerEnter:
Debug.Log($"XP Gem triggered by: {other.gameObject.name} (tag: {other.tag})");
```

---

### XP Bar Not Updating

**Problem:** XP gained but UI doesn't update.

**Solutions:**
- Verify XPBar component exists in scene
- Check XPBar has Fill Image assigned
- Verify XPBar's OnEnable subscribes to events
- Check for console errors about null references

**Debug:**
- Enable "Show Debug Info" on XPBar
- Add breakpoint in OnXPChanged method

---

### Level-Up Not Triggering

**Problem:** XP bar fills but level-up doesn't occur.

**Solutions:**
- Check XP curve settings (may need too much XP)
- Verify XPManager has XPSettings assigned
- Use debug button "Force Level Up" to test
- Check console for errors

**Debug:**
```csharp
// Add to XPManager.CheckLevelUp():
Debug.Log($"Checking level-up: CurrentXP={currentXP}, Needed={XPForNextLevel}");
```

---

### Game Stuck Paused After Level-Up

**Problem:** Game pauses but never resumes.

**Solutions:**
- Verify Skill System calls `XPManager.Instance.CompleteLevelUp()`
- Disable "Pause On Level-Up" in XPSettings for testing
- Manually call CompleteLevelUp() from debug menu

**Debug:**
- Check Time.timeScale in inspector (should return to 1.0)
- Add debug log in CompleteLevelUp()

---

### Pool Performance Issues

**Problem:** Lag when many gems spawn.

**Solutions:**
- Increase Initial Pool Size (200-500)
- Reduce gem particle effects
- Simplify gem mesh (use sphere instead of complex model)
- Disable pulse animation on gems

**Debug:**
- Check "Peak Active" in XPGemPool debug GUI
- If peak > 200, consider reducing gem spawn rate

---

### Gems Spawning Underground

**Problem:** XP gems appear below floor.

**Solutions:**
- Increase Gem Spawn Height in XPSettings (try 1.0)
- Verify enemy death position is correct
- Check floor collider isn't blocking gems

---

### No Level-Up Effects

**Problem:** Level-up occurs but no VFX/sound.

**Solutions:**
- Assign Level-Up VFX Prefab in XPSettings
- Assign Level-Up Sound in XPSettings
- Check particle prefab works independently
- Verify audio source exists on XPManager

---

## Performance Optimization

### Recommended Pool Sizes

Based on enemy spawn rate:

**Low Enemy Density (10-20 enemies/minute):**
```
Initial Pool Size: 50
Pool Expand Amount: 25
```

**Medium Enemy Density (20-40 enemies/minute):**
```
Initial Pool Size: 100
Pool Expand Amount: 50
```

**High Enemy Density (40+ enemies/minute - bullet hell):**
```
Initial Pool Size: 200
Pool Expand Amount: 100
```

### Performance Metrics

**Target Performance:**
- 100 active gems: 60 FPS
- 200 active gems: 45+ FPS
- Pool expansion: < 1ms

**If experiencing lag:**

1. **Reduce Visual Complexity:**
   - Use simple sphere mesh
   - Reduce particle emission rate (10 → 5)
   - Disable pulse animation
   - Lower poly gem models

2. **Optimize Update Loops:**
   - Increase magnet range (gems collect faster)
   - Reduce gem lifetime (auto-collect sooner)
   - Increase magnet speed (faster collection)

3. **Pool Configuration:**
   - Increase initial pool size (reduce expansions)
   - Pre-warm pool in loading screen

4. **Culling:**
   - Disable gems far from player (future optimization)
   - Limit max active gems (cap at 150)

### Memory Optimization

**Pool grows indefinitely?**
- Set "Auto Expand Pool" to false in XPSettings
- Log warning when pool depletes
- Increase initial pool size instead

**Gems not returning to pool?**
- Check for exceptions in collection code
- Verify ReturnToPool() is called
- Monitor "Available" count in debug GUI

---

## Advanced Configuration

### Custom XP Gem Types

Create multiple gem prefabs for different XP values:

**Small XP Gem (10-25 XP):**
- Color: Green
- Size: Small (0.25 scale)
- Emission: Low

**Medium XP Gem (25-50 XP):**
- Color: Blue/Cyan
- Size: Medium (0.35 scale)
- Emission: Medium

**Large XP Gem (50+ XP):**
- Color: Purple/Gold
- Size: Large (0.5 scale)
- Emission: High
- Extra particles

### Dynamic XP Values

Modify XP based on game state:

```csharp
// In Enemy.cs OnDeath():
int xpToAward = enemyData.xpValue;

// Bonus XP for kill streaks
if (GameManager.Instance.CurrentKillStreak > 10)
{
    xpToAward = Mathf.RoundToInt(xpToAward * 1.5f);
}

// Bonus XP for combo multiplier
xpToAward = Mathf.RoundToInt(xpToAward * ComboManager.Instance.Multiplier);

XPGemPool.Instance.SpawnGem(transform.position, xpToAward);
```

### XP Vacuum Ability

Create a skill that instantly collects all nearby XP:

```csharp
public void VacuumNearbyXP(float range)
{
    Collider[] gems = Physics.OverlapSphere(player.position, range, LayerMask.GetMask("XPGem"));

    foreach (Collider gem in gems)
    {
        XPGem xpGem = gem.GetComponent<XPGem>();
        if (xpGem != null)
        {
            xpGem.ForceCollect();
        }
    }
}
```

---

## Summary

You now have a fully functional XP & Leveling System!

**Key Files Created:**
- `XPManager.cs` - Core XP logic
- `XPSettings.cs` - Configuration
- `XPGem.cs` - Pickup component
- `XPGemPool.cs` - Object pooling
- `EnemyData.cs` - Updated with xpValue field
- `Enemy.cs` - Updated to spawn gems
- `XPBar.cs` - Updated to integrate with system

**Next Steps:**
1. Create Skill System to integrate with level-ups
2. Add level-up VFX particle effects
3. Add audio clips for level-up and collection
4. Balance XP curve during playtesting
5. Create additional gem types for variety

**For Help:**
- Check Troubleshooting section
- Enable debug info on all components
- Use context menu test commands
- Monitor console for errors

---

**System Version:** 1.0
**Unity Version:** 6000.2.6f2
**Last Updated:** 2025-10-30
