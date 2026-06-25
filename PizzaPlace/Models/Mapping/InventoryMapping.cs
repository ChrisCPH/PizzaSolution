namespace PizzaPlace.Models.Mapping
{
    public static class InventoryMapping
    {
        public static StockDto ToDto(this InventoryItem entity) =>
            new StockDto(entity.StockType, entity.Amount, entity.Id);

        public static InventoryItem ToEntity(this StockDto dto) =>
            new InventoryItem
            {
                Id = dto.Id,
                StockType = dto.StockType,
                Amount = dto.Amount
            };
    }
}
