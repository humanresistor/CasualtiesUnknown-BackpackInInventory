# Backpack In Inventory — Casualties: Unknown

> 未知伤亡（Casualties: Unknown）的 BepInEx 5 插件：**让双肩背包、材料包、腿包、皮带、子弹带、腰包这类可穿戴容器，也能放进物品栏槽位。**

- 中文 | [English](#english)
- 游戏版本：Casualties: Unknown **1.0.2**（正式版）
- 当前版本：**v1.2.2**

> ### ⚠️ AI 生成声明 / AI Disclosure
> **本项目的全部代码与文档由 AI（DeepSeek Harness 智能体）编写**，人类只提需求、做测试与验收。
> 测试范围仅限单人 / 沙盒模式，未做联机（Casualties: Together）完整回归，请自行备份存档。
> **All code and documentation in this repository were written by an AI assistant**; a human only specified requirements, tested, and accepted the result.
>
> ### 📜 随便用 / Use it freely
> 采用 [MIT](LICENSE) 协议：**任何人都可以自由拿去修改、再发布、商用、闭源二次分发**，只需保留版权与许可声明。
> Licensed under MIT: **anyone may fork, modify, redistribute, or use it commercially, including in closed-source form**, provided the copyright and license notice are kept.

> ### 🏠 自制自用 / Made for personal use
> 本插件是**为自己玩而写的自制插件**，实现来自对游戏本体 Assembly-CSharp.dll 的反编译分析，**未参考、未复制任何第三方 mod 的源码**。
> 若与其他 mod 功能相似、或实现思路接近，**纯属巧合**。
> This plugin was written **for personal use**. The implementation comes from decompiling the game itself (Assembly-CSharp.dll) — **no third-party mod source code was referenced or copied**. Any resemblance to other mods is purely coincidental.

---

## 它解决什么问题

游戏里能穿戴的容器（双肩背包、材料包、皮带……）原本**只能穿在身上**：把它们拖到物品栏，游戏只会弹一句
「可穿戴设备无法被手持（试试佩戴/存放？）」，然后什么都不发生。

本插件放开的就是这一条限制 —— 这些容器穿上身之后，照样能当普通物品塞进物品栏的空槽位。

### 保留的原版机制（没有改动）

- **空容器仍可放进另一个容器**（比如空的双肩背包塞进另一个空的双肩背包）—— 这是原版设定，插件没碰。
- **装着东西的容器不能放进别的容器** —— 原版会拦下，插件同样不放开。
- 外层容器自己已经被装在别的容器里时，也不允许再往里塞（原版用来防无限嵌套）。

换句话说：**容器之间怎么嵌套完全维持原版**，插件只影响"能不能进物品栏槽位"这一件事。

## 中文说明

### 原理

反编译 1.0.2 的 Assembly-CSharp.dll 后确认，拦住"可穿戴物品放进物品栏"的只有一处：

```csharp
// PlayerCamera.TryPerformInventoryAction
if (component.isBody)
{
    if (dragItem.Stats.wearable && !dragItem.Stats.wearableCanBeHeld)
    {
        DoAlert(Locale.GetOther("alertpickupwearable"));
        return true;   // ← 就是这里把背包挡住了
    }
    ...  // 真正 PickUpItem
}
```

物品数据里的 wearableCanBeHeld 字段，全库只有这一处读取（自动输液泵 autopump 把它设为 true 作示例）。
物品表在 Item.GlobalItems，由 Item.SetupItems() 在进入世界时填充。

插件做法：Harmony 后置补丁挂在 Item.SetupItems 之后，把穿戴槽位属于配置范围（默认覆盖所有可穿戴容器）
且 wearableCanBeHeld 为 false 的物品改成 true。另有 2 秒轮询兜底，防止 SetupItems 早于插件加载。

不碰 UI、不碰拖放逻辑、不改 Container.LoadItem。

### 默认放开的槽位

WearSlotIds = back,thighback,thigh,belt,bandolier,torsofront

| 穿戴槽位 | 对应物品（官方中文名 / 内部 id） |
|---|---|
| back | 双肩背包 bigpack、帆布背包 duffelbag、小型背包 smallpack、植物纤维挎包 slingbag |
| thighback | 材料包 materialpouch |
| thigh | 腿包 legpouch |
| belt | 皮带 belt |
| bandolier | 子弹带 bandolier |
| torsofront | 腰包 fannypack、随身水包 liquidpouch、战术胸挂 traumarig |

（喷气背包 jetpack 也在 back 槽，会一起被放开。）

### 全部放开（可选，默认关）

如果你希望**连护甲、衣服、头盔之类也都能手持 / 放进物品栏**，把配置里的 AllWearables 改成 true 即可：

- 打开后忽略槽位列表，对所有带 wearable 标记的物品生效（和同类 mod 的做法一致）
- 会影响平衡：护甲、衣物从此可以像普通物品一样收纳，也可以拿在手里
- 默认 false，只影响你明确列出的可穿戴容器

#### 附带修复：多肢体贴图残影

原版 `Body.PickUpItem`（把物品放进物品栏槽位）**不会调用** `Wearable.ClearSprites()`，而 `Wearable.secondaryLimbs`
机制会让一件装备在多个肢体上各画一份贴图（围巾、喷气背包那类）。结果就是物品已经进了物品栏，
身上那几份副贴图还挂着，出现“脱了但残影还在”。

本插件在拾取成功后补调一次 `ClearSprites()`（不在身上的物品调用它是空操作），开关是
`ClearWearableSpritesOnPickup`（默认 true）。

## 配置

配置文件位置：游戏目录下 BepInEx/config/humanresistor.cu.backpackininventory.cfg

| 键 | 默认 | 说明 |
|---|---|---|
| Inventory / Enabled | true | 总开关 |
| Inventory / WearSlotIds | 见上表 | 允许进物品栏的穿戴槽位，逗号分隔 |
| Inventory / AllWearables | false | 放开**所有** wearable（含护甲/衣服/头盔），忽略槽位列表 |
| Inventory / ExtraItemIds | 空 | 额外强制放开的物品 id |
| Inventory / BlacklistItemIds | 空 | 永不改动 |
| Inventory / VerboseLog | false | 逐个打印放开的物品（排查用） |
| Inventory / ClearWearableSpritesOnPickup | true | 防御性补丁：穿戴物放进物品栏时清理多肢体贴图残影 |

注意：BepInEx 优先读取已存在的配置文件，**改代码里的默认值不会覆盖已有配置**，需要同步改配置或删掉它重新生成。

## 安装

1. 游戏目录装 BepInEx 5.4.23.5 x64（解压到游戏根目录，出现 winhttp.dll 和 BepInEx 文件夹）
2. 把 BackpackInInventory.dll 放进 BepInEx/plugins/ 目录
3. 启动游戏

DLL 被游戏占用时无法覆盖，更新前先退出游戏。

## 构建（改代码才需要）

普通用户不用编译，直接下载 Release 里的 DLL 即可；只有想改代码时才需要下面这些。

需要 .NET SDK（本项目用 8.0.400 验证）。

```bash
dotnet build -c Release
# 产物：bin/Release/netstandard2.1/BackpackInInventory.dll
```

- 目标框架必须 **netstandard2.1**（游戏程序集是 2.1，用 2.0 会报 CS1705）
- 需要引用 BepInEx/core/BepInEx.dll、0Harmony.dll，以及游戏 Managed 目录下的
  Assembly-CSharp.dll、UnityEngine.dll、UnityEngine.CoreModule.dll
- 本机游戏路径在 csproj 的 GameDir 属性里配置

## 调试

游戏内控制台（数字 1 左边那个反引号键）：spawn bigpack、spawn materialpouch、spawn duffelbag、spawn slingbag。
日志：游戏目录下 BepInEx/LogOutput.log，搜索 Backpack In Inventory。

## 同类 mod

Nexus 上还有一个范围更大的同类：[All Wearable Can Be Held](https://www.nexusmods.com/scavprototype/mods/369)（作者 winddramon），
它无条件放开所有 wearable，没有配置项。两者改的是同一个游戏字段，**不要同时安装**。

---

## English

A tiny BepInEx 5 plugin for **Casualties: Unknown 1.0.2** that lets wearable containers
(backpack, duffel bag, material pouch, leg pouch, belt, bandolier, fanny pack) **be placed into inventory slots**.

In vanilla, items flagged as wearable cannot go into inventory slots — dragging one there only shows
the alert "Wearables can't be held in hand (Wear/store it instead?)". Vanilla blocks it with a single check:

```csharp
// PlayerCamera.TryPerformInventoryAction
if (dragItem.Stats.wearable && !dragItem.Stats.wearableCanBeHeld) { /* refuse */ }
```

The plugin sets wearableCanBeHeld to true for wearables whose wear slot is in the configured list,
right after Item.SetupItems() runs (plus a 2-second polling safety net).

### Vanilla behaviour that is kept

Container-in-container rules are **untouched**:

- An **empty** container may still be put into another container (vanilla behaviour).
- A container **holding items** cannot be put into another container (vanilla blocks it).
- If the outer container is already nested inside something else, no further nesting is allowed.

The plugin only affects "can this wearable go into an inventory slot", nothing else.
It does **not** touch UI, drag-and-drop logic, or Container.LoadItem.

### Unlock everything (optional, off by default)

Set AllWearables to true if you also want **armor, clothing and helmets** to be holdable / storable:

- Ignores WearSlotIds and applies to every item flagged wearable
- Affects balance: armor and clothes can then be stowed and carried like normal items
- Default false; by default only the listed wearable containers are affected

### Install

1. Install BepInEx 5.4.23.5 x64 into the game root.
2. Drop BackpackInInventory.dll into the BepInEx/plugins/ folder.
3. Launch the game through Steam.

Close the game before replacing the DLL, otherwise the file is locked.

### Build (only if you change the code)

Normal users do not need this — grab the DLL from Releases instead. Requires the .NET SDK
(verified with 8.0.400): `dotnet build -c Release`, output in `bin/Release/netstandard2.1/`.
The target framework must be **netstandard2.1**.

### Configuration

Config file: BepInEx/config/humanresistor.cu.backpackininventory.cfg

| Key | Default | Meaning |
|---|---|---|
| Inventory / Enabled | true | Master switch |
| Inventory / WearSlotIds | back,thighback,thigh,belt,bandolier,torsofront | Wear slots allowed into inventory |
| Inventory / AllWearables | false | Unlock every wearable item (armor, clothing, helmets), ignoring the slot list |
| Inventory / ExtraItemIds | (empty) | Extra item ids, comma separated |
| Inventory / BlacklistItemIds | (empty) | Never touch these items |
| Inventory / VerboseLog | false | Log every unlocked item (debugging) |
| Inventory / ClearWearableSpritesOnPickup | true | Defensive patch: clear multi-limb sprites when a wearable is put into an inventory slot |

### Similar mod

[All Wearable Can Be Held](https://www.nexusmods.com/scavprototype/mods/369) by winddramon does the same
thing with a wider, unconditional scope and no configuration. Both patch the same game field —
**do not install both**.

---

## 版本适配 / Compatibility

| Plugin | Game | Notes |
|---|---|---|
| 1.2.1 | Casualties: Unknown 1.0.2 (full game) | GUID 前缀改为 humanresistor，配置文件名随之变化 |
| 1.2.0 | Casualties: Unknown 1.0.2 (full game) | 新增 AllWearables 开关 / adds AllWearables switch |
| 1.1.2 | Casualties: Unknown 1.0.2 (full game) | 默认槽位覆盖所有可穿戴容器 |
| 1.1.1 | Casualties: Unknown 1.0.2 (full game) | 移除误加的容器嵌套功能 |

Only tested on the full game 1.0.2. The Demo (v7.0.1) has a different item-info layout and is untested.

## 免责声明 / Disclaimer

修改游戏运行时数据，**请自行备份存档**。作者不对存档损坏、成就失效、联机不同步等问题负责。
This modifies game runtime data — **back up your saves**. No warranty; no liability for corrupted saves,
disabled achievements, or multiplayer desync. See [LICENSE](LICENSE).

## License

[MIT](LICENSE) © 2026 humanresistor — free to modify and redistribute, including closed-source.
