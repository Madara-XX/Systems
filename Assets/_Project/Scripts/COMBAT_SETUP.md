# RoombaRampage - Combat System Setup Guide

Complete setup guide for the weapon and enemy combat system.

## Quick Start

1. Create WeaponData and EnemyData assets
2. Set up player with weapon system
3. Create enemy prefabs
4. Set up enemy spawner
5. Test the combat loop

---

## 1. Create WeaponData Assets

### Creating a Weapon
1. In Unity, right-click in Project window
2. `Create > RoombaRampage > Weapon Data`
3. Name it (e.g., "BasicGun", "SpreadShot", "RapidFire")
4. Configure the weapon in Inspector:
   - **Weapon Name**: Display name
   - **Damage**: Damage per projectile
   - **Fire Rate**: Cooldown between shots (seconds)
   - **Projectile Speed**: How fast projectiles travel
   - **Projectile Lifetime**: How long before despawn (seconds)
   - **Projectile Prefab**: Assign your projectile prefab (see below)
   - **Projectile Count**: Number of bullets per shot (1 for single, 3+ for spread)
   - **Spread Angle**: Angle between projectiles (only for multi-shot)
   - **Max Ammo**: -1 for infinite, or set a number

### Example Weapon Presets

**Basic Gun** (balanced)
- Damage: 25
- Fire Rate: 0.5
- Projectile Speed: 20
- Projectile Count: 1

**Spread Shot** (wide coverage)
- Damage: 15
- Fire Rate: 0.7
- Projectile Speed: 18
- Projectile Count: 3
- Spread Angle: 20

**Rapid Fire** (high DPS)
- Damage: 8
- Fire Rate: 0.15
- Projectile Speed: 25
- Projectile Count: 1

---

## 2. Create Projectile Prefab

### Basic Projectile Setup
1. Create empty GameObject in scene: `GameObject > Create Empty`
2. Name it "Projectile"
3. Add components:
   - **Rigidbody**: Disable gravity, freeze Y position and all rotation
   - **Capsule Collider**: Set as trigger, adjust size (e.g., radius 0.2, height 0.5)
   - **Projectile.cs**: Add the Projectile script
4. Add visual (child GameObject):
   - Create `Capsule` or `Sphere` as child
   - Scale to small size (e.g., 0.2, 0.5, 0.2)
   - Add material/color
   - Rotate if needed to face forward (Z axis)
5. Drag to Project window to create prefab
6. Delete from scene

### Projectile Configuration
- **Rigidbody**:
  - Is Kinematic: False
  - Use Gravity: False
  - Constraints: Freeze Position Y, Freeze Rotation XYZ
  - Collision Detection: Continuous Dynamic (prevents tunneling)
- **Collider**:
  - Is Trigger: True
  - Size to match visual

---

## 3. Set Up Player Weapon System

### Add Weapon to Player
1. Select your Player GameObject (must have `PlayerInput.cs`)
2. Add component: `WeaponController.cs`
3. In Inspector:
   - **Current Weapon**: Assign a WeaponData asset
   - **Fire Point**: (Optional) Create empty child GameObject at front of player as fire point
   - **Fire Point Offset**: If no fire point, set offset (e.g., 0, 0.5, 1)
   - **Show Debug Info**: Enable to see firing gizmos and info

### Player Tag
- Ensure Player GameObject has tag "Player"

### Fire Input
- Fire automatically uses the "Attack" button from `InputSystem_Actions.inputactions`
- Hold Attack button to fire (automatic fire based on fire rate)

---

## 4. Create Enemy Prefabs

### Basic Enemy Setup
1. Create new GameObject: `GameObject > 3D Object > Capsule`
2. Name it "BasicEnemy"
3. Set tag to "Enemy" (create tag if needed)
4. Configure Transform:
   - Position: (0, 0.5, 0) - half height for proper floor placement
   - Scale: Adjust size (e.g., 0.5, 1, 0.5)
5. Add components in this order:
   - **Rigidbody**: Disable gravity, freeze Y position and X/Z rotation
   - **Capsule Collider**: Adjust to match size
   - **EnemyHealth.cs**: Add script
   - **EnemyAI.cs**: Add script
   - **Enemy.cs**: Add script (main controller)
6. Create EnemyData asset (see below)
7. Assign EnemyData to all three scripts in Inspector
8. Add visual mesh/material as child if desired
9. Drag to Project window to create prefab

### Enemy Rigidbody Setup
- Is Kinematic: False
- Use Gravity: False
- Constraints: Freeze Position Y, Freeze Rotation X, Freeze Rotation Z
- Collision Detection: Continuous (for collision damage)
- Mass: 1 (or adjust for knockback)

### Enemy Collider Setup
- Is Trigger: False (needs collision for damage)
- Size: Match visual bounds

---

## 5. Create EnemyData Assets

### Creating Enemy Data
1. Right-click in Project: `Create > RoombaRampage > Enemy Data`
2. Name it (e.g., "BasicEnemy", "FastEnemy")
3. Configure in Inspector:
   - **Enemy Name**: Display name
   - **Max Health**: Health pool
   - **Collision Damage**: Damage to player on collision
   - **Damage Cooldown**: Time between collision hits
   - **Move Speed**: Movement speed (m/s)
   - **Rotation Speed**: Turn speed (degrees/sec)
   - **Stopping Distance**: Stop this far from player (0 = touch)
   - **AI Update Interval**: AI refresh rate (0.2 recommended)
   - **Detection Range**: How far enemy sees player (0 = infinite)
   - **Score Value**: Points awarded on death
   - **Prefab**: Assign the enemy prefab

### Example Enemy Presets

**Basic Enemy** (tanky, slow)
- Max Health: 100
- Collision Damage: 15
- Damage Cooldown: 1.5
- Move Speed: 3
- Score Value: 10

**Fast Enemy** (fragile, fast)
- Max Health: 40
- Collision Damage: 10
- Damage Cooldown: 1.0
- Move Speed: 7
- Score Value: 20

---

## 6. Set Up Enemy Spawner

### Create Spawner GameObject
1. Create empty GameObject: `GameObject > Create Empty`
2. Name it "EnemySpawner"
3. Position at center of arena (will spawn around this point)
4. Add component: `EnemySpawner.cs`
5. Configure in Inspector:
   - **Enemy Types**: Add EnemyData assets to list (drag from Project)
   - **Enemies Per Wave**: Starting enemy count (e.g., 5)
   - **Time Between Waves**: Delay between waves (e.g., 10 seconds)
   - **Auto Spawn**: Enable for automatic waves
   - **Spawn Center**: Leave empty to use spawner position
   - **Min/Max Spawn Radius**: Area to spawn enemies (e.g., 10-20)
   - **Spawn Height**: Y position (usually 0 or 0.5)
   - **Enable Progression**: Enable for scaling difficulty
   - **Enemy Increase Per Wave**: Add this many enemies each wave (e.g., 2)
   - **Max Enemies Per Wave**: Cap for late game (e.g., 50)

---

## 7. Set Up Managers

### ProjectilePool
1. Create empty GameObject: `GameObject > Create Empty`
2. Name it "ProjectilePool"
3. Add component: `ProjectilePool.cs`
4. Configure:
   - **Initial Pool Size**: 50 (adjust based on weapon fire rate)
   - **Allow Pool Expansion**: True (recommended)
   - **Max Pool Size**: 200 (prevents memory issues)
5. This singleton auto-initializes, no further setup needed

### ScoreManager
1. Create empty GameObject: `GameObject > Create Empty`
2. Name it "ScoreManager"
3. Add component: `ScoreManager.cs`
4. Configure:
   - **Starting Score**: 0
   - **On Score Changed**: Hook up UI text updates here
   - **On Score Added**: Optional feedback events
5. Persists across scenes automatically

---

## 8. Testing the Combat Loop

### Test Checklist

**Player Firing**
- [x] Player fires projectiles when holding Attack button
- [x] Projectiles spawn at correct position and rotation
- [x] Projectiles move forward on XZ plane
- [x] Fire rate cooldown works correctly
- [x] Multiple projectiles spread correctly (if using spread weapon)

**Projectile Behavior**
- [x] Projectiles despawn after lifetime
- [x] Projectiles hit enemies and deal damage
- [x] Projectiles despawn on enemy hit
- [x] No error messages in console

**Enemy Behavior**
- [x] Enemies spawn at correct positions
- [x] Enemies move toward player
- [x] Enemies take damage from projectiles
- [x] Enemy health bars visible (if debug enabled)
- [x] Enemies die when health reaches 0
- [x] Dead enemies award score

**Enemy Collision**
- [x] Enemies deal collision damage to player
- [x] Collision damage cooldown works
- [x] Player takes damage (check PlayerHealth)

**Score System**
- [x] Score increases when enemies die
- [x] Score value matches EnemyData
- [x] ScoreManager events fire correctly

**Wave System**
- [x] Enemies spawn in waves
- [x] Wave delay works correctly
- [x] Progressive difficulty increases enemy count
- [x] Spawner debug UI shows correct info

### Debug Tips
- Enable "Show Debug Info" on all components for visual gizmos
- Check Console for any error messages
- Use Scene view to watch projectile paths and enemy movement
- Verify all tags are set correctly: "Player", "Enemy"
- Ensure collision layers allow Player-Enemy and Projectile-Enemy collisions

---

## 9. Common Issues

**Projectiles don't spawn**
- Check WeaponData has projectile prefab assigned
- Verify ProjectilePool GameObject exists in scene
- Check PlayerInput component is on same GameObject as WeaponController

**Projectiles don't hit enemies**
- Verify enemy tag is "Enemy"
- Check projectile collider is trigger
- Ensure enemy has EnemyHealth component
- Verify collision layers in Physics settings

**Enemies don't move**
- Check player tag is "Player"
- Verify EnemyAI has EnemyData assigned
- Check Rigidbody constraints (Y should be frozen)
- Ensure AI is enabled (check debug info)

**Enemies don't damage player**
- Verify PlayerHealth component exists
- Check enemy collider is NOT trigger
- Ensure player has Rigidbody and collider
- Verify collision layers allow Enemy-Player collisions

**No score when enemies die**
- Check ScoreManager exists in scene
- Verify EnemyData has scoreValue > 0
- Ensure Enemy component is on enemy prefab

---

## 10. Performance Optimization

**For Bullet Hell Scenarios**
- Use ProjectilePool (already implemented)
- Keep initial pool size >= max projectiles on screen
- Enable pool expansion but set reasonable max
- Use Continuous Dynamic collision for projectiles
- Keep AI update interval >= 0.2 seconds
- Limit max enemies per wave

**Recommended Settings**
- Initial Pool Size: 50-100
- Max Pool Size: 200-500
- AI Update Interval: 0.2-0.3
- Max Enemies Per Wave: 30-50

---

## File Structure

```
Assets/_Project/Scripts/
├── Weapons/
│   ├── Projectile.cs
│   ├── ProjectilePool.cs
│   ├── WeaponController.cs
│   └── Data/
│       └── WeaponData.cs
├── Enemies/
│   ├── Enemy.cs
│   ├── EnemyHealth.cs
│   ├── EnemyAI.cs
│   ├── EnemySpawner.cs
│   └── Data/
│       └── EnemyData.cs
└── Managers/
    └── ScoreManager.cs
```

---

## Next Steps

1. Create UI to display score
2. Add weapon switching system
3. Implement more enemy types with different behaviors
4. Add visual/audio effects for impacts
5. Create weapon pickups or upgrades
6. Add particle effects for muzzle flash and death
7. Implement enemy object pooling for performance
8. Add boss enemies with unique mechanics

---

## Support

If you encounter issues:
1. Check all tags are set correctly
2. Verify all components have required references assigned
3. Enable debug info on components to visualize behavior
4. Check Unity Console for errors
5. Ensure Input Actions are properly configured in InputSystem_Actions.inputactions
