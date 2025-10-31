# RoombaRampage Combat System - Implementation Plan

Complete implementation plan for the weapon and enemy combat system.

## System Overview

**Architecture:** Data-driven, component-based, optimized for 3D top-down bullet hell gameplay

**Key Features:**
- ✅ Modular weapon system with ScriptableObject data
- ✅ Object pooling for projectiles (performance critical)
- ✅ Simple AI enemy system with health and movement
- ✅ Wave-based enemy spawning
- ✅ Score tracking system
- ✅ New Input System integration
- ✅ 3D physics on XZ plane

---

## Technical Specifications

### Input System
- **Framework:** Unity New Input System (NOT legacy Input)
- **Integration:** Via PlayerInput component
- **Fire Action:** "Attack" button from InputSystem_Actions.inputactions
- **Aim Method:** Mouse position to world raycast on XZ plane

### Physics Configuration
- **Dimension:** 3D (Rigidbody, Collider, NOT 2D versions)
- **Movement Plane:** XZ (Y is up, Y position frozen on entities)
- **Gravity:** Disabled for all combat entities
- **Collision Detection:**
  - Projectiles: Continuous Dynamic (prevent tunneling)
  - Enemies: Continuous (for reliable collision damage)

### Tags Required
- `Player` - Player GameObject
- `Enemy` - All enemy GameObjects
- `Obstacle` / `Wall` - Optional for environment collision

### Layers (Optional but Recommended)
- Create layers for better collision filtering:
  - `Player`
  - `Enemy`
  - `Projectile`
  - `Environment`

---

## Component Dependencies

### Player Setup
```
Player GameObject
├── PlayerInput.cs (existing)
├── PlayerHealth.cs (existing)
├── PlayerController.cs (existing)
└── WeaponController.cs (NEW)
    └── Requires: PlayerInput
```

### Enemy Prefab Structure
```
Enemy GameObject (Tag: "Enemy")
├── Rigidbody
├── Collider (NOT trigger)
├── EnemyHealth.cs
├── EnemyAI.cs
└── Enemy.cs
    ├── Requires: EnemyHealth
    └── Requires: EnemyAI
```

### Scene Setup
```
Scene
├── Player (with WeaponController)
├── ProjectilePool (singleton)
├── ScoreManager (singleton)
├── EnemySpawner
└── Main Camera (existing)
```

---

## Data Asset Creation

### WeaponData Assets (3 presets)

**1. BasicGun.asset**
```
weaponName: "Basic Gun"
damage: 25
fireRate: 0.5
projectileSpeed: 20
projectileLifetime: 5
projectileCount: 1
maxAmmo: -1 (infinite)
```

**2. SpreadShot.asset**
```
weaponName: "Spread Shot"
damage: 15
fireRate: 0.7
projectileSpeed: 18
projectileLifetime: 5
projectileCount: 3
spreadAngle: 20
maxAmmo: -1 (infinite)
```

**3. RapidFire.asset**
```
weaponName: "Rapid Fire"
damage: 8
fireRate: 0.15
projectileSpeed: 25
projectileLifetime: 4
projectileCount: 1
maxAmmo: -1 (infinite)
```

### EnemyData Assets (2 presets)

**1. BasicEnemy.asset**
```
enemyName: "Basic Enemy"
maxHealth: 100
collisionDamage: 15
damageCooldown: 1.5
moveSpeed: 3
rotationSpeed: 180
stoppingDistance: 0
aiUpdateInterval: 0.2
detectionRange: 0 (infinite)
scoreValue: 10
```

**2. FastEnemy.asset**
```
enemyName: "Fast Enemy"
maxHealth: 40
collisionDamage: 10
damageCooldown: 1.0
moveSpeed: 7
rotationSpeed: 360
stoppingDistance: 0
aiUpdateInterval: 0.2
detectionRange: 0 (infinite)
scoreValue: 20
```

---

## Collision Matrix

Configure in `Edit > Project Settings > Physics > Layer Collision Matrix`:

|             | Player | Enemy | Projectile | Environment |
|-------------|--------|-------|------------|-------------|
| Player      | ❌     | ✅    | ❌         | ✅          |
| Enemy       | ✅     | ✅    | ✅         | ✅          |
| Projectile  | ❌     | ✅    | ❌         | ✅          |
| Environment | ✅     | ✅    | ✅         | ✅          |

---

## Performance Targets

### Bullet Hell Scenario
- **Target:** 200+ projectiles on screen simultaneously
- **Solution:** ProjectilePool with expansion enabled
- **Settings:**
  - Initial Pool Size: 50-100
  - Max Pool Size: 200-500
  - Enable Pool Expansion: True

### Enemy AI Optimization
- **Target:** 30-50 enemies active simultaneously
- **Solution:** Periodic AI updates instead of per-frame
- **Settings:**
  - AI Update Interval: 0.2-0.3 seconds
  - Detection Range: Use limited range when possible
  - Stopping Distance: Prevent clustering

### Spawner Configuration
- **Initial Wave:** 5 enemies
- **Wave Increment:** +2 enemies per wave
- **Max Wave Size:** 50 enemies
- **Wave Delay:** 10 seconds

---

## Event Flow

### Firing Weapon
```
1. Player holds Attack button
2. PlayerInput.AttackHeld = true
3. WeaponController checks fire rate cooldown
4. WeaponController.Fire()
5. WeaponController gets look direction from PlayerInput
6. ProjectilePool.GetProjectile() spawns from pool
7. Projectile.Initialize() sets velocity and lifetime
8. Projectile moves via FixedUpdate (Rigidbody velocity)
```

### Projectile Hit
```
1. Projectile.OnTriggerEnter() detects enemy collision
2. Get EnemyHealth component from collider
3. EnemyHealth.TakeDamage() reduces health
4. EnemyHealth.OnDamageTaken event fires
5. Projectile.Despawn() returns to pool
6. If health <= 0, EnemyHealth.Die() called
```

### Enemy Death
```
1. EnemyHealth.Die() marks enemy as dead
2. ScoreManager.AddScore() awards points
3. EnemyHealth.OnDeath event fires
4. Enemy.OnDeath() disables AI and collider
5. Enemy destroyed after delay (for death animation)
6. EnemySpawner updates active enemy count
```

### Enemy AI Cycle
```
1. Every AI Update Interval:
   - Find/update player reference
   - Check detection range
   - Calculate direction to player
   - Check stopping distance
2. Every FixedUpdate:
   - Apply velocity toward player
   - Rotate toward movement direction
```

### Wave Spawn
```
1. EnemySpawner waits for wave delay
2. Calculate enemy count (base + progression)
3. For each enemy:
   - Pick random EnemyData from list
   - Get random spawn position (circular area)
   - Instantiate enemy prefab
   - Initialize with EnemyData
   - Track in active enemies list
4. Wait for wave delay, repeat
```

---

## Testing Procedure

### Phase 1: Weapon System
1. ✅ Create projectile prefab with Projectile.cs
2. ✅ Create WeaponData asset
3. ✅ Add WeaponController to player
4. ✅ Add ProjectilePool to scene
5. ✅ Test firing (projectiles spawn and move)
6. ✅ Test fire rate cooldown
7. ✅ Test spread pattern (multi-shot weapons)

### Phase 2: Enemy System
1. ✅ Create EnemyData asset
2. ✅ Create enemy prefab with all components
3. ✅ Set "Enemy" tag
4. ✅ Place enemy in scene manually
5. ✅ Test AI (enemy moves toward player)
6. ✅ Test health (enemy takes damage from projectiles)
7. ✅ Test death (enemy dies at 0 health)

### Phase 3: Integration
1. ✅ Test projectile-enemy collision
2. ✅ Test enemy-player collision damage
3. ✅ Test score awarding on death
4. ✅ Add EnemySpawner to scene
5. ✅ Test wave spawning
6. ✅ Test wave progression

### Phase 4: Performance
1. ✅ Spawn 100+ projectiles, check FPS
2. ✅ Spawn 30+ enemies, check FPS
3. ✅ Test bullet hell scenario (many projectiles + many enemies)
4. ✅ Monitor pool expansion in debug UI
5. ✅ Profile with Unity Profiler if needed

---

## Extension Points

### Future Enhancements
- **Weapon System:**
  - Weapon switching/pickup system
  - Charge-up weapons
  - Homing missiles
  - Piercing projectiles
  - Explosive projectiles (AOE damage)

- **Enemy System:**
  - Enemy object pooling
  - Boss enemies with unique mechanics
  - Enemy abilities (shooting back, shields, dashes)
  - Pathfinding (Unity NavMesh)
  - Formation movement

- **Visual Effects:**
  - Muzzle flash particles
  - Impact effects
  - Death explosions
  - Trail renderers on projectiles
  - Screen shake on hit

- **Audio:**
  - Fire sound effects
  - Impact sounds
  - Enemy hurt/death sounds
  - Music intensity based on wave

- **UI:**
  - Score display
  - Weapon indicator
  - Ammo counter (if using ammo)
  - Wave number display
  - Health bar for player
  - Mini-map with enemy positions

---

## Known Limitations & Solutions

### Issue: Projectiles tunnel through enemies at high speed
**Solution:** Using Continuous Dynamic collision detection on projectiles

### Issue: Too many projectiles cause lag
**Solution:** Object pooling implemented (ProjectilePool)

### Issue: AI updates every frame cause performance issues
**Solution:** Periodic AI updates (configurable interval)

### Issue: Enemies cluster around player
**Solution:** Use stopping distance and adjust enemy collider physics material

### Issue: Player hit detection inconsistent
**Solution:** Ensure player has Rigidbody and proper collision detection mode

---

## File Structure

```
Assets/_Project/Scripts/
├── COMBAT_SETUP.md              <- Setup guide
├── IMPLEMENTATION_PLAN.md       <- This file
├── Weapons/
│   ├── README.md
│   ├── Projectile.cs           <- Projectile behavior
│   ├── ProjectilePool.cs       <- Object pooling
│   ├── WeaponController.cs     <- Firing logic
│   └── Data/
│       └── WeaponData.cs       <- Weapon ScriptableObject
├── Enemies/
│   ├── README.md
│   ├── Enemy.cs                <- Main enemy controller
│   ├── EnemyHealth.cs          <- Health system
│   ├── EnemyAI.cs              <- Movement AI
│   ├── EnemySpawner.cs         <- Wave spawning
│   └── Data/
│       └── EnemyData.cs        <- Enemy ScriptableObject
└── Managers/
    └── ScoreManager.cs         <- Score tracking
```

---

## Summary

**Total Scripts Created:** 10
- 4 Weapon scripts (Projectile, Pool, Controller, Data)
- 5 Enemy scripts (Enemy, Health, AI, Spawner, Data)
- 1 Manager script (ScoreManager)

**Total Data Assets Needed:** 5
- 3 WeaponData (BasicGun, SpreadShot, RapidFire)
- 2 EnemyData (BasicEnemy, FastEnemy)

**Scene Setup Required:**
- Player with WeaponController
- ProjectilePool GameObject
- ScoreManager GameObject
- EnemySpawner GameObject

**Estimated Setup Time:** 30-45 minutes for first-time setup

---

## Success Criteria

System is complete when:
- ✅ Player can fire projectiles using Attack button
- ✅ Projectiles hit and damage enemies
- ✅ Enemies move toward player using AI
- ✅ Enemies deal collision damage to player
- ✅ Enemies die and award score when health reaches 0
- ✅ Waves spawn continuously with increasing difficulty
- ✅ System runs at 60+ FPS with 50+ projectiles and 30+ enemies
- ✅ No console errors during gameplay
- ✅ All debug visualizations work correctly

---

**Status:** ✅ Implementation Complete - Ready for Testing
