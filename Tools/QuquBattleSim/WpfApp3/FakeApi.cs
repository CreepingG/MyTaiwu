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

public class Item:Dictionary<int, int>
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
                { 2001, 1 }
            });
        }
        Item.Lib[itemId][key] = value.ToInt();
    }
}

class GetQuquWindow
{
    static public GetQuquWindow instance = new GetQuquWindow();

    public int GetQuquDate(int colorId, int partId, int index)
    {
        int colorValue = QuquData.instance.Source[colorId][index].ParseInt();
        int partValue = (partId > 0) ? QuquData.instance.Source[partId][index].ParseInt() : 0;
        int result =  colorValue + partValue;
        return result;
    }

    public QuquBattler MakeQuqu(int colorId, int partId)
    {
        int maxHp = GetQuquDate(colorId, partId, 1) + GetQuquDate(colorId, partId, 11) / 20;
        maxHp += Random.Range(-(maxHp * 35 / 100), maxHp * 35 / 100 + 1);
        var item = new Item
        {
            [2002] = colorId,
            [2003] = partId,
            [902] = maxHp,
            [901] = maxHp,
            [2007] = GetQuquDate(colorId, partId, 98) * Random.Range(0, 21) / 100,
        };

        var battler = QuquBattler.Create(item);
        for (int i = 0; i < Mathf.Min(3, maxHp - 1); i++)
        {
            if (Random.Range(0, 100) < 10)
            {
                battler.GetDamage(3, 1);
                battler.GetDamage(4);
            }
        }
        return battler;
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

    public T GetItemDate<T>(Item item, int index, bool other = true)
    {
        int value = item.ContainsKey(index) ? item[index] : 0;
        string result = "";
        if (!other) return (T)Convert.ChangeType(value, typeof(T));
        if (GetItemDate<int>(item, 2001, false) == 1)
        {
            int colorId = item[2002];
            int partId = item[2003];
            var cricketDate = QuquData.instance.Source;
            switch (index)
            {
                case 0:
                    result = GetQuquName(colorId,partId);
                    break;
                case 8:
                    value = GetQuquWindow.instance.GetQuquDate(colorId, partId, 1);
                    break;
                case 98:
                    result = cricketDate[colorId][97];
                    break;
                case 99:
                    result = ((partId > 0) ? cricketDate[partId][99] : cricketDate[colorId][99]);
                    break;
                case 904:
                    value = GetQuquWindow.instance.GetQuquDate(colorId, partId, 94);
                    break;
                case 905:
                    value = GetQuquWindow.instance.GetQuquDate(colorId, partId, 95);
                    break;
                case 52001:
                    value = GetQuquWindow.instance.GetQuquDate(colorId, partId, index);
                    break;
                case 52002:
                    value = GetQuquWindow.instance.GetQuquDate(colorId, partId, index);
                    break;
                case 52003:
                    value = GetQuquWindow.instance.GetQuquDate(colorId, partId, index);
                    break;
                case 52004:
                    value = GetQuquWindow.instance.GetQuquDate(colorId, partId, index);
                    break;
                case 52005:
                    value = GetQuquWindow.instance.GetQuquDate(colorId, partId, index);
                    break;
                case 52006:
                    value = GetQuquWindow.instance.GetQuquDate(colorId, partId, index);
                    break;
                case 52007:
                    value = GetQuquWindow.instance.GetQuquDate(colorId, partId, index);
                    break;
            }
        }
        return (T)Convert.ChangeType(result == "" ? (object)value : (object)result, typeof(T));
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