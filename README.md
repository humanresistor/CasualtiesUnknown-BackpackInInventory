# Backpack In Inventory（未知伤亡 / Casualties: Unknown 插件）

只做一件事：**允许把可穿戴的背包从背上拖进物品栏槽位**。
容器套容器（背包放进背包）**明确不做**，保持原版拦截。

## 原理

反编译 1.0.2 的 `Assembly-CSharp.dll` 后确认，拦住「可穿戴物品放进身体槽位」的只有一处：

```csharp
// PlayerCamera.TryPerformInventoryAction
if (component.isBody)
{
    if (dragItem.Stats.wearable && !dragItem.Stats.wearableCanBeHeld)
    {
        DoAlert(Locale.GetOther("alertpickupwearable"));
        return true;
    }
    ...  // 真正 PickUpItem
}
```

`ItemInfo.wearableCanBeHeld` 全库只有这一处读取（`autopump` 把它设为 true 作示例）。
物品数据在 `Item.GlobalItems`，由 `Item.SetupItems()` 在进入世界时填充（`WorldGeneration` 调用）。

插件做法：Harmony.Postfix 挂在 `Item.SetupItems` 之后，把 `wearSlotId` 属于配置槽位（默认 `back`）且
`wearableCanBeHeld == false` 的物品改成 true。另有一个 2 秒轮询的兜底，防止 SetupItems 早于插件加载。

不碰 UI、不碰拖放逻辑、不改 `Container.LoadItem`，所以「背包套背包」仍按原版被拦。

## 配置

`<游戏>\BepInEx\config\humanresistor.cu.backpackininventory.cfg`

| 键 | 默认 | 说明 |
|---|---|---|
| Inventory/Enabled | true | 总开关 |
| Inventory/WearSlotIds | back,thighback,thigh,belt,bandolier,torsofront | 允许进物品栏的穿戴槽位（覆盖所有可穿戴容器），逗号分隔 |
| Inventory/ExtraItemIds | 空 | 额外强制放开的物品 id |
| Inventory/BlacklistItemIds | 空 | 永不改动 |
| Inventory/VerboseLog | true | 逐个打印放开的物品 |

> 1.1.0 曾经加过 `[Nesting]` 段（允许容器套容器），1.1.1 已删除该功能；旧 cfg 里的残留键不会生效，可手动删掉。

## 构建

```bash
dotnet build -c Release
# 产物：bin/Release/netstandard2.1/BackpackInInventory.dll
```

- 目标框架必须 **netstandard2.1**（游戏程序集是 2.1，用 2.0 会 CS1705）
- 引用：`BepInEx/core/BepInEx.dll`、`0Harmony.dll`、
  `CasualtiesUnknown_Data/Managed/{Assembly-CSharp, UnityEngine, UnityEngine.CoreModule}.dll`
- `GameDir` 在 csproj 里配置

## 安装 / 调试

DLL 丢进 `<游戏>\BepInEx\plugins\`，重启游戏（DLL 被占用时无法覆盖，需先退出）。
游戏内控制台（反引号 `）：`spawn bigpack`、`spawn duffelbag`、`spawn slingbag`。
日志搜 `Backpack In Inventory`。

`decompiled/` 是 1.0.2 反编译源码（ilspycmd），供查证；`tests/ItemTableProbe` 是离线探针，因 `Item.SetupItems()` 会走 Unity `Locale.LoadLanguage()`（ECall）跑不通，留作记录。

## 版本适配

| 插件版本 | 游戏版本 | 说明 |
|---|---|---|
| 1.1.2 | Casualties: Unknown 1.0.2（正式版） | 当前版本 |

只测过正式版 1.0.2；Demo 版（v7.0.1）的 `ItemInfo` 布局不同，未验证。

## 免责声明

修改游戏运行时数据，**请自行备份存档**。作者不对存档损坏、成就失效、联机不同步等问题负责。
本插件只在单人/沙盒中验证过；联机（Casualties: Together）下未做完整回归。

## License

[MIT](LICENSE) © 2026 humanresistor
