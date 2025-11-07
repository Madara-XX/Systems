# Skill Selection System Setup Guide

Complete guide for configuring the bullet-hell style level-up skill selection system.

---

## Quick Setup (15 Minutes)

1. **Create SkillPool asset** (see [Creating Skill Pool](#creating-skill-pool))
2. **Setup UI Canvas** (see [UI Setup](#ui-setup))
3. **Add SkillSelectionManager** (see [Manager Setup](#manager-setup))
4. **Configure skill rarities** (see [Skill Configuration](#skill-configuration))
5. **Test!**

---

## System Overview

### What This Does
When the player levels up:
1. Game pauses automatically
2. Shows 3 random skill cards
3. Player selects one (click or press 1, 2, 3)
4. Skill is added/upgraded
5. Game resumes

### Key Features
- **Weighted Random Selection**: Rarer skills appear less often
- **Upgrade Priority**: Upgrades appear 2x more than new skills
- **Keyboard Shortcuts**: Press 1, 2, 3 for instant selection
- **Rarity System**: Common → Uncommon → Rare → Epic → Legendary
- **Smart Filtering**: Only shows skills you can get/upgrade

---

## Creating Skill Pool

### 1. Create SkillPool Asset

```
Project Window:
Right-click > Create > RoombaRampage > Skill Pool
Name: "SkillPool"
Save in: Assets/_Project/Data/SkillPool.asset
```

### 2. Configure SkillPool

Open `SkillPool.asset` in Inspector:

**Skill Pool:**
- **Available Skills**: Drag all your skill assets here (LaserSkill, LightningStrike, etc.)

**Selection Settings:**
- **Skill Offers Per Level Up**: `3` (how many options shown)
- **Favor Upgrades**: `✓ Enabled` (prioritize upgrades over new skills)
- **Upgrade Weight Multiplier**: `2.0` (upgrades 2x more likely)

**Rarity Weights** (leave defaults):
- Common: `60`
- Uncommon: `25`
- Rare: `12`
- Epic: `3`
- Legendary: `1`

---

## Skill Configuration

### Assign Rarities to Skills

For each skill asset (LaserSkill, LightningStrike, etc.), open in Inspector:

**Selection Settings:**
- **Base Rarity**: Rarity when offered as NEW skill
- **Upgrade Rarity**: Rarity when offered as UPGRADE (usually higher)
- **Can Be Offered**: `✓ Enabled`

**Example Configurations:**

**LaserSkill:**
```
Base Rarity: Common
Upgrade Rarity: Uncommon
Can Be Offered: ✓
```

**LightningStrike:**
```
Base Rarity: Uncommon
Upgrade Rarity: Rare
Can Be Offered: ✓
```

**BuffSkill (Speed):**
```
Base Rarity: Common
Upgrade Rarity: Uncommon
Can Be Offered: ✓
```

**BuffSkill (Damage):**
```
Base Rarity: Rare
Upgrade Rarity: Epic
Can Be Offered: ✓
```

---

## UI Setup

### 1. Create Canvas

```
Hierarchy:
Right-click > UI > Canvas
Name: "SkillSelectionCanvas"
```

**Configure Canvas:**
- Render Mode: `Screen Space - Overlay`
- Canvas Scaler > UI Scale Mode: `Scale With Screen Size`
- Reference Resolution: `1920 x 1080`
- Match: `0.5` (balanced)

### 2. Create Selection Panel

```
Under SkillSelectionCanvas:
Right-click > UI > Panel
Name: "SelectionPanel"
```

**Configure Panel:**
- Anchors: Stretch/Stretch (fill screen)
- Color: Black with 80% alpha (semi-transparent)

### 3. Add Canvas Group

Select `SkillSelectionCanvas`:
- Add Component > Canvas Group
- Alpha: `1`
- Interactable: `✓`
- Block Raycasts: `✓`

### 4. Create Title Text

```
Under SelectionPanel:
Right-click > UI > Text - TextMeshPro
Name: "TitleText"
```

**Configure:**
- Text: `"LEVEL UP!"`
- Font Size: `72`
- Alignment: Center/Middle
- Color: Yellow or Gold
- Position: Top center of screen
- Add Outline for glow effect

### 5. Create Instructions Text

```
Under SelectionPanel:
Right-click > UI > Text - TextMeshPro
Name: "InstructionsText"
```

**Configure:**
- Text: `"Press 1, 2, or 3 to select a skill"`
- Font Size: `32`
- Alignment: Center/Middle
- Position: Below title

### 6. Create Skill Cards Container

```
Under SelectionPanel:
Right-click > UI > Empty
Name: "SkillCardsContainer"
```

**Configure:**
- Add Component > Horizontal Layout Group
- Child Alignment: Middle Center
- Spacing: `30`
- Child Force Expand: Width ✗, Height ✗
- Anchors: Center

### 7. Create Skill Card (repeat 3 times)

```
Under SkillCardsContainer:
Right-click > UI > Panel
Name: "SkillCard1" (then SkillCard2, SkillCard3)
```

**Configure SkillCard:**

1. **Card Panel:**
   - Width: `350`
   - Height: `500`
   - Add Component > SkillCard script
   - Add Component > Button component

2. **Border Image:**
   ```
   Under SkillCard:
   Right-click > UI > Image
   Name: "BorderImage"
   ```
   - Anchors: Stretch/Stretch
   - Color: White (will change per rarity)
   - Image Type: Sliced (for borders)

3. **Background Image:**
   ```
   Under SkillCard:
   Right-click > UI > Image
   Name: "BackgroundImage"
   ```
   - Anchors: Stretch/Stretch
   - Color: Dark gray/black

4. **Icon Image:**
   ```
   Under SkillCard:
   Right-click > UI > Image
   Name: "IconImage"
   ```
   - Width: `128`, Height: `128`
   - Position: Top center
   - Preserve Aspect: ✓

5. **Name Text:**
   ```
   Under SkillCard:
   Right-click > UI > Text - TextMeshPro
   Name: "NameText"
   ```
   - Font Size: `32`
   - Alignment: Center
   - Color: White
   - Position: Below icon

6. **Level Text:**
   ```
   Under SkillCard:
   Right-click > UI > Text - TextMeshPro
   Name: "LevelText"
   ```
   - Font Size: `24`
   - Text: "NEW!" or "Level 1 → 2"
   - Color: Yellow for NEW, White for upgrades

7. **Description Text:**
   ```
   Under SkillCard:
   Right-click > UI > Text - TextMeshPro
   Name: "DescriptionText"
   ```
   - Font Size: `18`
   - Alignment: Left
   - Best Fit: ✓
   - Wrapping: Enabled
   - Position: Bottom half of card

8. **NEW Banner (Optional):**
   ```
   Under SkillCard:
   Right-click > UI > Image
   Name: "NewBanner"
   ```
   - Width: `80`, Height: `80`
   - Position: Top-left corner
   - Rotation: -45°
   - Color: Red or Gold
   - Child: Text saying "NEW!"

9. **Rarity Banner:**
   ```
   Under SkillCard:
   Right-click > UI > Image
   Name: "RarityBanner"
   ```
   - Anchors: Top/Stretch
   - Height: `10`
   - Color: Will change per rarity

### 8. Wire Up SkillCard Components

For each SkillCard (1, 2, 3), select the card and in the `SkillCard` script component:

**UI References:**
- Background Image: Drag `BackgroundImage`
- Border Image: Drag `BorderImage`
- Icon Image: Drag `IconImage`
- Name Text: Drag `NameText`
- Level Text: Drag `LevelText`
- Description Text: Drag `DescriptionText`
- New Banner: Drag `NewBanner` (or leave empty)
- Rarity Banner: Drag `RarityBanner`
- Select Button: Drag the card's Button component

**Rarity Colors** (optional, use defaults or customize):
- Common: Light gray
- Uncommon: Green
- Rare: Blue
- Epic: Purple
- Legendary: Gold

---

## Manager Setup

### 1. Create SkillSelectionManager

```
In Scene:
Right-click > Create Empty
Name: "SkillSelectionManager"
Add Component > SkillSelectionManager
```

**Configure:**

**References:**
- **Skill Pool**: Drag `SkillPool.asset`
- **Selection UI**: Drag `SkillSelectionCanvas` GameObject

**Settings:**
- **Auto Pause Game**: `✓ Enabled`

**Debug:**
- **Show Debug Logs**: `✓ Enabled` (for testing)

### 2. Wire Up SkillSelectionUI

Select `SkillSelectionCanvas` GameObject:
- Add Component > SkillSelectionUI

**Configure:**

**UI References:**
- **Selection Panel**: Drag `SelectionPanel`
- **Skill Cards**:
  - Size: `3`
  - Element 0: Drag `SkillCard1`
  - Element 1: Drag `SkillCard2`
  - Element 2: Drag `SkillCard3`
- **Title Text**: Drag `TitleText`
- **Instructions Text**: Drag `InstructionsText`
- **Canvas Group**: Drag `SkillSelectionCanvas` Canvas Group

**Animation:**
- Fade In Duration: `0.3`
- Fade Out Duration: `0.2`

**Debug:**
- Show Debug Logs: `✓ Enabled` (for testing)

---

## Testing

### 1. Basic Test

1. Enter Play Mode
2. Gain XP until you level up
3. **Expected Behavior:**
   - Game pauses
   - Selection panel appears with 3 skills
   - Cards show skill names, icons, descriptions
   - Borders colored by rarity
   - Can click card or press 1/2/3
   - After selection, game resumes

### 2. Test Skill Selection

**Test new skills:**
- Level up with no skills
- Should show 3 new skills
- Select one
- Skill should appear in player's skill list

**Test upgrades:**
- Have 1-2 skills already
- Level up
- Should show mix of upgrades and new skills
- Upgrades show "Level X → Y"
- New skills show "NEW!"

**Test maxed skills:**
- Max out all skills to level 10
- Level up
- Should show message or skip selection

### 3. Verify Rarity System

Enable Debug Logs on SkillSelectionManager.

Check Console for:
```
[SkillPool] Offer 1: LaserSkill (NEW, Common)
[SkillPool] Offer 2: LaserSkill (UPGRADE, Uncommon)
[SkillPool] Offer 3: LightningStrike (NEW, Uncommon)
```

Upgrades should appear more frequently than new skills.

### 4. Test Keyboard Shortcuts

- Press `1` → Selects first card
- Press `2` → Selects second card
- Press `3` → Selects third card

Should work even with UI not focused.

---

## Keyboard Controls

| Key | Action |
|-----|--------|
| **1** or **Numpad 1** | Select first skill |
| **2** or **Numpad 2** | Select second skill |
| **3** or **Numpad 3** | Select third skill |
| **Mouse Click** | Select clicked skill card |

---

## Troubleshooting

### Selection UI Doesn't Appear

**Problem**: No UI shows on level-up
**Solutions**:
- Check `SkillSelectionManager` is in scene
- Verify `SkillPool` asset is assigned
- Verify `SkillSelectionUI` component is assigned
- Check XPManager has OnLevelUp event
- Enable Debug Logs to see console messages
- Check SkillSelectionCanvas is active in hierarchy

### Cards Are Empty/Broken

**Problem**: Cards don't show skill info
**Solutions**:
- Verify all UI References assigned in SkillCard components
- Check TextMeshPro is imported (Window > TextMeshPro > Import Resources)
- Verify skill assets have icons assigned
- Check skills have descriptions filled in

### Game Doesn't Pause

**Problem**: Game keeps running during selection
**Solutions**:
- Verify `Auto Pause Game` is enabled on SkillSelectionManager
- Check SkillSelectionCanvas uses "Unscaled Time" render mode
- Ensure Time.timeScale is set to 0 in SkillSelectionManager

### Keyboard Shortcuts Don't Work

**Problem**: Pressing 1/2/3 does nothing
**Solutions**:
- Verify SkillSelectionUI.Update() is checking Input
- Make sure UI is visible (isVisible = true)
- Check no other script is capturing keyboard input
- Test with both number row and numpad keys

### No Skills Available

**Problem**: Console shows "No eligible skills available"
**Solutions**:
- Add skills to SkillPool.availableSkills list
- Set `Can Be Offered = true` on skill assets
- Check skills aren't all maxed out (level 10)
- Verify skills have maxLevel > startingLevel

### Same Skill Appears Multiple Times

**Problem**: Duplicate skills in one selection
**Solutions**:
- This is a bug - should not happen
- Check SkillPoolData.SelectWeightedRandomSkill() uses `usedSkills` HashSet
- Verify selection loop adds to usedSkills

### Upgrades Never Appear

**Problem**: Only new skills, never upgrades
**Solutions**:
- Check player has active skills (AddSkill was called)
- Verify `Favor Upgrades = true` in SkillPool
- Check upgrade weights > 1.0
- Enable Debug Logs to see selection logic

---

## Customization

### Change Number of Offers

In `SkillPool.asset`:
- Skill Offers Per Level Up: `2` (fewer choices)
- Skill Offers Per Level Up: `4` (more choices)
- Skill Offers Per Level Up: `5` (maximum)

### Adjust Rarity Weights

In `SkillPool.asset` > Rarity Weights:

**More Rare Skills:**
```
Common: 40
Uncommon: 30
Rare: 20
Epic: 8
Legendary: 2
```

**More Common Skills:**
```
Common: 80
Uncommon: 15
Rare: 4
Epic: 1
Legendary: 0.5
```

### Make Upgrades Less Common

In `SkillPool.asset`:
- Upgrade Weight Multiplier: `1.0` (no bonus)
- Upgrade Weight Multiplier: `1.5` (small bonus)
- Upgrade Weight Multiplier: `3.0` (strong bonus)

### Custom Rarity Colors

On each SkillCard component, change:
- Common Color: Your choice
- Uncommon Color: Your choice
- etc.

Colors will be applied to borders and rarity banner.

---

## Advanced Features

### Add Reroll Button (Future)

1. Add Button to SelectionPanel: "Reroll (Cost: X)"
2. Add method to SkillSelectionManager:
   ```csharp
   public void RerollOffers()
   {
       // Check currency
       // Generate new offers
       // Update UI
   }
   ```

### Add Sound Effects

1. Assign Audio Clips to SkillSelectionUI:
   - Open Sound
   - Select Sound
   - Hover Sound

2. Implement audio playback:
   ```csharp
   if (selectSound != null)
       AudioSource.PlayClipAtPoint(selectSound, Vector3.zero);
   ```

### Skill Synergies

Show special badge if selecting skill creates synergy:

```csharp
// In SkillCard.Setup()
if (HasSynergy(offer.skillData))
{
    synergyBadge.SetActive(true);
}
```

---

## Example Configurations

### Balanced (Recommended)

```
SkillPool:
  Skill Offers Per Level: 3
  Favor Upgrades: ✓
  Upgrade Weight Multiplier: 2.0

Rarity Weights:
  Common: 60
  Uncommon: 25
  Rare: 12
  Epic: 3
  Legendary: 1
```

### Upgrade-Heavy

```
SkillPool:
  Skill Offers Per Level: 3
  Favor Upgrades: ✓
  Upgrade Weight Multiplier: 3.0 (even more upgrades)

Skill Assets:
  All skills: Upgrade Rarity = 2 tiers higher than Base
```

### Discovery Mode

```
SkillPool:
  Skill Offers Per Level: 4 (more choices)
  Favor Upgrades: ✗ (equal weight)
  Upgrade Weight Multiplier: 1.0

Result: More variety, encourages trying new skills
```

---

## File Structure

```
Assets/_Project/
├── Scripts/
│   └── Skills/
│       ├── SkillSelection/
│       │   ├── SkillOffer.cs
│       │   ├── SkillPoolData.cs
│       │   └── SkillSelectionManager.cs
│       ├── UI/
│       │   ├── SkillCard.cs
│       │   └── SkillSelectionUI.cs
│       ├── Data/
│       │   └── SkillData.cs (updated with rarity)
│       └── SkillManager.cs (updated with helpers)
└── Data/
    └── SkillPool.asset
```

---

## Testing Checklist

- [ ] SkillPool asset created and configured
- [ ] All skills added to SkillPool.availableSkills
- [ ] All skills have rarities assigned
- [ ] SkillSelectionCanvas created with all UI elements
- [ ] All 3 SkillCards configured with components
- [ ] SkillSelectionManager in scene and configured
- [ ] SkillSelectionUI component added and wired up
- [ ] Play mode: Level up triggers selection
- [ ] Play mode: Can select with mouse click
- [ ] Play mode: Can select with 1/2/3 keys
- [ ] Play mode: New skills are added
- [ ] Play mode: Existing skills are upgraded
- [ ] Play mode: Rarity colors display correctly
- [ ] Play mode: Game pauses during selection
- [ ] Play mode: Game resumes after selection
- [ ] No console errors

---

## Next Steps

1. **Test with players** and gather feedback
2. **Add visual polish** (animations, particles, sound)
3. **Create more skills** to fill out the pool
4. **Balance rarity weights** based on playtesting
5. **Add advanced features** (rerolls, synergies)
6. **Create skill icons** for better visual appeal

---

## Support

For issues:
1. Enable Debug Logs on both managers
2. Check Unity Console for error messages
3. Verify all references are assigned (no "Missing" errors)
4. Test with simple setup (2 skills, basic UI)
5. Check XPManager is working and firing OnLevelUp event

**Happy skill building!** 🎮⚡🔥
