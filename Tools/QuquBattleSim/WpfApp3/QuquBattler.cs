public class QuquBattler
{
    public string PrintStatus()
    {
        return $"耐力 {Hp}/{MaxHp}, "
            + $"斗性 {Sp}/{MaxSp}, "
            + $"耐久 {Durability}/{MaxDurability}";
    }
    
    public enum BattlerStatus
    {
        存活, 无牙无叫, 耐力, 斗性, 耐久
    }

    public enum DamageTyp
    {
        耐力, 斗性, 耐久, 属性
    }

    public int[] GetDamage(DamageTyp typ, int value = 0)
    {
        var result = new int[2];
        switch (typ)
        {
            case DamageTyp.耐力:
                result[0] = Hp;
                Hp -= value;
                result[1] = Hp;
                if (Hp <= 0) Status = BattlerStatus.耐力;
                break;
            case DamageTyp.斗性:
                result[0] = Sp;
                Sp -= value;
                result[1] = Sp;
                if (Sp <= 0) Status = BattlerStatus.斗性;
                break;
            case DamageTyp.耐久:
                result[0] = Durability;
                Durability -= value;
                result[1] = Durability;
                if (Durability <= 0) Status = BattlerStatus.耐久;
                break;
            case DamageTyp.属性:
                AddInjury();
                break;
        }
        return result;
    }

    public static QuquBattler Create(Item item)
    {
        var colorId = item[2002];
        var partId = item[2003];
        QuquBattler result = new QuquBattler
        {
            Status = BattlerStatus.存活,
            Hp = GetQuquWindow.instance.GetQuquDate(colorId, partId, 11),
            MaxHp = GetQuquWindow.instance.GetQuquDate(colorId, partId, 11),
            Sp = GetQuquWindow.instance.GetQuquDate(colorId, partId, 12),
            MaxSp = GetQuquWindow.instance.GetQuquDate(colorId, partId, 12),
            Level = DateFile.instance.GetItemDate<int>(item, 8),
            ColorId = DateFile.instance.GetItemDate<int>(item, 2002),
            PartId = DateFile.instance.GetItemDate<int>(item, 2003),
            气势 = GetQuquWindow.instance.GetQuquDate(colorId, partId, 21),
            角力 = GetQuquWindow.instance.GetQuquDate(colorId, partId, 22),
            牙钳 = GetQuquWindow.instance.GetQuquDate(colorId, partId, 23),
            暴击率 = GetQuquWindow.instance.GetQuquDate(colorId, partId, 31),
            暴击增伤 = GetQuquWindow.instance.GetQuquDate(colorId, partId, 32),
            击伤调整 = GetQuquWindow.instance.GetQuquDate(colorId, partId, 36),
            格挡率 = GetQuquWindow.instance.GetQuquDate(colorId, partId, 33),
            格挡数值 = GetQuquWindow.instance.GetQuquDate(colorId, partId, 34),
            反击率 = GetQuquWindow.instance.GetQuquDate(colorId, partId, 35),
            Durability = DateFile.instance.GetItemDate<int>(item, 901),
            MaxDurability = DateFile.instance.GetItemDate<int>(item, 902),
            Name = DateFile.instance.GetQuquName(colorId, partId),
    };
        return result;
    }

    void AddInjury()
    {
        if (Random.Range(0, 100) >= 35)
        {
            if (Random.Range(0, 2) == 0)
            {
                MaxHp -= 5;
            }
            else
            {
                MaxSp -= 5;
            }
        }
        else
        {
            var rand = Random.Range(0, 3);
            switch (rand)
            {
                case 0:
                    牙钳 -= 1;
                    break;
                case 1:
                    气势 -= 1;
                    break;
                case 2:
                    角力 -= 1;
                    break;
            }
        }
    }

    public bool IsDead
    {
        get => Status != BattlerStatus.存活;
    }

    public BattlerStatus Status;//死因

    public string Name;

    public int Level;

    public int ColorId;

    public int PartId;

    public int 气势;

    public int 角力;

    public int 牙钳;

    public int 暴击率;

    public int 暴击增伤;

    public int 击伤调整;

    public int 格挡率;

    public int 格挡数值;

    public int 反击率;

    int MaxHp;

    int Hp;

    int MaxSp;

    int Sp;

    int Durability;

    int MaxDurability;
}
