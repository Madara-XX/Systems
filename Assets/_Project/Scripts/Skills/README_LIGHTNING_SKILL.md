# Lightning Strike Skill Setup Guide

Complete guide to configuring the lightning strike skill system with status effects.

---

## Quick Setup (5 Minutes)

1. **Create Lightning Prefab** (see [Lightning Prefab Setup](#lightning-prefab-setup))
2. **Create LightningStrikeSkillData asset** (see [Creating Skill Assets](#creating-skill-assets))
3. **Add SkillManager to Player** (if not already added)
4. **Assign Skill** and test!

---

## System Overview

### Architecture
- **StatusEffect.cs**: Reusable status effect system (Burn, Slow, Stun, Poison, Freeze)
- **StatusEffectManager.cs**: Manages active effects on enemies
- **LightningStrikeSkillData.cs**: Lightning skill configuration
- **LightningStrike.cs**: Visual component for jagged lightning rendering
- **SkillManager.cs**: Manages skill cooldowns and activation

### How It Works
1. SkillManager activates skill when cooldown reaches 0
2. LightningStrikeSkillData finds random enemies in range
3. For each target, creates lightning strike from sky to enemy
4. Deals instant damage and applies status effect
5. LightningStrike component renders jagged lightning bolt
6. StatusEffectManager handles DoT, slow, stun effects over time

---

## Lightning Prefab Setup

### Creating the Lightning Prefab

1. **Create Empty GameObject**:
   - In Unity: `GameObject > Create Empty`
   - Name it: `LightningStrike`

2. **Add LineRenderer Component**:
   - Click `Add Component > Effects > Line Renderer`
   - Configure as follows:

   **Positions**:
   - Position Count: `11` (will be overridden by script)
   - Positions: Default values (will be overridden)

   **Width**:
   - Width: `0.3` (will be overridden by script)
   - Start Width: `0.3`
   - End Width: `0.15` (tapers at end)

   **Color**:
   - Color Gradient: Start = Light Blue, End = Light Blue
   - Alpha: Start = 1.0, End = 1.0

   **Materials**:
   - Material: Create custom material (see below) OR use `Default-Line`

   **Optional Settings** (for better visuals):
   - Alignment: `View` (faces camera)
   - Texture Mode: `Stretch`
   - Shadow Casting: `Off`
   - Receive Shadows: `Off`
   - Light Probes: `Off`

3. **Add LightningStrike Script**:
   - Click `Add Component`
   - Search for `LightningStrike`
   - Add the component
   - Configure:
     - Segments: `10-20` (more = more jagged, but more expensive)
     - Displacement: `0.5-2.0` (how wild the lightning looks)

4. **Create Prefab**:
   - Drag `LightningStrike` GameObject from Hierarchy to Project window
   - Save in: `Assets/_Project/Prefabs/Skills/LightningStrike.prefab`
   - Delete from scene

### Lightning Material (Recommended)

For glowing lightning effect:

1. **Create Material**:
   ```
   Project Window:
   Right-click > Create > Material
   Name: "LightningMaterial"
   ```

2. **Configure Material**:
   - **Shader**: `Particles/Standard Unlit` (glows) OR `Universal Render Pipeline/Unlit`
   - **Rendering Mode**: `Additive` or `Transparent`
   - **Color**: Light blue/white (0.5, 0.5, 1, 1)
   - **Emission**: Enable and set to bright blue/white for glow

3. **Assign to Prefab**:
   - Open `LightningStrike` prefab
   - Select LineRenderer component
   - Drag `LightningMaterial` to Materials > Element 0

---

## Creating Skill Assets

### Create LightningStrikeSkillData Asset

1. **Create Asset**:
   ```
   Project Window:
   Right-click > Create > RoombaRampage > Skills > Lightning Strike Skill
   Name: "LightningStrike_Level1"
   ```

2. **Configure in Inspector**:

   **Skill Info**:
   - Skill Name: `"Lightning Storm"`
   - Description: `"Strikes random enemies with lightning from the sky, dealing damage and burning them."`
   - Icon: _(Optional) Assign a sprite for UI_

   **Cooldown Settings**:
   - Cooldown: `3.0` seconds
   - Auto Fire: `✓ Enabled`

   **Level Scaling**:
   - Starting Level: `1`
   - Max Level: `5`

   **Lightning Settings**:
   - **Lightning Prefab**: Drag your `LightningStrike` prefab here _(REQUIRED!)_
   - Base Damage: `50` (instant damage per strike)
   - Base Strike Count: `1` (level 1 hits 1 enemy)
   - Strikes Per Level: `1` (each level adds 1 strike)
   - Strike Range: `40` meters (search radius)
   - Lightning Duration: `0.5` seconds (visual duration)
   - Lightning Width: `0.3` units
   - Lightning Color: Light Blue `(0.5, 0.5, 1, 1)`
   - Hit Layer: `Everything` (or set to only hit enemies)
   - Lightning Origin Height: `30` (height above ground)
   - Strike Random Offset: `0.5` (position variance)

   **Status Effect** _(Choose one or set to None)_:

   **For Burn Effect**:
   - Effect Type: `Burn`
   - Duration: `3.0` seconds
   - Damage Per Second: `5` (DoT damage)
   - Effect Color: Orange `(1, 0.5, 0, 1)`

   **For Slow Effect**:
   - Effect Type: `Slow`
   - Duration: `4.0` seconds
   - Movement Speed Multiplier: `0.5` (50% speed)
   - Effect Color: Light Blue `(0.5, 0.5, 1, 1)`

   **For Stun Effect**:
   - Effect Type: `Stun`
   - Duration: `2.0` seconds
   - Effect Color: Yellow `(1, 1, 0, 1)`

### Example Configurations

**Burn Lightning** (DoT damage):
```
Base Damage: 40
Base Strike Count: 1
Strikes Per Level: 1
Cooldown: 3.0s
Status Effect: Burn
  Duration: 4s
  Damage Per Second: 8
```

**Crowd Control Lightning** (slow enemies):
```
Base Damage: 30
Base Strike Count: 3
Strikes Per Level: 2
Cooldown: 4.0s
Status Effect: Slow
  Duration: 5s
  Movement Speed Multiplier: 0.3
```

**Stun Lightning** (disable enemies):
```
Base Damage: 60
Base Strike Count: 1
Strikes Per Level: 1
Cooldown: 5.0s
Status Effect: Stun
  Duration: 2s
```

**Pure Damage Lightning** (no effect):
```
Base Damage: 100
Base Strike Count: 2
Strikes Per Level: 1
Cooldown: 3.5s
Status Effect: None
```

---

## Player Setup

### Add Skill to SkillManager

1. **Select Player GameObject**
2. **Find SkillManager component** (add if not present)
3. **Configure SkillManager**:
   - **Skills**: Expand the list
   - Click `+` to add a skill slot
   - **Skill Data**: Drag your `LightningStrike_Level1` asset here
   - **Level**: Set to `1` (starting level)
   - **Is Enabled**: `✓ Checked`
   - **Show Debug Info**: `✓ Checked` (for testing)

### Testing

1. **Enter Play Mode**
2. **Observe**:
   - Debug info appears in top-left corner
   - Shows skill cooldown timer
   - Lightning strikes random enemies when cooldown reaches 0
3. **Watch Scene View**:
   - Lightning bolts strike from sky to enemies
   - Jagged lightning effect with random path
   - Check if enemies take damage (if present)
   - If status effect enabled, enemies should show effect indicator

---

## Status Effects System

### Available Effect Types

| Effect Type | Description | Parameters Used |
|-------------|-------------|-----------------|
| **None** | No effect | - |
| **Burn** | Damage over time (fire) | `damagePerSecond`, `duration` |
| **Slow** | Reduces movement speed | `movementSpeedMultiplier`, `duration` |
| **Stun** | Prevents movement and actions | `duration` |
| **Poison** | Damage over time (poison) | `damagePerSecond`, `duration` |
| **Freeze** | Stops all movement | `duration` |

### Status Effect Configuration

**Burn/Poison**:
- Ticks damage every second
- Use `damagePerSecond` to control DoT damage
- Duration controls how long effect lasts
- Stacks by refreshing duration

**Slow**:
- Reduces enemy movement speed
- `0.5` = 50% speed, `0.1` = 10% speed
- Does not prevent actions
- Lowest multiplier wins if multiple slow effects

**Stun/Freeze**:
- Completely stops movement
- Prevents enemy actions
- Use shorter durations for balance

### StatusEffectManager Component

The `StatusEffectManager` is automatically added to enemies when they're hit by skills with status effects.

**Key Features**:
- Tracks multiple active effects simultaneously
- Handles effect stacking (refreshes duration)
- Automatically removes expired effects
- Provides movement speed modifiers
- Handles damage-over-time ticks

---

## Layer Configuration

For lightning to hit enemies, configure layers:

1. **Create Layers** (if not already done):
   ```
   Edit > Project Settings > Tags and Layers

   Add layers:
   - Layer 8: "Player"
   - Layer 9: "Enemy"
   ```

2. **Set GameObject Layers**:
   - Player: Set layer to `Player`
   - Enemies: Set layer to `Enemy`

3. **Configure LightningStrikeSkillData**:
   - Hit Layer: Select `Enemy` layer only
   - This ensures lightning only hits enemies

---

## Leveling Up Skills

### Via Inspector (Testing)

1. Select Player GameObject
2. Find SkillManager component
3. Expand Skills list
4. Increase `Level` value (1-5)
5. Observe more lightning strikes

### Via Code (Runtime)

```csharp
using RoombaRampage.Skills;

public class LevelUpExample : MonoBehaviour
{
    [SerializeField] private SkillData lightningSkill;
    private SkillManager skillManager;

    void Start()
    {
        skillManager = GetComponent<SkillManager>();
    }

    // Call this when player levels up
    public void OnPlayerLevelUp()
    {
        bool success = skillManager.LevelUpSkill(lightningSkill);
        if (success)
        {
            Debug.Log("Lightning skill leveled up!");
        }
    }

    // Add skill at runtime
    public void GrantLightningSkill()
    {
        skillManager.AddSkill(lightningSkill, level: 1);
    }
}
```

---

## Advanced Customization

### Creating Custom Status Effects

Add new effects by extending the `StatusEffectType` enum:

```csharp
// In StatusEffect.cs
public enum StatusEffectType
{
    None,
    Burn,
    Slow,
    Stun,
    Poison,
    Freeze,
    Bleed,      // Add your custom effect
    Weaken      // Add another
}
```

Then handle it in `ActiveStatusEffect.Update()`:

```csharp
// Handle custom effect logic
if (data.effectType == StatusEffectType.Bleed)
{
    // Bleed logic here
}
```

### Flickering Lightning Effect

To make lightning flicker, modify `LightningStrike.cs`:

```csharp
[SerializeField] private float flickerInterval = 0.1f;
private float flickerTimer;
private Vector3 startPos, endPos;

public void Initialize(Vector3 start, Vector3 end, ...)
{
    // Store positions
    startPos = start;
    endPos = end;

    // ... existing code ...
}

private void Update()
{
    // ... existing fade code ...

    // Flicker effect
    flickerTimer += Time.deltaTime;
    if (flickerTimer >= flickerInterval)
    {
        Regenerate(startPos, endPos);
        flickerTimer = 0f;
    }
}
```

### Chain Lightning Effect

Modify `LightningStrikeSkillData.cs` to chain between enemies:

```csharp
private void StrikeEnemyWithChain(GameObject firstEnemy, int chainCount)
{
    GameObject currentEnemy = firstEnemy;

    for (int i = 0; i < chainCount; i++)
    {
        StrikeEnemy(currentEnemy, level);

        // Find next nearest enemy
        Vector3 pos = currentEnemy.transform.position;
        currentEnemy = EnemyTargeting.FindNearestEnemy(pos, chainRange);

        if (currentEnemy == null) break;
    }
}
```

---

## Troubleshooting

### Lightning Doesn't Appear

**Problem**: No visual lightning strikes
**Solutions**:
- Check `LightningPrefab` is assigned in LightningStrikeSkillData
- Verify LightningStrike prefab has LineRenderer component
- Check lightning duration > 0
- Ensure material is assigned to LineRenderer
- Check if enemies are in range (`Strike Range`)

### Lightning Doesn't Hit Enemies

**Problem**: Lightning appears but enemies don't take damage
**Solutions**:
- Verify enemies have tag `"Enemy"`
- Check enemies have `EnemyHealth` component
- Ensure Hit Layer includes Enemy layer
- Verify strike range is sufficient
- Check if enemies are within `Strike Range`

### Status Effects Don't Apply

**Problem**: Enemies aren't burning/slowing
**Solutions**:
- Verify `Status Effect Type` is not set to `None`
- Check effect duration > 0
- Ensure `StatusEffectManager` component is being added automatically
- Check Console for errors

### Skill Doesn't Fire

**Problem**: Skill never activates
**Solutions**:
- Check Auto Fire is enabled in SkillData
- Verify skill is in SkillManager Skills list
- Ensure `Is Enabled` is checked
- Check cooldown isn't too high
- Verify level is at least 1

### Performance Issues

**Problem**: Game lags with many lightning strikes
**Solutions**:
- Reduce lightning duration (e.g., 0.2-0.3s)
- Reduce segments count (5-10 instead of 20)
- Increase cooldown to fire less frequently
- Reduce base strike count
- Use simpler LineRenderer material
- Disable flickering effect if enabled

---

## Skill Scaling Example

Level progression for a typical lightning skill:

| Level | Strike Count | Total Damage/Cast | Cooldown |
|-------|--------------|-------------------|----------|
| 1     | 1            | 50 (+ DoT)        | 3.0s     |
| 2     | 2            | 100 (+ DoT)       | 3.0s     |
| 3     | 3            | 150 (+ DoT)       | 3.0s     |
| 4     | 4            | 200 (+ DoT)       | 3.0s     |
| 5     | 5            | 250 (+ DoT)       | 3.0s     |

With Burn effect (5 DPS for 3s), total damage per cast at level 5:
- Instant: 250 damage
- DoT: 5 × 15 = 75 damage
- **Total: 325 damage**

---

## File Structure

```
Assets/_Project/
├── Scripts/
│   └── Skills/
│       ├── README_LIGHTNING_SKILL.md   <- This file
│       ├── SkillManager.cs              <- Manages all skills
│       ├── LightningStrike.cs           <- Lightning visual
│       ├── Helpers/
│       │   ├── EnemyTargeting.cs        <- Enemy finding utilities
│       │   ├── StatusEffect.cs          <- Status effect types & data
│       │   └── StatusEffectManager.cs   <- Manages effects on enemies
│       └── Data/
│           ├── SkillData.cs             <- Base skill class
│           ├── LaserSkillData.cs        <- Laser implementation
│           └── LightningStrikeSkillData.cs <- Lightning implementation
└── Prefabs/
    └── Skills/
        ├── LaserBeam.prefab             <- Laser visual
        └── LightningStrike.prefab       <- Lightning visual
```

---

## Testing Checklist

- [ ] LightningStrike prefab created with LineRenderer
- [ ] Material assigned to LineRenderer
- [ ] LightningStrikeSkillData asset created
- [ ] Lightning prefab assigned to skill data
- [ ] Status effect configured (Burn/Slow/etc.)
- [ ] SkillManager added to Player
- [ ] Skill added to SkillManager's Skills list
- [ ] Debug info enabled
- [ ] Play mode: Lightning strikes enemies every X seconds
- [ ] Lightning strikes from sky to ground
- [ ] Enemies take damage
- [ ] Status effects apply (check enemy status)
- [ ] Level up increases strike count
- [ ] No console errors

---

## Combining Skills

You can now run **both Lightning and Laser skills** simultaneously:

1. Add both skills to SkillManager's Skills list
2. Set different cooldowns for variety
3. Lightning for burst damage + effects
4. Laser for consistent damage

**Example Build**:
- **Laser**: Random targeting, 1s cooldown, 20 damage, 3 pierce
- **Lightning**: 3 strikes, 3s cooldown, 50 damage + burn

---

## Next Steps

1. **Create more status-based skills**:
   - Ice nova (freeze nearby enemies)
   - Poison cloud (area DoT)
   - Chain lightning (bounces between enemies)
   - Earthquake (stun in area)

2. **Enhance status effects**:
   - Visual particle effects per status type
   - Sound effects for applying effects
   - Screen shake for heavy effects
   - Status icons above enemies

3. **Advanced features**:
   - Status effect stacking (multiple stacks)
   - Status effect combos (burn + slow = extra damage)
   - Immunity after effect expires
   - Status cleanse mechanic

4. **UI improvements**:
   - Show active effects on enemy health bar
   - Cooldown timers with icons
   - Effect duration bars

---

## Support

For issues:
1. Check Debug Info is enabled on SkillManager
2. Verify all required components are present
3. Check Unity Console for errors
4. Ensure layers and tags are configured correctly
5. Test with a simple 1-strike setup first
6. Verify enemies have EnemyHealth component

**Happy lightning skill-building!** ⚡🔥❄️
