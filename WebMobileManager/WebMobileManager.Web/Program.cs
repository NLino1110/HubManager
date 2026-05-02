using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;
using WebMobileManager.Web;
using WebMobileManager.Web.Components;
using WebMobileManager.Web.Handlers;
using WebMobileManager.Web.Handlers.Models;
using WebMobileManager.Web.Services;

var builder = WebApplication.CreateBuilder(args);


builder.AddServiceDefaults();
builder.AddRedisOutputCache("cache");


builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<HttpClient>();

builder.Services.AddSignalR();
builder.Services.AddSingleton<ChatHub>();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<CustomAuthenticationStateProvider>()); 
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, BlazorAuthorizationMiddlewareResultHandler>();

//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultScheme = "CustomAuth";
//    options.DefaultAuthenticateScheme = "CustomAuth";
//    options.DefaultChallengeScheme = "CustomAuth";
//})
//.AddCookie("CustomAuth", options =>
//{
//    options.LoginPath = "/login"; // Asegúrate de que tu página de login sea @page "/login"
//});
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddAuthorizationCore();

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

app.MapDefaultEndpoints();

app.MapHub<ChatHub>("/chatHub");

app.Run();
