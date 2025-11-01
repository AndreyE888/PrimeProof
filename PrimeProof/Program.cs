using PrimeProof.Services;
using PrimeProof.Services.Interfaces;
using PrimeProof.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Регистрация сервисов тестирования
builder.Services.AddScoped<TestRunnerService>();
builder.Services.AddScoped<IPrimalityTest, TrialDivisionTest>();
builder.Services.AddScoped<IPrimalityTest, FermatTest>();
builder.Services.AddScoped<IPrimalityTest, MillerRabinTest>();
builder.Services.AddScoped<IPrimalityTest, AKSTest>();

// Настройка MVC
builder.Services.AddMvc()
    .AddViewOptions(options =>
    {
        options.HtmlHelperOptions.ClientValidationEnabled = true;
    });

// Настройка Kestrel для работы с большими числами (опционально)
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 50 * 1024 * 1024; // 50MB
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    // В разработке показываем подробные ошибки
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Маршрутизация
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Tests}/{action=Index}/{id?}");

// Дополнительные маршруты для API
app.MapControllerRoute(
    name: "api",
    pattern: "api/{controller=Tests}/{action=QuickCheck}");

// Глобальная обработка ошибок
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        // Логирование ошибок
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Произошла непредвиденная ошибка");
        
        // Перенаправление на страницу ошибки
        context.Response.Redirect("/Home/Error");
    }
});

// Middleware для добавления security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    await next();
});

Console.WriteLine("🚀 PrimeProof application starting...");
Console.WriteLine("📊 Available primality tests:");
Console.WriteLine("   • Trial Division Test");
Console.WriteLine("   • Fermat Test");
Console.WriteLine("   • Miller-Rabin Test");
Console.WriteLine("   • AKS Test");
Console.WriteLine("🌐 Application is running on: https://localhost:7000");

app.Run();