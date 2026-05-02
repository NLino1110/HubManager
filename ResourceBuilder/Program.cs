using Append.Blazor.Clipboard;
using BlazorDateRangePicker;
using BlazorDownloadFile;
using Blazored.LocalStorage;
using Blazored.Modal;
using Blazored.Toast;
using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using BlazorSpinner;
using BlazorTable;
using DataSourceManager;
using DataSourceManager.Tools;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Models.DMSA.Shared.Security;
using Models.DMSA.Shared.Tools;
using Quartz;
using ResourceBuilder.Data;
using ResourceBuilder.DBContext.PostgreSql;
using ResourceBuilder.Handlers;
using ResourceBuilder.Handlers.Models;
using ResourceBuilder.Services.Automata;
using ResourceBuilder.Services.Inventory;
using ResourceBuilder.Services.Sales;
using ResourceBuilder.Services.Sync;

var builder = WebApplication.CreateBuilder(args);

var appSettingSection = builder.Configuration.GetSection("AppSettings");
var appSettingDeserialized = builder.Configuration.GetSection("AppSettings").Get<AppSettings>();
var appSettingDeserializedSectionProfile = builder.Configuration.GetSection("AppSettings:Profiles:" + appSettingDeserialized.UseProfile);
appSettingDeserialized.profile = appSettingDeserializedSectionProfile.Get<Profile>();

builder.Services.Configure<AppSettings>(options =>
{
    options.UseProfile = appSettingDeserialized.UseProfile;
    options.profile = appSettingDeserialized.profile;
    //options = appSettingDeserialized;
});

ConfigurationHelper.setAppSettings(appSettingDeserialized);

DataConnection connection = new DataConnection();
//string Driver = ConfigurationHelper.GetAppSettings().profile.DataServers[0].Driver;

//builder.Services.AddDbContext<AppDbContext>(p => p.UseOracle(connection.GetConnectionString(),
//                b => b.UseOracleSQLCompatibility("11")));

builder.Services.AddDbContext<AppDbContext>(p => p.UseOracle(connection.GetConnectionString()));

builder.Services.AddDbContext<PostgreSqlContext>(options =>
    options.UseNpgsql("Host=127.0.0.1;Port=5434;Database=dmintegrations;Username=django;Password=DM@dj4ng0;ApplicationName=Blazor"));

builder.Logging.ClearProviders(); // Opcional: limpia los proveedores de logging por defecto
builder.Logging.AddConsole(); // Agrega logging en la consola
builder.Logging.AddDebug();
builder.Logging.AddFile("Logs/app-log.txt");

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddOutputCache();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddSingleton<BuilderService>();
builder.Services.AddSingleton<BrandService>();
builder.Services.AddScoped<InvoicesService>();

//builder.Services.AddScoped<Processor>();
//builder.Services.AddScoped<TaskManager>();

builder.Services.AddBlazorDownloadFile();
builder.Services.AddBlazoredModal();
builder.Services.AddBlazoredToast();
builder.Services.AddBlazorTable();
builder.Services.AddHttpClient<IUserService, UserService>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddScoped<SpinnerService>();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddSignalR();
builder.Services.AddSingleton<ChatHub>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddClipboard();

builder.Services.AddBlazorise(options =>
 {
     options.Immediate = true;
 })
    .AddBootstrap5Providers()
    .AddFontAwesomeIcons();


builder.Services.AddDateRangePicker(config =>
{
    config.Attributes = new Dictionary<string, object>
    {
        { "class", "form-control form-control-sm" }
    };
});

ServiceJobs.SetJobs(builder);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");    
    app.UseHsts();
}

app.UseHttpsRedirection();

var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".apk"] = "application/vnd.android.package-archive";

app.UseStaticFiles();

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
    //ServeUnknownFileTypes = true,
    //DefaultContentType = "image/png"
});

app.UseAntiforgery();

app.UseOutputCache();

app.UseRouting();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<ChatHub>("/chatHub");

using (var scope = app.Services.CreateScope())
{    
    var PgDbContext = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();
}

app.Run();
