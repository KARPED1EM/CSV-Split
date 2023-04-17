using Csv;
using System.Text;

internal static class Program
{
    private static readonly string PATH_0 = "../../../../TOHE/Resources/String.csv";
    private static readonly string PATH_1 = "../../../../String1.csv";
    private static readonly string PATH_2 = "../../../../String2.csv";
    private static readonly string PATH_Result = "../../../../Result.csv";

    private class CsvFile
    {
        public Dictionary<string, Dictionary<int, string>> map = new();
        public Dictionary<int, string> notes = new();
        public List<string> langs = new();
        public int endLine;
    }

    private static void Main()
    {
        CsvFile main = new();
        ReadFile(ref main, PATH_0);
        while (true)
        {
            Console.WriteLine("\n1: 一键分割\n2: 自定义分割文件\n3: 合并文件\n");
            var input = Console.ReadLine();
            if (input?.ToLower() == "exit") break;
            Console.Clear();
            if (!int.TryParse(input, out var op)) continue;
            switch (op)
            {
                case 1:
                    Split(main, PATH_1, "13 0 5 14");
                    Split(main, PATH_2, "13 12");

                    break;
                case 2:
                    Split(main, PATH_1);
                    break;
                case 3:
                    CsvFile f1 = new();
                    Console.Write("读取待合并文件 => ");
                    ReadFile(ref f1, PATH_1);
                    Console.WriteLine("-----------------------------------");
                    Merge(main, f1, PATH_Result);
                    break;
            }
            Console.WriteLine("-----------------------------------");
        }
    }

    private static bool ReadFile(ref CsvFile file, string path, bool cache = false)
    {
        file.map = new();
        file.notes = new();
        file.langs = new();
        FileStream fs = File.OpenRead(path);
        StreamReader sr = new(fs);
        int index = 0;
        while (true)
        {
            index++;
            var line = sr.ReadLine();
            file.endLine = index;
            if (line == null) break;
            if (!line.Equals("")) continue;
            file.notes.Add(index, line);
        }
        var options = new CsvOptions()
        {
            HeaderMode = HeaderMode.HeaderPresent,
            AllowNewLineInEnclosedFieldValues = false,
        };
        fs.Position = 0;
        foreach (var line in CsvReader.ReadFromStream(fs, options))
        {
            file.langs = line.Headers.Where(x => !x.Equals("id")).ToList();
            if (line.Values[0][0] == '#')
            {
                file.notes.Add(line.Index, line.Raw);
                continue;
            }
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
                if (!file.map.TryAdd(line.Values[0], dic))
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
        Console.WriteLine($"读取完成：共{file.map.Count}个字符串，{file.langs.Count - 1}个翻译");
        return true;
    }

    private static bool Split(CsvFile file, string path, string ip = "")
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
        foreach (var line in file.map)
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

    private static bool Merge(CsvFile main, CsvFile target, string path)
    {
        var sb = new StringBuilder();

        sb.Append("\"id\"");
        foreach (var l in main.langs) sb.Append($",\"{l}\"");
        sb.Append('\n');

        int index = 1;
        foreach (var str in main.map)
        {
        Start:
            index++;
            if (main.notes.ContainsKey(index))
            {
                sb.Append(main.notes[index] + "\n");
                goto Start;
            }
            sb.Append($"\"{str.Key}\"");
            if (!target.map.ContainsKey(str.Key))
            {
                foreach (var single in str.Value)
                    sb.Append($",\"{single.Value}\"");
                sb.Append('\n');
                continue;
            }
            foreach (var single in str.Value)
            {
                if (!target.langs.Contains(single.Key.ToString()) || single.Key == 13)
                    sb.Append($",\"{single.Value}\"");
                else
                    sb.Append($",\"{target.map[str.Key][single.Key]}\"");
            }
            sb.Append('\n');
        }

        if (!File.Exists(path)) File.Create(path);
        File.WriteAllText(path, sb.ToString());
        Console.WriteLine($"写到文件：{Path.GetFullPath(path)}");
        return false;
    }
}