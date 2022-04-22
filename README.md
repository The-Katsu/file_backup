## Консольное приложение для резервного копирования файлов.  
Файл с настройками приложения находится в папке backup под названием config.json.  
* [Формат путей к файлам](https://docs.microsoft.com/ru-ru/dotnet/standard/io/file-path-formats)
* [Генератор cron-выражений](https://www.freeformatter.com/cron-expression-generator-quartz.html).
```json
{
  "source": "ВАШ:\\ПУТЬ\\ИСТОЧНИКА",
  "destination": "ВАШ:\\ПУТЬ\\НАЗНАЧЕНИЯ",
  "frequency": "* CRON * ВЫРАЖЕНИЕ ? *"
}
```
