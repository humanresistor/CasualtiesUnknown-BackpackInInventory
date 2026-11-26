using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace BackpackInInventory
{
    /// <summary>
    /// 只做一件事：把可穿戴容器（背包）放进物品栏槽位时不再被原版拦住。
    /// 容器套容器（背包放进背包）保持原版行为——明确不做。
    /// </summary>
    [BepInPlugin(Guid, "Backpack In Inventory", Version)]
    public class Plugin : BaseUnityPlugin
    {
        public const string Guid = "humanresistor.cu.backpackininventory";
        public const string Version = "1.1.2";

        internal static ManualLogSource Log;
        internal static ConfigEntry<bool> CfgEnabled;
        internal static ConfigEntry<string> CfgSlotIds;
        internal static ConfigEntry<string> CfgExtraIds;
        internal static ConfigEntry<string> CfgBlacklist;
        internal static ConfigEntry<bool> CfgVerbose;

        private static Dictionary<string, ItemInfo> lastTable;
        private static readonly HashSet<string> done = new HashSet<string>();

        private void Awake()
        {
            Log = Logger;
            CfgEnabled = Config.Bind("Inventory", "Enabled", true, "允许把可穿戴容器（背包等）放进物品栏槽位。");
            CfgSlotIds = Config.Bind("Inventory", "WearSlotIds", "back,thighback,thigh,belt,bandolier,torsofront", "这些穿戴槽位上的物品允许被放进物品栏，逗号分隔。");
            CfgExtraIds = Config.Bind("Inventory", "ExtraItemIds", "", "额外强制允许的物品 id，逗号分隔（不受槽位限制）。");
            CfgBlacklist = Config.Bind("Inventory", "BlacklistItemIds", "", "永远不改动的物品 id，逗号分隔。");
            CfgVerbose = Config.Bind("Inventory", "VerboseLog", true, "放开每个物品时在日志里打印一行。");

            new Harmony(Guid).PatchAll(typeof(SetupItemsPatch));

            var go = new GameObject("BackpackInInventory.Watcher");
            Object.DontDestroyOnLoad(go);
            go.AddComponent<Watcher>();
            Log.LogInfo("Backpack In Inventory v" + Version + " 已加载（只放开物品栏，容器套容器保持原版）。");
        }

        internal static void Apply(string source)
        {
            if (Item.GlobalItems == null) return;
            if (ReferenceEquals(lastTable, Item.GlobalItems)) return;
            lastTable = Item.GlobalItems;
            done.Clear();

            if (!CfgEnabled.Value)
            {
                Log.LogInfo("(" + source + ") 已禁用，跳过。");
                return;
            }

            HashSet<string> slots = Split(CfgSlotIds.Value);
            HashSet<string> extra = Split(CfgExtraIds.Value);
            HashSet<string> black = Split(CfgBlacklist.Value);

            int changed = 0;
            foreach (KeyValuePair<string, ItemInfo> kv in Item.GlobalItems)
            {
                ItemInfo info = kv.Value;
                if (info == null || !info.wearable || info.wearableCanBeHeld) continue;

                string id = kv.Key.ToLowerInvariant();
                if (black.Contains(id) || done.Contains(id)) continue;

                bool match = extra.Contains(id) || (info.wearSlotId != null && slots.Contains(info.wearSlotId.ToLowerInvariant()));
                if (!match) continue;

                info.wearableCanBeHeld = true;
                done.Add(id);
                changed++;
                if (CfgVerbose.Value)
                    Log.LogInfo("允许放进物品栏: " + kv.Key + "（槽位 " + (info.wearSlotId ?? "?") + "）");
            }
            Log.LogInfo("(" + source + ") 物品表处理完成，本次放开 " + changed + " 个物品。");
        }

        internal static HashSet<string> Split(string raw)
        {
            var set = new HashSet<string>();
            if (string.IsNullOrEmpty(raw)) return set;
            foreach (string part in raw.Split(','))
            {
                string s = part.Trim().ToLowerInvariant();
                if (s.Length > 0) set.Add(s);
            }
            return set;
        }
    }

    internal class Watcher : MonoBehaviour
    {
        private float nextCheck;

        private void Update()
        {
            if (Time.unscaledTime < nextCheck) return;
            nextCheck = Time.unscaledTime + 2f;
            try { Plugin.Apply("轮询"); }
            catch (System.Exception e) { Plugin.Log.LogWarning("轮询失败: " + e.Message); }
        }
    }

    [HarmonyPatch(typeof(Item), nameof(Item.SetupItems))]
    internal static class SetupItemsPatch
    {
        [HarmonyPostfix]
        private static void Postfix()
        {
            Plugin.Apply("SetupItems");
        }
    }
}
