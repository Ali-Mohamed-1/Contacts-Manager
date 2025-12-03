using ServiceContracts;
using Services;
using Microsoft.EntityFrameworkCore;
using Entities.Data;
using RepositoryContracts;
using Repositories;
using Serilog;
using CRUDExample.Middleware;
using Entities.IdentityEntities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Logging with Serilog
builder.Host.UseSerilog((HostBuilderContext context, IServiceProvider services, LoggerConfiguration loggerConfig) =>
{
    loggerConfig
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services);
});

builder.Services.AddControllersWithViews();

// This scans the assembly where 'CountriesAdderService' exists
builder.Services.Scan(scan => scan
    .FromAssemblyOf<CountriesAdderService>() // Target the assembly containing your services
    .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Service"))) // Select classes ending in "Service"
    .AsImplementedInterfaces() // Automatically pairs Service -> IService
    .WithScopedLifetime()); // Registers them all as Scoped

builder.Services.AddScoped<ICountriesRepository, CountriesRepo>();
builder.Services.AddScoped<IPersonsRepository, PersonsRepo>();

// identity
builder.Services
    // entire app level
    .AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    // repository level
    .AddUserStore<UserStore<ApplicationUser, ApplicationRole, AppDbContext, Guid>>()
    .AddRoleStore<RoleStore<ApplicationRole, AppDbContext, Guid>>();

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

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandlingMiddleware();
}

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