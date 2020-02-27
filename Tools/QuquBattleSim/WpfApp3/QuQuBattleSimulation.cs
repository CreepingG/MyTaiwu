using System;
using System.Collections.Generic;

public class QuQuBattleSimulation
{
    private readonly QuquBattler[] battlers = new QuquBattler[2];

    private int[] Count;

    public static int 反击衰减 = 5;
    
    QuquBattler Single(int color, int part) => GetQuquWindow.instance.MakeQuqu(color, part);

    public QuQuBattleSimulation(int color1, int part1, int color2, int part2, bool weaken)
    {
        battlers[0] = Single(color1, part1);
        battlers[1] = Single(color2, part2);
        反击衰减 = weaken ? 5 : 0;
    }

    public bool showDetail = false;
    string detail = "";

    public string Report()
    {
        var name1 = battlers[0].Name;
        var name2 = battlers[1].Name;
        var result = ShowStartBattleState();
        return $"{name1} vs {name2}, {(result.win ? name1 : name2)} 胜\n"
            + $"计数：{Count[0]}, {Count[1]}, {Count[2]}\n"
            + $"结束状态\n"
            + $"    {name1}：{battlers[0].PrintStatus()}\n"
            + $"    {name2}：{battlers[1].PrintStatus()}\n"
            + detail;
    }

    public struct BattleResult
    {
        public bool win;
        public QuquBattler.BattlerStatus status;
        public BattleResult(bool win, QuquBattler.BattlerStatus status)
        {
            this.win = win;
            this.status = status;
        }
    }

    public BattleResult ShowStartBattleState()
    {
        int level = battlers[0].Level;
        int level2 = battlers[1].Level;
        int colorId = battlers[0].ColorId;
        int colorId2 = battlers[1].ColorId;
        Count = new int[3];
        if ((colorId != 0 && colorId2 == 0) || (level > level2 && level - level2 >= 6 && Random.Range(0, 100) < (level - level2) * 10))
        {
            return new BattleResult(true, QuquBattler.BattlerStatus.无牙无叫);
        }
        else if ((colorId2 != 0 && colorId == 0) || (level2 > level && level2 - level >= 6 && Random.Range(0, 100) < (level2 - level) * 10))
        {
            return new BattleResult(false, QuquBattler.BattlerStatus.无牙无叫);
        }
        else if (colorId == 0 && colorId2 == 0)
        {
            return new BattleResult(Random.Range(0, 2) == 0, QuquBattler.BattlerStatus.无牙无叫);
        }
        QuquBattleLoop();
        return new BattleResult(battlers[1].IsDead, battlers[1].IsDead ? battlers[1].Status : battlers[0].Status);
    }

    private void QuquBattleLoop()
    {
        do
        {
            Count[0]++;
            if (showDetail) detail += $"回合{Count[0]}\n";
            int 我方气势 = battlers[0].气势;
            int 敌方气势 = battlers[1].气势;
            bool 我方先手;
            if (我方气势 > 敌方气势)
            {
                SetDamage(battlers[1], QuquBattler.DamageTyp.斗性, 我方气势);
                我方先手 = Random.Range(0, 100) < 80;
            }
            else if (敌方气势 > 我方气势)
            {
                SetDamage(battlers[0], QuquBattler.DamageTyp.斗性, 敌方气势);
                我方先手 = Random.Range(0, 100) >= 80;
            }
            else
            {
                我方先手 = Random.Range(0, 2) == 0;
            }
            普攻(我方先手);
        } while (!battlers[0].IsDead && !battlers[1].IsDead);
    }

    private void SetDamage(QuquBattler battler, QuquBattler.DamageTyp typ, int value = 0)
    {
        var change = battler.GetDamage(typ, value);
        if (showDetail && typ!=QuquBattler.DamageTyp.属性)
        {
            detail += $"    {battler.Name} {typ}: {change[0]}>>{change[1]}\n";
        }
    }

    private void 普攻(bool 我方先手)
    {
        bool 我方进攻 = 我方先手;
        for (int i = 0; i < 2; i++)
        {
            Count[1]++;
            if (showDetail) detail += $"  进攻{Count[1]}\n";
            QuquBattler 进攻者 = battlers[我方进攻 ? 0 : 1];
            QuquBattler 防守者 = battlers[我方进攻 ? 1 : 0];
            bool 触发暴击 = false;
            bool 触发格挡 = false;
            bool 触发反击 = false;
            int 伤害 = 进攻者.牙钳;
            if (Random.Range(0, 100) < 进攻者.暴击率)
            {
                触发暴击 = true;
                伤害 += 进攻者.暴击增伤;
            }
            if (Random.Range(0, 100) < 防守者.格挡率)
            {
                触发格挡 = true;
                伤害 = Mathf.Max(0, 伤害 - 防守者.格挡数值);
            }
            触发反击 = (Random.Range(0, 100) < 防守者.反击率);
            if (缠斗(我方进攻, !我方进攻, 伤害, 触发暴击, 触发格挡, 触发反击)) i = 0; //如果有过暴击，则一定还有半轮
            if (进攻者.IsDead || 防守者.IsDead) return;
            我方进攻 = !我方进攻; //交换半场
        }
    }

    private bool 缠斗(bool 进攻方, bool 受伤方, int 耐力伤害, bool 触发暴击, bool 触发格挡, bool 触发反击, bool 是反击 = false, int 反击计数 = 0)
    {
        while(true)
        {
            Count[2]++;
            QuquBattler 伤害来源 = battlers[受伤方 ? 1 : 0];
            QuquBattler 受伤者 = battlers[受伤方 ? 0 : 1];

            int 斗性伤害 = 0;
            if (触发暴击 | 是反击)
            {
                斗性伤害 = 伤害来源.气势;
            }
            if (触发格挡)
            {
                斗性伤害 = Mathf.Max(0, 斗性伤害 - 受伤者.格挡数值);
            }
            else if (触发暴击)
            {
                int 击伤率 = 伤害来源.暴击率 + 伤害来源.击伤调整;
                if (Random.Range(0, 100) < 击伤率)
                {
                    SetDamage(受伤者, QuquBattler.DamageTyp.耐久, 1);
                    if (Random.Range(0, 100) < 击伤率)
                    {
                        SetDamage(受伤者, QuquBattler.DamageTyp.属性);
                    }
                }
            }
            SetDamage(受伤者, QuquBattler.DamageTyp.耐力, 耐力伤害);
            if (斗性伤害 > 0)
            {
                SetDamage(受伤者, QuquBattler.DamageTyp.斗性, 斗性伤害);
            }

            if (受伤者.IsDead || !触发反击) return 是反击; //若本次不触发反击，缠斗结束

            //下一次攻击的反击判定
            bool 反击将触发暴击 = false;
            bool 反击将触发格挡 = false;
            int 反击伤害 = 进攻方 != 受伤方 ? 受伤者.角力 : 受伤者.牙钳;
            if (Random.Range(0, 100) < 受伤者.暴击率)
            {
                反击将触发暴击 = true;
                反击伤害 += 受伤者.暴击增伤;
            }
            if (Random.Range(0, 100) < 伤害来源.格挡率)
            {
                反击将触发格挡 = true;
                反击伤害 = Mathf.Max(0, 反击伤害 - 伤害来源.格挡数值);
            }
            bool 反击将触发反击 = false;
            int 反击概率 = 伤害来源.反击率 - 反击计数 * 反击衰减;
            if (Random.Range(0, 100) < 反击概率)
            {
                反击将触发反击 = true;
            }
            受伤方 = !受伤方;
            耐力伤害 = 反击伤害;
            触发暴击 = 反击将触发暴击;
            触发格挡 = 反击将触发格挡;
            触发反击 = 反击将触发反击;
            是反击 = true;
            反击计数++;
        }
    }
}
