# Player Controller System - RoombaRampage

## Quick Start

### 1. Create ScriptableObject Assets

**PlayerStats Asset**:
- Location: `Assets/_Project/Data/Player/PlayerStats_Default.asset`
- Create: Right-click > Create > RoombaRampage > Player Stats
- Configure default values in Inspector

**PlayerEvents Asset**:
- Location: `Assets/_Project/Data/Events/PlayerEvents.asset`
- Create: Right-click > Create > RoombaRampage > Player Events
- Single global instance for all events

### 2. Setup Player GameObject

**Required Components**:
- Rigidbody2D (Dynamic, Continuous, Interpolate)
- CircleCollider2D (radius 0.5)
- SpriteRenderer (roomba sprite)
- PlayerController script
- PlayerInput script
- PlayerHealth script
- PlayerVisuals script

**References to Assign**:
- PlayerController.stats → PlayerStats_Default
- PlayerController.playerEvents → PlayerEvents
- PlayerHealth.stats → PlayerStats_Default
- PlayerHealth.playerEvents → PlayerEvents
- PlayerVisuals.spriteRenderer → SpriteRenderer
- PlayerVisuals.playerEvents → PlayerEvents

### 3. Configure Rigidbody2D

- Mass: 1.0
- Linear Drag: 1.5
- Angular Drag: 3.0
- Gravity Scale: 0
- Collision Detection: Continuous
- Interpolation: Interpolate
- Constraints: Freeze Rotation Z

### 4. Test Movement

- Enter Play Mode
- Use WASD to move
- Tune PlayerStats values in Inspector during Play Mode
- Copy tuned values back to asset when satisfied

## File Structure

```
Player/
├── PlayerController.cs      # Physics-based movement
├── PlayerInput.cs           # New Input System integration
├── PlayerHealth.cs          # Health & damage management
├── PlayerVisuals.cs         # Visual feedback effects
└── Data/
    ├── PlayerStats.cs       # Configuration ScriptableObject
    └── PlayerEvents.cs      # Event channel ScriptableObject
```

## Component Responsibilities

**PlayerController**: Movement physics, acceleration, rotation, boundaries
**PlayerInput**: Input capture, WASD/Mouse, enable/disable
**PlayerHealth**: Damage, healing, death, invulnerability
**PlayerVisuals**: Damage flash, invulnerability flash, death fade

**PlayerStats**: Configuration data (movement, health, combat)
**PlayerEvents**: Event channel (damage, death, respawn events)

## Documentation

Full architecture documentation:
- `C:\DEVELOPMENT\Systems\thoughts\30_10_25_IDEA\player_controller.md`
- `C:\DEVELOPMENT\Systems\thoughts\30_10_25_IDEA\IMPLEMENTATION_SUMMARY.md`

## Default Values

**Movement**: Acceleration 15, Max Speed 10, Rotation 150°/s
**Health**: Max Health 100, Invulnerability 1.5s
**Arena**: Width 20, Height 15

## Testing

Use context menu test methods:
- PlayerHealth > Test: Take 10 Damage
- PlayerHealth > Test: Heal 20 HP
- PlayerHealth > Test: Kill Player
- PlayerHealth > Test: Reset Health
- PlayerVisuals > Test: Play Damage Flash
- PlayerVisuals > Test: Play Death Effect

Enable debug toggles in Inspector for runtime visualization.

## Next Steps

1. Create PlayerStats and PlayerEvents assets
2. Setup Player prefab with all components
3. Test movement in play mode
4. Tune stats for desired feel
5. Implement combat system (PlayerWeapon)
6. Add audio integration
7. Create upgrade system
