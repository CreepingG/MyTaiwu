using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;
using System.Timers;
using v2.SpecialEffects.AttackSkills;
using v2.SpecialEffects.AttackSkills.Common;

namespace FuZhou
{
    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }

    public static class Main
    {
        public static bool enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            settings = Settings.Load<Settings>(modEntry);
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("增强封印型然山咒特效");
            GUILayout.EndHorizontal();
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }
    }
    
    // 修改特效
    [HarmonyPatch(typeof(SpecificSkillTypeSilence), "OnPerformSkillEnd")]
    class FuZhou_OnPerformSkillEnd_Patch
    {
        static bool Prefix(SpecificSkillTypeSilence __instance, params object[] args)
        {
            int characterId = (int)args[0];
            bool isAlly = (bool)args[1];
            int curSkillId = (int)args[2];
            int num2 = (int)args[3];
            int power = (int)args[4];
            if (__instance.DoesCharacterMatch(characterId) && curSkillId == __instance.srcSkillId)
            {
                var ShowBattleState = typeof(AttackSkillBase).GetMethod("ShowBattleState", BindingFlags.NonPublic | BindingFlags.Instance);
                var RemoveSelf = typeof(AttackSkillBase).GetMethod("RemoveSelf", BindingFlags.NonPublic | BindingFlags.Instance);
                var normalEffectKeepTimes = (int)typeof(SpecificSkillTypeSilence).GetField("normalEffectKeepTimes", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
                var normalAffecteSkillType = (int)typeof(SpecificSkillTypeSilence).GetField("normalAffecteSkillType", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
                if (power >= 100)
                {

                    List<int> list = BattleSystem.GetEquipedCombatSkillList(!isAlly, normalAffecteSkillType)
                        .FindAll((int skillId) => BattleSystem.IsCombatSkillCooledDown(!isAlly, normalAffecteSkillType, skillId));
                    if (list.Count > 0)
                    {
                        if (!__instance.isSrcSkillReversed)
                        {
                            int combatSkillId = list[UnityEngine.Random.Range(0, list.Count)];
                            BattleSystem.SetCombatSkillCoolDownFrames(!isAlly, normalAffecteSkillType, combatSkillId, -1);
                            ShowBattleState.Invoke(__instance, new object[] { characterId, isAlly, 0 });
                        }
                        else
                        {
                            foreach (var combatSkillId in list)
                            {
                                BattleSystem.SetCombatSkillCoolDownFrames(!isAlly, normalAffecteSkillType, combatSkillId, normalEffectKeepTimes * 4 / 10);
                            }
                            ShowBattleState.Invoke(__instance, new object[] { characterId, isAlly, 0 });
                        }
                    }
                }
                RemoveSelf.Invoke(__instance, new object[0]);
            }
            return false;
        }

    }

    // 改变文本
    [HarmonyPatch(typeof(ArchiveSystem.LoadGame), "LoadReadonlyData")]
    [HarmonyPriority(Priority.Last)]
    static class FuZhou_LoadReadonlyData_Patch
    {
        static void Postfix()
        {
            if (ArchiveSystem.LoadGame.LoadedReadonlyData()) //只读数据加载完毕
            {
                var data = DateFile.instance.gongFaFPowerDate;
                var pairs = new Dictionary<int, string>
                {
                    [566] = "轻灵身法",
                    [567] = "护体功法",
                    [569] = "奇窍功法"
                };
                foreach (var kvp in pairs)
                {
                    var key = kvp.Key;
                    var value = kvp.Value;
                    data[key][99] = $"发挥十成威力时使敌人的1个{value}无法再在本次战斗中{(key == 569 ? "生效" : "使用")}";
                    data[5000 + key][99] =  $"发挥十成威力时使敌人的所有{value}暂时无法{(key == 569 ? "生效" : "使用")}";
                    data[5000 + key][97] = data[key][97];
                }
            }
        }
    }
}