using Microsoft.EntityFrameworkCore;
using PizzaPlace;
using PizzaPlace.DB;
using PizzaPlace.Factories;
using PizzaPlace.Repositories;
using PizzaPlace.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(o =>
{
    o.AddPolicy("allowAll",
        policy =>
        {
            policy.WithOrigins(
            // "whatEver.homepage.com"
            );
            policy.AllowAnyHeader();
            policy.AllowCredentials();
            policy.AllowAnyMethod();
        });
});

builder.Services.AddControllers();
builder.Services.AddOpenApiDocument(d =>
{
    d.Title = "Pizza Place";
    d.Version = "v1";
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register services:
var services = builder.Services;
services.AddSingleton(TimeProvider.System);

//var fakeStockRepo = new FakeStockRepository();
//fakeStockRepo.AddStandardStock();
//services.AddSingleton<IStockRepository>(fakeStockRepo);

//var fakeRecipeRepo = new FakeRecipeRepository();
//fakeRecipeRepo.AddStandardRecipes();
//services.AddSingleton<IRecipeRepository>(fakeRecipeRepo);

services.AddScoped<IStockRepository, StockRepository>();
services.AddScoped<IRecipeRepository, RecipeRepository>();

services.AddScoped<StockSeeder>();
services.AddScoped<PizzaRecipeSeeder>();

services.AddTransient<IPizzaOven, NormalPizzaOven>();
services.AddTransient<IPizzaOven, AssemblyLinePizzaOven>();
services.AddTransient<IPizzaOven, GiantRevolvingPizzaOven>();

services.AddTransient<IStockService, StockService>();
services.AddTransient<IRecipeService, RecipeService>();
services.AddScoped<IOrderingService, OrderingService>();
services.AddTransient<IMenuService, MenuService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var stockSeeder = scope.ServiceProvider.GetRequiredService<StockSeeder>();
    await stockSeeder.SeedAsync();

    var recipeSeeder = scope.ServiceProvider.GetRequiredService<PizzaRecipeSeeder>();
    await recipeSeeder.SeedAsync();
}

app.UseOpenApi();
app.UseSwaggerUi();

app.UseCors();

app.MapControllers();

app.Run();
