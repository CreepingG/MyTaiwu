using System;
using System.Collections.Generic;

public class QuQuBattleSimulation
{
    private readonly QuquBattler[] battlers = new QuquBattler[2];

    private int[] Count;

    public static int ����˥�� = 5;
    
    QuquBattler Single(int color, int part) => GetQuquWindow.instance.MakeQuqu(color, part);

    public QuQuBattleSimulation(int color1, int part1, int color2, int part2, bool weaken)
    {
        battlers[0] = Single(color1, part1);
        battlers[1] = Single(color2, part2);
        ����˥�� = weaken ? 5 : 0;
    }

    public bool showDetail = false;
    string detail = "";

    public string Report()
    {
        var name1 = battlers[0].Name;
        var name2 = battlers[1].Name;
        var result = ShowStartBattleState();
        return $"{name1} vs {name2}, {(result.win ? name1 : name2)} ʤ\n"
            + $"������{Count[0]}, {Count[1]}, {Count[2]}\n"
            + $"����״̬\n"
            + $"    {name1}��{battlers[0].PrintStatus()}\n"
            + $"    {name2}��{battlers[1].PrintStatus()}\n"
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
            return new BattleResult(true, QuquBattler.BattlerStatus.�����޽�);
        }
        else if ((colorId2 != 0 && colorId == 0) || (level2 > level && level2 - level >= 6 && Random.Range(0, 100) < (level2 - level) * 10))
        {
            return new BattleResult(false, QuquBattler.BattlerStatus.�����޽�);
        }
        else if (colorId == 0 && colorId2 == 0)
        {
            return new BattleResult(Random.Range(0, 2) == 0, QuquBattler.BattlerStatus.�����޽�);
        }
        QuquBattleLoop();
        return new BattleResult(battlers[1].IsDead, battlers[1].IsDead ? battlers[1].Status : battlers[0].Status);
    }

    private void QuquBattleLoop()
    {
        do
        {
            Count[0]++;
            if (showDetail) detail += $"�غ�{Count[0]}\n";
            int �ҷ����� = battlers[0].����;
            int �з����� = battlers[1].����;
            bool �ҷ�����;
            if (�ҷ����� > �з�����)
            {
                SetDamage(battlers[1], QuquBattler.DamageTyp.����, �ҷ�����);
                �ҷ����� = Random.Range(0, 100) < 80;
            }
            else if (�з����� > �ҷ�����)
            {
                SetDamage(battlers[0], QuquBattler.DamageTyp.����, �з�����);
                �ҷ����� = Random.Range(0, 100) >= 80;
            }
            else
            {
                �ҷ����� = Random.Range(0, 2) == 0;
            }
            �չ�(�ҷ�����);
        } while (!battlers[0].IsDead && !battlers[1].IsDead);
    }

    private void SetDamage(QuquBattler battler, QuquBattler.DamageTyp typ, int value = 0)
    {
        var change = battler.GetDamage(typ, value);
        if (showDetail && typ!=QuquBattler.DamageTyp.����)
        {
            detail += $"    {battler.Name} {typ}: {change[0]}>>{change[1]}\n";
        }
    }

    private void �չ�(bool �ҷ�����)
    {
        bool �ҷ����� = �ҷ�����;
        for (int i = 0; i < 2; i++)
        {
            Count[1]++;
            if (showDetail) detail += $"  ����{Count[1]}\n";
            QuquBattler ������ = battlers[�ҷ����� ? 0 : 1];
            QuquBattler ������ = battlers[�ҷ����� ? 1 : 0];
            bool �������� = false;
            bool ������ = false;
            bool �������� = false;
            int �˺� = ������.��ǯ;
            if (Random.Range(0, 100) < ������.������)
            {
                �������� = true;
                �˺� += ������.��������;
            }
            if (Random.Range(0, 100) < ������.����)
            {
                ������ = true;
                �˺� = Mathf.Max(0, �˺� - ������.����ֵ);
            }
            �������� = (Random.Range(0, 100) < ������.������);
            if (����(�ҷ�����, !�ҷ�����, �˺�, ��������, ������, ��������)) i = 0; //����й���������һ�����а���
            if (������.IsDead || ������.IsDead) return;
            �ҷ����� = !�ҷ�����; //�����볡
        }
    }

    private bool ����(bool ������, bool ���˷�, int �����˺�, bool ��������, bool ������, bool ��������, bool �Ƿ��� = false, int �������� = 0)
    {
        while(true)
        {
            Count[2]++;
            QuquBattler �˺���Դ = battlers[���˷� ? 1 : 0];
            QuquBattler ������ = battlers[���˷� ? 0 : 1];

            int �����˺� = 0;
            if (�������� | �Ƿ���)
            {
                �����˺� = �˺���Դ.����;
            }
            if (������)
            {
                �����˺� = Mathf.Max(0, �����˺� - ������.����ֵ);
            }
            else if (��������)
            {
                int ������ = �˺���Դ.������ + �˺���Դ.���˵���;
                if (Random.Range(0, 100) < ������)
                {
                    SetDamage(������, QuquBattler.DamageTyp.�;�, 1);
                    if (Random.Range(0, 100) < ������)
                    {
                        SetDamage(������, QuquBattler.DamageTyp.����);
                    }
                }
            }
            SetDamage(������, QuquBattler.DamageTyp.����, �����˺�);
            if (�����˺� > 0)
            {
                SetDamage(������, QuquBattler.DamageTyp.����, �����˺�);
            }

            if (������.IsDead || !��������) return �Ƿ���; //�����β�������������������

            //��һ�ι����ķ����ж�
            bool �������������� = false;
            bool ������������ = false;
            int �����˺� = ������ != ���˷� ? ������.���� : ������.��ǯ;
            if (Random.Range(0, 100) < ������.������)
            {
                �������������� = true;
                �����˺� += ������.��������;
            }
            if (Random.Range(0, 100) < �˺���Դ.����)
            {
                ������������ = true;
                �����˺� = Mathf.Max(0, �����˺� - �˺���Դ.����ֵ);
            }
            bool �������������� = false;
            int �������� = �˺���Դ.������ - �������� * ����˥��;
            if (Random.Range(0, 100) < ��������)
            {
                �������������� = true;
            }
            ���˷� = !���˷�;
            �����˺� = �����˺�;
            �������� = ��������������;
            ������ = ������������;
            �������� = ��������������;
            �Ƿ��� = true;
            ��������++;
        }
    }
}
