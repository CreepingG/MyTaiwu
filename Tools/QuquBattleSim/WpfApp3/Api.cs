using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;

class QuquSystem
{
    public Dictionary<int, Dictionary<int, string>> Source;
    public static QuquSystem instance = new QuquSystem();
    QuquSystem()
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

    private readonly int[][][] cache = new int[10][][];

    public int[][] GetAllTypes(int level)
    {
        if (cache[level] != null) return cache[level];
        if (level == 9)
        {
            return cache[level] = Source.Where(kvp => kvp.Value[5] == "1").Select(kvp => new int[] { kvp.Key, 0 }).ToArray();
        }
        if (level == 8)
        {
            return cache[level] = Source.Where(kvp => kvp.Value[7] != "0").Select(kvp => new int[] { kvp.Key, 0 }).ToArray();
        }
        var colors = Source.Where(kvp => kvp.Value[3] != "0").Where(kvp => kvp.Value[1].ToInt() == level);
        var parts = Source.Where(kvp => kvp.Value[4] != "0").Where(kvp => kvp.Value[1].ToInt() < level);
        var colorBetter = colors.SelectMany(color => parts.Select(part => new int[] { color.Key, part.Key }));

        var colors2 = Source.Where(kvp => kvp.Value[3] != "0").Where(kvp => kvp.Value[1].ToInt() < level);
        var parts2 = Source.Where(kvp => kvp.Value[4] != "0").Where(kvp => kvp.Value[1].ToInt() == level);
        var partBetter = colors2.SelectMany(color => parts2.Select(part => new int[] { color.Key, part.Key }));

        var same = colors.SelectMany(color => parts2.Select(part => new int[] { color.Key, part.Key }));

        return cache[level] = colorBetter.Concat(partBetter).Concat(same).ToArray();
    }

    public int GetData(int colorId, int partId, int index)
    {
        int colorValue = Source[colorId][index].ToInt();
        int partValue = (partId > 0) ? Source[partId][index].ToInt() : 0;
        int result = colorValue + partValue;
        return result;
    }
    
    public string GetName(int colorId, int partId)
    {
        try
        {
            return ((partId <= 0) ? Source[colorId][0] : ((int.Parse(Source[colorId][2]) >= int.Parse(Source[partId][2])) ? (Source[colorId][0].Split('|')[0] + Source[partId][0]) : (Source[partId][0] + Source[colorId][0].Split('|')[1])));
        }
        catch
        {
            return "??";
        }
    }

    public QuquBattler MakeOne(int colorId, int partId)
    {
        int maxHp = GetData(colorId, partId, 1) + GetData(colorId, partId, 11) / 20;
        maxHp += Random.Range(-(maxHp * 35 / 100), maxHp * 35 / 100 + 1);
        var item = new Item
        {
            [2002] = colorId,
            [2003] = partId,
            [902] = maxHp,
            [901] = maxHp,
            [2007] = GetData(colorId, partId, 98) * Random.Range(0, 21) / 100,
        };

        var battler = QuquBattler.Create(item);
        for (int i = 0; i < Mathf.Min(3, maxHp - 1); i++)
        {
            if (Random.Range(0, 100) < 10)
            {
                battler.GetDamage(QuquBattler.DamageTyp.耐力, 1);
                battler.GetDamage(QuquBattler.DamageTyp.属性);
            }
        }
        return battler;
    }
}

public class Item:Dictionary<int, int>
{
    public static Dictionary<int, Item> Lib = new Dictionary<int, Item>();
    public T Get<T>(int index, bool other = true)
    {
        int value = this.ContainsKey(index) ? this[index] : 0;
        string result = "";
        if (!other) return (T)Convert.ChangeType(value, typeof(T));
        if (Get<int>(2001, false) == 1)
        {
            int colorId = this[2002];
            int partId = this[2003];
            var cricketDate = QuquSystem.instance.Source;
            switch (index)
            {
                case 0:
                    result = QuquSystem.instance.GetName(colorId, partId);
                    break;
                case 8:
                    value = QuquSystem.instance.GetData(colorId, partId, 1);
                    break;
                case 98:
                    result = cricketDate[colorId][97];
                    break;
                case 99:
                    result = ((partId > 0) ? cricketDate[partId][99] : cricketDate[colorId][99]);
                    break;
                case 904:
                    value = QuquSystem.instance.GetData(colorId, partId, 94);
                    break;
                case 905:
                    value = QuquSystem.instance.GetData(colorId, partId, 95);
                    break;
                case 52001:
                    value = QuquSystem.instance.GetData(colorId, partId, index);
                    break;
                case 52002:
                    value = QuquSystem.instance.GetData(colorId, partId, index);
                    break;
                case 52003:
                    value = QuquSystem.instance.GetData(colorId, partId, index);
                    break;
                case 52004:
                    value = QuquSystem.instance.GetData(colorId, partId, index);
                    break;
                case 52005:
                    value = QuquSystem.instance.GetData(colorId, partId, index);
                    break;
                case 52006:
                    value = QuquSystem.instance.GetData(colorId, partId, index);
                    break;
                case 52007:
                    value = QuquSystem.instance.GetData(colorId, partId, index);
                    break;
            }
        }
        return (T)Convert.ChangeType(result == "" ? (object)value : (object)result, typeof(T));
    }
    public static void SetProperty(int itemId, int key, string value)
    {
        if (!Lib.ContainsKey(itemId))
        {
            Lib.Add(itemId, new Item {
                { 2001, 1 }
            });
        }
        Lib[itemId][key] = value.ToInt();
    }
}

static class Extension
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
        catch
        {
            return 0;
        }
    }
    public static string ToString<T1, T2>(this Dictionary<T1,T2> dict)
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

class Writer
{
    readonly FileStream stream;

    public Writer(string fileName, string folderName = null)
    {
        if (!string.IsNullOrWhiteSpace(folderName))
        {
            if (!Directory.Exists(fileName)) Directory.CreateDirectory(fileName); //创建目录
            fileName = folderName + "\\" + fileName;
        }
        stream = new FileStream(fileName, FileMode.Create);
    }

    ~Writer()
    {
        stream.Close();
    }

    public void WriteLine(object s)
    {
        byte[] data = Encoding.Default.GetBytes(s.ToString() + '\n');
        stream.Write(data, 0, data.Length);
        stream.Flush();
    }
} 