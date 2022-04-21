using Newtonsoft.Json.Linq;
using System.Security.Cryptography;

var data = GetJsonData();

ShowSourceFiles(data.root.source);

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

if (Directory.GetDirectories((string)data.root.destination).Contains("base"))
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

foreach (var file in Directory.GetFiles(destinationPath, "*.*", SearchOption.AllDirectories))
    Console.WriteLine(file);


static void FileBackup(string source, string destination)
{
    foreach (string dirPath in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
        if (Directory.GetFiles(dirPath).Length != 0)
            Directory.CreateDirectory(dirPath.Replace(source, destination));

    foreach (string newPath in Directory.GetFiles(source, "*.*", SearchOption.AllDirectories))
        File.Copy(newPath, newPath.Replace(source, destination), true);
}

static bool FileCompare(string filepath1, string filepath2)
{
    var barr1 = CalculateMD5(filepath1);
    var barr2 = CalculateMD5(filepath2);
    return barr1.SequenceEqual(barr2);
}

static byte[] CalculateMD5(string filepath)
{
    using var md5 = MD5.Create();
    using var stream = File.OpenRead(filepath);
    return md5.ComputeHash(stream); 
}

static void ShowSourceFiles(string source)
{
    var files = Directory.GetFileSystemEntries(source, "*", SearchOption.AllDirectories);
    Console.WriteLine("Cписок файлов и папок в папке источнике:");
    files.ToList().ForEach(d => Console.WriteLine(d.Replace(source, string.Empty)));
}

static dynamic GetJsonData()
{
    using var sr = new StreamReader("../../../config.json");
    var json = sr.ReadToEnd();
    return JObject.Parse(json);
}