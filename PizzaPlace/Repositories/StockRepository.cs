using Microsoft.EntityFrameworkCore;
using PizzaPlace;
using PizzaPlace.DB;
using PizzaPlace.Models;
using PizzaPlace.Models.Mapping;
using PizzaPlace.Models.Types;
using PizzaPlace.Repositories;

public class StockRepository : IStockRepository
{
    private readonly AppDbContext _context;

    public StockRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<StockDto> AddToStock(StockDto stock)
    {
        if (stock.Amount < 0)
            throw new PizzaException("Stock cannot have negative amount.");

        var existing = await _context.InventoryItems
            .FirstOrDefaultAsync(x => x.StockType == stock.StockType);

        if (existing != null)
        {
            existing.Amount += stock.Amount;

            await _context.SaveChangesAsync();
            return existing.ToDto();
        }

        var entity = stock.ToEntity();
        _context.InventoryItems.Add(entity);

        await _context.SaveChangesAsync();

        return entity.ToDto();
    }

    public async Task<StockDto> GetStock(StockType stockType)
    {
        var stock = await _context.InventoryItems
            .FirstOrDefaultAsync(x => x.StockType == stockType);

        return stock?.ToDto() ?? new StockDto(stockType, 0);
    }

    public async Task<StockDto> TakeStock(StockType stockType, int amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Unable to take zero or negative amount.");

        var stock = await _context.InventoryItems
            .FirstOrDefaultAsync(x => x.StockType == stockType);

        if (stock == null || stock.Amount < amount)
            throw new PizzaException("Not enough stock to take the given amount.");

        stock.Amount -= amount;

        await _context.SaveChangesAsync();

        return new StockDto(stockType, amount);
    }
}