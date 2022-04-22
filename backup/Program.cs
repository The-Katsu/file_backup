using Microsoft.Extensions.Logging;

var loggerFactory = LoggerFactory.Create(
    builder => builder
        .AddConsole()
        .AddDebug()
        .SetMinimumLevel(LogLevel.Trace)
    );

var logger = loggerFactory.CreateLogger<Program>();

FileBackup(logger);

static void FileBackup(ILogger<Program> logger)
{
    try
    {
        logger.LogTrace("Считываем данные с файла конфигурации...");
        var data = GetJsonData();

        logger.LogInformation("Данные файла конфигурации:" +
            $"\nsource: {data.source}" +
            $"\ndestination: {data.destination}" +
            $"\ncron: {data.frequency}");

        logger.LogInformation("Список директорий в источнике:" + GetFoldersList(data.source));
        logger.LogInformation("Список файлов в источнике:" + GetFilesList(data.source));

        logger.LogTrace("Поиск новых файлов или модификаций...");

        var sourceFiles = Directory.GetFiles((string)data.source, "*.*", SearchOption.AllDirectories);
        var destinationFiles = Directory.GetFiles((string)data.destination, "*.*", SearchOption.AllDirectories);
        List<string> newFiles = sourceFiles.ToList();

        foreach (var file1 in sourceFiles)
            foreach (var file2 in destinationFiles)
                if (FileCompare(file1, file2))
                {
                    newFiles.Remove(file1);
                    break;
                }

        if (newFiles.Count == 0)
        {
            logger.LogInformation("Нет новых файлов или модификаций!");
            return;
        }

        logger.LogInformation("Новые файлы или модификации:" + ListToString(newFiles));

        logger.LogTrace("Получаем директории файлов...");

        List<string> newFolders = new();

        foreach (var filename in newFiles)
            newFolders.Add(Path.GetDirectoryName(filename));

        newFolders = newFolders.Distinct().ToList();

        logger.LogInformation("Директории файлов" + ListToString(newFolders));

        string destinationFolder = "\\base";

        if (Directory.Exists((string)data.destination + destinationFolder))
            destinationFolder = string.Format("\\inc_{0:yyyy_MM_dd_HH_mm_ss}", DateTime.Now);

        string destinationPath = (string)data.destination + destinationFolder;

        logger.LogTrace("Создаём новые резервные директории...");

        foreach (var folder in newFolders)
            Directory.CreateDirectory(folder.Replace((string)data.source, destinationPath));

        logger.LogInformation("Новые резервные директории:" + GetFoldersList(destinationPath));

        logger.LogTrace("Создаём резервные копии...");

        foreach (var file in newFiles)
            File.Copy(file, file.Replace((string)data.source, destinationPath));

        logger.LogInformation("Созданы резервные копии:" + GetFilesList(destinationPath));

        logger.LogInformation("Резервное копирование завершено!");
    }
    catch (Exception ex) 
    { 
        logger.LogError(ex.Message); 
    }
}

//Чтение данных из config.json
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
            "\n Генератор cron-выражений https://clck.ru/gMpAH");
    return data;
}

//---
//ХЭШ
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
//--

//-------------------
//Визуализация данных
static string GetFilesList(string path)
{
    string files = "";
    Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
        .ToList().ForEach(file => files += $"\n{file}");
    return files;
}

static string GetFoldersList(string path)
{
    string folders = "";
    Directory.GetDirectories(path, "*", SearchOption.AllDirectories)
        .ToList().ForEach(folder => folders += $"\n{folder}");
    return folders;
}

static string ListToString(List<string> lst)
{
    string str = "";
    lst.ForEach(item => str += $"\n{item}");
    return str;
}
//-------------------