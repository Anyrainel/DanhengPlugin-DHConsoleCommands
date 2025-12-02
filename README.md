English | [简体中文](README.zh-CN.md)

# DanhengPlugin-DHConsoleCommands

Raw commands to be used by [DHConsole](https://github.com/Anyrainel/DHConsole). No i18n support.

## Usage

Download the latest release from [release](https://github.com/Anyrainel/DanhengPlugin-DHConsoleCommands/releases/latest).
Then place it in the `Plugins` folder in your Danheng server directory.

## Commands

- **buildchar** (adapted from [CharacterBuilder](https://github.com/EggLinks/DanhengPlugin-CharacterBuilder))
  - `buildchar <avatarId>`: Build specified character with recommended relics and light cone.
  - `buildchar recommend <avatarId>`: Show recommended relics for a character without applying them.
  - `buildchar all`: Build all characters (up to ID 2000).
- **claim**
  - `claim promotion`: Claim all promotion rewards (Star Rail Passes) for all characters.
- **equip**
  - `equip item <avatarId> <itemId> l<level> r<rank>`: Equip a light cone to a character.
  - `equip relic <avatarId> <relicId> <mainAffixId> <subAffixId*4>:<level*4>`: Equip a custom relic to a character.
- **remove** (adapted from [Clean](https://github.com/AfricanCh/DanhengPlugin-Clean))
  - `remove relics`: Remove all unequipped relics.
  - `remove equipment`: Remove all unequipped light cones.
  - `remove avatar <avatarId>`: Remove specified character, unequip their items, and kick the player to save changes.
- **fetch**
  - `fetch owned`: Show all owned character IDs.
  - `fetch avatar <avatarId>`: Show detailed character info (stats, equipment, relics).
  - `fetch inventory`: Show all material items in inventory.
  - `fetch player`: Show player basic info (level, gender, path).
  - `fetch scene`: Show props in the current scene.
  - `fetch npc`: Show NPCs in the current scene.
- **gametext**
  - `gametext avatar #<language>`: List character names in the specified language.
  - `gametext item #<language>`: List item names in the specified language.
  - `gametext mainmission #<language>`: List main mission names in the specified language.
  - `gametext submission #<language>`: List sub mission names in the specified language.
  - `gametext relic`: List relic types and their IDs.
- **debuglink**
  - `debuglink item`: Show light cone -> character equipment status.
  - `debuglink relic`: Show relic -> character equipment status.
  - `debuglink avataritem`: Show character -> light cone equipment status.
  - `debuglink avatarrelic`: Show character -> relic equipment status.

## Relic Recommendation Algorithm

The `buildchar` command uses a heuristic algorithm to generate fully upgraded (Level 15, Rarity 5) relics for characters.

1.  **Set Selection**:
    *   **4-Piece Set** (Head, Hands, Body, Feet): Uses the primary recommended set from game data.
    *   **2-Piece Set** (Planar Sphere, Link Rope): Uses the primary recommended planar set from game data.
2.  **Main Affix Selection**:
    *   **Head**: HP
    *   **Hands**: ATK
    *   **Body/Feet/Sphere/Rope**: Selects the recommended property for that slot. Defaults to `HPAddedRatio` if no recommendation exists.
3.  **Sub-Affix Selection & Upgrade**:
    *   **Initialization**: Starts with the character's recommended sub-affix list from game data, removing the Main Affix if present.
    *   **Truncation**: If the list has more than 4 affixes, it keeps only the first 4.
    *   **Filling**: If the list has fewer than 4 affixes, it fills the remaining slots with the following priority until 4 slots are filled:
        1.  **Speed** and **Effect Resistance**.
        2.  **Delta Stats** (HP, ATK, DEF) if corresponding Ratio stats are present.
        3.  **Ratio Stats** (HP%, ATK%, DEF%) if not already present.
    *   **Enhancement**: The relic is simulated to have 5 random upgrades distributed among the *original recommended* sub-affixes (the "priority" affixes), ignoring any filled affixes.