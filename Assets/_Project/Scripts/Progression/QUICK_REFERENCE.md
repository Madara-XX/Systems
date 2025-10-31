# XP System Quick Reference Card

Fast reference for common XP system tasks.

---

## Quick API Reference

### Award XP
```csharp
XPManager.Instance.AddXP(25);
```

### Get Current Level
```csharp
int level = XPManager.Instance.CurrentLevel;
```

### Get XP Progress (0-1)
```csharp
float progress = XPManager.Instance.GetXPProgress();
```

### Reset for New Run
```csharp
XPManager.Instance.ResetXP();
```

### Spawn XP Gem
```csharp
XPGemPool.Instance.SpawnGem(enemyPosition, 25);
```

### Resume After Skill Selection
```csharp
XPManager.Instance.CompleteLevelUp();
```

---

## Quick Event Subscription

### Listen for Level-Up
```csharp
private void OnEnable()
{
    XPManager.Instance.OnLevelUp.AddListener(OnPlayerLevelUp);
}

private void OnDisable()
{
    XPManager.Instance.OnLevelUp.RemoveListener(OnPlayerLevelUp);
}

private void OnPlayerLevelUp(int newLevel)
{
    Debug.Log($"Leveled up to {newLevel}!");
}
```

### Listen for XP Change
```csharp
XPManager.Instance.OnXPGained.AddListener(OnXPChanged);

private void OnXPChanged(int current, int needed)
{
    Debug.Log($"XP: {current}/{needed}");
}
```

---

## Quick Setup Checklist

### Scene Setup
- [ ] Create GameObject: `XPManager`
  - [ ] Add XPManager component
  - [ ] Assign XPSettings asset
- [ ] Create GameObject: `XPGemPool`
  - [ ] Add XPGemPool component
  - [ ] Assign XPSettings asset
  - [ ] Assign XP Gem prefab

### Asset Setup
- [ ] Create XPSettings asset in `Data/Progression/`
  - [ ] Set Base XP: 100
  - [ ] Set Exponent: 1.5
  - [ ] Set Max Level: 30
- [ ] Create XP Gem prefab in `Prefabs/XP/`
  - [ ] Add mesh (sphere, scale 0.3)
  - [ ] Add Sphere Collider (trigger)
  - [ ] Add XPGem component
  - [ ] Add glowing material

### Enemy Setup
- [ ] Open EnemyData assets
- [ ] Set XP Value for each enemy type
  - [ ] Basic: 25 XP
  - [ ] Fast: 40 XP

---

## Quick Troubleshooting

### Gems Not Spawning?
```csharp
// Check in Enemy.cs OnDeath():
Debug.Log($"Enemy XP Value: {enemyData.xpValue}");
Debug.Log($"Pool Instance: {XPGemPool.Instance != null}");
```

### Gems Not Moving?
- Check player has "Player" tag (case-sensitive)
- Increase Magnet Range in XPSettings (try 10)

### Game Stuck Paused?
```csharp
// Call this after skill selection:
XPManager.Instance.CompleteLevelUp();
```

### XP Bar Not Updating?
- Verify XPBar Fill Image is assigned
- Check OnEnable subscribes to events
- Enable Show Debug Info on XPBar

---

## Quick Configuration Values

### Recommended XP Curve (10-min runs)
```
Base XP: 100
Exponent: 1.5
Max Level: 30
Result: 10-15 level-ups
```

### Enemy XP Values
```
Basic (100 HP):   25 XP
Fast (40 HP):     40 XP
Tank (200 HP):    50 XP
Boss (1000 HP):   200-500 XP
```

### Performance Settings
```
Initial Pool Size: 100
Magnet Range: 6 units
Magnet Speed: 8
```

---

## Quick Test Commands

### Via Inspector Context Menu

**XPManager:**
- Right-click > Test: Add 100 XP
- Right-click > Test: Force Level Up

**XPGemPool:**
- Right-click > Test: Spawn 50 Gems

**XPSettings:**
- Right-click > Print XP Curve
- Right-click > Estimate Levels in 10-Min Run

### Via Debug GUI (Enable "Show Debug Info")
- Click "Add 50 XP" button
- Click "Spawn 10 Gems" button
- Click "Reset XP" button

---

## Common Code Snippets

### Skill System Integration
```csharp
// In SkillManager.cs:
private void OnEnable()
{
    XPManager.Instance.OnLevelUp.AddListener(ShowSkillSelection);
}

private void ShowSkillSelection(int newLevel)
{
    skillUI.Show();
}

private void OnSkillSelected(Skill skill)
{
    ApplySkill(skill);
    XPManager.Instance.CompleteLevelUp(); // Resume game
}
```

### Custom Enemy XP Drop
```csharp
// In Enemy.cs OnDeath():
if (isBoss)
{
    // Spawn multiple gems for boss
    XPGemPool.Instance.SpawnMultipleGems(transform.position, 500, 5);
}
else
{
    // Single gem for normal enemy
    XPGemPool.Instance.SpawnGem(transform.position, enemyData.xpValue);
}
```

### XP Multiplier Bonus
```csharp
// Award bonus XP for combos:
int baseXP = enemyData.xpValue;
float multiplier = ComboManager.Instance.GetMultiplier();
int totalXP = Mathf.RoundToInt(baseXP * multiplier);

XPGemPool.Instance.SpawnGem(transform.position, totalXP);
```

### Check if Max Level
```csharp
if (XPManager.Instance.HasReachedMaxLevel)
{
    Debug.Log("Player is max level!");
    // Award bonus score instead of XP
    ScoreManager.Instance.AddScore(xpValue * 10);
}
```

---

## File Locations (Absolute Paths)

### Scripts
```
C:\DEVELOPMENT\Systems\Assets\_Project\Scripts\Progression\XP\XPManager.cs
C:\DEVELOPMENT\Systems\Assets\_Project\Scripts\Progression\XP\XPGem.cs
C:\DEVELOPMENT\Systems\Assets\_Project\Scripts\Progression\XP\XPGemPool.cs
C:\DEVELOPMENT\Systems\Assets\_Project\Scripts\Progression\XP\Data\XPSettings.cs
```

### Documentation
```
C:\DEVELOPMENT\Systems\Assets\_Project\Scripts\Progression\XP_SETUP.md
C:\DEVELOPMENT\Systems\Assets\_Project\Scripts\Progression\README.md
C:\DEVELOPMENT\Systems\Assets\_Project\Scripts\Progression\SKILL_SYSTEM_INTEGRATION.md
C:\DEVELOPMENT\Systems\Assets\_Project\Scripts\Progression\QUICK_REFERENCE.md (this file)
```

### Assets to Create
```
C:\DEVELOPMENT\Systems\Assets\_Project\Data\Progression\XPSettings.asset
C:\DEVELOPMENT\Systems\Assets\_Project\Prefabs\XP\XPGem.prefab
```

---

## XP Curve Quick Calculator

### Formula
```
XP = BaseXP * (Level ^ Exponent)
```

### Example (Base: 100, Exp: 1.5)
```
Level 1→2:   100 XP
Level 2→3:   212 XP
Level 3→4:   349 XP
Level 5→6:   671 XP
Level 10→11: 1995 XP
Level 15→16: 3711 XP
Level 20→21: 6325 XP
```

### Quick Adjustments
- **Level too fast?** Increase Base XP (+20) or Exponent (+0.1)
- **Level too slow?** Decrease Base XP (-20) or Exponent (-0.1)

---

## Debug Keyboard Shortcuts (Add to your project)

```csharp
// In XPManager.cs Update():
#if UNITY_EDITOR
private void Update()
{
    if (Input.GetKeyDown(KeyCode.F1))
    {
        AddXP(100); // Quick XP gain
    }
    if (Input.GetKeyDown(KeyCode.F2))
    {
        AddXP(XPForNextLevel); // Force level up
    }
    if (Input.GetKeyDown(KeyCode.F3))
    {
        ResetXP(); // Reset to level 1
    }
}
#endif
```

---

## Performance Quick Check

### Expected Stats (10-min run)
```
Enemies Killed: ~250
XP Gems Spawned: ~250
Peak Active Gems: 50-100
Pool Size: 100-200
FPS: 60 with 100 gems
```

### If Experiencing Lag
1. Increase initial pool size (200)
2. Simplify gem mesh (sphere only)
3. Reduce particle effects
4. Increase magnet speed (faster collection)

---

## One-Line Fixes for Common Issues

### Gems spawning underground
```csharp
// In XPSettings: Gem Spawn Height = 1.0 (increase from 0.5)
```

### Pool running out
```csharp
// In XPSettings: Initial Pool Size = 200 (increase from 100)
```

### Magnet range too small
```csharp
// In XPSettings: Magnet Range = 10 (increase from 6)
```

### Level-ups too slow
```csharp
// In XPSettings: Base XP = 80 (decrease from 100)
```

### Level-ups too fast
```csharp
// In XPSettings: Base XP = 120 (increase from 100)
```

---

## Links to Full Documentation

- **Full Setup Guide:** [XP_SETUP.md](./XP_SETUP.md)
- **System Overview:** [README.md](./README.md)
- **Skill Integration:** [SKILL_SYSTEM_INTEGRATION.md](./SKILL_SYSTEM_INTEGRATION.md)
- **Complete Summary:** `C:\DEVELOPMENT\Systems\XP_SYSTEM_SUMMARY.md`

---

**Quick Reference v1.0**
**Last Updated:** 2025-10-30
