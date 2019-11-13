using System;
using System.Collections.Generic;

public class QuQuBattleSimulation
{
    private QuquBattler[] _actorQuquBattlers;

    private QuquBattler[] _enemyQuquBattlers;

    private int winTurn;

    private Queue<Action> nextStep;

    private int turnCount;

    private int attackCount;

    public static int reAttackWeaken = 5;

    /* 修改的部分 */
    QuquBattler[] Single(int color, int part) => new QuquBattler[] { CreateQuquBattler(GetQuquWindow.instance.MakeQuqu(color, part)) };

    public QuQuBattleSimulation(int color1, int part1, int color2, int part2)
    {
        _actorQuquBattlers = Single(color1, part1);
        _enemyQuquBattlers = Single(color2, part2);
    }

    public string Report()
    {
        var name1 = DateFile.instance.GetItemDate(_actorQuquBattlers[0].QuquId, 0);
        var name2 = DateFile.instance.GetItemDate(_enemyQuquBattlers[0].QuquId, 0);
        var result = ShowStartBattleState();
        return $"{name1} vs {name2}, {(result == 1 ? name1 : name2)} wins\n turnCount:{turnCount}, attackCount:{attackCount}";
    }

    public int GetTrueResult()
    {
        return (_actorQuquBattlers[0].Hp > 0
            && _actorQuquBattlers[0].Sp > 0
            && DateFile.instance.GetItemDate(_actorQuquBattlers[0].QuquId, 901).ToInt() > 0)
            ? 1 : 2;
    }
    /* 修改的部分 */

    public QuQuBattleSimulation(int[] actor1Ququ, int[] actor2Ququ)
    {
        _actorQuquBattlers = new QuquBattler[actor1Ququ.Length];
        _enemyQuquBattlers = new QuquBattler[actor2Ququ.Length];
        for (int i = 0; i < 3; i++)
        {
            int ququId = actor1Ququ[i];
            int ququId2 = actor2Ququ[i];
            _actorQuquBattlers[i] = CreateQuquBattler(ququId);
            _enemyQuquBattlers[i] = CreateQuquBattler(ququId2);
        }
    }

    public QuquBattler CreateQuquBattler(int ququId)
    {
        QuquBattler result = default(QuquBattler);
        result.QuquId = ququId;
        result.Hp = GetQuquWindow.instance.GetQuquDate(ququId, 11);
        result.MaxHp = GetQuquWindow.instance.GetQuquDate(ququId, 11);
        result.Sp = GetQuquWindow.instance.GetQuquDate(ququId, 12);
        result.MaxSp = GetQuquWindow.instance.GetQuquDate(ququId, 12);
        result.Level = DateFile.instance.GetItemDate(ququId, 8).ParseInt();
        result.ColorId = DateFile.instance.GetItemDate(ququId, 2002).ParseInt();
        result.PartId = DateFile.instance.GetItemDate(ququId, 2003).ParseInt();
        result.Constitution = GetQuquWindow.instance.GetQuquDate(ququId, 11);
        result.Fighting = GetQuquWindow.instance.GetQuquDate(ququId, 12);
        result.Momentum = GetQuquWindow.instance.GetQuquDate(ququId, 21);
        result.Wrestling = GetQuquWindow.instance.GetQuquDate(ququId, 22);
        result.Pliers = GetQuquWindow.instance.GetQuquDate(ququId, 23);
        result.Fatal = GetQuquWindow.instance.GetQuquDate(ququId, 31);
        result.Damage = GetQuquWindow.instance.GetQuquDate(ququId, 32);
        result.Disability = GetQuquWindow.instance.GetQuquDate(ququId, 36);
        result.Defense = GetQuquWindow.instance.GetQuquDate(ququId, 33);
        result.DamageReduction = GetQuquWindow.instance.GetQuquDate(ququId, 34);
        result.Counterattack = GetQuquWindow.instance.GetQuquDate(ququId, 35);
        result.Durability = DateFile.instance.GetItemDate(ququId, 901).ParseInt();
        result.MaxDurability = DateFile.instance.GetItemDate(ququId, 902).ParseInt();
        return result;
    }

    public bool GetResult()
    {
        winTurn = 0;
        for (int i = 0; i < _actorQuquBattlers.Length; i++)
        {
            var result = ShowStartBattleState(i);
            switch (result)
            {
                case 1:
                    winTurn++;
                    break;
            }
        }
        return winTurn >= 2;
    }

    public int ShowStartBattleState(int index = 0)
    {
        int num = 0;
        int level = _actorQuquBattlers[index].Level;
        int level2 = _enemyQuquBattlers[index].Level;
        int colorId = _actorQuquBattlers[index].ColorId;
        int colorId2 = _enemyQuquBattlers[index].ColorId;
        if ((colorId != 0 && colorId2 == 0) || (level > level2 && level - level2 >= 6 && Random.Range(0, 100) < (level - level2) * 10))
        {
            num = 1;
        }
        else if ((colorId2 != 0 && colorId == 0) || (level2 > level && level2 - level >= 6 && Random.Range(0, 100) < (level2 - level) * 10))
        {
            num = 2;
        }
        else if (colorId == 0 && colorId2 == 0)
        {
            num = 1 + Random.Range(0, 1);
        }
        if (num > 0)
        {
            return num;
        }
        turnCount = attackCount = 0;
        reAttackWeaken = Test.Weaken ? 5 : 0;
        nextStep = new Queue<Action>();
        QuquBattleLoopStart(index);
        while (nextStep.Count > 0)
        {
            var action = nextStep.Dequeue();
            action();
        }
        int result = GetTrueResult();
        Item.Lib.Remove(_actorQuquBattlers[0].QuquId);
        Item.Lib.Remove(_enemyQuquBattlers[0].QuquId);
        return result;
    }

    private bool QuquIsDead(int index)
    {
        bool flag = _actorQuquBattlers[index].Hp <= 0 || _actorQuquBattlers[index].Sp <= 0
            || DateFile.instance.GetItemDate(_actorQuquBattlers[index].QuquId, 901).ToInt() <= 0;
        bool flag2 = _enemyQuquBattlers[index].Hp <= 0 || _enemyQuquBattlers[index].Sp <= 0
            || DateFile.instance.GetItemDate(_enemyQuquBattlers[index].QuquId, 901).ToInt() <= 0;
        return flag | flag2;
    }

    private void QuquBattleLoopStart(int index)
    {
        int momentum = _actorQuquBattlers[index].Momentum;
        int momentum2 = _enemyQuquBattlers[index].Momentum;
        int 先手方 = 0;
        if (momentum > momentum2)
        {
            ShowDamage(index, true, false, 2, momentum, 0.6f, false, false, false, false, false, 0, 0);
            先手方 = ((Random.Range(0, 100) < 80) ? 1 : 2);
        }
        else if (momentum2 > momentum)
        {
            ShowDamage(index, false, true, 2, momentum2, 0.6f, false, false, false, false, false, 0, 0);
            先手方 = ((Random.Range(0, 100) >= 80) ? 1 : 2);
        }
        else
        {
            先手方 = 1 + Random.Range(0, 2);
        }
        if (!QuquIsDead(index))
        {
            switch (先手方)
            {
                case 1:
                    nextStep.Enqueue(() => { QuquBaseAttack(index, 1, newTurn: false); });
                    return;
                case 2:
                    nextStep.Enqueue(() => { QuquBaseAttack(index, 2, newTurn: false); });
                    return;
            }
        }
        return;
    }

    private void QuquBaseAttack(int index, int attacker, bool newTurn)
    {
        turnCount++;
        bool cHit = false;
        bool def = false;
        bool flag = false;
        float num = 0.4f;
        if (attacker == 1)
        {
            int num2 = _actorQuquBattlers[index].Pliers;
            if (Random.Range(0, 100) < _actorQuquBattlers[index].Fatal)
            {
                cHit = true;
                num2 += _actorQuquBattlers[index].Damage;
            }
            if (Random.Range(0, 100) < _enemyQuquBattlers[index].Defense)
            {
                def = true;
                num2 = Mathf.Max(0, num2 - _enemyQuquBattlers[index].DamageReduction);
            }
            flag = (Random.Range(0, 100) < _enemyQuquBattlers[index].Counterattack);
            nextStep.Enqueue(() => { ShowDamage(index, true, false, 1, num2, 0.1f, cHit, def, flag, false, newTurn, 0, 22); });
            return;
        }
        int num3 = _enemyQuquBattlers[index].Pliers;
        if (Random.Range(0, 100) < _enemyQuquBattlers[index].Fatal)
        {
            cHit = true;
            num3 += _enemyQuquBattlers[index].Damage;
        }
        if (Random.Range(0, 100) < _actorQuquBattlers[index].Defense)
        {
            def = true;
            num3 = Mathf.Max(0, num3 - _actorQuquBattlers[index].DamageReduction);
        }
        flag = (Random.Range(0, 100) < _actorQuquBattlers[index].Counterattack);
        nextStep.Enqueue(() => { ShowDamage(index, false, true, 1, num3, 0.1f, cHit, def, flag, false, newTurn, 0, 22); });
        return;
    }

    private void ShowDamage(int index, bool attacker, bool defer, int typ, int damage, float delay, bool cHit, bool def, bool reAttack, bool isReAttack, bool newTurn, int reAttackTurn, int reAttackTyp)
    {
        attackCount++;
        bool cHit2 = false;
        bool def2 = false;
        QuquBattler ququBattler = _actorQuquBattlers[index];
        QuquBattler ququBattler2 = _enemyQuquBattlers[index];
        switch (typ)
        {
            case 1:
                {
                    if (defer)
                    {
                        int num = 0;
                        if (cHit | isReAttack)
                        {
                            num = ququBattler2.Momentum;
                        }
                        if (def)
                        {
                            num = Mathf.Max(0, num - ququBattler.DamageReduction);
                        }
                        else if (cHit)
                        {
                            int num2 = ququBattler2.Fatal + ququBattler2.Disability;
                            if (Random.Range(0, 100) < num2)
                            {
                                DateFile.instance.ChangeItemHp(DateFile.instance.MianActorID(), ququBattler.QuquId, -1);
                                if (Random.Range(0, 100) < num2)
                                {
                                    GetQuquWindow.instance.QuquAddInjurys(ququBattler.QuquId);
                                }
                            }
                        }
                        _actorQuquBattlers[index].Hp -= damage;
                        ShowDebug(_actorQuquBattlers[index], 1, damage);
                        if (num > 0)
                        {
                            _actorQuquBattlers[index].Sp -= num;
                            ShowDebug(_actorQuquBattlers[index], 2, num);
                        }
                        if (QuquIsDead(index))
                        {
                            return;
                        }
                        if (reAttack)
                        {
                            int num3 = (reAttackTyp == 22) ? ququBattler.Wrestling : ququBattler.Pliers;
                            if (Random.Range(0, 100) < ququBattler.Fatal)
                            {
                                cHit2 = true;
                                num3 += ququBattler.Damage;
                            }
                            if (Random.Range(0, 100) < ququBattler2.Defense)
                            {
                                def2 = true;
                                num3 = Mathf.Max(0, num3 - ququBattler2.DamageReduction);
                            }
                            bool reAttack2 = false;
                            int num4 = ququBattler2.Counterattack - reAttackTurn * reAttackWeaken;
                            if (Random.Range(0, 100) < num4)
                            {
                                reAttack2 = true;
                            }
                            nextStep.Enqueue(() => { ShowDamage(index, attacker, false, 1, num3, 0.1f, cHit2, def2, reAttack2, true, false, reAttackTurn+1, (reAttackTyp == 22) ? 23 : 22); });
                            return;
                        }
                        if (attacker)
                        {
                            if (newTurn)
                            {
                                nextStep.Enqueue(() => { QuquBattleLoopStart(index); });
                                return;
                            }
                            nextStep.Enqueue(() => { QuquBaseAttack(index, 2, newTurn: true); });
                            return;
                        }
                        if (newTurn)
                        {
                            nextStep.Enqueue(() => { QuquBattleLoopStart(index); });
                            return;
                        }
                        nextStep.Enqueue(() => { QuquBaseAttack(index, 1, newTurn: true); });
                        return;
                    }
                    int num6 = 0;
                    if (cHit | isReAttack)
                    {
                        num6 = ququBattler.Momentum;
                    }
                    if (def)
                    {
                        num6 = Mathf.Max(0, num6 - ququBattler2.DamageReduction);
                    }
                    else if (cHit)
                    {
                        int num7 = ququBattler.Fatal + ququBattler.Disability;
                        if (Random.Range(0, 100) < num7)
                        {
                            DateFile.instance.ChangeItemHp(DateFile.instance.MianActorID(), ququBattler2.QuquId, -1);
                            if (Random.Range(0, 100) < num7)
                            {
                                GetQuquWindow.instance.QuquAddInjurys(ququBattler2.QuquId);
                            }
                        }
                    }
                    _enemyQuquBattlers[index].Hp -= damage;
                    ShowDebug(_enemyQuquBattlers[index], 1, damage);
                    if (num6 > 0)
                    {
                        _enemyQuquBattlers[index].Sp -= num6;
                        ShowDebug(_enemyQuquBattlers[index], 2, num6);
                    }
                    if (QuquIsDead(index))
                    {
                        return;
                    }
                    if (reAttack)
                    {
                        int num8 = (reAttackTyp == 22) ? ququBattler2.Wrestling : ququBattler2.Pliers;
                        if (Random.Range(0, 100) < ququBattler2.Fatal)
                        {
                            cHit2 = true;
                            num8 += ququBattler2.Damage;
                        }
                        if (Random.Range(0, 100) < ququBattler.Defense)
                        {
                            def2 = true;
                            num8 = Mathf.Max(0, num8 - ququBattler.DamageReduction);
                        }
                        bool reAttack3 = false;
                        int num9 = ququBattler.Counterattack - reAttackTurn * reAttackWeaken;
                        if (Random.Range(0, 100) < num9)
                        {
                            reAttack3 = true;
                        }
                        nextStep.Enqueue(() => { ShowDamage(index, attacker, true, 1, num8, 0.1f, cHit2, def2, reAttack3, true, false, reAttackTurn+1, (reAttackTyp == 22) ? 23 : 22); });
                        return;
                    }
                    if (attacker)
                    {
                        if (newTurn)
                        {
                            nextStep.Enqueue(() => { QuquBattleLoopStart(index); });
                            return;
                        }

                        nextStep.Enqueue(() => { QuquBaseAttack(index, 2, newTurn: true); });
                        return;
                    }
                    if (newTurn)
                    {
                        nextStep.Enqueue(() => { QuquBattleLoopStart(index); });
                        return;
                    }

                    nextStep.Enqueue(() => { QuquBaseAttack(index, 1, newTurn: true); });
                    return;
                }
            case 2:
                if (defer)
                {
                    _actorQuquBattlers[index].Sp -= damage;
                    ShowDebug(_actorQuquBattlers[index], typ, damage);
                }
                else
                {
                    _enemyQuquBattlers[index].Sp -= damage;
                    ShowDebug(_enemyQuquBattlers[index], typ, damage);
                }
                break;
        }
        return;
    }

    private void ShowDebug(QuquBattler battler, int type, int damage)
    {
    }
}
