using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections;

namespace FightBoss
{
    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
        }
        public bool keepLevel = true;
        public bool poison = false;
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
            if (DateFile.instance == null || DateFile.instance.mianActorId <= 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("存档未加载");
                GUILayout.EndHorizontal();
                return;
            }
            bool flag;
            int tmp;
            GUILayout.BeginHorizontal();
            GUILayout.Label("ID(2001-2009)：", GUILayout.Width(100));
            if (int.TryParse(GUILayout.TextField(PresetActorId.ToString()), out tmp))
            {
                PresetActorId = tmp;
                GUILayout.Label(DateFile.instance.GetActorName(PresetActorId));
            }
            if (GUILayout.Button("开战")) CallBattle();
            if (GUILayout.Button("逃跑(不要在战斗准备界面使用)")) EndBattle();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("游戏难度(0-3)：", GUILayout.Width(100));
            if (int.TryParse(GUILayout.TextField(DateFile.instance.enemyBorn.ToString(), GUILayout.Width(150)), out tmp)
                && tmp >= 0 && tmp <= 3)
                DateFile.instance.enemyBorn = tmp;
            
            GUILayout.FlexibleSpace();

            GUILayout.Label("世界进度(0-36)：", GUILayout.Width(100));
            if (int.TryParse(GUILayout.TextField(XXLevel.ToString(), GUILayout.Width(150)), out tmp))
                XXLevel = tmp;

            GUILayout.FlexibleSpace();

            settings.keepLevel = GUILayout.Toggle(settings.keepLevel, "保持世界进度", GUILayout.Width(150));
            settings.poison = GUILayout.Toggle(settings.poison, "带毒", GUILayout.Width(150));
            GUILayout.EndHorizontal();
        }
        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        public static int XXLevel = -1;
        public static int PresetActorId = 2009;
        public static int Difficulty = -1;
        public static Dictionary<int, int[]> xxPointValue;

        public static void CallBattle()
        {
            Game.Instance.ChangeGameState(eGameState.Battle,
                9001 + DateFile.instance.GetWorldXXLevel(), // teamId
                0, // typ 死斗
                18, // mapTyp 背景
                new List<int> { PresetActorId } // mianEnemyIds
            );
            // 创建副本
            if (settings.keepLevel)
            {
                xxPointValue = new Dictionary<int, int[]>();
                foreach (var kvp in DateFile.instance.xxPointValue)
                {
                    xxPointValue[kvp.Key] = new int[3];
                    for (int i = 0; i < 3; i++)
                    {
                        xxPointValue[kvp.Key][i] = kvp.Value[i];
                    }
                }
            }
        }

        public static void EndBattle()
        {
            BattleEndWindow.instance.BattleEnd(false, 1);
        }
    }
    
    [HarmonyPatch(typeof(DateFile), "MakeXXEnemy")]//捕获相枢化身
    public static class FightBoss_MakeXXEnemy_Patch
    {
        static void Postfix(int baseActorId, int __result)
        {
            if (!Main.enabled) return;
            SetPoison(__result, baseActorId);
        }
        static void SetPoison(int enemyId, int baseId)
        {
            // 根据相枢化身内力属性的毒抗 生成毒素随机池
            var qiTyp = DateFile.instance.qiValueStateDate[DateFile.instance.GetActorQiTyp(enemyId)];
            var values = Enumerable.Range(8, 6).Select(i => qiTyp[i].ParseInt()).AsEnumerable();
            var poisonTyps = values.SelectMany((v, i) => Enumerable.Repeat(i + 71, v)).ToArray(); 
            foreach(var i in new int[] { 1, 2, 3, 4, 6, 7 }) //兵器&护具
            {
                var equipPosition = i + 300;
                var equipId = DateFile.instance.GetActorDate(enemyId, equipPosition, false).ParseInt();
                var poisonLevel = Mathf.Clamp(DateFile.instance.GetWorldXXLevel(), 1, 9).ToString();
                var poisonTyp = poisonTyps.Random();
                var poisonValue = DateFile.instance.presetitemDate.Select(kvp => kvp.Value)
                    .First(item => item[5] == "30" && item[8] == poisonLevel && item[poisonTyp] != "0") // 毒药 同品级 指定毒素
                    [poisonTyp];
                GameData.Items.SetItemProperty(equipId, poisonTyp, poisonValue);
            }
        }
    }

    [HarmonyPatch(typeof(DateFile), "GetWorldXXLevel")]//改变世界进度
    public static class FightBoss_GetWorldXXLevel_Patch
    {
        static bool Prefix(bool getBaseLevel, ref int __result)
        {
            if (!Main.enabled || Main.XXLevel < 0)
            {
                return true;
            }
            __result =  getBaseLevel ? Main.XXLevel : Main.XXLevel / 4;
            return false;
        }
    }

    [HarmonyPatch(typeof(DateFile), "LoadDate")]//加载存档
    public static class FightBoss_LoadDate_Patch
    {
        static void Postfix()
        {
            Main.XXLevel = DateFile.instance.GetWorldXXLevel(true);
            //Main.Difficulty = DateFile.instance.enemyBorn;
        }
    }

    [HarmonyPatch(typeof(BattleEndWindow), "BattleEnd")]//战斗结束
    public static class FightBoss_BattleEnd_Patch
    {
        static void Postfix()
        {
            if (Main.xxPointValue != null && Main.settings.keepLevel)
            {
                DateFile.instance.xxPointValue = Main.xxPointValue;
            }
            Main.xxPointValue = null;
        }
    }

}