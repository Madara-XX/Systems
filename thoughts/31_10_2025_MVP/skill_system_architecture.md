# Skill System Architecture - RoombaRampage

**Created:** 2025-10-31
**Status:** Design Phase
**Integration:** Player, Weapon, Enemy, XP Systems

---

## Overview

Roguelike skill progression system that pauses the game on level-up and offers 3 random skill choices. Skills modify player stats, weapon performance, and add special effects.

---

## Architecture Design

### Class Hierarchy

```
Progression/
├── Skills/
│   ├── Data/
│   │   ├── SkillData.cs           # ScriptableObject for skill definitions
│   │   └── SkillDatabase.cs       # Collection of all skills
│   ├── SkillManager.cs             # Singleton - manages active skills
│   ├── SkillEffect.cs              # Skill effect types and application logic
│   ├── SkillRarity.cs              # Rarity enum
│   └── SkillTypes.cs               # Effect type enums
```

---

## Core Classes

### 1. SkillRarity.cs (Enum)

```csharp
namespace RoombaRampage.Skills
{
    public enum SkillRarity
    {
        Common = 0,     // 60% chance - Basic upgrades
        Rare = 1,       // 30% chance - Strong upgrades
        Legendary = 2   // 10% chance - Game-changing effects
    }
}
```

### 2. SkillEffectType.cs (Enum)

```csharp
namespace RoombaRampage.Skills
{
    public enum SkillEffectType
    {
        // Weapon Stats (Multiplicative %)
        WeaponDamage,           // +X% damage
        FireRate,               // +X% fire rate (reduces cooldown)
        ProjectileSpeed,        // +X% projectile speed

        // Weapon Stats (Additive +)
        ProjectileCount,        // +N projectiles per shot

        // Player Stats (Multiplicative %)
        MaxHealth,              // +X% max health
        MovementSpeed,          // +X% movement speed
        PickupRange,            // +X% XP gem magnet range

        // Special Effects (Boolean flags)
        PiercingShot,           // Projectiles pierce through enemies
        BouncingShot,           // Projectiles bounce off walls
        HomingShot,             // Projectiles home toward enemies
        ExplosiveShot,          // Projectiles explode on hit

        // Health Effects
        HealthRegen,            // Regenerate X HP per second
        DamageResistance,       // Reduce damage by X%

        // Utility
        ExtraLife,              // +1 extra life (respawn once)
        LuckBoost,              // +X% better loot/XP
    }
}
```

### 3. SkillEffect.cs (Struct)

```csharp
namespace RoombaRampage.Skills
{
    [System.Serializable]
    public struct SkillEffect
    {
        [Tooltip("Type of effect")]
        public SkillEffectType effectType;

        [Tooltip("Effect value (% for multiplicative, flat for additive)")]
        public float value;

        [Tooltip("Human-readable effect description")]
        public string description;

        // Constructor
        public SkillEffect(SkillEffectType type, float val, string desc)
        {
            effectType = type;
            value = val;
            description = desc;
        }
    }
}
```

### 4. SkillData.cs (ScriptableObject)

```csharp
namespace RoombaRampage.Skills
{
    [CreateAssetMenu(fileName = "Skill_", menuName = "RoombaRampage/Skill Data")]
    public class SkillData : ScriptableObject
    {
        [Header("Skill Identity")]
        [Tooltip("Skill name (fun roomba-themed)")]
        public string skillName;

        [Tooltip("Skill description")]
        [TextArea(2, 4)]
        public string description;

        [Tooltip("Skill icon")]
        public Sprite icon;

        [Header("Skill Properties")]
        [Tooltip("Skill rarity")]
        public SkillRarity rarity;

        [Tooltip("Maximum times this skill can be stacked (-1 = infinite)")]
        public int maxStacks = 1;

        [Header("Skill Effects")]
        [Tooltip("List of effects this skill applies")]
        public List<SkillEffect> effects;

        [Header("Flavor")]
        [Tooltip("Fun flavor text")]
        [TextArea(1, 2)]
        public string flavorText;
    }
}
```

### 5. SkillDatabase.cs (ScriptableObject)

```csharp
namespace RoombaRampage.Skills
{
    [CreateAssetMenu(fileName = "SkillDatabase", menuName = "RoombaRampage/Skill Database")]
    public class SkillDatabase : ScriptableObject
    {
        [Tooltip("All available skills in the game")]
        public List<SkillData> allSkills;

        [Header("Rarity Weights")]
        [Tooltip("Weight for common skills (default: 60)")]
        public float commonWeight = 60f;

        [Tooltip("Weight for rare skills (default: 30)")]
        public float rareWeight = 30f;

        [Tooltip("Weight for legendary skills (default: 10)")]
        public float legendaryWeight = 10f;

        // Get skills by rarity
        public List<SkillData> GetSkillsByRarity(SkillRarity rarity)
        {
            return allSkills.FindAll(skill => skill.rarity == rarity);
        }

        // Get weight for rarity
        public float GetRarityWeight(SkillRarity rarity)
        {
            switch (rarity)
            {
                case SkillRarity.Common: return commonWeight;
                case SkillRarity.Rare: return rareWeight;
                case SkillRarity.Legendary: return legendaryWeight;
                default: return 1f;
            }
        }
    }
}
```

### 6. SkillManager.cs (Singleton)

```csharp
namespace RoombaRampage.Skills
{
    public class SkillManager : MonoBehaviour
    {
        public static SkillManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private SkillDatabase skillDatabase;

        [Header("References")]
        [SerializeField] private Player.PlayerController playerController;
        [SerializeField] private Player.PlayerHealth playerHealth;
        [SerializeField] private Weapons.WeaponController weaponController;

        [Header("Skill Selection")]
        [SerializeField] private int skillChoicesPerLevelUp = 3;

        // Active skills tracking
        private Dictionary<SkillData, int> activeSkills = new Dictionary<SkillData, int>();

        // Accumulated stat modifiers
        private float weaponDamageMultiplier = 1f;
        private float fireRateMultiplier = 1f;
        private float movementSpeedMultiplier = 1f;
        // ... etc

        private void Awake()
        {
            // Singleton setup
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // Subscribe to XP level-up event
            if (Progression.XPManager.Instance != null)
            {
                Progression.XPManager.Instance.OnLevelUp.AddListener(OnPlayerLevelUp);
            }
        }

        private void OnPlayerLevelUp(int newLevel)
        {
            // Generate skill choices
            List<SkillData> choices = GenerateSkillChoices();

            // Show skill selection UI (will be implemented later)
            // SkillSelectionUI.Instance.ShowSkillChoices(choices);
        }

        public List<SkillData> GenerateSkillChoices()
        {
            List<SkillData> choices = new List<SkillData>();

            for (int i = 0; i < skillChoicesPerLevelUp; i++)
            {
                SkillData skill = SelectRandomSkill();
                if (skill != null && !choices.Contains(skill))
                {
                    choices.Add(skill);
                }
            }

            return choices;
        }

        private SkillData SelectRandomSkill()
        {
            // Weighted random selection by rarity
            // Filter out maxed skills
            // Return random skill
        }

        public void ApplySkill(SkillData skill)
        {
            // Add to active skills
            if (!activeSkills.ContainsKey(skill))
            {
                activeSkills[skill] = 0;
            }
            activeSkills[skill]++;

            // Apply each effect
            foreach (var effect in skill.effects)
            {
                ApplySkillEffect(effect);
            }

            // Resume game
            if (Progression.XPManager.Instance != null)
            {
                Progression.XPManager.Instance.CompleteLevelUp();
            }
        }

        private void ApplySkillEffect(SkillEffect effect)
        {
            switch (effect.effectType)
            {
                case SkillEffectType.WeaponDamage:
                    weaponDamageMultiplier += effect.value / 100f;
                    ApplyWeaponStats();
                    break;

                case SkillEffectType.MovementSpeed:
                    movementSpeedMultiplier += effect.value / 100f;
                    ApplyPlayerStats();
                    break;

                // ... handle all effect types
            }
        }

        private void ApplyWeaponStats()
        {
            // Apply accumulated multipliers to weapon
        }

        private void ApplyPlayerStats()
        {
            // Apply accumulated multipliers to player
        }
    }
}
```

---

## Integration Points

### XP System Integration

```csharp
// In SkillManager.Start():
XPManager.Instance.OnLevelUp.AddListener(OnPlayerLevelUp);

// When skill selected:
XPManager.Instance.CompleteLevelUp(); // Resumes game
```

### Player System Integration

```csharp
// SkillManager modifies PlayerController
playerController.SetSpeedMultiplier(movementSpeedMultiplier);

// SkillManager modifies PlayerHealth
playerHealth.SetMaxHealthMultiplier(maxHealthMultiplier);
```

### Weapon System Integration

```csharp
// SkillManager modifies WeaponController
weaponController.SetDamageMultiplier(weaponDamageMultiplier);
weaponController.SetFireRateMultiplier(fireRateMultiplier);
weaponController.SetProjectileCountBonus(projectileCountBonus);
```

---

## 15 Initial Skills (Roomba-Themed)

### Common Skills (60%)

1. **"Turbo Spin"**
   - Description: "Your motors whir with newfound energy!"
   - Effect: +15% movement speed
   - Max Stacks: 5
   - Flavor: "Zoom zoom, motherduster!"

2. **"Heavy Duty Battery"**
   - Description: "Extended battery capacity for longer cleaning sessions"
   - Effect: +20% max health
   - Max Stacks: 5
   - Flavor: "Now with 20% more juice!"

3. **"Overclocked Motor"**
   - Description: "Push your motors beyond factory specifications"
   - Effect: +12% fire rate
   - Max Stacks: 5
   - Flavor: "Warranty? What warranty?"

4. **"Laser Upgrade"**
   - Description: "Military-grade laser cleaning attachment"
   - Effect: +25% weapon damage
   - Max Stacks: 5
   - Flavor: "For those REALLY stubborn stains"

5. **"Speed Loader"**
   - Description: "Faster ammunition reloading mechanism"
   - Effect: +10% fire rate
   - Max Stacks: 5
   - Flavor: "Reload? I barely know 'load!'"

6. **"Magnetic Vacuum"**
   - Description: "Enhanced magnetic field for better pickup"
   - Effect: +30% pickup range
   - Max Stacks: 3
   - Flavor: "I'm a XP magnet!"

### Rare Skills (30%)

7. **"Armor Plating"**
   - Description: "Reinforced titanium plating"
   - Effect: +15% damage resistance
   - Max Stacks: 3
   - Flavor: "Built like a tank, cleans like a dream"

8. **"Multi-Barrel Assembly"**
   - Description: "Additional firing mechanisms installed"
   - Effect: +1 projectile
   - Max Stacks: 3
   - Flavor: "Why stop at one?"

9. **"Regenerative Circuits"**
   - Description: "Self-repairing nanobots integrated"
   - Effect: +1 HP regen per 5 seconds
   - Max Stacks: 3
   - Flavor: "Heal thyself, roomba"

10. **"Bouncy Bullets"**
    - Description: "Rubber-coated ammunition that ricochets"
    - Effect: Projectiles bounce off walls
    - Max Stacks: 1
    - Flavor: "Angles? I got all the angles"

11. **"Piercing Shot"**
    - Description: "Armor-penetrating rounds"
    - Effect: Projectiles pierce through enemies
    - Max Stacks: 1
    - Flavor: "Nothing stops the cleaning!"

### Legendary Skills (10%)

12. **"Guided Missiles"**
    - Description: "AI-guided projectiles that home in on dirt... and enemies"
    - Effect: Projectiles home toward nearest enemy
    - Max Stacks: 1
    - Flavor: "Heat-seeking dust destroyer"

13. **"Explosive Rounds"**
    - Description: "High-explosive payload delivery system"
    - Effect: Projectiles explode on impact (AoE damage)
    - Max Stacks: 1
    - Flavor: "Overkill? No such thing"

14. **"Nine Lives Protocol"**
    - Description: "Emergency backup power cell activation"
    - Effect: +1 extra life (respawn once)
    - Max Stacks: 3
    - Flavor: "I'll be back!"

15. **"Lucky Roomba"**
    - Description: "Enhanced probability matrix"
    - Effect: +25% XP gain, +better rare skill chance
    - Max Stacks: 2
    - Flavor: "Feeling lucky, punk?"

---

## Data Flow

```
1. Player kills enemies → Gain XP
2. XP reaches threshold → XPManager.OnLevelUp event fires
3. XPManager pauses game (Time.timeScale = 0)
4. SkillManager receives OnLevelUp event
5. SkillManager.GenerateSkillChoices() → 3 random skills (weighted by rarity)
6. SkillSelectionUI shows skill choices (UI implementation)
7. Player clicks a skill → SkillManager.ApplySkill(skill)
8. SkillManager applies effects to player/weapon systems
9. SkillManager calls XPManager.CompleteLevelUp()
10. Game resumes (Time.timeScale = 1)
```

---

## File Structure

```
Assets/_Project/
├── Scripts/
│   └── Progression/
│       └── Skills/
│           ├── Data/
│           │   ├── SkillData.cs
│           │   └── SkillDatabase.cs
│           ├── SkillManager.cs
│           ├── SkillEffect.cs
│           ├── SkillRarity.cs
│           └── SkillEffectType.cs
├── Data/
│   └── Progression/
│       ├── Skills/
│       │   ├── SkillDatabase.asset
│       │   └── Common/
│       │       ├── Skill_TurboSpin.asset
│       │       ├── Skill_HeavyDutyBattery.asset
│       │       └── ...
│       │   └── Rare/
│       │       └── ...
│       │   └── Legendary/
│       │       └── ...
└── UI/
    └── Skills/
        └── SkillSelectionUI.cs (next phase)
```

---

## Implementation Order

1. ✅ Create architecture document (this file)
2. ⏳ Create enum files (SkillRarity, SkillEffectType)
3. ⏳ Create SkillEffect struct
4. ⏳ Create SkillData ScriptableObject
5. ⏳ Create SkillDatabase ScriptableObject
6. ⏳ Create SkillManager singleton
7. ⏳ Create 15 skill assets
8. ⏳ Implement skill selection logic
9. ⏳ Integrate with player/weapon systems
10. ⏳ Create SkillSelectionUI (Phase 2)
11. ⏳ Test complete flow

---

## Testing Checklist

- [ ] XP level-up triggers skill selection
- [ ] 3 random skills shown (correct rarity distribution)
- [ ] Skills apply effects correctly
- [ ] Skills stack properly
- [ ] Maxed skills don't reappear
- [ ] Game pauses during selection
- [ ] Game resumes after selection
- [ ] Player stats update immediately
- [ ] Weapon stats update immediately
- [ ] Special effects work (piercing, bouncing, etc.)

---

**Next Steps:**
1. Implement core skill classes
2. Create initial skill assets
3. Integrate with XP system
4. Build skill selection UI

---

**Status:** Ready for Implementation
**Estimated Time:** 2-3 hours
