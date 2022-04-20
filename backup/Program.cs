using Newtonsoft.Json.Linq;

var data = GetJsonData("../../../config.json");

ShowSourceFiles(data.root.source);

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