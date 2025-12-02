[English](README.md) | 简体中文

# DanhengPlugin-DHConsoleCommands

[DHConsole](https://github.com/Anyrainel/DHConsole) 使用的原生指令。不支持国际化 (i18n)。

## 用法 (Usage)

从 [release](https://github.com/Anyrainel/DanhengPlugin-DHConsoleCommands/releases/latest) 下载最新版本。
然后将其放入 Danheng 服务器目录中的 `Plugins` 文件夹。

## 指令 (Commands)

- **buildchar** (改编自 [CharacterBuilder](https://github.com/EggLinks/DanhengPlugin-CharacterBuilder))
  - `buildchar <avatarId>`: 为指定角色构建推荐的遗器和光锥。
  - `buildchar recommend <avatarId>`: 显示角色的推荐遗器而不应用更改。
  - `buildchar all`: 构建所有角色 (ID 2000 以内)。
- **claim**
  - `claim promotion`: 领取所有角色的所有晋阶奖励 (星轨通票)。
- **equip**
  - `equip item <avatarId> <itemId> l<level> r<rank>`: 为角色装备光锥。
  - `equip relic <avatarId> <relicId> <mainAffixId> <subAffixId*4>:<level*4>`: 为角色装备自定义遗器。
- **remove** (改编自 [Clean](https://github.com/AfricanCh/DanhengPlugin-Clean))
  - `remove relics`: 移除所有未装备的遗器。
  - `remove equipment`: 移除所有未装备的光锥。
  - `remove avatar <avatarId>`: 移除指定角色，卸下其物品，并将玩家踢下线以保存更改。
- **fetch**
  - `fetch owned`: 显示所有拥有的角色 ID。
  - `fetch avatar <avatarId>`: 显示详细的角色信息 (属性, 装备, 遗器)。
  - `fetch inventory`: 显示背包中的所有材料物品。
  - `fetch player`: 显示玩家基本信息 (等级, 性别, 命途)。
  - `fetch scene`: 显示当前场景中的物件 (Props)。
  - `fetch npc`: 显示当前场景中的 NPC。
- **gametext**
  - `gametext avatar #<language>`: 列出指定语言的角色名称。
  - `gametext item #<language>`: 列出指定语言的物品名称。
  - `gametext mainmission #<language>`: 列出指定语言的主线任务名称。
  - `gametext submission #<language>`: 列出指定语言的子任务名称。
  - `gametext relic`: 列出遗器类型及其 ID。
- **debuglink**
  - `debuglink item`: 显示光锥 -> 角色装备状态。
  - `debuglink relic`: 显示遗器 -> 角色装备状态。
  - `debuglink avataritem`: 显示角色 -> 光锥装备状态。
  - `debuglink avatarrelic`: 显示角色 -> 遗器装备状态。

## 遗器推荐算法 (Relic Recommendation Algorithm)

`buildchar` 指令使用启发式算法为角色生成完全升级 (15级, 5星) 的遗器。

1.  **套装选择**:
    *   **4件套** (头部, 手部, 躯干, 脚部): 使用游戏数据中首选的推荐套装。
    *   **2件套** (位面球, 连结绳): 使用游戏数据中首选的推荐位面饰品套装。
2.  **主词条选择**:
    *   **头部**: 生命值 (HP)
    *   **手部**: 攻击力 (ATK)
    *   **躯干/脚部/位面球/连结绳**: 选择该部位的推荐属性。如果没有推荐，默认为 `生命值百分比` (HPAddedRatio)。
3.  **副词条选择与升级**:
    *   **初始化**: 从游戏数据中获取角色的推荐副词条列表，如果包含主词条则将其移除。
    *   **截断**: 如果列表超过 4 个词条，仅保留前 4 个。
    *   **填充**: 如果列表少于 4 个词条，则按以下优先级填充剩余槽位，直到填满 4 个：
        1.  **速度** (Speed) 和 **效果抵抗** (Effect Resistance)。
        2.  **固定数值属性** (HP, ATK, DEF)，如果对应的百分比属性词条存在。
        3.  **百分比属性** (HP%, ATK%, DEF%)， 如果尚未拥有该词条。
    *   **强化**: 遗器将被模拟进行 5 次随机强化，分配给 *原始推荐* 的副词条 ("优先"词条)，忽略任何填充的词条。
