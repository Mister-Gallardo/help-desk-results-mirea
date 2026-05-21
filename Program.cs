using HelpDesk.Results;

var builder = WebApplication.CreateBuilder(args);

// Регистрация сервиса заявок в контейнере зависимостей
builder.Services.AddSingleton<ITicketRepository, InMemoryTicketRepository>();

var app = builder.Build();

// ─── Обработка исключений ────────────────────────────────────────────────────
// Middleware обработки исключений подключается первым, до любых маршрутов,
// чтобы перехватывать ошибки из всего последующего pipeline.
if (app.Environment.IsDevelopment())
{
    // В режиме разработки показывает полную страницу с деталями исключения.
    // Это удобно для отладки, но недопустимо в Production.
    app.UseDeveloperExceptionPage();
}
else
{
    // В Production перенаправляет на безопасный обработчик без stack trace.
    app.UseExceptionHandler("/error/exception");
}

// ─── Обработка HTTP-ошибок ───────────────────────────────────────────────────
// UseStatusCodePagesWithReExecute перехватывает ответы без тела (например, 404),
// повторно выполняет указанный маршрут и сохраняет исходный статус-код.
// Это лучше, чем WithRedirects, потому что клиент видит правильный статус-код,
// а не 302 → 200.
app.UseStatusCodePagesWithReExecute("/error/status/{0}");

// ─── Статические файлы ──────────────────────────────────────────────────────
app.UseStaticFiles();

// ────────────────────────────────────────────────────────────────────────────
// МАРШРУТЫ ОБРАБОТКИ ОШИБОК
// Эти маршруты должны быть определены до бизнес-маршрутов и не должны сами
// генерировать исключения или создавать циклические переадресации.
// ────────────────────────────────────────────────────────────────────────────

// Обработчик серверных исключений для Production.
// Возвращает понятный ответ 500 без технических деталей.
app.MapGet("/error/exception", (ILogger<Program> logger) =>
{
    logger.LogError("Необработанное исключение перехвачено обработчиком.");
    return Results.Json(
        new { error = "Внутренняя ошибка сервера. Попробуйте позже.", code = 500 },
        statusCode: 500
    );
});

// Обработчик HTTP-ошибок: принимает код ошибки и возвращает понятное описание.
app.MapGet("/error/status/{code:int}", (int code) =>
{
    var description = code switch
    {
        400 => "Некорректный запрос. Проверьте параметры.",
        401 => "Необходима аутентификация. Войдите в систему.",
        403 => "Доступ запрещён. У вас нет прав на это действие.",
        404 => "Страница или ресурс не найдены.",
        405 => "Метод запроса не поддерживается.",
        408 => "Время ожидания запроса истекло.",
        418 => "Я чайник. Сервер отказывается заваривать кофе.",
        500 => "Внутренняя ошибка сервера.",
        503 => "Сервис временно недоступен.",
        _   => $"Получен HTTP-статус {code}."
    };

    return Results.Json(new { statusCode = code, message = description }, statusCode: code);
});

// ────────────────────────────────────────────────────────────────────────────
// ГЛАВНАЯ СТРАНИЦА — Custom IResult (HtmlResult)
// ────────────────────────────────────────────────────────────────────────────

app.MapGet("/", () =>
{
    var html = """
        <!DOCTYPE html>
        <html lang="ru">
        <head>
            <meta charset="utf-8" />
            <title>HelpDesk.Results</title>
            <style>
                body { font-family: sans-serif; max-width: 800px; margin: 40px auto; padding: 0 20px; color: #333; }
                h1 { color: #1a73e8; }
                h2 { color: #444; margin-top: 30px; }
                ul { line-height: 2; }
                a { color: #1a73e8; text-decoration: none; }
                a:hover { text-decoration: underline; }
                .badge { background:#e8f0fe; color:#1a73e8; padding:2px 8px; border-radius:4px; font-size:0.85em; margin-left:6px; }
            </style>
        </head>
        <body>
            <h1>HelpDesk.Results</h1>
            <p>Учебный сервис заявок на ASP.NET Core (.NET 10). Демонстрирует Results API и обработку ошибок.</p>

            <h2>Заявки</h2>
            <ul>
                <li><a href="/api/tickets">/api/tickets</a> <span class="badge">JSON</span> — список всех заявок</li>
                <li><a href="/api/tickets/1">/api/tickets/1</a> <span class="badge">OK</span> — заявка по ID</li>
                <li><a href="/api/tickets/999">/api/tickets/999</a> <span class="badge">404</span> — несуществующая заявка</li>
                <li><a href="/api/tickets/create?title=Printer&priority=2">/api/tickets/create?title=Printer&priority=2</a> <span class="badge">201</span> — создать заявку</li>
                <li><a href="/api/tickets/create?priority=2">/api/tickets/create?priority=2</a> <span class="badge">400</span> — без title</li>
            </ul>

            <h2>Results API</h2>
            <ul>
                <li><a href="/about/text">/about/text</a> <span class="badge">Text</span></li>
                <li><a href="/about/content">/about/content</a> <span class="badge">Content</span></li>
                <li><a href="/status/unauthorized">/status/unauthorized</a> <span class="badge">401</span></li>
                <li><a href="/status/forbidden">/status/forbidden</a> <span class="badge">403</span></li>
                <li><a href="/status/custom/418">/status/custom/418</a> <span class="badge">418</span></li>
                <li><a href="/files/readme">/files/readme</a> <span class="badge">File</span></li>
                <li><a href="/html/help">/html/help</a> <span class="badge">HTML</span></li>
            </ul>

            <h2>Перенаправления</h2>
            <ul>
                <li><a href="/redirect/old-tickets">/redirect/old-tickets</a> — LocalRedirect на /api/tickets</li>
                <li><a href="/redirect/ticket/1">/redirect/ticket/1</a> — RedirectToRoute на ticket-details</li>
            </ul>

            <h2>Обработка ошибок</h2>
            <ul>
                <li><a href="/throw">/throw</a> <span class="badge">500</span> — намеренное исключение</li>
                <li><a href="/unknown">/unknown</a> <span class="badge">404</span> — несуществующий маршрут</li>
            </ul>
        </body>
        </html>
        """;

    return Results.Extensions.Html(html);
});

// ────────────────────────────────────────────────────────────────────────────
// МАРШРУТЫ Results.Text и Results.Content
// ────────────────────────────────────────────────────────────────────────────

app.MapGet("/about/text", () =>
    Results.Text("HelpDesk.Results — учебный сервис для демонстрации Results API в ASP.NET Core .NET 10.")
);

app.MapGet("/about/content", () =>
    Results.Content(
        content: "<b>HelpDesk.Results</b> — учебный проект. Content-Type задан явно.",
        contentType: "text/html",
        contentEncoding: System.Text.Encoding.UTF8
    )
);

// ────────────────────────────────────────────────────────────────────────────
// МАРШРУТЫ API ЗАЯВОК
// ────────────────────────────────────────────────────────────────────────────

// Список всех заявок — Results.Ok с JSON-сериализацией
app.MapGet("/api/tickets", (ITicketRepository repository) =>
    Results.Ok(repository.GetAll())
);

// Заявка по идентификатору — Results.Ok или Results.NotFound
app.MapGet("/api/tickets/{id:int}", (int id, ITicketRepository repository) =>
{
    var ticket = repository.GetById(id);
    return ticket is null
        ? Results.NotFound(new { message = $"Заявка с ID {id} не найдена." })
        : Results.Ok(ticket);
}).WithName("ticket-details");

// Создание заявки — Results.Created или Results.BadRequest
app.MapGet("/api/tickets/create", (string? title, int? priority, ITicketRepository repository) =>
{
    if (string.IsNullOrWhiteSpace(title))
        return Results.BadRequest(new { error = "Параметр 'title' обязателен и не может быть пустым." });

    var ticket = repository.Create(title, priority ?? 3);
    return Results.Created($"/api/tickets/{ticket.Id}", ticket);
});

// ────────────────────────────────────────────────────────────────────────────
// МАРШРУТЫ СТАТУС-КОДОВ
// ────────────────────────────────────────────────────────────────────────────

app.MapGet("/status/unauthorized", () =>
    Results.Unauthorized()
);

app.MapGet("/status/forbidden", () =>
    Results.StatusCode(403)
);

app.MapGet("/status/custom/{code:int}", (int code) =>
    Results.StatusCode(code)
);

// ────────────────────────────────────────────────────────────────────────────
// ПЕРЕНАПРАВЛЕНИЯ
// ────────────────────────────────────────────────────────────────────────────

// Локальная переадресация — старый адрес перенаправляет на новый
app.MapGet("/redirect/old-tickets", () =>
    Results.LocalRedirect("/api/tickets")
);

// Переадресация на именованный маршрут ticket-details
app.MapGet("/redirect/ticket/{id:int}", (int id) =>
    Results.RedirectToRoute("ticket-details", new { id })
);

// ────────────────────────────────────────────────────────────────────────────
// ФАЙЛ
// ────────────────────────────────────────────────────────────────────────────

// Отдаёт файл из заранее известного каталога wwwroot/files.
// Путь к файлу не принимается из запроса — это защищает от path traversal.
app.MapGet("/files/readme", (IWebHostEnvironment env) =>
{
    var filePath = Path.Combine(env.WebRootPath, "files", "readme.txt");
    if (!File.Exists(filePath))
        return Results.NotFound(new { message = "Файл readme.txt не найден." });

    return Results.File(filePath, contentType: "text/plain; charset=utf-8", fileDownloadName: "readme.txt");
});

// ────────────────────────────────────────────────────────────────────────────
// СОБСТВЕННЫЙ IResult — HtmlResult через маршрут /html/help
// ────────────────────────────────────────────────────────────────────────────

app.MapGet("/html/help", () =>
{
    var html = """
        <!DOCTYPE html>
        <html lang="ru">
        <head><meta charset="utf-8" /><title>Справка HelpDesk.Results</title>
        <style>body{font-family:sans-serif;max-width:700px;margin:40px auto;padding:0 20px;} h1{color:#c0392b;}</style>
        </head>
        <body>
            <h1>Справка по HelpDesk.Results</h1>
            <p>Этот ответ сформирован через собственный класс <code>HtmlResult</code>, реализующий интерфейс <code>IResult</code>.</p>
            <p>Метод <code>ExecuteAsync</code> устанавливает заголовок <code>Content-Type: text/html; charset=utf-8</code>
               и записывает HTML-строку в тело ответа.</p>
            <p><a href="/">Вернуться на главную</a></p>
        </body>
        </html>
        """;

    return Results.Extensions.Html(html);
});

// ────────────────────────────────────────────────────────────────────────────
// НАМЕРЕННОЕ ИСКЛЮЧЕНИЕ
// ────────────────────────────────────────────────────────────────────────────

// В Development покажет страницу разработчика (DeveloperExceptionPage).
// В Production управление перейдёт к UseExceptionHandler("/error/exception").
app.MapGet("/throw", () =>
{
    throw new InvalidOperationException(
        "Это намеренное исключение для проверки обработчика ошибок.");
    // Следующая строка никогда не выполняется, нужна только для компилятора
    return Results.Ok();
});

app.Run();
