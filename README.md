## Консольное приложение для резервного копирования файлов.  
Файл с настройками приложения находится в папке backup под названием config.json.  
На данный момент используется cron-выражение "0 * * ? * *" - каждую минуту.
* [Формат путей к файлам](https://docs.microsoft.com/ru-ru/dotnet/standard/io/file-path-formats)
* [Генератор cron-выражений](https://www.freeformatter.com/cron-expression-generator-quartz.html).
```json
{
  "source": "ВАШ:\\ПУТЬ\\ИСТОЧНИКА",
  "destination": "ВАШ:\\ПУТЬ\\НАЗНАЧЕНИЯ",
  "frequency": "* CRON * ВЫРАЖЕНИЕ ? *"
}
```
Путь к файлу конфигурации указывается на 7 строке Program.cs.
```cs
var data = GetJsonData("config.json");
```
---  
В программе используется логирование средствами ILogger, по умолчанию стоит уровень Information, для Trace и Debug сообщений измените уровень логирования на 23 строчке Program.cs в блоке конфигурации логгера.   
*  [Про уровни логирования](https://docs.microsoft.com/ru-ru/dotnet/api/microsoft.extensions.logging.loglevel?view=dotnet-plat-ext-6.0)
```cs
.ConfigureLogging(logging =>
        {
            logging.AddConsole();
            logging.AddDebug();
            logging.SetMinimumLevel(LogLevel.Information); <- тут
        })                                          
``` 
---
Аннотация ``` [DisallowConcurrentExecution] ``` для класса FileBackupJob исключает параллельное выполнение, в случае, если предыдущее копирование не было завершено, то новое не начнётся, а будет пропущено.  

---
Сравнение файлов выполняется на основе подсчёта хэша, функция CalculateMD5 считает хэш файла, а FileCompare на сравнивает полученные результаты. В качестве параметров передаётся путь расположения файла из источника и файла из папки назначения.
```cs
private byte[] CalculateMD5(string filepath)
{
    using var md5 = System.Security.Cryptography.MD5.Create();
    using var stream = File.OpenRead(filepath);
    return md5.ComputeHash(stream);
}
private bool FileCompare(string filepath1, string filepath2)
{
    var barr1 = CalculateMD5(filepath1);
    var barr2 = CalculateMD5(filepath2);
    return barr1.SequenceEqual(barr2);
}
```