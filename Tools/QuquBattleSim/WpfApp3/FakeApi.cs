using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;

class QuquData
{
    public Dictionary<int, Dictionary<int, string>> Source;
    public static QuquData instance = new QuquData();
    QuquData()
    {
        StreamReader sr = new StreamReader("Cricket_Date.txt", Encoding.UTF8);
        Source = new Dictionary<int, Dictionary<int, string>>();
        var keys = sr.ReadLine().Split(',');
        while(!sr.EndOfStream)
        {
            var arr = sr.ReadLine().Split(',');
            Source.Add
            (
                arr[0].ParseInt(), 
                Enumerable.Zip(keys, arr, (k, v) => new KeyValuePair<int, string>(k== "#" ? -1 : k.ParseInt(), v))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            );
        }
    }
}

class Item:Dictionary<int, string>
{
    public static Dictionary<int, Item> Lib = new Dictionary<int, Item>();
}

class Items
{
    public static void SetItemProperty(int itemId, int key, string value)
    {
        if (!Item.Lib.ContainsKey(itemId))
        {
            Item.Lib.Add(itemId, new Item {
                { 2001, "1" }
            });
        }
        Item.Lib[itemId][key] = value;
    }
}

class GetQuquWindow
{
    static public GetQuquWindow instance = new GetQuquWindow();

    public int GetQuquDate(int itemId, int index, bool injurys = true, bool attrAdd = true)
    {
        var item = Item.Lib[itemId];
        var colorId = item[2002].ParseInt();
        var partId = item[2003].ParseInt();
        int colorValue = QuquData.instance.Source[colorId][index].ParseInt();
        int partValue = (partId > 0) ? QuquData.instance.Source[partId][index].ParseInt() : 0;
        int result =  colorValue + partValue;
        if (injurys)
        {
            string[] array = DateFile.instance.GetItemDate(itemId, 2004).Split('|');
            for (int i = 0; i < array.Length; i++)
            {
                int injuryIndex = int.Parse(array[i]);
                if (injuryIndex != 0 && injuryIndex == index)
                {
                    result = Mathf.Max(result * 30 / 100, result - ((index != 11 && index != 12) ? 1 : 5));
                }
            }
        }
        return result;
    }

    public void QuquAddInjurys(int itemId)
    {
        string itemDate = DateFile.instance.GetItemDate(itemId, 2004);
        itemDate = ((Random.Range(0, 100) >= 35) ? (itemDate + "|" + (11 + Random.Range(0, 2))) : (itemDate + "|" + (21 + Random.Range(0, 3))));
        Items.SetItemProperty(itemId, 2004, itemDate);
    }

    public void MakeQuqu(int itemId, int colorId, int partId)
    {
        Items.SetItemProperty(itemId, 2002, colorId.ToString());
        Items.SetItemProperty(itemId, 2003, partId.ToString());
        int maxHp = int.Parse(DateFile.instance.GetItemDate(itemId, 8)) + GetQuquDate(itemId, 11) / 20;
        maxHp += Random.Range(-(maxHp * 35 / 100), maxHp * 35 / 100 + 1);
        Items.SetItemProperty(itemId, 902, maxHp.ToString());
        Items.SetItemProperty(itemId, 901, maxHp.ToString());
        Items.SetItemProperty(itemId, 2007, (GetQuquDate(itemId, 98) * Random.Range(0, 21) / 100).ToString());
        for (int i = 0; i < Mathf.Min(3, maxHp - 1); i++)
        {
            if (Random.Range(0, 100) < 10)
            {
                DateFile.instance.ChangeItemHp(DateFile.instance.MianActorID(), itemId, -1);
                QuquAddInjurys(itemId);
            }
        }
    }

    public int MakeQuqu(int colorId, int partId)
    {
        var itemId = Random.Int();
        MakeQuqu(itemId, colorId, partId);
        return itemId;
    }
}

class DateFile
{
    static public DateFile instance = new DateFile();

    public string GetQuquName(int colorId, int partId)
    {
        var cricketDate = QuquData.instance.Source;
        try
        {
            return ((partId <= 0) ? cricketDate[colorId][0] : ((int.Parse(cricketDate[colorId][2]) >= int.Parse(cricketDate[partId][2])) ? (cricketDate[colorId][0].Split('|')[0] + cricketDate[partId][0]) : (cricketDate[partId][0] + cricketDate[colorId][0].Split('|')[1])));
        }
        catch(Exception e)
        {
            return "??";
        }
    }

    public string GetItemDate(int id, int index, bool other = true)
    {
        var item = Item.Lib[id];
        string result = item.ContainsKey(index) ? item[index] : "0";
        if (!other) return result;
        if (GetItemDate(id, 2001, false) == "1")
        {
            int colorId = item[2002].ToInt();
            int partId = item[2003].ToInt();
            var cricketDate = QuquData.instance.Source;
            switch (index)
            {
                case 0:
                    result = ((partId <= 0) ? cricketDate[colorId][0] : ((int.Parse(cricketDate[colorId][2]) >= int.Parse(cricketDate[partId][2])) ? (cricketDate[colorId][0].Split('|')[0] + cricketDate[partId][0]) : (cricketDate[partId][0] + cricketDate[colorId][0].Split('|')[1])));
                    break;
                case 8:
                    result = GetQuquWindow.instance.GetQuquDate(id, 1).ToString();
                    break;
                case 98:
                    result = cricketDate[colorId][97];
                    break;
                case 99:
                    result = ((partId > 0) ? cricketDate[partId][99] : cricketDate[colorId][99]);
                    break;
                case 904:
                    result = GetQuquWindow.instance.GetQuquDate(id, 94).ToString();
                    break;
                case 905:
                    result = GetQuquWindow.instance.GetQuquDate(id, 95).ToString();
                    break;
                case 52001:
                    result = GetQuquWindow.instance.GetQuquDate(id, index).ToString();
                    break;
                case 52002:
                    result = GetQuquWindow.instance.GetQuquDate(id, index).ToString();
                    break;
                case 52003:
                    result = GetQuquWindow.instance.GetQuquDate(id, index).ToString();
                    break;
                case 52004:
                    result = GetQuquWindow.instance.GetQuquDate(id, index).ToString();
                    break;
                case 52005:
                    result = GetQuquWindow.instance.GetQuquDate(id, index).ToString();
                    break;
                case 52006:
                    result = GetQuquWindow.instance.GetQuquDate(id, index).ToString();
                    break;
                case 52007:
                    result = GetQuquWindow.instance.GetQuquDate(id, index).ToString();
                    break;
            }
        }
        return result;
    }

    public void ChangeItemHp(int actorId, int itemId, int hpValue, int maxHpValue = 0, bool removeItem = true, int loseType = 1)
    {
        var item = Item.Lib[itemId];
        int newMaxHp = Mathf.Max(0, int.Parse(GetItemDate(itemId, 902)) + maxHpValue);
        int newHp = Mathf.Clamp(int.Parse(GetItemDate(itemId, 901)) + hpValue, 0, newMaxHp);
        item[901] = newHp.ToString();
        item[902] = newMaxHp.ToString();
    }

    public int MianActorID() => 0;
}

static class Utils
{
    public static int ParseInt(this string intStr)
    {
        return Convert.ToInt32(intStr);
    }
    public static int ToInt(this string intStr)
    {
        try
        {
            return Convert.ToInt32(intStr);
        }
        catch (Exception e)
        {
            return 0;
        }
    }
    public static string String<T1, T2>(this Dictionary<T1,T2> dict)
    {
        var result = "{\n";
        foreach (var kvp in dict)
        {
            result += $"  {kvp.Key} : {kvp.Value}\n";
        }
        result += "}";
        return result;
    }
}

static class Random
{
    static byte[] Buffer;
    static System.Random rd = new System.Random();

    static void Rebuild()
    {
        rd = new System.Random((int)DateTime.Now.Ticks * rd.Next());
    }

    public static int Range(int min, int max)
    {
        Rebuild();
        return rd.Next(min, max);
    }

    public static int Int()
    {
        Buffer = Guid.NewGuid().ToByteArray();
        return BitConverter.ToInt32(Buffer, 0);
    }
}

static class Mathf
{
    public static Func<int, int, int> Max = Math.Max;
    public static Func<int, int, int> Min = Math.Min;
    public static int Clamp(int value, int min, int max) => Math.Min(Math.Max(value, min), max);
}