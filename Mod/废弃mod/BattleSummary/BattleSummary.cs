using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;

namespace BattleSummary
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
    }
    public enum EventTyp
    {
        CommonAttack,//普攻
        GongFa,//摧破
        ReAttack,//反击
        Echo,//反震
        Poison,//毒
        OtherFPower,//别离步撕伤口等
    }
    public class DamageEvent
    {
        public bool isActor;
        public int value;
        public int qiValue;
        public EventTyp eventTyp;
        public int id = 0;//功法id或毒id，普攻为0
        public int time;
        public DamageEvent(int time, bool isActor, EventTyp eventTyp, int id = 0, int value = 0, int qiValue = 0)
        {
            this.time = time;
            this.isActor = isActor;
            this.eventTyp = eventTyp;
            this.value = value;
            this.qiValue = qiValue;
            this.id = id;
        }
    }
    public static class BattleRecord
    {
        struct Action
        {
            public readonly bool isActor;
            public readonly EventTyp eventTyp;
            //public readonly int power;
            //public readonly int id;
            public Action(EventTyp eventTyp, bool isActor/*,int power = 100, int id = 0*/)
            {
                this.eventTyp = eventTyp;
                this.isActor = isActor;
                //this.power = power;
                //this.id = id;
            }
        }
        public readonly static Dictionary<EventTyp, string> eventTypName = new Dictionary<EventTyp, string>
        {
            {EventTyp.CommonAttack,"普攻" },
            {EventTyp.GongFa,"功法" },
            {EventTyp.ReAttack,"反击" },
            {EventTyp.Echo ,"反震" },
            {EventTyp.Poison ,"毒" },
            {EventTyp.OtherFPower,"其他" },
        };
        static List<DamageEvent> damageEvents = new List<DamageEvent>();
        static Stack<Action> actions = new Stack<Action>();
        static int updateCnt = 0;
        static List<string> infos = new List<string>();

        [HarmonyPatch(typeof(BattleSystem), "Update")]
        public static class BattleSummary_Update_Patch
        {
            static void Prefix()
            {
                if (!Main.enabled) return;
                updateCnt++;
                /*var nowAction = actions.Peek();
                string text = $"{updateCnt}: {eventTypName[nowAction.eventTyp]}";
                TipsWindow.instance.SetTips(0, new string[]
                {
                    text
                }, 60, -755f, -380f, 450, 150);*/
                int cnt = infos.Count;
                if (cnt > 0)
                {
                    string text = "";
                    int i = Mathf.Min(cnt, 10);
                    for (; i != 0; i--)
                    {
                        text += infos[cnt - i] + "\n";
                    }
                    TipsWindow.instance.SetTips(0, new string[]
                    {
                        text
                    }, 60, -900f, -200f, 450, 300);
                }
            }
        }
        [HarmonyPatch(typeof(DateFile), "PlayeAttackSE")]
        public static class BattleSummary_PlayeAttackSE_Patch
        {
            static void Prefix(int index)
            {
                if (!Main.enabled) return;
                infos.Add($"{updateCnt} PlayeAttackSE {index}");
            }
        }
        [HarmonyPatch(typeof(DateFile), "PlayeBattleHitSE")]
        public static class BattleSummary_PlayeBattleHitSE_Patch
        {
            static void Prefix(int index)
            {
                if (!Main.enabled) return;
                infos.Add($"{updateCnt} PlayeBattleHitSE {index}");
            }
        }


        [HarmonyPatch(typeof(BattleSystem), "Start")]
        public static class BattleSummary_Start_Patch
        {
            static void Prefix()
            {
                updateCnt = 0;
                damageEvents = new List<DamageEvent>();
                actions = new Stack<Action>();
            }
        }
        /*
        static int GetAttackPower(bool isActor)
        {
            return (int)typeof(BattleSystem).GetField(isActor ? "actorAttackPower" : "enemyAttackPower",
                    BindingFlags.NonPublic | BindingFlags.Instance).GetValue(BattleSystem.instance);
        }
        [HarmonyPatch(typeof(BattleSystem), "ActionEventAttack")]
        public static class BattleSummary_ActionEventAttack_Patch
        {
            static void Prefix(BattleSystem __instance, bool isActor)
            {
                actions.Push(new Action(EventTyp.CommonAttack, isActor));
            }
            static void Postfix()
            {
                actions.Pop();
            }
        }
        static int UsingGongFa(bool isActor)
        {
            return (int)typeof(BattleSystem).GetField(isActor ? "actorNowUseingGongFa" : "enemyNowUseingGongFa",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(BattleSystem.instance);
        }
        [HarmonyPatch(typeof(BattleSystem), "ActionEventDefWindow")]
        public static class BattleSummary_ActionEventDefWindow_Patch
        {
            static void Prefix(BattleSystem __instance, bool isActor, int defTyp)
            {
                if (defTyp == 1)
                {
                    actions.Push(new Action(EventTyp.GongFa, isActor));
                    damageEvents.Add(new DamageEvent(updateCnt, isActor, EventTyp.GongFa, UsingGongFa(isActor)));
                }
            }
            static void Postfix(BattleSystem __instance, bool isActor, int defTyp)
            {
                if (defTyp == 4)
                {
                    actions.Pop();
                }
            }
        }
        [HarmonyPatch(typeof(BattleSystem), "SetDamage")]
        public static class BattleSummary_SetDamage_Patch
        {
            static void Postfix(BattleSystem __instance, bool isActor, int damageTyp, int injuryId, int injuryPower, int deferKey, int attackerKey)
            {
                echoId = 0;
                FPowerId = 0;
            }
        }
        static int[] GetInjuryDamage(int injuryId, int injuryPower)
        {
            var injury = DateFile.instance.injuryDate[injuryId];
            int[] injuryBase = { int.Parse(injury[1]), int.Parse(injury[2]) };
            return new int[] { injuryBase[0] * injuryPower / 100, injuryBase[1] * injuryPower / 100 };
        }
        [HarmonyPatch(typeof(BattleSystem), "AddBattleInjury")]
        public static class BattleSummary_AddBattleInjury_Patch
        {//InvalidOperationException: Sequence contains no elements
            static void Prefix(BattleSystem __instance, bool isActor, int actorId, int injuryId, int injuryPower)
            {
                isActor = !isActor;
                int[] damage = GetInjuryDamage(injuryId, injuryPower);
                if (echoId != 0)
                {
                    damageEvents.Add(new DamageEvent(updateCnt, isActor, EventTyp.Echo, echoId, damage[0], damage[1]));
                    return;
                }
                if (FPowerId != 0)
                {
                    damageEvents.Add(new DamageEvent(updateCnt, isActor, EventTyp.OtherFPower, FPowerId, damage[0], damage[1]));
                    return;
                }
                var nowAction = actions.Peek();
                var lastDamage = damageEvents.Last();
                if (nowAction.eventTyp == EventTyp.GongFa && nowAction.isActor == isActor)
                {
                    lastDamage.value = damage[0];
                    lastDamage.qiValue = damage[1];
                }
                else if(nowAction.eventTyp == EventTyp.CommonAttack && nowAction.isActor == isActor)
                {
                    lastDamage.value = damage[0];
                    lastDamage.qiValue = damage[1];
                }
                else
                {
                    damageEvents.Add(new DamageEvent(updateCnt, isActor, nowAction.eventTyp, UsingGongFa(isActor), damage[0], damage[1]));
                }
                
            }
        }
        static int echoId = 0;
        static int FPowerId = 0;
        [HarmonyPatch(typeof(BattleSystem), "GetGongFaFEffect")]
        public static class BattleSummary_GetGongFaFEffect_Patch
        {
            static void Prefix(BattleSystem __instance, bool __result, int effectId, bool isActor)
            {
                bool isGuiShenXia = effectId == 30009 || effectId == 40009;
                if(isGuiShenXia && __result == true)
                {
                    FPowerId = effectId;
                }
            }
        }
        [HarmonyPatch(typeof(BattleVaule), "GetAttackDef")]
        public static class BattleSummary_GetAttackDef_Patch
        {
            static void Prefix(BattleVaule __instance, bool isActor, int gongFaId)
            {
                if (gongFaId > 0)
                {
                    echoId = gongFaId;
                }
            }
        }
        [HarmonyPatch(typeof(BattleSystem), "ShowBattleState")]
        public static class BattleSummary_ShowBattleState_Patch
        {
            static void Prefix(BattleSystem __instance, int stateId, bool isActor)
            {
                if (stateId == 10302 || stateId == 10304)
                {
                    var lastEvent = damageEvents.Last();
                    lastEvent.eventTyp = EventTyp.OtherFPower;
                    lastEvent.id = stateId;
                }
            }
        }*/
    }
    
}