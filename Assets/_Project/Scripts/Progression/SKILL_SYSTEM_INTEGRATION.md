# Skill System Integration Guide

Quick reference for integrating the Skill System with the XP System.

---

## Overview

The XP System is designed to pause the game and wait for the Skill System to show skill selection UI when a player levels up.

**Integration Flow:**
```
Player Levels Up → XP System Pauses Game → Skill System Shows UI → Player Selects Skill → Skill System Applies Skill → XP System Resumes Game
```

---

## Quick Integration Checklist

When implementing the Skill System, follow these steps:

### Step 1: Subscribe to Level-Up Events

```csharp
// In SkillManager.cs or similar:
using RoombaRampage.Progression;

private void OnEnable()
{
    if (XPManager.Instance != null)
    {
        XPManager.Instance.OnLevelUp.AddListener(OnPlayerLevelUp);
    }
}

private void OnDisable()
{
    if (XPManager.Instance != null)
    {
        XPManager.Instance.OnLevelUp.RemoveListener(OnPlayerLevelUp);
    }
}
```

### Step 2: Show Skill Selection UI

```csharp
private void OnPlayerLevelUp(int newLevel)
{
    Debug.Log($"[SkillManager] Player leveled up to {newLevel}!");

    // Generate 3 random skill options
    List<Skill> skillOptions = GenerateSkillOptions(3);

    // Show skill selection UI
    skillSelectionUI.Show(skillOptions, newLevel);
}
```

### Step 3: Resume Game After Selection

```csharp
private void OnSkillSelected(Skill selectedSkill)
{
    Debug.Log($"[SkillManager] Player selected: {selectedSkill.name}");

    // Apply the selected skill
    ApplySkill(selectedSkill);

    // Hide UI
    skillSelectionUI.Hide();

    // IMPORTANT: Tell XP Manager to resume game
    if (XPManager.Instance != null)
    {
        XPManager.Instance.CompleteLevelUp();
    }
}
```

---

## XP Manager API Reference

### Properties

```csharp
XPManager.Instance.CurrentLevel          // Current player level (int)
XPManager.Instance.IsLevelUpPending      // Is waiting for skill selection? (bool)
XPManager.Instance.HasReachedMaxLevel    // At level cap? (bool)
```

### Events

```csharp
// Subscribe to level-up
XPManager.Instance.OnLevelUp.AddListener(OnPlayerLevelUp);

// Event signature:
void OnPlayerLevelUp(int newLevel) { }

// Also available:
XPManager.Instance.OnMaxLevelReached.AddListener(OnMaxLevelReached);
```

### Methods

```csharp
// Resume game after skill selection (REQUIRED!)
XPManager.Instance.CompleteLevelUp();

// Optional: Check if level-up is pending
if (XPManager.Instance.IsLevelUpPending)
{
    ShowSkillSelectionUI();
}
```

---

## XPSettings Configuration

Control pause behavior via XPSettings asset:

```
XP Settings:
  Pause On Level-Up: ✓ (enables pause for skill selection)
  Pause Delay: 0.3s (delay before pause - lets VFX play first)
  Heal On Level-Up: ✓ (restore player health on level-up)
```

**For Testing Without Skill System:**
- Disable "Pause On Level-Up" in XPSettings
- Game will continue without waiting for skill selection

---

## Example Skill System Structure

### SkillManager.cs

```csharp
using UnityEngine;
using RoombaRampage.Progression;
using System.Collections.Generic;

public class SkillManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SkillSelectionUI skillSelectionUI;
    [SerializeField] private SkillDatabase skillDatabase;

    private List<Skill> activeSkills = new List<Skill>();

    private void OnEnable()
    {
        // Subscribe to XP System
        if (XPManager.Instance != null)
        {
            XPManager.Instance.OnLevelUp.AddListener(OnPlayerLevelUp);
        }
    }

    private void OnDisable()
    {
        // Unsubscribe
        if (XPManager.Instance != null)
        {
            XPManager.Instance.OnLevelUp.RemoveListener(OnPlayerLevelUp);
        }
    }

    private void OnPlayerLevelUp(int newLevel)
    {
        Debug.Log($"[SkillManager] Player reached level {newLevel}");

        // Generate 3 random skill choices
        List<Skill> choices = GenerateSkillChoices(3);

        // Show UI (game is paused by XP Manager)
        skillSelectionUI.Show(choices, newLevel);
    }

    private List<Skill> GenerateSkillChoices(int count)
    {
        // TODO: Implement skill generation logic
        // - Filter available skills (not already maxed)
        // - Weight by rarity
        // - Ensure no duplicates
        // - Return 3 choices

        return skillDatabase.GetRandomSkills(count);
    }

    public void OnSkillChosen(Skill skill)
    {
        Debug.Log($"[SkillManager] Skill chosen: {skill.skillName}");

        // Apply skill upgrade
        ApplySkill(skill);

        // Track active skills
        activeSkills.Add(skill);

        // Hide UI
        skillSelectionUI.Hide();

        // CRITICAL: Resume game
        if (XPManager.Instance != null)
        {
            XPManager.Instance.CompleteLevelUp();
        }
    }

    private void ApplySkill(Skill skill)
    {
        // TODO: Apply skill effects to player
        // Examples:
        // - Increase damage
        // - Add extra projectiles
        // - Increase fire rate
        // - Add passive effects

        skill.Apply();
    }
}
```

### SkillSelectionUI.cs

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillSelectionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject uiPanel;
    [SerializeField] private SkillOptionButton[] skillButtons;
    [SerializeField] private TextMeshProUGUI levelText;

    private SkillManager skillManager;

    private void Awake()
    {
        skillManager = FindObjectOfType<SkillManager>();
        Hide();
    }

    public void Show(List<Skill> skills, int level)
    {
        // Display UI
        uiPanel.SetActive(true);
        levelText.text = $"Level {level}!";

        // Setup skill buttons
        for (int i = 0; i < skillButtons.Length; i++)
        {
            if (i < skills.Count)
            {
                skillButtons[i].Setup(skills[i], this);
            }
        }

        // Note: Game is already paused by XP Manager (Time.timeScale = 0)
    }

    public void Hide()
    {
        uiPanel.SetActive(false);
    }

    public void OnSkillButtonClicked(Skill skill)
    {
        // Forward to manager
        skillManager.OnSkillChosen(skill);
    }
}
```

---

## Testing Without Skill System

During XP system development, you can test without a full Skill System:

### Option 1: Disable Pause

1. Open XPSettings asset
2. Uncheck "Pause On Level-Up"
3. Level-ups will trigger but game continues

### Option 2: Auto-Resume (Temporary)

Add this to XPManager temporarily:

```csharp
// In XPManager.cs OnLevelUp:
private void TriggerLevelUpEffects()
{
    // ... existing code ...

    // TEMP: Auto-resume after 2 seconds (for testing)
    StartCoroutine(AutoResumeForTesting());
}

private IEnumerator AutoResumeForTesting()
{
    yield return new WaitForSecondsRealtime(2f);
    CompleteLevelUp();
}
```

### Option 3: Manual Resume

Use debug button in XPManager GUI:
1. Enable "Show Debug Info"
2. Click "Complete Level-Up" button when paused

---

## Common Integration Issues

### Issue: Game Stuck Paused

**Problem:** Level-up occurs, game pauses, but never resumes.

**Solution:** Verify `CompleteLevelUp()` is called after skill selection:

```csharp
// After skill is selected:
XPManager.Instance.CompleteLevelUp();
```

### Issue: Multiple Level-Ups

**Problem:** Player gains a lot of XP and levels up multiple times.

**Solution:** XP Manager handles this automatically. Each level-up will:
1. Trigger OnLevelUp event
2. Pause game
3. Wait for CompleteLevelUp()
4. Process next level-up if XP is still high enough

**Implementation:**
```csharp
// Queue level-ups if multiple occur
private Queue<int> pendingLevelUps = new Queue<int>();

private void OnPlayerLevelUp(int newLevel)
{
    // Add to queue
    pendingLevelUps.Enqueue(newLevel);

    // Process next if not currently showing UI
    if (!skillSelectionUI.IsVisible)
    {
        ProcessNextLevelUp();
    }
}

private void ProcessNextLevelUp()
{
    if (pendingLevelUps.Count > 0)
    {
        int level = pendingLevelUps.Dequeue();
        ShowSkillSelection(level);
    }
}

public void OnSkillChosen(Skill skill)
{
    ApplySkill(skill);
    skillSelectionUI.Hide();

    // Resume game
    XPManager.Instance.CompleteLevelUp();

    // Process next level-up if any
    ProcessNextLevelUp();
}
```

### Issue: UI Not Showing

**Problem:** Level-up occurs but skill UI doesn't appear.

**Solution:** Check subscription:
```csharp
private void Start()
{
    // Verify subscription
    if (XPManager.Instance != null)
    {
        Debug.Log("[SkillManager] Subscribed to level-up events");
    }
    else
    {
        Debug.LogError("[SkillManager] XPManager not found!");
    }
}
```

---

## Data Flow Diagram

```
[Enemy Dies]
      ↓
[XP Gem Spawns]
      ↓
[Player Collects Gem]
      ↓
[XPManager.AddXP(value)]
      ↓
[Check: XP >= XPForNextLevel?]
      ↓ (Yes)
[Level-Up Sequence:]
  1. Play VFX/SFX
  2. Heal player (if enabled)
  3. Time slow (if enabled)
  4. Fire OnLevelUp event ─────────┐
  5. Pause game (Time.timeScale = 0)│
  6. Set IsLevelUpPending = true    │
      ↓                             │
[WAITING...]                        │
                                    │
[SkillManager]  ←───────────────────┘
      ↓
[Generate 3 skill options]
      ↓
[Show SkillSelectionUI]
      ↓
[PLAYER CHOOSES SKILL] ← (Game is paused, UI is interactive)
      ↓
[SkillManager.OnSkillChosen(skill)]
      ↓
[Apply skill effects]
      ↓
[XPManager.CompleteLevelUp()] ←─── CRITICAL!
      ↓
[Resume game (Time.timeScale = 1)]
      ↓
[IsLevelUpPending = false]
      ↓
[GAME CONTINUES]
```

---

## Skill System Checklist

When implementing Skill System, ensure:

- [ ] SkillManager subscribes to XPManager.OnLevelUp
- [ ] Skill selection UI is created
- [ ] Skill database/data structure exists
- [ ] Skill application logic implemented
- [ ] **CompleteLevelUp() called after skill selected** ← CRITICAL
- [ ] UI works with Time.timeScale = 0 (use unscaled time)
- [ ] Multiple level-ups handled (queue system)
- [ ] Max level handling (no skills at level cap)

---

## Additional Resources

**XP System Documentation:**
- [XP_SETUP.md](./XP_SETUP.md) - Full setup guide
- [README.md](./README.md) - System overview

**Example Skill Data Structure:**

```csharp
[CreateAssetMenu(fileName = "NewSkill", menuName = "RoombaRampage/Skill")]
public class Skill : ScriptableObject
{
    public string skillName;
    public string description;
    public Sprite icon;
    public int maxLevel = 5;
    public int currentLevel = 0;

    public enum SkillRarity { Common, Uncommon, Rare, Epic, Legendary }
    public SkillRarity rarity = SkillRarity.Common;

    public virtual void Apply()
    {
        currentLevel++;
        // Apply skill effect to player
    }
}
```

---

## Support

If you encounter issues integrating the Skill System:

1. Enable debug info on XPManager
2. Add debug logs in your SkillManager
3. Verify OnLevelUp event is firing
4. Check Time.timeScale in inspector
5. Ensure CompleteLevelUp() is called

**Common Debug Logs:**

```csharp
// In SkillManager:
Debug.Log($"[SkillManager] OnLevelUp received: Level {newLevel}");
Debug.Log($"[SkillManager] Showing skill UI");
Debug.Log($"[SkillManager] Skill selected, calling CompleteLevelUp()");

// In XPManager (add temporarily):
Debug.Log($"[XPManager] Level-up triggered: {currentLevel}");
Debug.Log($"[XPManager] IsLevelUpPending: {isLevelUpPending}");
Debug.Log($"[XPManager] CompleteLevelUp called, resuming game");
```

---

**Last Updated:** 2025-10-30
**Unity Version:** 6000.2.6f2
