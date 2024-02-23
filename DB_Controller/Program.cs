using DB_Controller.DbSettings;
using DB_Controller.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => { options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(15); });

// Add services to the container.
builder.Services.AddControllersWithViews();

IConfiguration Configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .Build();

builder.Services.Configure<GeneralDbSettings>(Configuration.GetSection("GeneralDbSettings"));
builder.Services.Configure<InfluxDbSettings>(Configuration.GetSection("InfluxDbSettings"));
builder.Services.Configure<TimescaleDbSettings>(Configuration.GetSection("TimescaleDbSettings"));
builder.Services.AddScoped<ExceptionFilter>();

Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug().Enrich.FromLogContext()
            .WriteTo.File("logs/myapp.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
