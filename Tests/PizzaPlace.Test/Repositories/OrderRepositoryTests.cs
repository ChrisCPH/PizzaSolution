using PizzaPlace.DB;
using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Repositories;

namespace PizzaPlace.Test.Repositories
{
    [TestClass]
    public class OrderRepositoryTests
    {
        private static IOrderRepository CreateRepository(AppDbContext context)
            => new OrderRepository(context);

        private static Order GetPendingOrder() => new Order
        {
            Id = Guid.NewGuid(),
            State = OrderState.Pending,
            CreatedAt = DateTimeOffset.UtcNow,
            LineItems = []
        };

        private static async Task<PizzaRecipe> AddRecipe(AppDbContext context, PizzaRecipeType type)
        {
            var recipe = new PizzaRecipe { RecipeType = type, CookingTimeMinutes = 10 };
            context.PizzaRecipes.Add(recipe);
            await context.SaveChangesAsync();
            return recipe;
        }

        [TestMethod]
        public async Task AddOrder_ShouldInsertIntoDatabase()
        {
            // Arrange
            using var context = TestDbFactory.Create();
            var repo = CreateRepository(context);
            var order = GetPendingOrder();

            // Act
            var id = await repo.AddOrder(order);

            // Assert
            Assert.AreEqual(order.Id, id);
            Assert.AreEqual(1, context.Orders.Count());
        }

        [TestMethod]
        public async Task AddOrder_ShouldPersistLineItems()
        {
            // Arrange
            using var context = TestDbFactory.Create();
            var repo = CreateRepository(context);
            var recipe = await AddRecipe(context, PizzaRecipeType.StandardPizza);
            var order = GetPendingOrder();
            order.LineItems = [new OrderLineItem { PizzaRecipeId = recipe.Id, Amount = 5, CompletedAmount = 0 }];

            // Act
            await repo.AddOrder(order);

            // Assert
            Assert.AreEqual(1, context.OrderLineItems.Count());
            Assert.AreEqual(5, context.OrderLineItems.First().Amount);
        }

        [TestMethod]
        public async Task GetOrder_ShouldReturnCorrectOrder()
        {
            // Arrange
            using var context = TestDbFactory.Create();
            var repo = CreateRepository(context);
            var order = GetPendingOrder();
            await repo.AddOrder(order);

            // Act
            var result = await repo.GetOrder(order.Id);

            // Assert
            Assert.AreEqual(order.Id, result.Id);
            Assert.AreEqual(OrderState.Pending, result.State);
        }

        [TestMethod]
        public async Task GetOrder_ShouldIncludeLineItemsWithRecipe()
        {
            // Arrange
            using var context = TestDbFactory.Create();
            var repo = CreateRepository(context);
            var recipe = await AddRecipe(context, PizzaRecipeType.StandardPizza);
            var order = GetPendingOrder();
            order.LineItems = [new OrderLineItem { PizzaRecipeId = recipe.Id, Amount = 3, CompletedAmount = 0 }];
            await repo.AddOrder(order);

            // Act
            var result = await repo.GetOrder(order.Id);

            // Assert
            Assert.AreEqual(1, result.LineItems.Count);
            Assert.AreEqual(PizzaRecipeType.StandardPizza, result.LineItems.First().PizzaRecipe.RecipeType);
        }

        [TestMethod]
        public async Task GetOrder_NonExistentOrder_ShouldThrow()
        {
            // Arrange
            using var context = TestDbFactory.Create();
            var repo = CreateRepository(context);
            var missingId = Guid.NewGuid();

            // Act
            var ex = await Assert.ThrowsExceptionAsync<PizzaException>(() => repo.GetOrder(missingId));

            // Assert
            Assert.AreEqual($"Order {missingId} not found.", ex.Message);
        }

        [TestMethod]
        public async Task UpdateOrder_ShouldUpdateState()
        {
            // Arrange
            using var context = TestDbFactory.Create();
            var repo = CreateRepository(context);
            var order = GetPendingOrder();
            await repo.AddOrder(order);

            // Act
            await repo.UpdateOrder(new Order { Id = order.Id, State = OrderState.Completed, LineItems = [] });

            // Assert
            Assert.AreEqual(OrderState.Completed, context.Orders.First().State);
        }

        [TestMethod]
        public async Task UpdateOrder_ShouldUpdateCompletedAmount()
        {
            // Arrange
            using var context = TestDbFactory.Create();
            var repo = CreateRepository(context);
            var recipe = await AddRecipe(context, PizzaRecipeType.StandardPizza);
            var order = GetPendingOrder();
            order.LineItems = [new OrderLineItem { PizzaRecipeId = recipe.Id, Amount = 5, CompletedAmount = 0 }];
            await repo.AddOrder(order);

            // Act
            await repo.UpdateOrder(new Order
            {
                Id = order.Id,
                State = OrderState.Completed,
                LineItems = [new OrderLineItem { PizzaRecipeId = recipe.Id, CompletedAmount = 5 }]
            });

            // Assert
            Assert.AreEqual(5, context.OrderLineItems.First().CompletedAmount);
        }

        [TestMethod]
        public async Task UpdateOrder_ShouldPersistErrorMessage()
        {
            // Arrange
            using var context = TestDbFactory.Create();
            var repo = CreateRepository(context);
            var order = GetPendingOrder();
            await repo.AddOrder(order);

            // Act
            await repo.UpdateOrder(new Order { Id = order.Id, State = OrderState.Failed, Error = "Oven broke.", LineItems = [] });

            // Assert
            var saved = context.Orders.First();
            Assert.AreEqual(OrderState.Failed, saved.State);
            Assert.AreEqual("Oven broke.", saved.Error);
        }

        [TestMethod]
        public async Task UpdateOrder_NonExistentOrder_ShouldThrow()
        {
            // Arrange
            using var context = TestDbFactory.Create();
            var repo = CreateRepository(context);
            var missingId = Guid.NewGuid();

            // Act
            var ex = await Assert.ThrowsExceptionAsync<PizzaException>(
                () => repo.UpdateOrder(new Order { Id = missingId, State = OrderState.Completed, LineItems = [] }));

            // Assert
            Assert.AreEqual($"Order {missingId} not found.", ex.Message);
        }
    }
}