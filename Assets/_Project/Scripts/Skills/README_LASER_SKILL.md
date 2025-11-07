# Laser Skill Setup Guide

Complete guide to configuring the autofire laser skill system.

---

## Quick Setup (5 Minutes)

1. **Create Laser Prefab** (see [Laser Prefab Setup](#laser-prefab-setup))
2. **Create LaserSkillData asset** (see [Creating Skill Assets](#creating-skill-assets))
3. **Add SkillManager to Player** (see [Player Setup](#player-setup))
4. **Assign Skill** and test!

---

## System Overview

### Architecture
- **SkillData.cs**: Base ScriptableObject for all skills
- **LaserSkillData.cs**: Laser-specific skill configuration
- **LaserBeam.cs**: Visual component for laser rendering
- **SkillManager.cs**: Manages skill cooldowns and activation

### How It Works
1. SkillManager ticks down cooldowns every frame
2. When cooldown reaches 0 and skill is autofire, it activates
3. LaserSkillData generates random directions and fires lasers
4. Each laser raycasts, damages enemies, and creates visuals
5. LaserBeam component renders the laser and auto-destroys

---

## Laser Prefab Setup

### Creating the Laser Prefab

1. **Create Empty GameObject**:
   - In Unity: `GameObject > Create Empty`
   - Name it: `LaserBeam`

2. **Add LineRenderer Component**:
   - Click `Add Component > Effects > Line Renderer`
   - Configure as follows:

   **Positions**:
   - Position Count: `2`
   - Position 0: `(0, 0, 0)`
   - Position 1: `(0, 0, 1)`

   **Width**:
   - Width: `0.1` (will be overridden by script)
   - Start Width: `0.1`
   - End Width: `0.1`

   **Color**:
   - Color Gradient: Start = Red, End = Red (will be overridden)

   **Materials**:
   - Material: Use built-in `Default-Line` material OR create custom material:
     - Right-click in Project > Create > Material
     - Name: `LaserMaterial`
     - Shader: `Particles/Standard Unlit` or `Unlit/Color`
     - Set color to bright red/cyan/your choice
     - Assign to LineRenderer material slot

   **Optional Settings** (for better visuals):
   - Alignment: `View` (faces camera)
   - Texture Mode: `Stretch`
   - Shadow Casting: `Off`
   - Receive Shadows: `Off`
   - Light Probes: `Off`

3. **Add LaserBeam Script**:
   - Click `Add Component`
   - Search for `LaserBeam`
   - Add the component

4. **Create Prefab**:
   - Drag `LaserBeam` GameObject from Hierarchy to Project window
   - Save in: `Assets/_Project/Prefabs/Skills/LaserBeam.prefab`
   - Delete from scene

### Laser Material (Optional but Recommended)

For better visuals, create a custom laser material:

1. **Create Material**:
   ```
   Project Window:
   Right-click > Create > Material
   Name: "LaserMaterial"
   ```

2. **Configure Material**:
   - **Shader**: `Particles/Standard Unlit` (glows) OR `Unlit/Color` (simple)
   - **Rendering Mode**: `Fade` or `Transparent`
   - **Color**: Bright color (red, cyan, yellow)
   - **Emission**: Enable and set same color for glow effect

3. **Assign to Prefab**:
   - Open `LaserBeam` prefab
   - Select LineRenderer component
   - Drag `LaserMaterial` to Materials > Element 0

---

## Creating Skill Assets

### Create LaserSkillData Asset

1. **Create Asset**:
   ```
   Project Window:
   Right-click > Create > RoombaRampage > Skills > Laser Skill
   Name: "LaserSkill_Level1"
   ```

2. **Configure in Inspector**:

   **Skill Info**:
   - Skill Name: `"Laser Burst"`
   - Description: `"Fires lasers in random directions that pierce through enemies."`
   - Icon: _(Optional) Assign a sprite for UI_

   **Cooldown Settings**:
   - Cooldown: `2.0` seconds
   - Auto Fire: `✓ Enabled`

   **Level Scaling**:
   - Starting Level: `1`
   - Max Level: `5`

   **Laser Settings**:
   - **Laser Prefab**: Drag your `LaserBeam` prefab here _(REQUIRED!)_
   - Base Damage: `20`
   - Base Laser Count: `1` (level 1 fires 1 laser)
   - Lasers Per Level: `1` (each level adds 1 laser)
   - Pierce Count: `3` (hits 3 enemies)
   - Laser Range: `30` meters
   - Laser Duration: `0.3` seconds (visual)
   - Laser Width: `0.1` units
   - Laser Color: Red `(1, 0, 0, 1)`
   - Hit Layer: `Everything` (or set to only hit enemies)

### Example Configurations

**Fast Laser** (rapid fire, low damage):
```
Cooldown: 0.5
Base Damage: 10
Base Laser Count: 1
Lasers Per Level: 1
Pierce Count: 2
```

**Heavy Laser** (slow fire, high damage):
```
Cooldown: 4.0
Base Damage: 50
Base Laser Count: 3
Lasers Per Level: 2
Pierce Count: 5
```

**Shotgun Laser** (many weak lasers):
```
Cooldown: 1.5
Base Damage: 8
Base Laser Count: 5
Lasers Per Level: 3
Pierce Count: 1
```

---

## Player Setup

### Add SkillManager to Player

1. **Select Player GameObject**
2. **Add Component**: Search `SkillManager` and add it
3. **Configure SkillManager**:
   - **Skills**: Expand the list
   - Click `+` to add a skill slot
   - **Skill Data**: Drag your `LaserSkill_Level1` asset here
   - **Level**: Set to `1` (starting level)
   - **Is Enabled**: `✓ Checked`
   - **Show Debug Info**: `✓ Checked` (for testing)

### Testing

1. **Enter Play Mode**
2. **Observe**:
   - Debug info appears in top-left corner
   - Shows skill cooldown timer
   - Lasers fire automatically when cooldown reaches 0
3. **Watch Scene View**:
   - Red laser lines should appear from player
   - Lasers point in random directions
   - Check if enemies take damage (if present)

---

## Layer Configuration (Important!)

For lasers to hit enemies, configure layers:

1. **Create Layers** (if not already done):
   ```
   Edit > Project Settings > Tags and Layers

   Add layers:
   - Layer 8: "Player"
   - Layer 9: "Enemy"
   - Layer 10: "Projectile"
   ```

2. **Set GameObject Layers**:
   - Player: Set layer to `Player`
   - Enemies: Set layer to `Enemy`

3. **Configure LaserSkillData**:
   - Hit Layer: Select `Enemy` layer only
   - This ensures lasers only hit enemies, not environment

4. **Collision Matrix** (optional):
   ```
   Edit > Project Settings > Physics > Layer Collision Matrix

   Ensure Enemy layer collides with:
   - Player (for collision damage)
   - Projectile (for weapon hits)
   ```

---

## Leveling Up Skills

### Via Inspector (Testing)

1. Select Player GameObject
2. Find SkillManager component
3. Expand Skills list
4. Increase `Level` value (1-5)
5. Observe more lasers firing

### Via Code (Runtime)

```csharp
using RoombaRampage.Skills;

public class LevelUpExample : MonoBehaviour
{
    [SerializeField] private SkillData laserSkill;
    private SkillManager skillManager;

    void Start()
    {
        skillManager = GetComponent<SkillManager>();
    }

    // Call this when player levels up
    public void OnPlayerLevelUp()
    {
        bool success = skillManager.LevelUpSkill(laserSkill);
        if (success)
        {
            Debug.Log("Laser skill leveled up!");
        }
    }

    // Add skill at runtime
    public void GrantLaserSkill()
    {
        skillManager.AddSkill(laserSkill, level: 1);
    }
}
```

---

## Advanced Customization

### Creating Custom Laser Effects

#### Pulsing Laser
Modify `LaserBeam.cs` Update():
```csharp
// Add pulsing effect
float pulse = Mathf.Sin(Time.time * 10f) * 0.5f + 0.5f;
lineRenderer.startWidth = width * (0.5f + pulse * 0.5f);
lineRenderer.endWidth = width * (0.5f + pulse * 0.5f);
```

#### Multi-Color Laser
In `LaserSkillData`, change colors per laser:
```csharp
Color[] colors = { Color.red, Color.blue, Color.green, Color.yellow };
Color laserColor = colors[i % colors.length];
```

### Custom Skill Behavior

Create your own skill by inheriting `SkillData`:

```csharp
using RoombaRampage.Skills;

[CreateAssetMenu(fileName = "CustomSkill", menuName = "RoombaRampage/Skills/Custom Skill")]
public class CustomSkillData : SkillData
{
    public override void Activate(GameObject caster, int level)
    {
        // Your custom skill logic here
        Debug.Log($"Custom skill activated at level {level}");
    }
}
```

---

## Troubleshooting

### Lasers Don't Appear

**Problem**: No visual lasers
**Solutions**:
- Check `LaserPrefab` is assigned in LaserSkillData
- Verify LaserBeam prefab has LineRenderer component
- Check laser duration > 0
- Ensure material is assigned to LineRenderer

### Lasers Don't Damage Enemies

**Problem**: Lasers appear but enemies don't take damage
**Solutions**:
- Verify enemies have tag `"Enemy"`
- Check enemies have `EnemyHealth` component
- Ensure Hit Layer includes Enemy layer
- Verify laser range is sufficient

### Skill Doesn't Fire

**Problem**: Skill never activates
**Solutions**:
- Check Auto Fire is enabled in SkillData
- Verify skill is in SkillManager Skills list
- Ensure `Is Enabled` is checked
- Check cooldown isn't too high

### Performance Issues

**Problem**: Game lags with many lasers
**Solutions**:
- Reduce laser duration (e.g., 0.1-0.2s)
- Increase cooldown to fire less frequently
- Reduce base laser count
- Use simpler LineRenderer material
- Disable LineRenderer shadows/light probes

---

## Skill Scaling Example

Level progression for a typical laser skill:

| Level | Laser Count | Total Damage/Cast | Cooldown |
|-------|-------------|-------------------|----------|
| 1     | 1           | 20                | 2.0s     |
| 2     | 2           | 40                | 2.0s     |
| 3     | 3           | 60                | 2.0s     |
| 4     | 4           | 80                | 2.0s     |
| 5     | 5           | 100               | 2.0s     |

To add cooldown reduction per level, override in LaserSkillData:

```csharp
public override float GetCooldown(int level)
{
    // Reduce cooldown by 0.1s per level
    return Mathf.Max(0.5f, cooldown - (0.1f * (level - 1)));
}
```

---

## File Structure

```
Assets/_Project/
├── Scripts/
│   └── Skills/
│       ├── README_LASER_SKILL.md      <- This file
│       ├── SkillManager.cs            <- Manages all skills
│       ├── LaserBeam.cs               <- Laser visual
│       └── Data/
│           ├── SkillData.cs           <- Base skill class
│           └── LaserSkillData.cs      <- Laser implementation
└── Prefabs/
    └── Skills/
        └── LaserBeam.prefab           <- Laser visual prefab
```

---

## Testing Checklist

- [ ] LaserBeam prefab created with LineRenderer
- [ ] Material assigned to LineRenderer
- [ ] LaserSkillData asset created
- [ ] Laser prefab assigned to skill data
- [ ] SkillManager added to Player
- [ ] Skill added to SkillManager's Skills list
- [ ] Debug info enabled
- [ ] Play mode: Lasers appear every X seconds
- [ ] Lasers damage enemies (if present)
- [ ] Level up increases laser count
- [ ] No console errors

---

## Next Steps

1. **Create more skill types**:
   - Orbital shields
   - Homing missiles
   - AOE explosions
   - Damage-over-time fields

2. **Add skill UI**:
   - Cooldown indicators
   - Skill icons
   - Level display

3. **Implement upgrade system**:
   - Skill selection on level up
   - Skill tree
   - Synergies between skills

4. **Visual effects**:
   - Particle systems on laser start/end
   - Screen shake on activation
   - Audio feedback

---

## Support

For issues:
1. Check Debug Info is enabled on SkillManager
2. Verify all required components are present
3. Check Unity Console for errors
4. Ensure layers and tags are configured correctly
5. Test with a simple 1-laser setup first

**Happy skill-building!** 🎯⚡
