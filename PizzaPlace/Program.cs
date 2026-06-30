using Microsoft.EntityFrameworkCore;
using PizzaPlace;
using PizzaPlace.DB;
using PizzaPlace.Factories;
using PizzaPlace.Middleware;
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
services.AddScoped<IOrderRepository, OrderRepository>();
services.AddScoped<IMenuRepository, MenuRepository>();

services.AddScoped<InventorySeeder>();
services.AddScoped<PizzaRecipeSeeder>();
services.AddScoped<MenuSeeder>();

services.AddTransient<IPizzaOven, NormalPizzaOven>();
services.AddTransient<IPizzaOven, AssemblyLinePizzaOven>();
services.AddTransient<IPizzaOven, GiantRevolvingPizzaOven>();

services.AddTransient<IStockService, StockService>();
services.AddTransient<IRecipeService, RecipeService>();
services.AddScoped<IOrderingService, OrderingService>();
services.AddTransient<IMenuService, MenuService>();

services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var stockSeeder = scope.ServiceProvider.GetRequiredService<InventorySeeder>();
    await stockSeeder.SeedAsync();

    var recipeSeeder = scope.ServiceProvider.GetRequiredService<PizzaRecipeSeeder>();
    await recipeSeeder.SeedAsync();

    var menuSeeder = scope.ServiceProvider.GetRequiredService<MenuSeeder>();
    await menuSeeder.SeedAsync();
}

app.UseMiddleware<PizzaExceptionMiddleware>();

app.UseOpenApi();
app.UseSwaggerUi();

app.UseCors();

app.MapControllers();

app.Run();
