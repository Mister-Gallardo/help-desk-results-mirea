Это учебный файл-инструкция сервиса HelpDesk.Results.

Описание проекта
----------------
HelpDesk.Results — учебное Minimal API-приложение на ASP.NET Core (.NET 10).
Оно демонстрирует обработку исключений, HTTP-ошибок и использование Results API.

Доступные маршруты
------------------
/                          — главная страница (HTML через собственный IResult)
/about/text                — текстовый ответ через Results.Text
/about/content             — текст с явным Content-Type через Results.Content
/api/tickets               — список всех заявок (JSON)
/api/tickets/{id}          — заявка по идентификатору
/api/tickets/create        — создание заявки (query: title, priority)
/status/unauthorized       — ответ 401
/status/forbidden          — ответ 403
/status/custom/418         — ответ 418 I'm a Teapot
/redirect/old-tickets      — перенаправление на /api/tickets
/redirect/ticket/{id}      — перенаправление на именованный маршрут ticket-details
/files/readme              — скачать этот файл
/throw                     — намеренное исключение (проверка обработчика)
/unknown                   — любой несуществующий путь вернёт 404 с описанием

Инструкция по запуску
---------------------
1. Откройте терминал в папке HelpDesk.Results.
2. Выполните: dotnet run
3. Перейдите по адресу https://localhost:5001 или http://localhost:5000.

Для переключения в Production выполните:
  $env:ASPNETCORE_ENVIRONMENT="Production" (PowerShell)
  export ASPNETCORE_ENVIRONMENT=Production (bash)
