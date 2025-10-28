using ServiceContracts;
using Services;
using Microsoft.EntityFrameworkCore;
using Entities.Data;
using RepositoryContracts;
using Repositories;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Logging with Serilog
builder.Host.UseSerilog((HostBuilderContext context, IServiceProvider services, LoggerConfiguration loggerConfig) =>
{
    loggerConfig
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services);
});

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<ICountryService, CountriesService>();
builder.Services.AddScoped<IPersonsService, PersonsService>();

builder.Services.AddScoped<ICountriesRepository, CountriesRepo>();
builder.Services.AddScoped<IPersonsRepository, PersonsRepo>();

builder.Services.AddHttpLogging();

// Conditionally register database provider based on environment
if (builder.Environment.IsEnvironment("Test"))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseInMemoryDatabase("TestingDB");
    });
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("connectionString"));
    });
}

var app = builder.Build();

app.UseHttpLogging();

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

app.Run();

public partial class Program { }
// this for making the Program class public for testing purposes
// and to do so we need to add it in our project file:
// <ItemGroup>
//       < InternalsVisibleTo Include = "CRUDtest" />
//  </ ItemGroup >