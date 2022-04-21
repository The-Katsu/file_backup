using Newtonsoft.Json.Linq;
using System.Security.Cryptography;

var data = GetJsonData();

ShowSourceFiles(data.root.source);

FileBackup(data.root.source, data.root.destination);

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