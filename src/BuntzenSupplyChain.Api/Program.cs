using BuntzenSupplyChain.Application.Interfaces;
using BuntzenSupplyChain.Infrastructure.Data;
using BuntzenSupplyChain.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add ASP.NET Core MVC Controllers with Views and API Controllers
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register EF Core DbContext (Supports MS SQL Server Container or SQLite Fallback)
var sqlConnectionString = builder.Configuration.GetConnectionString("SqlServer") 
                          ?? "Server=localhost,1433;Database=BuntzenSupplyChainDB;User Id=sa;Password=PHSA_SC_Perf_2026!;TrustServerCertificate=True;";
var useSqlServer = builder.Configuration.GetValue<bool>("UseSqlServer");

builder.Services.AddDbContext<BuntzenDbContext>(options =>
{
    if (useSqlServer)
    {
        options.UseSqlServer(sqlConnectionString);
    }
    else
    {
        options.UseSqlite("Data Source=BuntzenSupplyChain.db");
    }
});

// Register Custom Services
builder.Services.AddScoped<IXmlXsltTransformationService, XmlXsltTransformationService>();
builder.Services.AddScoped<ISqlPerformanceTuningService, SqlPerformanceTuningService>();

var app = builder.Build();

// Seed Database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BuntzenDbContext>();
    await DbSeeder.SeedAsync(db);
}

// Configure HTTP Request Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PHSA SC Performance API v1");
    });
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Map ASP.NET MVC Default Route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map Web API Controllers
app.MapControllers();

app.Run();
