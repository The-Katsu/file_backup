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
В программе используется логирование средствами ILogger, изменить уровень логгирования можно в блоке конфигурации логгера, на 22 строчке Program.cs.   
*  [Про уровни логирования](https://docs.microsoft.com/ru-ru/dotnet/api/microsoft.extensions.logging.loglevel?view=dotnet-plat-ext-6.0)
```cs
.ConfigureLogging(logging =>
        {
            logging.AddConsole();
            logging.AddDebug();
            logging.SetMinimumLevel(LogLevel.Trace); <- тут
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
---
## Пример работы приложения
```powershell
>dotnet run

trce: FileBackupJob[0]
      Получаем данные файла конфигурации...
info: FileBackupJob[0]
      Данные файла конфигурации:
source: C:\Users\Vladimir\source\repos\backup\tmp\source
destination: C:\Users\Vladimir\source\repos\backup\tmp\destination
info: FileBackupJob[0]
      Список директорий в источнике:
C:\Users\Vladimir\source\repos\backup\tmp\source\documents
C:\Users\Vladimir\source\repos\backup\tmp\source\media
C:\Users\Vladimir\source\repos\backup\tmp\source\photos
C:\Users\Vladimir\source\repos\backup\tmp\source\photos\0001
C:\Users\Vladimir\source\repos\backup\tmp\source\photos\0002
info: FileBackupJob[0]
      Список файлов в источнике:
C:\Users\Vladimir\source\repos\backup\tmp\source\documents\doc1.docx
C:\Users\Vladimir\source\repos\backup\tmp\source\documents\text1.txt
C:\Users\Vladimir\source\repos\backup\tmp\source\media\5eeea355389655.59822ff824b72.gif
C:\Users\Vladimir\source\repos\backup\tmp\source\photos\111.jpg
C:\Users\Vladimir\source\repos\backup\tmp\source\photos\Что-такое-фото.jpg
C:\Users\Vladimir\source\repos\backup\tmp\source\photos\0001\image1.png
C:\Users\Vladimir\source\repos\backup\tmp\source\photos\0001\image2.jpg
C:\Users\Vladimir\source\repos\backup\tmp\source\photos\0001\image3.jpg
C:\Users\Vladimir\source\repos\backup\tmp\source\photos\0001\image4.jpg
C:\Users\Vladimir\source\repos\backup\tmp\source\photos\0002\korj1.jpg
C:\Users\Vladimir\source\repos\backup\tmp\source\photos\0002\korj2.jpg
trce: FileBackupJob[0]
      Поиск новых файлов или модификаций...
info: FileBackupJob[0]
      Новые файлы или модификации:
C:\Users\Vladimir\source\repos\backup\tmp\source\documents\doc1.docx
C:\Users\Vladimir\source\repos\backup\tmp\source\media\5eeea355389655.59822ff824b72.gif
trce: FileBackupJob[0]
      Получаем директории файлов...
info: FileBackupJob[0]
      Директории файлов
C:\Users\Vladimir\source\repos\backup\tmp\source\documents
C:\Users\Vladimir\source\repos\backup\tmp\source\media
trce: FileBackupJob[0]
      Создаём новые резервные директории...
info: FileBackupJob[0]
      Новые резервные директории:
C:\Users\Vladimir\source\repos\backup\tmp\destination\inc_2022_04_23_17_27_00\documents
C:\Users\Vladimir\source\repos\backup\tmp\destination\inc_2022_04_23_17_27_00\media
trce: FileBackupJob[0]
      Создаём резервные копии...
info: FileBackupJob[0]
      Созданы резервные копии:
C:\Users\Vladimir\source\repos\backup\tmp\destination\inc_2022_04_23_17_27_00\documents\doc1.docx
C:\Users\Vladimir\source\repos\backup\tmp\destination\inc_2022_04_23_17_27_00\media\5eeea355389655.59822ff824b72.gif
info: FileBackupJob[0]
      Резервное копирование завершено!
```