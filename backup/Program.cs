using Newtonsoft.Json.Linq;

var data = GetJsonData("../../../config.json");

ShowSourceFiles(data.root.source);

FileBackup(data.root.source, data.root.destination);

static void FileBackup(string source, string destination)
{
    foreach (string dirPath in Directory.GetDirectories(source, "*",
        SearchOption.AllDirectories))
        Directory.CreateDirectory(dirPath.Replace(source, destination));

    foreach (string newPath in Directory.GetFiles(source, "*.*",
        SearchOption.AllDirectories))
        File.Copy(newPath, newPath.Replace(source, destination), true);
}

static void ShowSourceFiles(string source)
{
    var files = Directory.GetFileSystemEntries(source, "*", SearchOption.AllDirectories);
    Console.WriteLine("Cписок файлов и папок в папке источнике:");
    files.ToList().ForEach(d => Console.WriteLine(d.Replace(source, string.Empty)));
}

static dynamic GetJsonData(string source)
{
    using var sr = new StreamReader(source);
    var json = sr.ReadToEnd();
    return JObject.Parse(json);
}