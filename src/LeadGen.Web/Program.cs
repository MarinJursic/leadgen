using System.Text.Json.Serialization;
using LeadGen.Core.Configuration;
using LeadGen.Core.Services;
using LeadGen.Infrastructure;
using LeadGen.Infrastructure.Data;
using LeadGen.Infrastructure.Logging;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

var logStore = new SafeFileLogStore(Path.Combine(builder.Environment.ContentRootPath, "App_Data", "logs"));
builder.Logging.AddProvider(logStore);

builder.Services.AddSingleton<IAppLogReader>(logStore);
builder.Services.AddSingleton<IAppLogWriter>(logStore);
builder.Services.AddHttpContextAccessor();
builder.Services.AddLeadGenInfrastructure(builder.Configuration);
builder.Services
    .AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var correlationId = GetCorrelationId(context.HttpContext);
        var message = string.Join(" ", context.ModelState.Values
            .SelectMany(value => value.Errors)
            .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage) ? "Invalid request." : error.ErrorMessage));

        return new BadRequestObjectResult(new
        {
            error = new
            {
                code = "validation_error",
                message = string.IsNullOrWhiteSpace(message) ? "The request is invalid." : message,
                correlationId
            }
        });
    };
});

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        var correlationId = GetCorrelationId(context);
        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("SafeErrorHandler");
        logger.LogError(exception, "Unhandled request error");

        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                error = new
                {
                    code = "server_error",
                    message = "A safe server error occurred. Check logs with the correlation id.",
                    correlationId
                }
            });
            return;
        }

        context.Response.Redirect($"/Home/Error?correlationId={Uri.EscapeDataString(correlationId)}");
    });
});

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers.TryGetValue("X-Correlation-ID", out var provided)
        && !string.IsNullOrWhiteSpace(provided)
            ? provided.ToString()
            : Guid.NewGuid().ToString("N");

    context.Items["CorrelationId"] = correlationId;
    CorrelationContext.Current = correlationId;
    context.Response.Headers["X-Correlation-ID"] = correlationId;
    try
    {
        await next(context);
    }
    finally
    {
        CorrelationContext.Current = null;
    }
});

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    Directory.CreateDirectory(Path.Combine(app.Environment.ContentRootPath, "App_Data"));
    var db = scope.ServiceProvider.GetRequiredService<LeadGenDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();

static string GetCorrelationId(HttpContext context)
{
    return context.Items.TryGetValue("CorrelationId", out var id) && id is string value
        ? value
        : context.TraceIdentifier;
}

public partial class Program;
