# Enemy System

AI-driven enemy system with health, movement, and spawning for RoombaRampage.

## Components

### EnemyData.cs (ScriptableObject)
Data-driven enemy configuration. Create instances via `Create > RoombaRampage > Enemy Data`.

**Key Properties:**
- Health, movement speed, damage
- AI behavior settings (update rate, detection range, stopping distance)
- Score reward value
- Prefab reference
- Audio/VFX references (for future implementation)

### Enemy.cs
Main enemy controller. Integrates health, AI, and collision damage.

**Requirements:**
- EnemyHealth component
- EnemyAI component
- "Enemy" tag
- Rigidbody and Collider

**Features:**
- Collision damage to player
- Death handling
- Component coordination
- Score awarding on death

### EnemyHealth.cs
Health management system for enemies.

**Features:**
- Damage and healing
- Death events
- Health percentage calculation
- Debug health bar display
- ScoreManager integration

**Events:**
- OnDamageTaken - Invoked when damaged
- OnDeath - Invoked when health reaches 0

### EnemyAI.cs
Simple AI that moves enemy toward player.

**Features:**
- Tag-based player finding ("Player" tag)
- Movement on XZ plane (3D top-down)
- Configurable update rate for performance
- Detection range support
- Stopping distance (optional)
- Smooth rotation toward player

**Requirements:**
- Rigidbody (no gravity, Y frozen)

### EnemySpawner.cs
Wave-based enemy spawning system.

**Features:**
- Random enemy type selection
- Circular spawn area (min/max radius)
- Wave progression (increasing difficulty)
- Auto-spawn with configurable delay
- Manual spawn controls
- Active enemy tracking

**Configuration:**
- Add EnemyData assets to spawn list
- Set spawn area radius
- Configure wave settings
- Enable/disable progression

## Setup Quick Reference

1. Create EnemyData asset
2. Create enemy prefab with all components:
   - Rigidbody (no gravity, Y frozen)
   - Collider (NOT trigger)
   - EnemyHealth.cs
   - EnemyAI.cs
   - Enemy.cs
3. Set enemy tag to "Enemy"
4. Assign EnemyData to all components
5. Assign prefab to EnemyData
6. Add EnemySpawner to scene
7. Add EnemyData to spawner's enemy list
8. Configure spawn area and wave settings

See `COMBAT_SETUP.md` for detailed instructions.

## Enemy Types Examples

**Basic Enemy** - Balanced health and speed
**Fast Enemy** - Low health, high speed, worth more points
**Tank Enemy** - High health, slow, high damage
**Swarm Enemy** - Very low health, medium speed, spawns in large groups

All enemy types use the same component setup with different EnemyData values.

## AI Behavior

- Finds player by "Player" tag
- Updates decision-making periodically (AI Update Interval)
- Moves toward player on XZ plane
- Rotates to face movement direction
- Stops at stopping distance (optional)
- Respects detection range (0 = infinite)

## Performance Tips

- Use AI Update Interval >= 0.2 for better performance
- Limit max enemies per wave (30-50 recommended)
- Consider implementing enemy object pooling for bullet hell scenarios
- Use detection range to reduce AI overhead for distant enemies
