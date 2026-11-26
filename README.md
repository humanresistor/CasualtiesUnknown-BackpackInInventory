# Backpack In Inventory — Casualties: Unknown

> 未知伤亡（Casualties: Unknown）的 BepInEx 5 插件：**允许把可穿戴容器（双肩包、材料包、腿包、腰带、弹药带、腰包）放进物品栏槽位。**

- 中文 | [English](#english)
- 游戏版本：Casualties: Unknown **1.0.2**（正式版）
- 当前版本：**v1.1.2**

> ### ⚠️ AI 生成声明 / AI Disclosure
> **本项目的全部代码与文档由 AI（DeepSeek Harness 智能体）编写**，人类只提需求、做测试与验收。
> 测试范围仅限单人 / 沙盒模式，未做联机（Casualties: Together）完整回归，请自行备份存档。
> **All code and documentation in this repository were written by an AI assistant**; a human only specified requirements, tested, and accepted the result.
>
> ### 📜 随便用 / Use it freely
> 采用 [MIT](LICENSE) 协议：**任何人都可以自由拿去修改、再发布、商用、闭源二次分发**，只需保留版权与许可声明。
> Licensed under MIT: **anyone may fork, modify, redistribute, or use it commercially, including in closed-source form**, provided the copyright and license notice are kept.

---

## 它解决什么问题

原版里背包、材料包这类"可穿戴容器"只能穿在身上（`back` / `thighback` / `thigh` / `belt` / `bandolier` / `torsofront` 等槽位），
**拖进物品栏会被拦下**并提示"这东西只能穿戴"。本插件放开这条限制。

**不会**做的事：容器套容器（背包放进背包）保持原版行为，未做任何改动。

## 中文说明

### 原理

反编译 1.0.2 的 `Assembly-CSharp.dll` 后确认，拦住"可穿戴物品放进身体槽位"的只有一处：

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

`ItemInfo.wearableCanBeHeld` 全库只有这一处读取（`autopump` 把它设为 true 作示例）。
物品数据在 `Item.GlobalItems`，由 `Item.SetupItems()` 在进入世界时填充。

插件做法：Harmony.Postfix 挂在 `Item.SetupItems` 之后，把 `wearSlotId` 属于配置槽位（默认覆盖所有可穿戴容器）且
`wearableCanBeHeld == false` 的物品改成 true。另有 2 秒轮询兜底，防止 `SetupItems` 早于插件加载。

不碰 UI、不碰拖放逻辑、不改 `Container.LoadItem`。

### 默认放开的槽位

`WearSlotIds = back,thighback,thigh,belt,bandolier,torsofront`

| 槽位 | 对应物品 |
|---|---|
| `back` | 双肩包 bigpack、行旅袋 duffelbag、小背包 smallpack、单肩包 slingbag |
| `thighback` | 材料包 materialpouch |
| `thigh` | 腿包 legpouch |
| `belt` | 腰带 belt |
| `bandolier` | 弹药带 bandolier |
| `torsofront` | 腰包 fannypack、液体袋 liquidpouch、创伤背心 traumarig |

### 配置

`<游戏>\BepInEx\config\humanresistor.cu.backpackininventory.cfg`

| 键 | 默认 | 说明 |
|---|---|---|
| Inventory/Enabled | true | 总开关 |
| Inventory/WearSlotIds | 见上表 | 允许进物品栏的穿戴槽位，逗号分隔 |
| Inventory/ExtraItemIds | 空 | 额外强制放开的物品 id |
| Inventory/BlacklistItemIds | 空 | 永不改动 |
| Inventory/VerboseLog | true | 逐个打印放开的物品 |

> BepInEx 优先读取已存在的 cfg，**改代码默认值不会覆盖已有配置**，需要同步改 cfg 或删掉它重新生成。

### 安装

1. 游戏目录装 [BepInEx 5.4.23.5 x64](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.5)（解压到游戏根目录，出现 `winhttp.dll` + `BepInEx/`）
2. 把 `BackpackInInventory.dll` 放进 `<游戏>\BepInEx\plugins\`
3. 启动游戏

DLL 被游戏占用时无法覆盖，更新前先退出游戏。

### 构建

需要 .NET SDK（本项目用 8.0.400 验证）。

```bash
dotnet build -c Release
# 产物：bin/Release/netstandard2.1/BackpackInInventory.dll
```

- 目标框架必须 **netstandard2.1**（游戏程序集是 2.1，用 2.0 会报 CS1705）
- 引用：`BepInEx/core/BepInEx.dll`、`0Harmony.dll`、
  `CasualtiesUnknown_Data/Managed/{Assembly-CSharp, UnityEngine, UnityEngine.CoreModule}.dll`
- `GameDir` 在 csproj 里配置成本机游戏路径

### 调试

游戏内控制台（反引号 `）：`spawn bigpack`、`spawn materialpouch`、`spawn duffelbag`、`spawn slingbag`。
日志：`<游戏>\BepInEx\LogOutput.log`，搜 `Backpack In Inventory`。

---

## English

A tiny BepInEx 5 plugin for **Casualties: Unknown 1.0.2** that lets wearable containers
(backpack, material pouch, leg pouch, belt, bandolier, fanny pack) **be placed into inventory slots**
instead of being wearable-only.

Vanilla blocks this with a single check in `PlayerCamera.TryPerformInventoryAction`:

```csharp
if (dragItem.Stats.wearable && !dragItem.Stats.wearableCanBeHeld) { /* refuse */ }
```

The plugin simply sets `ItemInfo.wearableCanBeHeld = true` for wearables whose `wearSlotId`
is in the configured list, right after `Item.SetupItems()` runs (plus a 2-second polling safety net).
It does **not** touch UI, drag-and-drop logic, or `Container.LoadItem`, so container-in-container
nesting keeps its vanilla behaviour.

### Install

1. Install [BepInEx 5.4.23.5 x64](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.5) into the game root.
2. Drop `BackpackInInventory.dll` into `<Game>\BepInEx\plugins\`.
3. Launch the game (through Steam).

### Configuration

`<Game>\BepInEx\config\humanresistor.cu.backpackininventory.cfg`

| Key | Default | Meaning |
|---|---|---|
| Inventory/Enabled | true | Master switch |
| Inventory/WearSlotIds | back,thighback,thigh,belt,bandolier,torsofront | Wear slots allowed into inventory |
| Inventory/ExtraItemIds | (empty) | Extra item ids, comma separated |
| Inventory/BlacklistItemIds | (empty) | Never touch these items |
| Inventory/VerboseLog | true | Log every unlocked item |

---

## 版本适配 / Compatibility

| Plugin | Game | Notes |
|---|---|---|
| 1.1.2 | Casualties: Unknown 1.0.2 (full game) | current |

Only tested on the full game 1.0.2. The Demo (v7.0.1) has a different `ItemInfo` layout and is untested.

## 免责声明 / Disclaimer

修改游戏运行时数据，**请自行备份存档**。作者不对存档损坏、成就失效、联机不同步等问题负责。
This modifies game runtime data — **back up your saves**. No warranty; no liability for corrupted saves,
disabled achievements, or multiplayer desync. See [LICENSE](LICENSE).

## License

[MIT](LICENSE) © 2026 humanresistor — free to modify and redistribute, including closed-source.
