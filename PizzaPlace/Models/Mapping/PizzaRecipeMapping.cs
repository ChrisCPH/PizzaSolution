namespace PizzaPlace.Models.Mapping
{
    public static class PizzaRecipeMapping
    {
        public static PizzaRecipeDto ToDto(this PizzaRecipe entity)
        {
            return new PizzaRecipeDto(
                entity.RecipeType,
                new ComparableList<StockDto>(
                    entity.Ingredients.Select(i => i.ToDto()).ToList()
                ),
                entity.CookingTimeMinutes,
                entity.Id
            );
        }

        public static PizzaRecipe ToEntity(this PizzaRecipeDto dto)
        {
            return new PizzaRecipe
            {
                Id = dto.Id,
                RecipeType = dto.RecipeType,
                CookingTimeMinutes = dto.CookingTimeMinutes,
                Ingredients = dto.Ingredients.Select(i => i.ToStockEntity()).ToList()
            };
        }
    }
}
