using PizzaPlace.Models;
using PizzaPlace.Repositories;

namespace PizzaPlace.Services;

public class StockService(IStockRepository stockRepository) : IStockService
{
    public async Task<bool> HasInsufficientStock(PizzaOrder order, ComparableList<PizzaRecipeDto> recipeDtos)
    {
        var required = order.RequestedOrder
            .GroupBy(x => x.PizzaType)
            .SelectMany(group =>
            {
                var recipe = recipeDtos.First(r => r.RecipeType == group.Key);
                var totalAmount = group.Sum(x => x.Amount);
                return recipe.Ingredients.Select(i => new StockDto(i.StockType, i.Amount * totalAmount));
            })
            .GroupBy(x => x.StockType)
            .Select(g => new StockDto(g.Key, g.Sum(x => x.Amount)));

        foreach (var ingredient in required)
        {
            var available = await stockRepository.GetStock(ingredient.StockType);
            if (available == null || available.Amount < ingredient.Amount)
                return true;
        }

        return false;
    }

    public async Task<ComparableList<StockDto>> GetStock(PizzaOrder order, ComparableList<PizzaRecipeDto> recipeDtos)
    {
        var stockTypes = recipeDtos
            .SelectMany(r => r.Ingredients)
            .Select(i => i.StockType)
            .Distinct();

        ComparableList<StockDto> stock = [];
        foreach (var stockType in stockTypes)
            stock.Add(await stockRepository.GetStock(stockType));

        return stock;
    }

    public async Task<ComparableList<StockDto>> Restock(ComparableList<StockDto> stock)
    {
        ComparableList<StockDto> updatedStock = [];
        foreach (var stockDto in stock)
            updatedStock.Add(await stockRepository.AddToStock(stockDto));

        return updatedStock;
    }
}
