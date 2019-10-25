using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;

namespace GangPower
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
            GUILayout.BeginHorizontal();
            GUILayout.EndHorizontal();
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }
        
        static string GetGangName(int gangId) => DateFile.instance.gangDate[gangId][0];
    }

    public static class DoPower
    {
        static int MianId { get => DateFile.instance.mianActorId; }
        static int PartId { get => DateFile.instance.mianPartId; }
        static int PlaceId { get => DateFile.instance.mianPlaceId; }
        static bool HasGangPower(int actorId, int gangId)
        {
            var NeiGongs = DateFile.instance.GetActorEquipGongFa(actorId)[0];
            foreach (int gongfaId in NeiGongs)
            {
                int gongfaGang = int.Parse(DateFile.instance.gongFaDate[gongfaId][3]);
                if (gongfaGang == gangId) return true;
            }
            return false;
        }
        static bool HasGangPower(int gangId) => HasGangPower(MianId, gangId);
        static string GetActorName(int actorId, bool showGang = true)
        {
            string actorName = DateFile.instance.GetActorName(actorId);
            if (!showGang) return actorName;

            int gang = int.Parse(DateFile.instance.GetActorDate(actorId, 19, false));
            string gangName = DateFile.instance.GetGangDate(gang, 0);
            int num20 = int.Parse(DateFile.instance.GetActorDate(actorId, 20, false));
            int grade = 10 - Mathf.Abs(num20);
            grade = Mathf.Clamp(grade, 1, 9);
            int gangValueId = DateFile.instance.GetGangValueId(gang, 10 - grade);
            bool isMale = DateFile.instance.GetActorDate(actorId, 14) == "1";
            int statusKey = num20 < 0 ? (isMale ? 1002 : 1003) : 1001;
            string statusName = DateFile.instance.presetGangGroupDateValue[DateFile.instance.GetGangValueId(gang, num20)][statusKey];
            statusName = DateFile.instance.SetColoer(20001 + grade, statusName, false);

            return gangName + statusName + " " + actorName;
        }
        static void Log(string text) => Main.Logger.Log(text);
        //4 武当
        public static class WuDang
        {
            const int GangId = 4;
            [HarmonyPatch(typeof(DateFile), "GetActorQiTyp")]
            public static class GangPower_GetActorQiTyp_Patch
            {
                static bool Prefix(DateFile __instance, ref int __result, int id)
                {
                    if (!Main.enabled) return true;
                    if (DateFile.instance.actorsDate[id][6] == "2") return true;//相枢
                    if (HasGangPower(id, GangId))//武当
                    {
                        __result = 6;//天人一体
                        return false;
                    }
                    return true;
                }
            }
        }
        //8 璇女
        public static class XuanNv
        {
            const int GangId = 8;
            [HarmonyPatch(typeof(ActorFace), "UpdateFace")]
            public static class GangPower_UpdateFace_Patch
            {
                static void Postfix(ActorFace __instance, int age, int gender, int actorGenderChange, int actorId, int[] faceDate, bool life)
                {
                    if (!Main.enabled) return;
                    bool showMale = gender == 1;
                    if (actorGenderChange == 1) showMale = !showMale;
                    if (showMale) return;
                    if (!__instance.ageImage.gameObject.activeSelf) return;

                    if (HasGangPower(actorId, GangId))//璇女
                    {
                        __instance.ageImage.gameObject.SetActive(false);//隐藏皱纹
                    }
                }
            }
            [HarmonyPatch(typeof(DateFile), "GetMaxDayTime")]
            public static class GangPower_GetMaxDayTime_Patch
            {
                static void Postfix(DateFile __instance, ref int __result)
                {
                    if (!Main.enabled) return;
                    int age = DateFile.instance.MianAge();
                    if (age < 20) return;

                    if (HasGangPower(GangId))//璇女
                    {
                        __result = int.Parse(DateFile.instance.ageDate[20][1]);//按20岁算
                    }
                }
            }
        }
        //3 百花
        public static class BaiHua
        {
            static readonly int[] Click_Id = { int.MinValue + 153, int.MinValue + 154 };
            const int GangId = 3;
            [HarmonyPatch(typeof(WorldMapSystem), "DoMapHealing")]
            public static class GangPower_DoMapHealing_Patch
            {
                static readonly string[] actionName = { "疗伤", "驱毒" };
                static void Postfix(int typ)
                {
                    if (!Main.enabled) return;

                    var family = DateFile.instance.GetFamily(false, true);

                    int skillId = typ == 0 ? 509 : 510;
                    int skillValue = DateFile.instance.GetActorValue(MianId, skillId, true);
                    foreach(int actorId in family)
                    {
                        if (actorId == MianId) continue;
                        skillValue += DateFile.instance.GetActorValue(actorId, skillId, true) / 2;
                    }
                    int healingPower = Mathf.Min(100 + skillValue / 2, 350);
                    Log($"{actionName[typ]} healingPower = {healingPower}");

                    int actorsCnt = 0;
                    int oldFame = DateFile.instance.GetActorFame(MianId);
                    var mapActors = DateFile.instance.worldMapActorDate[PartId][PlaceId];
                    foreach(int actorId in mapActors)
                    {
                        if (actorId <= 0 || family.Contains(actorId)) continue;//排除同道
                        if (DateFile.instance.GetActorDate(actorId, 6, false) != "0") continue;//相枢或失心人
                        if (DateFile.instance.GetActorDate(actorId, 8, false) != "1") continue;//特殊NPC
                        if (DateFile.instance.GetActorDate(actorId, 26, false) != "0") continue;//死人
                        if (DateFile.instance.GetActorDate(actorId, 26, false) != "0") continue;//俘虏
                        Log(GetActorName(actorId));
                        if(typ == 0)
                        {
                            bool injured = ActorMenu.instance.Hp(actorId, false) > 0 || ActorMenu.instance.Sp(actorId, false) > 0;
                            if (injured)
                            {
                                var keys = new List<int>(DateFile.instance.actorInjuryDate[actorId].Keys);
                                foreach (int injuryId in keys)
                                {
                                    DateFile.instance.RemoveInjury(actorId, injuryId, -healingPower);
                                }
                                int injuryCnt = keys.Count;
                                Log($"{GetActorName(actorId)} 伤口数：{injuryCnt}");
                                int favorValue = healingPower * injuryCnt;
                                DateFile.instance.ChangeFavor(actorId, favorValue, true, false);
                                DateFile.instance.SetActorFameList(MianId, 15, 1, actorId);
                                actorsCnt++;
                            }
                        }
                        else
                        {
                            int poisonCnt = 0;
                            for (int i = 0; i < 6; i++)
                            {
                                int poisonValue = ActorMenu.instance.Poison(actorId, i, false);
                                if (poisonValue > 0)
                                {
                                    int newValue = Mathf.Max(0, poisonValue - healingPower);
                                    DateFile.instance.actorsDate[actorId][51 + i] = newValue.ToString();
                                    string poisonName = DateFile.instance.poisonDate[i][0];
                                    Log($"{poisonName} {poisonValue}=>{newValue}");
                                    poisonCnt++;
                                }
                            }
                            if (poisonCnt > 0)
                            {
                                int favorValue = healingPower * poisonCnt;
                                DateFile.instance.ChangeFavor(actorId, favorValue, true, false);
                                DateFile.instance.SetActorFameList(MianId, 15, 1, actorId);
                                actorsCnt++;
                            }
                        }
                    }

                    int newFame = DateFile.instance.GetActorFame(MianId);
                    string[] tipsText = new string[3];
                    tipsText[0] = DateFile.instance.SetColoer(10002, "【悬壶济世】");
                    tipsText[1] = $"共为{actorsCnt}人进行了{actionName[typ]}";
                    if (newFame != oldFame)
                    {
                        int tipsId = newFame > oldFame ? 5001 : 5002;
                        string MianName = DateFile.instance.GetActorName();
                        tipsText[2] = DateFile.instance.tipsMassageDate[tipsId][99].Replace("D0", MianName);
                    }
                    TipsWindow.instance.SetTips(0, tipsText, 300);
                }
            }
        }
        //15 血犼
        public static class XueHou
        {
            const int GangId = 15;
            [HarmonyPatch(typeof(DateFile), "RemoveActor")]
            public static class GangPower_RemoveActor_Patch
            {
                static bool Prefix(ref List<int> actorId, bool die)
                {
                    if (!Main.enabled) return true;
                    if (die) return true;
                    var ids = new List<int>(actorId);
                    foreach (int id in ids)
                    {
                        if (HasGangPower(id, GangId))
                        {
                            actorId.Remove(id);
                            Log($"拦截 {GetActorName(id)}");
                        }
                    }
                    if (actorId.Count <= 0) return false;
                    return true;
                }
            }
        }
        //13 界青
        public static class JieQing
        {
            const int GangId = 13;
            //把战斗判定1019、宝典战斗判定1045替换成受伤判定1023
            [HarmonyPatch(typeof(StorySystem), "StorySystemOpend")]
            public static class GangPower_StorySystemOpend_Patch
            {
            }
        }
    }
}