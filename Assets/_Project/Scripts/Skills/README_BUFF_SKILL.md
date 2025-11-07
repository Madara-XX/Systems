# Buff Skill Setup Guide

Complete guide to configuring stat-boosting buff skills (permanent or temporary).

---

## Quick Setup (3 Minutes)

1. **Create BuffSkillData asset** (see [Creating Skill Assets](#creating-skill-assets))
2. **Configure buffs** (choose stats to enhance)
3. **Add to SkillManager** on Player
4. **Test!**

---

## System Overview

### Architecture
- **PlayerBuff.cs**: Reusable buff system for player stat modifications
- **PlayerBuffManager.cs**: Manages active buffs on player
- **BuffSkillData.cs**: Buff skill configuration
- **SkillManager.cs**: Manages skill cooldowns and activation

### How It Works
1. SkillManager activates buff skill when cooldown reaches 0 (or on trigger)
2. BuffSkillData applies configured buffs to player
3. PlayerBuffManager calculates modified stats
4. Buffs can be permanent or temporary
5. Temporary buffs automatically expire after duration
6. Multiple buffs can stack

---

## Available Stats to Buff

### Movement Stats
| Stat | Description | Base Value | Good Range |
|------|-------------|------------|------------|
| **MaxSpeed** | Maximum movement speed | 10 | 8-20 |
| **Acceleration** | How fast you accelerate | 15 | 10-30 |
| **RotationSpeed** | Turning speed (degrees/sec) | 150 | 90-360 |

### Combat Stats
| Stat | Description | Base Value | Good Range |
|------|-------------|------------|------------|
| **Damage** | Base damage per shot | 10 | 5-50 |
| **FireRate** | Time between shots (lower = faster) | 0.2s | 0.05-1.0s |
| **ProjectileSpeed** | Speed of projectiles | 20 | 10-50 |

### Health Stats
| Stat | Description | Base Value | Good Range |
|------|-------------|------------|------------|
| **MaxHealth** | Maximum health points | 100 | 50-500 |
| **HealthRegenRate** | HP regenerated per second | 0 | 0-10 |

### Turbo Stats
| Stat | Description | Base Value | Good Range |
|------|-------------|------------|------------|
| **TurboSpeedMultiplier** | Speed boost when turbo active | 1.8× | 1.2-3.0× |
| **TurboRegenRate** | Turbo energy regen per second | 15 | 5-50 |
| **MaxTurboEnergy** | Maximum turbo energy | 100 | 50-200 |

### Physics Stats
| Stat | Description | Base Value | Good Range |
|------|-------------|------------|------------|
| **Mass** | Player mass (affects physics) | 1 | 0.5-5 |
| **Drag** | Movement friction | 1.5 | 0-5 |

---

## Buff Application Types

### Add (Flat Bonus)
Adds a flat value to the base stat.

**Examples**:
- `+5 MaxSpeed` → 10 becomes 15
- `+20 Damage` → 10 becomes 30
- `+50 MaxHealth` → 100 becomes 150

**Best for**: Health, Damage, Flat speed bonuses

### Multiply (Percentage Bonus)
Multiplies the base stat by a value.

**Examples**:
- `×1.5 MaxSpeed` → 10 becomes 15
- `×2.0 Damage` → 10 becomes 20
- `×1.2 Acceleration` → 15 becomes 18

**Best for**: Scaling existing stats proportionally

### Override (Set Value)
Replaces the stat with a specific value.

**Examples**:
- `MaxSpeed = 20` → Always 20 regardless of base
- `Damage = 50` → Always 50

**Best for**: Special game modes, temporary power states

---

## Creating Skill Assets

### Create BuffSkillData Asset

1. **Create Asset**:
   ```
   Project Window:
   Right-click > Create > RoombaRampage > Skills > Buff Skill
   Name: "SpeedBoost_Permanent"
   ```

2. **Configure in Inspector**:

   **Skill Info**:
   - Skill Name: `"Speed Boost"`
   - Description: `"Permanently increases your movement speed."`
   - Icon: _(Optional) Assign a sprite for UI_

   **Cooldown Settings**:
   - Cooldown: `0` (instant, one-time buff)
   - Auto Fire: `✗ Disabled` (buff applies once)

   **Level Scaling**:
   - Starting Level: `1`
   - Max Level: `5`

   **Buff Configuration**:
   - **Buffs**: Click `+` to add a buff
   - **Scale With Level**: `✓` (buff gets stronger per level)
   - **Level Scaling**: `1.1` (10% increase per level)

3. **Configure Individual Buff**:
   - **Stat Type**: Choose stat to buff (e.g., `MaxSpeed`)
   - **Application Type**: Choose how to apply (e.g., `Multiply`)
   - **Value**: Buff amount (e.g., `1.5` for 50% boost)
   - **Duration**: `0` for permanent, or seconds for temporary
   - **Effect Color**: Visual color (e.g., Cyan for speed)

---

## Example Configurations

### Permanent Speed Boost
```
Skill Name: "Turbo Upgrade"
Cooldown: 0 (one-time)
Auto Fire: Disabled

Buff #1:
  Stat Type: MaxSpeed
  Application Type: Multiply
  Value: 1.3 (30% faster)
  Duration: 0 (permanent)
  Effect Color: Cyan

Buff #2:
  Stat Type: Acceleration
  Application Type: Multiply
  Value: 1.2 (20% faster acceleration)
  Duration: 0 (permanent)
  Effect Color: Cyan
```

### Temporary Damage Boost
```
Skill Name: "Rampage Mode"
Cooldown: 15 (15 second cooldown)
Auto Fire: Enabled

Buff:
  Stat Type: Damage
  Application Type: Multiply
  Value: 2.0 (double damage)
  Duration: 8 (8 seconds)
  Effect Color: Red
```

### Health Regeneration
```
Skill Name: "Healing Aura"
Cooldown: 30
Auto Fire: Enabled

Buff #1:
  Stat Type: HealthRegenRate
  Application Type: Add
  Value: 5 (5 HP per second)
  Duration: 10
  Effect Color: Green

Buff #2:
  Stat Type: MaxHealth
  Application Type: Multiply
  Value: 1.2 (20% more max HP)
  Duration: 0 (permanent)
  Effect Color: Green
```

### Turbo Master
```
Skill Name: "Turbo Mastery"
Cooldown: 0
Auto Fire: Disabled

Buff #1:
  Stat Type: TurboSpeedMultiplier
  Application Type: Multiply
  Value: 1.5 (50% faster turbo)
  Duration: 0 (permanent)

Buff #2:
  Stat Type: TurboRegenRate
  Application Type: Multiply
  Value: 2.0 (double regen)
  Duration: 0 (permanent)

Buff #3:
  Stat Type: MaxTurboEnergy
  Application Type: Multiply
  Value: 1.5 (50% more energy)
  Duration: 0 (permanent)
  Effect Color: Yellow
```

### Combat Enhancement
```
Skill Name: "Berserker"
Cooldown: 20
Auto Fire: Enabled

Buff #1:
  Stat Type: Damage
  Application Type: Multiply
  Value: 1.8 (80% more damage)
  Duration: 6

Buff #2:
  Stat Type: FireRate
  Application Type: Multiply
  Value: 0.6 (40% faster fire rate)
  Duration: 6

Buff #3:
  Stat Type: MaxSpeed
  Application Type: Multiply
  Value: 1.2 (20% faster movement)
  Duration: 6
  Effect Color: Red
```

### Glass Cannon
```
Skill Name: "High Risk High Reward"
Cooldown: 0
Auto Fire: Disabled

Buff #1:
  Stat Type: Damage
  Application Type: Multiply
  Value: 3.0 (triple damage!)
  Duration: 0 (permanent)

Buff #2:
  Stat Type: MaxHealth
  Application Type: Multiply
  Value: 0.5 (half health)
  Duration: 0 (permanent)
  Effect Color: Red
```

---

## Player Setup

### Add Skill to SkillManager

1. **Select Player GameObject**
2. **Find SkillManager component**
3. **Configure SkillManager**:
   - **Skills**: Click `+` to add skill slot
   - **Skill Data**: Drag `SpeedBoost_Permanent` asset
   - **Level**: Set to `1`
   - **Is Enabled**: `✓ Checked`

### For Permanent Buffs (One-Time)

**Option 1: Manual Activation**
- Set `Auto Fire` to `✗ Disabled`
- Set `Cooldown` to `0`
- Call via code when player picks up upgrade

**Option 2: Auto-Apply on Start**
- Set `Auto Fire` to `✓ Enabled`
- Set `Cooldown` to `0`
- Buff applies immediately when game starts

### For Temporary Buffs (Repeating)

- Set `Auto Fire` to `✓ Enabled`
- Set `Cooldown` to desired seconds (e.g., `15s`)
- Buff activates repeatedly on cooldown

### Testing

1. **Enter Play Mode**
2. **For Permanent Buffs**:
   - Check if stat changed (e.g., faster movement)
   - Should only apply once
3. **For Temporary Buffs**:
   - Watch debug UI for buff timers
   - Buffs should activate on cooldown
   - Effects should expire after duration

---

## Level Scaling

### How Scaling Works

With `Scale With Level` enabled and `Level Scaling: 1.1`:

**Multiplicative Buffs** (×1.5):
| Level | Multiplier | Effective Boost |
|-------|------------|-----------------|
| 1     | ×1.5       | 50% |
| 2     | ×1.6       | 60% |
| 3     | ×1.7       | 70% |
| 4     | ×1.8       | 80% |
| 5     | ×1.9       | 90% |

**Additive Buffs** (+10):
| Level | Bonus | Effective Boost |
|-------|-------|-----------------|
| 1     | +10   | +10 |
| 2     | +11   | +11 |
| 3     | +12.1 | +12 |
| 4     | +13.3 | +13 |
| 5     | +14.6 | +15 |

**Duration Scaling** (5 seconds):
| Level | Duration |
|-------|----------|
| 1     | 5.0s |
| 2     | 6.0s (20% longer) |
| 3     | 7.2s |
| 4     | 8.4s |
| 5     | 9.6s |

### Disable Scaling

Set `Scale With Level` to `✗ Disabled` for:
- Fixed buffs that shouldn't change
- Percentage-based upgrades (e.g., always ×2.0)
- One-time permanent buffs

---

## Advanced: Applying Buffs via Code

### Apply Buff at Runtime

```csharp
using RoombaRampage.Skills.Helpers;

public class BuffExample : MonoBehaviour
{
    private PlayerBuffManager buffManager;

    void Start()
    {
        buffManager = GetComponent<PlayerBuffManager>();
    }

    // Apply speed boost
    public void GrantSpeedBoost()
    {
        PlayerBuffData speedBuff = new PlayerBuffData(
            BuffStatType.MaxSpeed,
            BuffApplicationType.Multiply,
            1.5f,  // 50% faster
            10f    // 10 seconds
        );

        buffManager.ApplyBuff(speedBuff);
    }

    // Apply permanent damage buff
    public void GrantPermanentDamage()
    {
        PlayerBuffData damageBuff = new PlayerBuffData(
            BuffStatType.Damage,
            BuffApplicationType.Add,
            20f,   // +20 damage
            0f     // Permanent
        );

        buffManager.ApplyBuff(damageBuff);
    }
}
```

### Remove Buffs

```csharp
// Remove all temporary buffs
buffManager.ClearTemporaryBuffs();

// Remove all buffs (including permanent)
buffManager.ClearAllBuffs();

// Remove specific stat buff
buffManager.RemoveBuff(BuffStatType.MaxSpeed);
```

### Check Buff Status

```csharp
// Check if buff is active
if (buffManager.HasBuff(BuffStatType.MaxSpeed))
{
    Debug.Log("Speed buff is active!");
}

// Get remaining duration
float remaining = buffManager.GetBuffDuration(BuffStatType.Damage);
Debug.Log($"Damage buff expires in {remaining}s");
```

---

## Combining Multiple Buffs

### Buff Stacking Rules

1. **Same Stat, Same Type**: Refreshes duration
   - Speed ×1.5 + Speed ×1.5 → Refreshes to full duration

2. **Same Stat, Different Type**: Both apply
   - Speed ×1.5 + Speed +5 → Both active simultaneously

3. **Different Stats**: All stack
   - Speed ×1.5 + Damage ×2.0 → Both active

### Example: Multi-Buff Build

```
Skills Active:
1. Speed Boost (×1.3 MaxSpeed, permanent)
2. Combat Master (×1.5 Damage, 8s)
3. Turbo Expert (×2.0 TurboRegenRate, permanent)

Result:
- Movement: 30% faster (permanent)
- Damage: 50% more (8 seconds)
- Turbo: Recharges 2× faster (permanent)
```

---

## Visual Effects (Optional)

### Adding Buff Effect Prefab

1. **Create Visual Effect**:
   - Create particle system or animated sprite
   - Attach to player when buff activates
   - Effect should be self-contained

2. **Assign to Skill**:
   - Open BuffSkillData asset
   - **Buff Effect Prefab**: Drag effect prefab
   - **Effect Duration**: How long visual stays

3. **Effect Suggestions**:
   - **Speed Buff**: Blue trailing particles
   - **Damage Buff**: Red glow around player
   - **Health Buff**: Green pulse
   - **Turbo Buff**: Yellow energy ring

---

## Troubleshooting

### Buff Doesn't Apply

**Problem**: Stats don't change
**Solutions**:
- Check `PlayerBuffManager` is on player
- Verify `PlayerStats` asset is assigned
- Enable debug info to see active buffs
- Check skill is enabled in SkillManager

### Permanent Buff Applies Multiple Times

**Problem**: Buff keeps stacking
**Solutions**:
- Set `Auto Fire` to `✗ Disabled`
- Set `Cooldown` to `0`
- Apply via code once, not in Update loop

### Temporary Buff Doesn't Expire

**Problem**: Buff lasts forever
**Solutions**:
- Check `Duration` > 0 (0 = permanent)
- Verify PlayerBuffManager's Update is running
- Check for errors in Console

### Stats Don't Update Visually

**Problem**: Buff applies but movement feels same
**Solutions**:
- PlayerController may need manual refresh
- Check if stat is actually used by PlayerController
- Some stats may require restart (e.g., MaxHealth)

### Level Scaling Not Working

**Problem**: Buff same strength at all levels
**Solutions**:
- Check `Scale With Level` is `✓ Enabled`
- Verify `Level Scaling` > 1.0
- Ensure skill level > 1 in SkillManager

---

## Buff Strategy Guide

### Early Game Buffs
- **Movement** - Navigate faster, escape enemies
- **Health Regen** - Survive longer
- **Turbo Regen** - Use abilities more often

### Mid Game Buffs
- **Damage** - Kill enemies faster
- **Fire Rate** - DPS increase
- **Max Health** - Tank more hits

### Late Game Buffs
- **Multiple Stat Combos** - Speed + Damage
- **Multiplicative Stacking** - ×1.5 + ×1.5 = ×2.25
- **Specialization** - Max out one stat type

### Buff Synergies

**Speed Build**:
- MaxSpeed ×1.5
- Acceleration ×1.5
- Turbo Speed ×2.0
- **Result**: Extremely mobile, kiting playstyle

**Tank Build**:
- MaxHealth ×2.0
- Health Regen +10
- Damage Reduction (future)
- **Result**: Survive heavy damage

**Glass Cannon**:
- Damage ×3.0
- Fire Rate ×2.0
- MaxHealth ×0.5
- **Result**: High risk, high reward

---

## File Structure

```
Assets/_Project/
├── Scripts/
│   └── Skills/
│       ├── README_BUFF_SKILL.md         <- This file
│       ├── SkillManager.cs              <- Manages all skills
│       ├── Helpers/
│       │   ├── PlayerBuff.cs            <- Buff types & data
│       │   ├── PlayerBuffManager.cs     <- Manages buffs on player
│       │   ├── StatusEffect.cs          <- Enemy status effects
│       │   └── EnemyTargeting.cs        <- Enemy targeting utilities
│       └── Data/
│           ├── SkillData.cs             <- Base skill class
│           ├── LaserSkillData.cs        <- Laser skill
│           ├── LightningStrikeSkillData.cs <- Lightning skill
│           └── BuffSkillData.cs         <- Buff skill
```

---

## Testing Checklist

- [ ] BuffSkillData asset created
- [ ] At least one buff configured
- [ ] Buff stat type selected
- [ ] Buff application type selected
- [ ] Duration set (0 = permanent, >0 = temporary)
- [ ] PlayerBuffManager added to Player (auto-adds if missing)
- [ ] Skill added to SkillManager
- [ ] Debug info enabled
- [ ] Play mode: Buff applies correctly
- [ ] Stats visibly change (movement, damage, etc.)
- [ ] Temporary buffs expire after duration
- [ ] Permanent buffs stay forever
- [ ] Level scaling works (if enabled)
- [ ] No console errors

---

## Integration with Other Skills

### Combine with Laser Skill
```
Build: "Rapid Laser"
1. Fire Rate Buff (×2.0, permanent)
2. Damage Buff (×1.5, permanent)
3. Laser Skill (2s cooldown, 30 damage)
Result: Fast-firing powerful lasers
```

### Combine with Lightning Skill
```
Build: "Storm Caller"
1. Cooldown Reduction Buff (future)
2. Lightning Skill (3 strikes, 50 damage + burn)
3. Movement Speed (×1.3, permanent)
Result: Mobile lightning mage
```

### Meta Buff Strategy
```
Phase 1: Get movement buffs (survive early game)
Phase 2: Add damage buffs (kill faster)
Phase 3: Stack multiplicative buffs (exponential power)
```

---

## Next Steps

1. **Create specialized buff skills**:
   - Critical hit chance buff
   - Projectile size buff
   - Pickup range buff
   - XP gain multiplier

2. **Add visual polish**:
   - Particle effects for each stat type
   - Screen effects (speed lines, red tint)
   - Audio feedback

3. **Advanced features**:
   - Buff conditions (e.g., "below 50% HP")
   - Buff triggers (e.g., "on kill")
   - Buff caps/diminishing returns
   - Buff removal items

4. **UI improvements**:
   - Show active buffs with icons
   - Display buff durations
   - Buff selection screen

---

## Support

For issues:
1. Check Debug Info on PlayerBuffManager
2. Verify PlayerStats asset assigned
3. Check Console for errors
4. Test with simple one-buff skill first
5. Ensure stat is actually used by game systems

**Happy buffing!** 💪⚡🔥
