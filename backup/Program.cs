

static void FileBackup(string source, string destination)
{
    var data = GetJsonData();

    Console.WriteLine("Поиск новых файлов или модификаций...");

    var sourceFiles = Directory.GetFiles((string)data.root.source, "*.*", SearchOption.AllDirectories);

    var destinationFiles = Directory.GetFiles((string)data.root.destination, "*.*", SearchOption.AllDirectories);

    List<string> newFiles = sourceFiles.ToList();

    foreach (var file1 in sourceFiles)
    {
        foreach (var file2 in destinationFiles)
        {
            if (FileCompare(file1, file2))
            {
                newFiles.Remove(file1);
                break;
            }
        }
    }

    Console.WriteLine("Найдены файлы:");
    newFiles.ForEach(file => Console.WriteLine(file));

    Console.WriteLine("Получаем директории файлов...");

    List<string> newFolders = new();
    foreach (var filename in newFiles)
        newFolders.Add(Path.GetDirectoryName(filename));
    newFolders = newFolders.Distinct().ToList();

    Console.WriteLine("Новые директории:");
    newFolders.ForEach(folder => Console.WriteLine(folder));

    Console.WriteLine("Создаём новые резервные директории...");

    string destinationFolder = "\\base";

    if (Directory.Exists((string)data.root.destination + destinationFolder))
        destinationFolder = string.Format("\\inc_{0:yyyy_MM_dd_HH_mm_ss}", DateTime.Now);

    string destinationPath = (string)data.root.destination + destinationFolder;

    foreach (var folder in newFolders)
        Directory.CreateDirectory(folder.Replace((string)data.root.source, destinationPath));

    Console.WriteLine("Новые резервные директории:");
    foreach (var folder in Directory.GetDirectories(destinationPath, "*", SearchOption.AllDirectories))
        Console.WriteLine(folder);

    Console.WriteLine("Создаём резервные копии...");
    foreach (var file in newFiles)
        File.Copy(file, file.Replace((string)data.root.source, destinationPath));

    Console.WriteLine("Созданы резервные копии:");
    foreach (var file in Directory.GetFiles(destinationPath, "*.*", SearchOption.AllDirectories))
        Console.WriteLine(file);

    Console.WriteLine("Копирование завершено!");
}

static bool FileCompare(string filepath1, string filepath2)
{
    var barr1 = CalculateMD5(filepath1);
    var barr2 = CalculateMD5(filepath2);
    return barr1.SequenceEqual(barr2);
}

static byte[] CalculateMD5(string filepath)
{
    using var md5 = System.Security.Cryptography.MD5.Create();
    using var stream = File.OpenRead(filepath);
    return md5.ComputeHash(stream); 
}

static string GetFilesList(string path)
{
    string files = "Список файлов:";
    Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
        .ToList().ForEach(file => files += $"\n{file}");
    return files;
}

static string GetFoldersList(string path)
{
    string folders = "Список папок:";
    Directory.GetDirectories(path, "*", SearchOption.AllDirectories)
        .ToList().ForEach(folder => folders += $"\n{folder}");
    return folders;
}

static dynamic GetJsonData()
{
    using var sr = new StreamReader("../../../config.json");
    var json = sr.ReadToEnd();
    dynamic data = Newtonsoft.Json.Linq.JObject.Parse(json);
    if (!Directory.Exists((string)data.source))
        throw new DirectoryNotFoundException($"Введённой директории-источника не существует - {data.source}");
    if (!Directory.Exists((string)data.destination))
        throw new DirectoryNotFoundException($"Введённой резервной директории не сущетсвует - {data.destination}");
    if (!Quartz.CronExpression.IsValidExpression((string)data.frequency))
        throw new System.Data.InvalidExpressionException($"Неправильно введено cron-выражени - {data.frequency}" +
            $"\n Смотрите https://clck.ru/gMpAH с примерамии cron");
    return data;
}