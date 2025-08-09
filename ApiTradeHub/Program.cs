using ApiTradeHub.Middleware;
using ApiTradeHub.Services.Sales;
using DataSourceManager;
using DataSourceManager.Tools;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OData.ModelBuilder;
using Models.DMSA.Mbw.Sales;
using Models.DMSA.Shared.Security;
using Models.DMSA.Shared.Tools;
using System.Configuration;

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

//AppDbContext appDbContext = new AppDbContext();
builder.Services.AddDbContext<AppDbContext>(p=>p.UseOracle(connection.GetConnectionString(),
                b => b.UseOracleSQLCompatibility("11")));

// Add services to the container.
builder.Services.AddProblemDetails();

builder.Services.AddControllers()
    .AddOData(opt =>
    {
        var odataBuilder = new ODataConventionModelBuilder();
        odataBuilder.EntitySet<FacBonificadosXArticulo>("Bonificados");
        opt.AddRouteComponents("odata", odataBuilder.GetEdmModel())
           .EnableQueryFeatures();
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddSingleton<EcommerceService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//Para la seguridad
//app.UseMiddleware<ApiKeyMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.UseStaticFiles();

var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".apk"] = "application/vnd.android.package-archive";
provider.Mappings[".json"] = "application/json";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider    
});

app.Run();
