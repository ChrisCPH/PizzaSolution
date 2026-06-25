namespace PizzaPlace.Models.Mapping
{
    public static class StockMapping
    {
        public static StockDto ToDto(this Stock entity)
        {
            return new StockDto(
                entity.StockType,
                entity.Amount,
                entity.Id
            );
        }

        public static Stock ToStockEntity(this StockDto dto)
        {
            return new Stock
            {
                Id = dto.Id,
                StockType = dto.StockType,
                Amount = dto.Amount
            };
        }
    }
}
