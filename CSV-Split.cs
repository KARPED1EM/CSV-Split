using Csv;
using System.Text;

static class Program
{
    static string PATH_0 = "../../../../TOHE/Resources/String.csv";
    static string PATH_1 = "../../../../String1.csv";
    static string PATH_2 = "../../../../String2.csv";

    static Dictionary<string, Dictionary<int, string>> map = new();
    static List<string> langs = new();

    static void Main()
    {
        ReadFile();
        while (true)
        {
            Console.WriteLine("\n1: 输出文件1\n2: 输出文件2\n");
            var input = Console.ReadLine();
            if (input?.ToLower() == "exit") break;
            if (!int.TryParse(input, out var op))
            {
                Console.Clear();
                continue;
            }
            switch (op)
            {
                case 1:
                    Console.Clear();
                    Split(PATH_1, "13 0 5 14");
                    Console.WriteLine("-----------------------------------");
                    break;
                case 2:
                    Console.Clear();
                    Split(PATH_2, "13 12");
                    Console.WriteLine("-----------------------------------");
                    break;
                default:
                    Console.Clear();
                    continue;
            }
        }
    }

    static bool ReadFile(bool cache = false)
    {
        map = new();
        langs = new();
        FileStream fs = File.OpenRead(PATH_0);
        var options = new CsvOptions()
        {
            HeaderMode = HeaderMode.HeaderPresent,
            AllowNewLineInEnclosedFieldValues = false,
        };
        foreach (var line in CsvReader.ReadFromStream(fs, options))
        {
            if (langs.Count < 1) langs = line.Headers.ToList();
            if (line.Values[0][0] == '#') continue;
            try
            {
                var sb = new StringBuilder();
                sb.Append($"\"{line.Values[0]}\"");
                Dictionary<int, string> dic = new();
                for (int i = 1; i < line.ColumnCount; i++)
                {
                    sb.Append($",\"{line.Values[i]}\"");
                    int id = int.Parse(line.Headers[i]);
                    dic[id] = line.Values[i];
                }
                if (!map.TryAdd(line.Values[0], dic))
                    Console.WriteLine($"重复项：第{line.Index}行 => \"{line.Values[0]}\"");
                else
                    if (cache) Console.WriteLine(sb.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }
        if (cache) Console.WriteLine("--------------------------------");
        Console.WriteLine($"读取完成：共{map.Count}个字符串，{langs.Count - 2}个翻译");
        return true;
    }

    static bool Split(string path, string ip = "")
    {
        string input;
        if (ip == "")
        {
            Console.Write("输出的语言：");
            input = Console.ReadLine();
        }
        else input = ip;
        if (input == null) return false;
        var la = input.Split(" ");
        if (la.Length == 0) return false;

        if (!File.Exists(path))
        {
            Console.WriteLine($"创建新文件：{path}");
            File.Create(path);
        }
        var sb = new StringBuilder();
        sb.Append("\"id\"");
        foreach (var l in la) sb.Append($",\"{l}\"");
        sb.Append('\n');
        foreach (var line in map)
        {
            sb.Append($"\"{line.Key}\"");
            foreach (var l in la)
            {
                if (!int.TryParse(l, out var ln)) return false;
                if (!line.Value.ContainsKey(ln))
                {
                    Console.WriteLine($"未找到语言：{ln}");
                    return false;
                }
                sb.Append($",\"{line.Value[ln]}\"");
            }
            sb.Append('\n');
        }
        File.WriteAllText(path, sb.ToString());
        Console.WriteLine($"写到文件：{Path.GetFullPath(path)}");
        return true;
    }


}