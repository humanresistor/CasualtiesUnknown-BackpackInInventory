# Changelog

## 1.2.2
- 新增防御性补丁：穿戴物放进物品栏槽位后调用 Wearable.ClearSprites()，修复多肢体贴图残影（配置项 ClearWearableSpritesOnPickup）

## 1.2.1
- 插件标识前缀改为 humanresistor，配置文件名随之变化（humanresistor.cu.backpackininventory.cfg）
- 升级时把旧配置文件改名为新文件名即可保留设置，也可删掉让插件重新生成默认配置

## 1.2.0
- 新增 AllWearables 开关（默认 false）：打开后放开所有 wearable 物品（含护甲、衣服、头盔），忽略 WearSlotIds
- README 增加与 Nexus「All Wearable Can Be Held」的对比与共存提示

## 1.1.2
- 默认放开的穿戴槽位扩展到所有可穿戴容器：back, thighback, thigh, belt, bandolier, torsofront
- 清理 1.0.0 遗留的 [General] 配置段，统一到 [Inventory]

## 1.1.1
- 移除 1.1.0 误加的「容器套容器」功能，恢复原版嵌套行为

## 1.1.0
- （已撤回）曾尝试允许容器互相嵌套，与预期相反，1.1.1 删除

## 1.0.0
- 首个版本：允许把 back 槽的可穿戴容器放进物品栏槽位
