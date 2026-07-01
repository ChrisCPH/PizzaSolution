using PizzaPlace.DB;
using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Repositories;


namespace PizzaPlace.Test.Repositories
{
    [TestClass]
    public class MenuRepositoryTests
    {
        private static IMenuRepository CreateRepository(AppDbContext context)
            => new MenuRepository(context);

        private static async Task<PizzaRecipe> AddRecipe(AppDbContext context, PizzaRecipeType type)
        {
            var recipe = new PizzaRecipe { RecipeType = type, CookingTimeMinutes = 10 };
            context.PizzaRecipes.Add(recipe);
            await context.SaveChangesAsync();
            return recipe;
        }

        private static Menu GetMenu(string title = "Test Menu") =>
            new Menu { Title = title, Items = [] };

        [TestMethod]
        public async Task AddMenu_ShouldInsertIntoDatabase()
        {
            // Arrange
            using var context = TestDbFactory.Create();
            var repo = CreateRepository(context);
            var menu = GetMenu();

            // Act
            var id = await repo.AddMenu(menu);

            // Assert
            Assert.IsTrue(id > 0);
            Assert.AreEqual(1, context.Menus.Count());
        }

        [TestMethod]
        public async Task AddMenu_ShouldPersistTitle()
        {
            // Arrange
            using var context = TestDbFactory.Create();
            var repo = CreateRepository(context);
            var menu = GetMenu("Lunch Menu");

            // Act
            await repo.AddMenu(menu);

            // Assert
            Assert.AreEqual("Lunch Menu", context.Menus.First().Title);
        }

        [TestMethod]
        public async Task GetMenu_ShouldReturnCorrectMenu()
        {
            // Arrange
            using var context = TestDbFactory.Create();
            var repo = CreateRepository(context);
            var menu = GetMenu("Standard Menu");
            await repo.AddMenu(menu);

            // Act
            var result = await repo.GetMenu(menu.Id);

            // Assert
            Assert.AreEqual("Standard Menu", result.Title);
        }

        [TestMethod]
        public async Task GetMenu_ShouldIncludeItemsWithRecipe()
        {
            // Arrange
            using var context = TestDbFactory.Create();
            var repo = CreateRepository(context);
            var recipe = await AddRecipe(context, PizzaRecipeType.StandardPizza);
            var menu = GetMenu();
            menu.Items = [new MenuItem { Description = "Margherita", Price = 89, PizzaRecipeId = recipe.Id }];
            await repo.AddMenu(menu);

            // Act
            var result = await repo.GetMenu(menu.Id);

            // Assert
            Assert.AreEqual(1, result.Items.Count);
            Assert.AreEqual(PizzaRecipeType.StandardPizza, result.Items.First().PizzaRecipe.RecipeType);
        }

        [TestMethod]
        public async Task GetMenu_NonExistentMenu_ShouldThrow()
        {
            // Arrange
            using var context = TestDbFactory.Create();
            var repo = CreateRepository(context);

            // Act
            var ex = await Assert.ThrowsExceptionAsync<PizzaException>(() => repo.GetMenu(999));

            // Assert
            Assert.AreEqual("Menu 999 not found.", ex.Message);
        }

        // GetAllMenus

        [TestMethod]
        public async Task GetAllMenus_ShouldReturnAllMenus()
        {
            // Arrange
            using var context = TestDbFactory.Create();
            var repo = CreateRepository(context);
            await repo.AddMenu(GetMenu("Standard Menu"));
            await repo.AddMenu(GetMenu("Lunch Menu"));

            // Act
            var result = await repo.GetAllMenus();

            // Assert
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public async Task GetAllMenus_Empty_ShouldReturnEmptyList()
        {
            // Arrange
            using var context = TestDbFactory.Create();
            var repo = CreateRepository(context);

            // Act
            var result = await repo.GetAllMenus();

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task UpdateMenu_ShouldUpdateTitle()
        {
            // Arrange
            using var context = TestDbFactory.Create();
            var repo = CreateRepository(context);
            var menu = GetMenu("Old Title");
            await repo.AddMenu(menu);

            // Act
            await repo.UpdateMenu(new Menu { Id = menu.Id, Title = "New Title", Items = [] });

            // Assert
            Assert.AreEqual("New Title", context.Menus.First().Title);
        }

        [TestMethod]
        public async Task UpdateMenu_NonExistentMenu_ShouldThrow()
        {
            // Arrange
            using var context = TestDbFactory.Create();
            var repo = CreateRepository(context);

            // Act
            var ex = await Assert.ThrowsExceptionAsync<PizzaException>(
                () => repo.UpdateMenu(new Menu { Id = 999, Title = "Ghost Menu", Items = [] }));

            // Assert
            Assert.AreEqual("Menu 999 not found.", ex.Message);
        }

        [TestMethod]
        public async Task DeleteMenu_ShouldRemoveFromDatabase()
        {
            // Arrange
            using var context = TestDbFactory.Create();
            var repo = CreateRepository(context);
            var menu = GetMenu();
            await repo.AddMenu(menu);

            // Act
            await repo.DeleteMenu(menu.Id);

            // Assert
            Assert.AreEqual(0, context.Menus.Count());
        }

        [TestMethod]
        public async Task DeleteMenu_NonExistentMenu_ShouldThrow()
        {
            // Arrange
            using var context = TestDbFactory.Create();
            var repo = CreateRepository(context);

            // Act
            var ex = await Assert.ThrowsExceptionAsync<PizzaException>(() => repo.DeleteMenu(999));

            // Assert
            Assert.AreEqual("Menu 999 not found.", ex.Message);
        }

        [TestMethod]
        public async Task AddMenuItem_ShouldInsertIntoDatabase()
        {
            // Arrange
            using var context = TestDbFactory.Create();
            var repo = CreateRepository(context);
            var recipe = await AddRecipe(context, PizzaRecipeType.StandardPizza);
            var menu = GetMenu();
            await repo.AddMenu(menu);

            // Act
            var id = await repo.AddMenuItem(new MenuItem { Description = "Margherita", Price = 89, MenuId = menu.Id, PizzaRecipeId = recipe.Id });

            // Assert
            Assert.IsTrue(id > 0);
            Assert.AreEqual(1, context.MenuItems.Count());
        }

        [TestMethod]
        public async Task AddMenuItem_NonExistentMenu_ShouldThrow()
        {
            // Arrange
            using var context = TestDbFactory.Create();
            var repo = CreateRepository(context);
            var recipe = await AddRecipe(context, PizzaRecipeType.StandardPizza);

            // Act
            var ex = await Assert.ThrowsExceptionAsync<PizzaException>(
                () => repo.AddMenuItem(new MenuItem { Description = "Margherita", Price = 89, MenuId = 999, PizzaRecipeId = recipe.Id }));

            // Assert
            Assert.AreEqual("Menu 999 not found.", ex.Message);
        }

        [TestMethod]
        public async Task UpdateMenuItem_ShouldUpdateFields()
        {
            // Arrange
            using var context = TestDbFactory.Create();
            var repo = CreateRepository(context);
            var recipe = await AddRecipe(context, PizzaRecipeType.StandardPizza);
            var menu = GetMenu();
            await repo.AddMenu(menu);
            var itemId = await repo.AddMenuItem(new MenuItem { Description = "Old Name", Price = 89, MenuId = menu.Id, PizzaRecipeId = recipe.Id });

            // Act
            await repo.UpdateMenuItem(new MenuItem { Id = itemId, Description = "New Name", Price = 99, SoldOut = true, PizzaRecipeId = recipe.Id });

            // Assert
            var saved = context.MenuItems.First();
            Assert.AreEqual("New Name", saved.Description);
            Assert.AreEqual(99, saved.Price);
            Assert.IsTrue(saved.SoldOut);
        }

        [TestMethod]
        public async Task UpdateMenuItem_NonExistentItem_ShouldThrow()
        {
            // Arrange
            using var context = TestDbFactory.Create();
            var repo = CreateRepository(context);
            var recipe = await AddRecipe(context, PizzaRecipeType.StandardPizza);

            // Act
            var ex = await Assert.ThrowsExceptionAsync<PizzaException>(
                () => repo.UpdateMenuItem(new MenuItem { Id = 999, Description = "Ghost", Price = 0, PizzaRecipeId = recipe.Id }));

            // Assert
            Assert.AreEqual("MenuItem 999 not found.", ex.Message);
        }

        [TestMethod]
        public async Task DeleteMenuItem_ShouldRemoveFromDatabase()
        {
            // Arrange
            using var context = TestDbFactory.Create();
            var repo = CreateRepository(context);
            var recipe = await AddRecipe(context, PizzaRecipeType.StandardPizza);
            var menu = GetMenu();
            await repo.AddMenu(menu);
            var itemId = await repo.AddMenuItem(new MenuItem { Description = "Margherita", Price = 89, MenuId = menu.Id, PizzaRecipeId = recipe.Id });

            // Act
            await repo.DeleteMenuItem(itemId);

            // Assert
            Assert.AreEqual(0, context.MenuItems.Count());
        }

        [TestMethod]
        public async Task DeleteMenuItem_NonExistentItem_ShouldThrow()
        {
            // Arrange
            using var context = TestDbFactory.Create();
            var repo = CreateRepository(context);

            // Act
            var ex = await Assert.ThrowsExceptionAsync<PizzaException>(() => repo.DeleteMenuItem(999));

            // Assert
            Assert.AreEqual("MenuItem 999 not found.", ex.Message);
        }
    }
}
