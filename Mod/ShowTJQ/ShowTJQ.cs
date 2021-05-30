using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;
using v2.SpecialEffects.AttackSkills;
using v2.SpecialEffects.AttackSkills.Wudangpai.FistAndPalm;

namespace ShowTJQ
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
            GUILayout.Label("显示太极拳反噬的功法");
            GUILayout.EndHorizontal();
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }
    }
    
    // 修改特效
    [HarmonyPatch(typeof(TaijiQuan), "OnPerformSkillAttack")]
    class ShowTJQ_OnPerformSkillAttack_Patch

    {
        static bool Prefix(TaijiQuan __instance, params object[] args)
        {
            if (!__instance.isSrcSkillReversed) return true;
            int characterId = (int)args[0];
            bool isAlly = (bool)args[1];
            int curSkillId = (int)args[2];
            int enemyId = (int)args[3];
            int power = (int)args[4];
            bool flag2 = (bool)args[5];
            if (__instance.isSrcSkillReversed && __instance.DoesCharacterMatch(characterId) && curSkillId == __instance.srcSkillId && power < 3 && (bool)args[9] && (flag2 || (int)args[10] <= 0))
            {
                List<int> equipedCombatSkillList = BattleSystem.GetEquipedCombatSkillList(!isAlly, 0);
                if (equipedCombatSkillList.Count > 0)
                {
                    int targetSkillId = equipedCombatSkillList[UnityEngine.Random.Range(0, equipedCombatSkillList.Count)];
                    string text = "太极拳=>" + DateFile.instance.gongFaDate[targetSkillId][0] + "：";
                    string[] typ = { "卸力", "拆招", "闪避" };
                    int 化解 = -1;
                    for (int i = 0; i < 3; i++)
                    {
                        int 命中值 = BattleVaule.instance.SetGongFaValue(!isAlly, enemyId, true, targetSkillId, 601 + i, -1);
                        int 化解值 = BattleVaule.instance.GetDeferDefuse(isAlly, characterId, true, 3 + i, 0);
                        if (i != 0) text += "|";
                        text += typ[i] + BattleVaule.instance.GetDeferDefuse(isAlly, characterId, true, 3 + i, 0) + "/" + 命中值;
                        if (化解值 >= 命中值 && 命中值 >= 0)
                        {
                            化解 = i;
                            break;
                        }
                    }
                    if (化解 >= 0)
                    {
                        BattleSystem.SetDamageToSelf(enemyId, !isAlly, targetSkillId, 500, getWin: false);
                        ShowBattleState(__instance, characterId, isAlly);
                    }
                    else
                    {
                        text += "|失败";
                    }
                    Show(text);
                }
            }
            return false;
        }
        static MethodInfo _ShowBattleState = typeof(AttackSkillBase).GetMethod("ShowBattleState", BindingFlags.NonPublic | BindingFlags.Instance);
        static void ShowBattleState(object __instance, int characterId, bool isAlly, int index = 0)
        {
            _ShowBattleState.Invoke(__instance, new object[]{ characterId, isAlly, index});
        }

        public static void Show(object text)
        {
            int id = -489819;
            var date = DateFile.instance.tipsMassageDate;
            if (!date.ContainsKey(id))
            {
                date.Add(id, new Dictionary<int, string>
                {
                    [1] = "3",
                    [2] = "0",
                    [99] = "D0"
                });
            }
            TipsWindow.instance.SetTips(id, new string[] { text.ToString() }, 300);
        }
    }

}