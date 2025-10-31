# RoombaRampage - Unity Implementation TODO List

## 📋 Project Setup & Architecture

### Folder Structure
- [ ] Create `Assets/_Project/` main folder structure
- [ ] Set up `Scripts/` with subfolders: Core, Player, Combat, Enemies, Progression, UI, Audio, Utilities
- [ ] Set up `ScriptableObjects/` with subfolders: Skills, Weapons, Enemies, Waves, Config
- [ ] Set up `Prefabs/` with subfolders: Player, Enemies, Projectiles, VFX, UI, Pickups
- [ ] Set up `Scenes/` folder: MainMenu, Arena_LivingRoom, ChargingStation, TestScenes
- [ ] Set up `Materials/`, `Audio/SFX/`, `Audio/Music/`, `VFX/`, `Animations/` folders
- [ ] Create `.gitignore` for Unity project (Library, Temp, Logs, etc.)

### Core Systems Setup
- [ ] Create `GameManager.cs` singleton for game state management
- [ ] Create `RunManager.cs` for wave/level progression
- [ ] Create `SaveSystem.cs` for persistent data (upgrades, unlocks)
- [ ] Set up scene management system (loading, transitions)
- [ ] Configure Unity Input System actions (Move, Aim, Shoot, Pause, Use Skill)

---

## 🤖 Player Systems

### Player Controller (Movement & Controls)
- [ ] Create `PlayerController.cs` - top-down driving physics
- [ ] Implement steering mechanics (rotation, drift feel)
- [ ] Implement acceleration/deceleration with proper physics
- [ ] Add collision detection and obstacle avoidance
- [ ] Implement smooth camera follow system
- [ ] Add screen boundaries/arena constraints
- [ ] Test controller feel and responsiveness

### Player Health & Stats
- [ ] Create `PlayerHealth.cs` component
- [ ] Create `PlayerStats.cs` scriptable object for base stats
- [ ] Implement health system with events (OnDamage, OnDeath)
- [ ] Create visual damage feedback (screen flash, shake)
- [ ] Implement invincibility frames after taking damage
- [ ] Add health regeneration at charging stations

### Player Combat
- [ ] Create weapon system architecture (base `Weapon.cs` class)
- [ ] Implement auto-firing weapon system
- [ ] Create projectile pooling system (`ObjectPool<Projectile>`)
- [ ] Implement different weapon types (Laser, Bullet, Missile)
- [ ] Create weapon ScriptableObjects for data-driven config
- [ ] Implement weapon switching/upgrades
- [ ] Add muzzle flash VFX
- [ ] Add screen shake on firing

---

## 🎯 Combat Systems

### Damage System
- [ ] Create `IDamageable` interface
- [ ] Create `DamageSystem.cs` for damage calculations
- [ ] Implement damage types (normal, explosive, piercing)
- [ ] Add critical hit system
- [ ] Implement damage numbers popup UI
- [ ] Add hit effects (particles, sound)

### Projectile System
- [ ] Create `Projectile.cs` base class with IPoolable
- [ ] Implement projectile movement (straight, homing, arc)
- [ ] Add projectile lifetime and auto-return to pool
- [ ] Create different projectile prefabs (laser, bullet, missile)
- [ ] Implement projectile collision detection
- [ ] Add projectile VFX trails

---

## 👾 Enemy Systems

### Enemy Base Architecture
- [ ] Create `EnemyBase.cs` abstract class
- [ ] Implement enemy health system (use Health component)
- [ ] Create enemy movement patterns (chase, circle, flee)
- [ ] Implement basic enemy AI state machine (Idle, Chase, Attack, Dead)
- [ ] Add enemy collision avoidance (don't stack on each other)

### Enemy Types
- [ ] Create `SwarmBug.cs` - basic fast melee enemy
- [ ] Create ranged enemy type (shoots at player)
- [ ] Create tank enemy type (slow, high HP)
- [ ] Create boss enemy controller
- [ ] Design boss attack patterns
- [ ] Create enemy ScriptableObjects for stats

### Enemy Spawning
- [ ] Create `EnemySpawner.cs` system
- [ ] Implement wave system with escalating difficulty
- [ ] Create spawn points around arena
- [ ] Implement enemy pooling system
- [ ] Add boss spawn logic (end of waves)
- [ ] Balance spawn rates and quantities

---

## 📈 Progression Systems

### XP & Leveling
- [ ] Create `XPSystem.cs` with events
- [ ] Implement XP gain on enemy kill
- [ ] Create level-up system with XP curve
- [ ] Add visual XP collection (gems fly to player)
- [ ] Implement level-up notification/fanfare

### Skill System
- [ ] Create `Skill` ScriptableObject base class
- [ ] Define skill effect types (FireRate, Damage, MoveSpeed, Health, etc.)
- [ ] Create `SkillManager.cs` to track active skills
- [ ] Implement skill selection UI on level-up
- [ ] Create skill pool (common, rare, legendary)
- [ ] Add skill rarity system and visuals
- [ ] Implement skill stacking logic
- [ ] Create 20+ unique skills with fun names
- [ ] Add skill tooltips and descriptions

### Upgrade System (Charging Station)
- [ ] Create permanent upgrade system
- [ ] Design upgrade tree/options
- [ ] Implement currency system (scrap metal?)
- [ ] Create upgrade UI at charging station
- [ ] Add upgrade persistence (SaveSystem)

---

## 🎨 UI Systems

### HUD
- [ ] Create `HUDController.cs`
- [ ] Health bar display
- [ ] XP bar with level indicator
- [ ] Active skills display
- [ ] Wave/timer display
- [ ] Kill counter
- [ ] Boss health bar (when boss active)

### Menus
- [ ] Create main menu scene
- [ ] Create pause menu with resume/settings/quit
- [ ] Create level-up skill selection UI
- [ ] Create charging station upgrade UI
- [ ] Create game over/victory screen
- [ ] Create settings menu (audio, controls, graphics)

### Feedback & Polish UI
- [ ] Damage numbers system
- [ ] Floating text for events ("LEVEL UP!", "BOSS INCOMING!")
- [ ] Screen overlays (damage vignette, low health warning)
- [ ] Combo counter UI
- [ ] Minimap (optional)

---

## 🎵 Audio Systems

### Audio Manager
- [ ] Create `AudioManager.cs` singleton
- [ ] Implement sound effect pooling
- [ ] Add music manager with crossfading
- [ ] Implement audio volume settings
- [ ] Add audio mixer groups (SFX, Music, Master)

### Sound Effects
- [ ] Player weapon firing sounds
- [ ] Player hit/damage sounds
- [ ] Enemy death sounds (different per type)
- [ ] Projectile impact sounds
- [ ] Level-up sound
- [ ] UI click/hover sounds
- [ ] Charging station ambient sound

### Music
- [ ] Main menu music
- [ ] Gameplay music (intense, looping)
- [ ] Boss battle music
- [ ] Charging station music (calm)
- [ ] Victory/game over music

---

## ✨ VFX & Juice

### Player VFX
- [ ] Muzzle flash particles
- [ ] Dust trail particles while moving
- [ ] Damage hit effect on player
- [ ] Level-up particle explosion
- [ ] Invincibility shield effect

### Combat VFX
- [ ] Projectile trails
- [ ] Impact particles (hit enemy)
- [ ] Enemy death explosion
- [ ] Boss death epic explosion
- [ ] Critical hit special effect

### Environment VFX
- [ ] Charging station glow/pulses
- [ ] Pickup item shine/rotation
- [ ] Arena environmental particles

### Screen Effects
- [ ] Camera shake (shooting, hits, explosions)
- [ ] Chromatic aberration on damage
- [ ] Slow-mo on level-up (brief)
- [ ] Screen flash on boss spawn

---

## 🏠 Scenes & Environments

### Main Menu Scene
- [ ] Create main menu layout
- [ ] Add play/settings/quit buttons
- [ ] Add background animation
- [ ] Implement scene loading

### Arena Scene (Living Room)
- [ ] Design arena layout (furniture as obstacles)
- [ ] Add collision boundaries
- [ ] Place spawn points
- [ ] Add environmental props
- [ ] Lighting setup (URP)
- [ ] Add background decorations

### Charging Station Scene
- [ ] Design charging hub area
- [ ] Create upgrade station interaction
- [ ] Add ambient animations
- [ ] Implement scene transition

### Test Scenes
- [ ] Create player movement test scene
- [ ] Create combat test scene
- [ ] Create enemy spawning test scene
- [ ] Create UI test scene

---

## 🎮 Gameplay Features

### Pickups & Items
- [ ] Create pickup system (health, ammo, power-ups)
- [ ] Implement magnetic attraction to player
- [ ] Add pickup VFX and sounds
- [ ] Create temporary power-up system (speed boost, invincibility)

### Loot System
- [ ] Create loot box/crate system
- [ ] Implement random loot generation
- [ ] Add loot rarity (common, rare, epic)
- [ ] Create opening animation

### Charging Stations (In-Level)
- [ ] Create charging station prefab
- [ ] Implement health restoration
- [ ] Add interaction prompt UI
- [ ] Add cooldown/limited uses

### Wave System
- [ ] Implement wave countdown between spawns
- [ ] Add wave difficulty scaling
- [ ] Create boss wave logic (every 5 waves?)
- [ ] Add visual wave indicator

---

## 💬 Character & Flavor

### Roomba Personality
- [ ] Create dialog/message system
- [ ] Write funny roomba comments (10+ per category)
  - [ ] Starting run messages
  - [ ] Taking damage messages
  - [ ] Level-up messages
  - [ ] Killing enemies messages
  - [ ] Low health messages
  - [ ] Boss encounter messages
  - [ ] Victory messages
- [ ] Implement randomized message display
- [ ] Add message UI with character portrait

### Skill Names
- [ ] Create funny/themed skill names (20+)
- [ ] Examples: "Dust Buster Deluxe", "Turbo Suction", "Laser Light Show", "Roomba Rage"

---

## 🧪 Testing & Polish

### Core Gameplay Testing
- [ ] Test player movement feel (acceleration, steering)
- [ ] Test combat feedback (satisfying hits)
- [ ] Test difficulty curve (too easy/hard?)
- [ ] Test wave progression pacing
- [ ] Test skill balance

### Performance Optimization
- [ ] Profile frame rate in intense scenarios
- [ ] Optimize object pooling (enemies, projectiles)
- [ ] Optimize particle systems (max particles)
- [ ] Test on target platforms (PC, mobile?)
- [ ] Implement LOD if needed

### Bug Testing
- [ ] Test edge cases (player stuck, enemies stuck)
- [ ] Test all UI states and transitions
- [ ] Test save/load system
- [ ] Test pause/resume functionality
- [ ] Test skill stacking limits

### Balance & Tuning
- [ ] Balance weapon damage values
- [ ] Balance enemy health pools
- [ ] Balance XP curve for 10-minute runs
- [ ] Tune movement speed/acceleration
- [ ] Adjust spawn rates

---

## 📦 Build & Release

### Build Configuration
- [ ] Configure build settings for PC
- [ ] Configure build settings for mobile (if applicable)
- [ ] Set up version numbering
- [ ] Create build pipeline/script
- [ ] Test standalone builds

### Release Preparation
- [ ] Create game icon
- [ ] Write game description
- [ ] Create screenshots/trailer
- [ ] Set up itch.io/Steam page
- [ ] Prepare patch notes template

---

## 🔧 Development Tools

### Editor Tools
- [ ] Create custom editor for skill creation
- [ ] Create wave configuration editor
- [ ] Add debug menu (god mode, spawn enemies, etc.)
- [ ] Create scene validation tools
- [ ] Add quick test buttons in editor

### Debug Features
- [ ] Add god mode toggle
- [ ] Add enemy spawn debug commands
- [ ] Add level-up debug command
- [ ] Add skill unlock debug panel
- [ ] Add FPS counter option

---

## 📝 Documentation

- [ ] Document code architecture
- [ ] Create skill design template
- [ ] Write enemy design guidelines
- [ ] Document save data format
- [ ] Create contributing guide (if open source)

---

## Priority Phases

### Phase 1: Core Gameplay Loop (MVP)
✅ Priority items to get playable prototype:
1. Player controller with basic movement
2. Simple weapon that shoots straight
3. Basic enemy that chases player
4. Health system for player and enemies
5. Enemy spawner with waves
6. Basic HUD (health, wave counter)
7. Game over/victory conditions

### Phase 2: Progression Systems
1. XP and leveling
2. Skill system (5-10 skills)
3. Level-up UI
4. Skill selection on level-up

### Phase 3: Content & Polish
1. Multiple enemy types
2. Boss encounters
3. More skills (20+)
4. VFX and audio
5. Charging station scene
6. Permanent upgrades

### Phase 4: Juice & Personality
1. Roomba dialog system
2. Screen effects and camera shake
3. Particle effects
4. Sound effects and music
5. Polish UI

### Phase 5: Launch Preparation
1. Balance tuning
2. Performance optimization
3. Bug fixing
4. Build and release

---

**Estimated Development Time**: 6-8 weeks for full game (solo dev)
**MVP Playable**: 1-2 weeks
**Current Phase**: Phase 1 - Core Gameplay Loop
