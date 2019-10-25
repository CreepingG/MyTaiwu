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
            GUILayout.Label("ID(2001-2009)：");
            if (int.TryParse(GUILayout.TextField(PresetActorId.ToString()), out tmp))
            {
                PresetActorId = tmp;
                GUILayout.Label(DateFile.instance.GetActorName(PresetActorId));
            }
            if (GUILayout.Button("开战")) CallBattle();
            if (GUILayout.Button("逃跑(不要在战斗准备界面使用)")) EndBattle();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("游戏难度(0-3)：");
            if (int.TryParse(GUILayout.TextField(DateFile.instance.enemyBorn.ToString()), out tmp))
                if (tmp >= 0 && tmp <= 3)
                    DateFile.instance.enemyBorn = tmp;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("世界进度(0-36)：");
            if (int.TryParse(GUILayout.TextField(XXLevel.ToString()), out tmp)) XXLevel = tmp;
            GUILayout.EndHorizontal();
        }
        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        public static int XXLevel = -1;
        public static int PresetActorId = 2009;
        public static int Difficulty = -1;

        public static void CallBattle()
        {
            Game.Instance.ChangeGameState(eGameState.Battle,
                9001 + DateFile.instance.GetWorldXXLevel(), // teamId
                0, // typ 死斗
                18, // mapTyp 背景
                new List<int> { PresetActorId } // mianEIds
            );
            //StartBattle.instance.ShowStartBattleWindow(9001 + DateFile.instance.GetWorldXXLevel(), 0, 18, new List<int> { PresetActorId });
        }

        public static void EndBattle()
        {
            BattleEndWindow.instance.BattleEnd(false, 1);
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

}