# Changelog

## 1.2.0
- 新增 AllWearables 开关（默认 false）：打开后放开所有 wearable 物品（含护甲、衣服、头盔），忽略 WearSlotIds
- README 增加与 Nexus「All Wearable Can Be Held」的对比与共存提示

## 1.1.2
- 默认放开的穿戴槽位扩展到所有可穿戴容器：back, thighback, thigh, belt, bandolier, torsofront
  - 包含材料包（thighback）、腿包、皮带、子弹带、腰包等
- 清理 1.0.0 遗留的 [General] 配置段，统一到 [Inventory]

## 1.1.1
- 移除 1.1.0 误加的「容器套容器」功能，恢复原版嵌套行为
- 版本只保留「可穿戴容器放入物品栏」

## 1.1.0
- （已撤回）曾尝试允许容器互相嵌套，与预期相反，1.1.1 删除

## 1.0.0
- 首个版本：允许把 back 槽的可穿戴容器放进物品栏槽位
