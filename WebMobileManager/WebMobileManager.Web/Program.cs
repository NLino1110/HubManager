using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;
using System.Text.RegularExpressions;
using WebMobileManager.Web;
using WebMobileManager.Web.Components;
using WebMobileManager.Web.Handlers;
using WebMobileManager.Web.Handlers.Models;
using WebMobileManager.Web.Services;
using WebMobileManager.Web.Services.Interfaces;
using WebMobileManager.Web.Services.Sqlite;

var builder = WebApplication.CreateBuilder(args);


builder.AddServiceDefaults();
builder.AddRedisOutputCache("cache");

//SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_e_sqlite3());
SQLitePCL.Batteries_V2.Init();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<HttpClient>();

builder.Services.AddScoped<IPackageService, PackageService>();
builder.Services.AddSingleton<PackageDb>();
builder.Services.AddSingleton<PackageFileDb>();

builder.Services.AddSignalR();
builder.Services.AddSingleton<ChatHub>();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<CustomAuthenticationStateProvider>()); 
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, BlazorAuthorizationMiddlewareResultHandler>();

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddAuthorizationCore();

builder.Services.AddControllers();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);    
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseOutputCache();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapControllers();
app.MapDefaultEndpoints();

app.MapHub<ChatHub>("/chatHub");

app.UseStaticFiles();

app.MapGet("/uploads/images/{fileName}", async (string fileName, IWebHostEnvironment env) =>
{    
    var basePath = Path.Combine(env.WebRootPath, "uploads", "images");
    var cachePath = Path.Combine(basePath, "cache");

    Directory.CreateDirectory(cachePath);

    var match = Regex.Match(fileName, @"(.+)_(\d+)\.(jpg|jpeg|png)");

    string originalFile;
    int? size = null;

    if (match.Success)
    {
        originalFile = match.Groups[1].Value + "." + match.Groups[3].Value;
        size = int.Parse(match.Groups[2].Value);
    }
    else
    {
        originalFile = fileName;
    }

    var originalPath = Path.Combine(basePath, originalFile);
    var cachedPath = Path.Combine(cachePath, fileName);

    // 1. cache
    if (File.Exists(cachedPath))
        return Results.File(cachedPath, "image/jpeg");

    if (!File.Exists(originalPath))
        return Results.NotFound();

    // 2. sin resize
    if (size == null)
        return Results.File(originalPath, "image/jpeg");

    // 3. resize con SkiaSharp
    using var inputStream = File.OpenRead(originalPath);
    using var original = SkiaSharp.SKBitmap.Decode(inputStream);

    int newWidth = size.Value;
    int newHeight = (int)(original.Height * (size.Value / (double)original.Width));

    var info = new SkiaSharp.SKImageInfo(newWidth, newHeight);

    using var resized = original.Resize(
        info,
        new SkiaSharp.SKSamplingOptions(SkiaSharp.SKFilterMode.Linear)
    );

    using var image = SkiaSharp.SKImage.FromBitmap(resized);
    using var data = image.Encode(SkiaSharp.SKEncodedImageFormat.Jpeg, 80);

    var bytes = data.ToArray();

    // 4. guardar cache
    await File.WriteAllBytesAsync(cachedPath, bytes);

    return Results.File(bytes, "image/jpeg");
});

app.Run();
