# Weapon System

Modular weapon system for RoombaRampage with projectile pooling and data-driven design.

## Components

### WeaponData.cs (ScriptableObject)
Data-driven weapon configuration. Create instances via `Create > RoombaRampage > Weapon Data`.

**Key Properties:**
- Damage, fire rate, projectile speed
- Projectile prefab reference
- Multi-shot configuration (count, spread angle)
- Ammo system (optional)
- Audio/VFX references (for future implementation)

### WeaponController.cs
Main weapon controller attached to player. Handles firing logic and input.

**Requirements:**
- PlayerInput component (uses New Input System)
- Attack button from InputSystem_Actions.inputactions

**Features:**
- Automatic fire rate control
- Look direction aiming
- Multi-projectile spread patterns
- Weapon switching support
- Ammo management

### Projectile.cs
Projectile behavior for bullets/missiles.

**Features:**
- Moves on XZ plane (3D top-down)
- Lifetime-based despawning
- Damage on enemy collision
- Object pool compatible

**Requirements:**
- Rigidbody (no gravity, Y position frozen)
- Collider (trigger enabled)

### ProjectilePool.cs (Singleton)
Object pooling system for performance optimization.

**Features:**
- Automatic pool creation per prefab type
- Dynamic pool expansion
- Pool size limits
- Efficient recycling

**Usage:**
- Place in scene once
- Auto-initializes pools on demand
- Call `GetProjectile()` to spawn
- Projectiles auto-return via `ReturnProjectile()`

## Setup Quick Reference

1. Create WeaponData asset
2. Create projectile prefab with Projectile.cs
3. Assign projectile prefab to WeaponData
4. Add WeaponController to player
5. Assign WeaponData to WeaponController
6. Add ProjectilePool GameObject to scene
7. Test with Attack button

See `COMBAT_SETUP.md` for detailed instructions.

## Example Weapon Types

**Single Shot** - Standard balanced weapon
**Spread Shot** - Wide area coverage (3+ projectiles)
**Rapid Fire** - High fire rate, lower damage
**Sniper** - Slow fire rate, high damage, high speed

All weapon types use the same WeaponData ScriptableObject with different values.
