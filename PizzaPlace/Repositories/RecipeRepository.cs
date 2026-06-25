using Microsoft.EntityFrameworkCore;
using PizzaPlace;
using PizzaPlace.DB;
using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Models.Mapping;
using PizzaPlace.Repositories;

public class RecipeRepository : IRecipeRepository
{
    private readonly AppDbContext _context;

    public RecipeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<long> AddRecipe(PizzaRecipeDto recipe)
    {
        if (await _context.PizzaRecipes.AnyAsync(x => x.RecipeType == recipe.RecipeType))
            throw new PizzaException($"Recipe already added for {recipe.RecipeType}.");

        var entity = recipe.ToEntity();

        _context.PizzaRecipes.Add(entity);
        await _context.SaveChangesAsync();

        return entity.Id;
    }

    public async Task<PizzaRecipeDto> GetRecipe(PizzaRecipeType recipeType)
    {
        var recipe = await _context.PizzaRecipes
            .Include(x => x.Ingredients)
            .FirstOrDefaultAsync(x => x.RecipeType == recipeType);

        if (recipe == null)
            throw new PizzaException($"Recipe does not exist for {recipeType}.");

        return recipe.ToDto();
    }

    public async Task<PizzaRecipeDto> UpdateRecipe(PizzaRecipeDto recipe)
    {
        var existing = await _context.PizzaRecipes
            .Include(x => x.Ingredients)
            .FirstOrDefaultAsync(x => x.RecipeType == recipe.RecipeType);

        if (existing == null)
            throw new PizzaException($"Recipe does not exist for {recipe.RecipeType}.");

        existing.CookingTimeMinutes = recipe.CookingTimeMinutes;

        _context.RemoveRange(existing.Ingredients);
        existing.Ingredients = recipe.Ingredients
            .Select(i => i.ToStockEntity())
            .ToList();

        await _context.SaveChangesAsync();

        return existing.ToDto();
    }
}