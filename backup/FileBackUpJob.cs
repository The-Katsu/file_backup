using Microsoft.Extensions.Logging;
using Quartz;

[DisallowConcurrentExecution]
public class FileBackupJob : IJob
{
    private readonly ILogger<FileBackupJob> _logger;
    public FileBackupJob(ILogger<FileBackupJob> logger) => _logger = logger;

    public Task Execute(IJobExecutionContext context)
    {
        try
        {
            _logger.LogTrace("Получаем данные файла конфигурации...");

            JobDataMap dataMap = context.JobDetail.JobDataMap;
            string source = dataMap.GetString("source");
            string destination = dataMap.GetString("destination");

            _logger.LogInformation("Данные файла конфигурации:" +
                $"\nsource: {source}" +
                $"\ndestination: {destination}");

            var sourceFiles = Directory.GetFiles(source, "*.*", SearchOption.AllDirectories);

            if (sourceFiles.Length == 0)
            {
                _logger.LogInformation("В директории-источнике нет файлов.");
                return Task.CompletedTask;
            }

            var destinationFiles = Directory.GetFiles(destination, "*.*", SearchOption.AllDirectories);
            List<string> newFiles = sourceFiles.ToList();

            _logger.LogInformation("Список директорий в источнике:" + GetFoldersList(source));
            _logger.LogInformation("Список файлов в источнике:" + GetFilesList(source));

            _logger.LogTrace("Поиск новых файлов или модификаций...");

            foreach (var file1 in sourceFiles)
                foreach (var file2 in destinationFiles)
                    if (FileCompare(file1, file2))
                    {
                        newFiles.Remove(file1);
                        break;
                    }

            if (newFiles.Count == 0)
            {
                _logger.LogInformation("Нет новых файлов или модификаций!");
                return Task.CompletedTask;
            }

            _logger.LogInformation("Новые файлы или модификации:" + ListToString(newFiles));

            _logger.LogTrace("Получаем директории файлов...");

            List<string> newFolders = new();

            foreach (var filename in newFiles)
                newFolders.Add(Path.GetDirectoryName(filename));

            newFolders = newFolders.Distinct().ToList();

            _logger.LogInformation("Директории файлов" + ListToString(newFolders));

            string destinationFolder = "\\base";

            if (Directory.Exists(destination + destinationFolder))
                destinationFolder = string.Format("\\inc_{0:yyyy_MM_dd_HH_mm_ss}", DateTime.Now);

            string destinationPath = destination + destinationFolder;

            _logger.LogTrace("Создаём новые резервные директории...");

            foreach (var folder in newFolders)
                Directory.CreateDirectory(folder.Replace(source, destinationPath));

            _logger.LogInformation("Новые резервные директории:" + GetFoldersList(destinationPath));

            _logger.LogTrace("Создаём резервные копии...");

            foreach (var file in newFiles)
                File.Copy(file, file.Replace(source, destinationPath));

            _logger.LogInformation("Созданы резервные копии:" + GetFilesList(destinationPath));

            _logger.LogInformation("Резервное копирование завершено!");

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.GetType() + ": " + ex.Message);
            return Task.FromException(ex);
        }
    }

    //---
    //ХЭШ
    private bool FileCompare(string filepath1, string filepath2)
    {
        var barr1 = CalculateMD5(filepath1);
        var barr2 = CalculateMD5(filepath2);
        return barr1.SequenceEqual(barr2);
    }

    private byte[] CalculateMD5(string filepath)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        using var stream = File.OpenRead(filepath);
        return md5.ComputeHash(stream);
    }
    //--

    //-------------------
    //Визуализация данных
    private string GetFilesList(string path)
    {
        string files = "";
        Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
            .ToList().ForEach(file => files += $"\n{file}");
        return files;
    }

    private string GetFoldersList(string path)
    {
        string folders = "";
        Directory.GetDirectories(path, "*", SearchOption.AllDirectories)
            .ToList().ForEach(folder => folders += $"\n{folder}");
        return folders;
    }

    private string ListToString(List<string> lst)
    {
        string str = "";
        lst.ForEach(item => str += $"\n{item}");
        return str;
    }
    //-------------------
}
