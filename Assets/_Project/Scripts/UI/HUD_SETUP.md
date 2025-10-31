# HUD System Setup Guide

Complete guide for setting up the RoombaRampage HUD (Heads-Up Display) system in Unity 6 with URP.

## Table of Contents
1. [Overview](#overview)
2. [Canvas Setup](#canvas-setup)
3. [Component Setup](#component-setup)
4. [Integration](#integration)
5. [Visual Styling](#visual-styling)
6. [Testing](#testing)
7. [Troubleshooting](#troubleshooting)

---

## Overview

The HUD system consists of 6 components:

- **HealthBar** - Reusable health bar (player, boss)
- **ScoreDisplay** - Score counter with count-up animation
- **WaveDisplay** - Wave number and enemy count
- **KillCounter** - Kill tracking with optional combo
- **XPBar** - XP bar (placeholder for future XP system)
- **HUDManager** - Central HUD controller

### HUD Layout

```
┌─────────────────────────────────────────────────────────────┐
│ [████████░░░] 85/100 HP    WAVE 5 - 12 Enemies             │
│                                             SCORE: 12,340   │
│                                             KILLS: 147      │
│                                                              │
│                    [GAMEPLAY AREA]                          │
│                                                              │
│                                                              │
│ [████████████░░░░░] LVL 8                                   │
└─────────────────────────────────────────────────────────────┘
```

---

## Canvas Setup

### 1. Create Main HUD Canvas

1. **Hierarchy** → Right-click → **UI** → **Canvas**
2. Rename to `HUD_Canvas`
3. Configure Canvas component:
   - **Render Mode**: Screen Space - Overlay
   - **Pixel Perfect**: Unchecked (for smooth scaling)
   - **Sort Order**: 10 (render above other UI)

4. Configure Canvas Scaler:
   - **UI Scale Mode**: Scale With Screen Size
   - **Reference Resolution**: 1920 x 1080
   - **Screen Match Mode**: Match Width Or Height
   - **Match**: 0.5 (balance between width/height)

### 2. Add EventSystem

If you don't have an EventSystem:
- **Hierarchy** → Right-click → **UI** → **Event System**
- Should auto-create when you make the Canvas

### 3. Create HUD Layout Structure

Create empty GameObjects to organize HUD elements:

```
HUD_Canvas
├── HUDManager (script)
├── TopLeft_Container
│   └── PlayerHealthBar
├── TopCenter_Container
│   ├── WaveDisplay
│   └── BossHealthBar (hidden by default)
├── TopRight_Container
│   ├── ScoreDisplay
│   └── KillCounter
└── Bottom_Container
    └── XPBar
```

**To create containers:**
1. Right-click `HUD_Canvas` → **Create Empty**
2. Rename (e.g., `TopLeft_Container`)
3. Add **Rect Transform** (auto-added)
4. Set anchors/position (see layout section below)

---

## Component Setup

### Player Health Bar

#### A. Create UI Elements

1. **TopLeft_Container** → Right-click → **UI** → **Image**
   - Rename: `PlayerHealthBar_Background`
   - Color: Dark gray (RGBA: 20, 20, 20, 180)
   - **Rect Transform**:
     - Anchors: Top-Left
     - Position: X=150, Y=-30
     - Width: 300, Height: 40

2. **PlayerHealthBar_Background** → Right-click → **UI** → **Image**
   - Rename: `PlayerHealthBar_Fill`
   - Color: Green (will be set by script)
   - **Image Type**: Filled
   - **Fill Method**: Horizontal
   - **Fill Origin**: Left
   - **Fill Amount**: 1.0
   - **Rect Transform**: Stretch to fill parent (anchors min=0,0 max=1,1, offsets=0)

3. **PlayerHealthBar_Background** → Right-click → **UI** → **TextMeshPro - Text**
   - Rename: `PlayerHealthBar_Text`
   - Text: "100/100"
   - Font: Bold, Size: 18
   - Alignment: Center (both axes)
   - Color: White
   - **Rect Transform**: Stretch to fill parent

#### B. Add HealthBar Script

1. Select `PlayerHealthBar_Background`
2. **Add Component** → Search "HealthBar" → Add script
3. Configure in Inspector:
   - **Fill Image**: Drag `PlayerHealthBar_Fill`
   - **Health Text**: Drag `PlayerHealthBar_Text`
   - **Fill Speed**: 5
   - **Enable Damage Flash**: ✓
   - **High Health Color**: RGB(50, 255, 50)
   - **Medium Health Color**: RGB(255, 255, 0)
   - **Low Health Color**: RGB(255, 50, 50)

### Boss Health Bar

Same as Player Health Bar, but:
- Position: TopCenter, X=0, Y=-30
- Width: 600, Height: 50
- Larger text (size 20)
- Initially disabled (uncheck GameObject)

---

### Score Display

#### A. Create UI Element

1. **TopRight_Container** → Right-click → **UI** → **TextMeshPro - Text**
   - Rename: `ScoreDisplay`
   - Text: "Score: 0"
   - Font: Bold, Size: 24
   - Alignment: Right, Top
   - Color: White or Gold (RGBA: 255, 215, 0, 255)
   - **Rect Transform**:
     - Anchors: Top-Right
     - Position: X=-30, Y=-30
     - Width: 300, Height: 40

#### B. Add ScoreDisplay Script

1. Select `ScoreDisplay`
2. **Add Component** → Search "ScoreDisplay" → Add script
3. Configure:
   - **Score Text**: Auto-assigned (self)
   - **Prefix**: "Score: "
   - **Use Thousand Separator**: ✓
   - **Enable Animation**: ✓
   - **Count Up Speed**: 50

---

### Wave Display

#### A. Create UI Element

1. **TopCenter_Container** → Right-click → **UI** → **TextMeshPro - Text**
   - Rename: `WaveDisplay`
   - Text: "Wave 1 - Enemies: 0"
   - Font: Bold, Size: 20
   - Alignment: Center, Top
   - Color: White
   - **Rect Transform**:
     - Anchors: Top-Center
     - Position: X=0, Y=-80
     - Width: 400, Height: 40

#### B. Add WaveDisplay Script

1. Select `WaveDisplay`
2. **Add Component** → Search "WaveDisplay" → Add script
3. Configure:
   - **Wave Text**: Auto-assigned (self)
   - **Enemy Spawner**: Drag from scene (EnemySpawner GameObject)
   - **Display Format**: "Wave {0} - Enemies: {1}"
   - **Show Enemy Count**: ✓
   - **Update Interval**: 0.5

---

### Kill Counter

#### A. Create UI Element

1. **TopRight_Container** → Right-click → **UI** → **TextMeshPro - Text**
   - Rename: `KillCounter`
   - Text: "Kills: 0"
   - Font: Bold, Size: 20
   - Alignment: Right, Top
   - Color: White
   - **Rect Transform**:
     - Anchors: Top-Right
     - Position: X=-30, Y=-70
     - Width: 300, Height: 40

#### B. Add KillCounter Script

1. Select `KillCounter`
2. **Add Component** → Search "KillCounter" → Add script
3. Configure:
   - **Kill Count Text**: Auto-assigned (self)
   - **Player Events**: Drag from Project (Assets/_Project/Data/Events/PlayerEvents)
   - **Prefix**: "Kills: "
   - **Enable Combo**: ✓ (optional)
   - **Combo Window**: 3.0

**Note:** Ensure PlayerEvents asset exists at `Assets/_Project/Data/Events/PlayerEvents`

---

### XP Bar

#### A. Create UI Elements

1. **Bottom_Container** → Right-click → **UI** → **Image**
   - Rename: `XPBar_Background`
   - Color: Dark gray (RGBA: 20, 20, 20, 180)
   - **Rect Transform**:
     - Anchors: Bottom-Stretch (min X=0, max X=1)
     - Position: Y=20
     - Height: 30
     - Left/Right Offsets: 50

2. **XPBar_Background** → Right-click → **UI** → **Image**
   - Rename: `XPBar_Fill`
   - Color: Cyan (RGBA: 76, 204, 255, 255)
   - **Image Type**: Filled
   - **Fill Method**: Horizontal
   - **Rect Transform**: Stretch to fill parent

3. **XPBar_Background** → Right-click → **UI** → **TextMeshPro - Text**
   - Rename: `XPBar_LevelText`
   - Text: "LVL 1"
   - Font: Bold, Size: 16
   - Alignment: Left, Center
   - Color: White
   - **Rect Transform**: Position X=10, stretch vertically

#### B. Add XPBar Script

1. Select `XPBar_Background`
2. **Add Component** → Search "XPBar" → Add script
3. Configure:
   - **Fill Image**: Drag `XPBar_Fill`
   - **Level Text**: Drag `XPBar_LevelText`
   - **Fill Speed**: 3
   - **XP Color**: RGB(76, 204, 255) - Cyan
   - **Test Level**: 1
   - **Test XP Progress**: 0.5

---

### HUD Manager

#### Add HUDManager Script

1. Select `HUD_Canvas` GameObject
2. **Add Component** → Search "HUDManager" → Add script
3. Configure:
   - **Player Health Bar**: Drag `PlayerHealthBar_Background`
   - **Boss Health Bar**: Drag `BossHealthBar_Background`
   - **Score Display**: Drag `ScoreDisplay`
   - **Wave Display**: Drag `WaveDisplay`
   - **Kill Counter**: Drag `KillCounter`
   - **XP Bar**: Drag `XPBar_Background`
   - **Player Health**: Drag Player GameObject from scene
   - **Enemy Spawner**: Drag EnemySpawner from scene
   - **Player Events**: Drag PlayerEvents asset
   - **Show On Start**: ✓
   - **Auto Initialize**: ✓

---

## Integration

### Connect to Existing Systems

#### 1. Player Health Integration

The HUDManager automatically subscribes to PlayerEvents. Ensure:

**A. PlayerEvents Asset Exists:**
1. Create: **Assets/_Project/Data/Events/** folder
2. Right-click → **Create** → **RoombaRampage** → **Player Events**
3. Name: `PlayerEvents`

**B. Assign to PlayerHealth:**
1. Select Player GameObject
2. Find **PlayerHealth** component
3. Drag `PlayerEvents` asset to **Player Events** field

#### 2. Score Integration

ScoreDisplay auto-connects to `ScoreManager.Instance`. Ensure:
- ScoreManager GameObject exists in scene
- ScoreManager is initialized before HUD

#### 3. Enemy Spawner Integration

WaveDisplay requires EnemySpawner reference:
- Drag EnemySpawner GameObject to WaveDisplay script
- Or let HUDManager auto-find it

#### 4. Kill Counter Integration

**Enemy Death → Kill Counter Flow:**

Add this to your `EnemyHealth.cs` or `Enemy.cs` death handling:

```csharp
// In Enemy death method:
if (playerEvents != null)
{
    int scoreValue = enemyData?.scoreValue ?? 10;
    playerEvents.RaiseEnemyKilled(scoreValue);
}
```

**Or integrate in ScoreManager:**

```csharp
// In ScoreManager.AddScore():
public void AddScore(int amount)
{
    currentScore += amount;
    OnScoreChanged?.Invoke(currentScore);

    // Trigger enemy killed event for kill counter
    if (playerEvents != null)
    {
        playerEvents.RaiseEnemyKilled(amount);
    }
}
```

---

## Visual Styling

### Recommended Colors

**Health Bar:**
- High Health: RGB(50, 255, 50) - Bright Green
- Medium Health: RGB(255, 255, 0) - Yellow
- Low Health: RGB(255, 50, 50) - Red
- Background: RGBA(20, 20, 20, 180) - Dark Semi-transparent

**Score/Kills:**
- Text: RGB(255, 215, 0) - Gold
- Or: RGB(255, 255, 255) - White

**Wave Display:**
- Text: RGB(255, 255, 255) - White

**XP Bar:**
- Fill: RGB(76, 204, 255) - Cyan
- Or: RGB(200, 100, 255) - Purple

### Font Recommendations

**Best Fonts for HUD:**
1. **Orbitron** - Sci-fi style (free on Google Fonts)
2. **Rajdhani** - Clean, modern
3. **Exo 2** - Geometric, techy
4. **Audiowide** - Bold, readable

**Import Font to Unity:**
1. Download .ttf file
2. Import to `Assets/Fonts/`
3. Create TextMeshPro font asset:
   - **Window** → **TextMeshPro** → **Font Asset Creator**
   - Select font, set size 32-48
   - Generate atlas

### Text Effects

**Add Outline (Improves Readability):**
1. Select TMP text
2. **Material Preset**: Dropdown → Create new preset
3. **Outline** section:
   - **Thickness**: 0.2
   - **Color**: Black
   - **Softness**: 0

**Add Glow:**
1. **Glow** section:
   - **Color**: White (low alpha ~30)
   - **Offset**: 0
   - **Inner**: 0.5
   - **Outer**: 0.5

---

## Testing

### Testing Checklist

#### Health Bar Tests
- [ ] Health bar displays correctly on game start
- [ ] Health bar updates when player takes damage
- [ ] Health bar changes color (green → yellow → red)
- [ ] Damage flash effect triggers
- [ ] Health text shows correct values (e.g., "85/100")
- [ ] Smooth animation (no instant jumps)

**Test Commands (PlayerHealth context menu):**
- "Test: Take 10 Damage"
- "Test: Heal 20 HP"
- "Test: Kill Player"

#### Score Display Tests
- [ ] Score starts at 0
- [ ] Score updates when ScoreManager.AddScore() called
- [ ] Count-up animation works smoothly
- [ ] Thousand separators display (e.g., "12,340")
- [ ] Score persists across scenes (if using DontDestroyOnLoad)

**Test via ScoreManager debug GUI or console:**
```csharp
ScoreManager.Instance.AddScore(100);
```

#### Wave Display Tests
- [ ] Wave number displays correctly
- [ ] Enemy count updates as enemies spawn/die
- [ ] Display format correct: "Wave 5 - Enemies: 12"
- [ ] Updates every 0.5 seconds (not every frame)

**Test via EnemySpawner:**
- Use "Spawn Wave Now" button in EnemySpawner debug GUI

#### Kill Counter Tests
- [ ] Starts at 0
- [ ] Increments when enemies die
- [ ] Combo counter works (if enabled)
- [ ] Resets on player death/respawn

**Test via KillCounter context menu:**
- "Test: Add Kill"
- "Test: Add 5 Kills"

#### XP Bar Tests
- [ ] XP bar displays at bottom
- [ ] Level text shows (e.g., "LVL 1")
- [ ] Fill animates smoothly
- [ ] Placeholder test data works

**Test via XPBar context menu:**
- "Test: Set 75% XP"
- "Test: Level Up"

### Debug Mode

Enable debug info on any component:
1. Select component in Inspector
2. Expand **Debug** section
3. Check **Show Debug Info**
4. Play mode → See debug output in Console and on-screen

---

## Troubleshooting

### Common Issues

#### "HUD not visible in game"

**Solutions:**
1. Check Canvas Render Mode is "Screen Space - Overlay"
2. Ensure HUD_Canvas GameObject is active
3. Check Canvas Sort Order (should be 10+)
4. Verify Camera has Canvas Scaler

#### "Health bar doesn't update"

**Solutions:**
1. Check PlayerHealth has PlayerEvents assigned
2. Verify PlayerEvents asset exists
3. Check HUDManager has correct references
4. Enable debug mode on HealthBar and HUDManager
5. Ensure HUDManager.Initialize() was called

#### "Score display shows 0 forever"

**Solutions:**
1. Verify ScoreManager exists in scene
2. Check ScoreManager is initialized first
3. Test manually: `ScoreManager.Instance.AddScore(100);`
4. Enable debug mode on ScoreDisplay

#### "Wave display doesn't update"

**Solutions:**
1. Check EnemySpawner reference is assigned
2. Verify EnemySpawner is spawning enemies
3. Check Update Interval (lower = more frequent)
4. Enable debug mode on WaveDisplay

#### "Kill counter not counting"

**Solutions:**
1. Check PlayerEvents reference is assigned
2. Verify enemies raise `PlayerEvents.RaiseEnemyKilled()` on death
3. Check Event References in Inspector
4. Test manually: "Test: Add Kill" context menu

#### "TextMeshPro not found"

**Solutions:**
1. **Window** → **TextMeshPro** → **Import TMP Essential Resources**
2. Reimport scripts: Right-click Scripts folder → Reimport
3. Restart Unity

#### "Components not showing in Inspector"

**Solutions:**
1. Check for compile errors in Console
2. Verify all scripts have correct namespace: `RoombaRampage.UI`
3. Reimport scripts
4. Restart Unity

#### "Fill images not filling correctly"

**Solutions:**
1. Check Image Type is "Filled"
2. Verify Fill Method is "Horizontal"
3. Check Fill Origin is "Left"
4. Ensure Rect Transform stretches to parent
5. Anchors: min(0,0) max(1,1), offsets all 0

### Performance Issues

**HUD causing lag:**
1. Increase WaveDisplay Update Interval (0.5 → 1.0)
2. Disable combo counter if not needed
3. Reduce score count-up speed
4. Disable damage flash effect
5. Use simpler fonts (no complex glyphs)

---

## Advanced Customization

### Anchoring Layout (Responsive Design)

**TopLeft_Container:**
- Anchors: min(0, 1), max(0, 1)
- Pivot: (0, 1)
- Position: X=20, Y=-20

**TopRight_Container:**
- Anchors: min(1, 1), max(1, 1)
- Pivot: (1, 1)
- Position: X=-20, Y=-20

**TopCenter_Container:**
- Anchors: min(0.5, 1), max(0.5, 1)
- Pivot: (0.5, 1)
- Position: X=0, Y=-20

**Bottom_Container:**
- Anchors: min(0, 0), max(1, 0)
- Pivot: (0.5, 0)
- Position: X=0, Y=20

### Adding Screen Shake on Low Health

Create new script `LowHealthScreenShake.cs`:

```csharp
// Attach to Camera
public class LowHealthScreenShake : MonoBehaviour
{
    [SerializeField] private Player.PlayerHealth playerHealth;
    [SerializeField] private float shakeThreshold = 0.2f; // 20% health
    [SerializeField] private float shakeIntensity = 0.1f;

    private void Update()
    {
        if (playerHealth != null && playerHealth.HealthNormalized < shakeThreshold)
        {
            // Apply shake (integrate with your camera system)
        }
    }
}
```

### Adding HUD Fade In/Out

Add to HUDManager:

```csharp
public void FadeHUD(float targetAlpha, float duration)
{
    CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
    if (canvasGroup == null)
        canvasGroup = gameObject.AddComponent<CanvasGroup>();

    StartCoroutine(FadeCoroutine(canvasGroup, targetAlpha, duration));
}

private IEnumerator FadeCoroutine(CanvasGroup cg, float target, float duration)
{
    float start = cg.alpha;
    float elapsed = 0f;

    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        cg.alpha = Mathf.Lerp(start, target, elapsed / duration);
        yield return null;
    }

    cg.alpha = target;
}
```

---

## Quick Setup Summary

**5-Minute Setup:**

1. Create Canvas with Screen Space Overlay, 1920x1080 reference
2. Create layout containers (TopLeft, TopRight, TopCenter, Bottom)
3. Create UI elements (Images for bars, TMP texts for displays)
4. Add scripts to corresponding GameObjects
5. Add HUDManager to Canvas, assign all references
6. Create PlayerEvents asset, assign to PlayerHealth and HUDManager
7. Test in Play mode using context menus

**That's it!** Your HUD should now be fully functional.

---

## Additional Resources

**Documentation:**
- TextMeshPro: https://docs.unity3d.com/Packages/com.unity.textmeshpro@latest
- Canvas: https://docs.unity3d.com/Manual/UICanvas.html
- UI Events: https://docs.unity3d.com/Manual/UIInteractionComponents.html

**Free HUD Assets:**
- Sci-Fi HUD textures: https://opengameart.org
- Free fonts: https://fonts.google.com

---

**Created for RoombaRampage - Unity 6 with URP**
**Version 1.0 - 2025**
