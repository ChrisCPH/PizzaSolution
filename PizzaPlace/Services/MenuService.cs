using PizzaPlace.Models;
using PizzaPlace.Repositories;

namespace PizzaPlace.Services;

public class MenuService(
    IMenuRepository menuRepository,
    IRecipeRepository recipeRepository,
    IStockRepository stockRepository) : IMenuService
    {

    private const string StandardMenuTitle = "Standard Menu";
    private const string LunchMenuTitle = "Lunch Menu";

    public async Task<Menu> GetMenu(DateTimeOffset menuDate)
    {
        var hour = menuDate.UtcDateTime.Hour;
        var title = hour >= 11 && hour < 14 ? LunchMenuTitle : StandardMenuTitle;

        var menus = await menuRepository.GetAllMenus();
        var menu = menus.FirstOrDefault(m => m.Title == title);

        if (menu == null)
            throw new PizzaException($"Menu '{title}' not found.");

        return menu;
    }

    public async Task<long> AddMenu(Menu menu)
    {
        var existing = await menuRepository.GetAllMenus();
        if (existing.Any(m => m.Title == menu.Title))
            throw new PizzaException($"A menu with the title '{menu.Title}' already exists.");

        return await menuRepository.AddMenu(menu);
    }
    public Task<Menu> GetMenuById(long menuId) => menuRepository.GetMenu(menuId);
    public Task<List<Menu>> GetAllMenus() => menuRepository.GetAllMenus();
    public async Task<Menu> UpdateMenu(Menu menu)
    {
        var existing = await menuRepository.GetAllMenus();
        if (existing.Any(m => m.Title == menu.Title && m.Id != menu.Id))
            throw new PizzaException($"A menu with the title '{menu.Title}' already exists.");

        return await menuRepository.UpdateMenu(menu);
    }

    public Task DeleteMenu(long menuId) =>
        menuRepository.DeleteMenu(menuId);

    public async Task<long> AddMenuItem(MenuItem item)
    {
        await ValidateRecipeExists(item.PizzaRecipeId);
        return await menuRepository.AddMenuItem(item);
    }

    public async Task<MenuItem> UpdateMenuItem(MenuItem item)
    {
        await ValidateRecipeExists(item.PizzaRecipeId);

        var existing = await menuRepository.GetMenuItemById(item.Id);

        if (existing.SoldOut && !item.SoldOut)
            await ValidateSufficientStockForRecipe(item.PizzaRecipeId);

        return await menuRepository.UpdateMenuItem(item);
    }

    public Task DeleteMenuItem(long itemId) =>
        menuRepository.DeleteMenuItem(itemId);

    private async Task ValidateRecipeExists(long pizzaRecipeId)
    {
        var recipe = await recipeRepository.GetRecipeById(pizzaRecipeId);
        if (recipe == null)
            throw new PizzaException($"Recipe {pizzaRecipeId} does not exist.");
    }

    private async Task ValidateSufficientStockForRecipe(long pizzaRecipeId)
    {
        var recipe = await recipeRepository.GetRecipeById(pizzaRecipeId);
        if (recipe == null)
            throw new PizzaException($"Recipe {pizzaRecipeId} does not exist.");

        foreach (var ingredient in recipe.Ingredients)
        {
            var stock = await stockRepository.GetStock(ingredient.StockType);
            if (stock.Amount < ingredient.Amount)
                throw new PizzaException($"Insufficient stock to un-sold-out this item. Not enough {ingredient.StockType}.");
        }
    }
}