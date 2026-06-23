using Microsoft.AspNetCore.Mvc;
using PizzaPlace.Controllers;
using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaPlace.Test.Controllers
{
    [TestClass]
    public class RecipeControllerTests
    {
        private static RecipeController GetController(Mock<IRecipeService> recipeService) =>
            new(recipeService.Object);

        [TestMethod]
        public async Task AddRecipe()
        {
            // Arrange
            var recipe = new PizzaRecipeDto(PizzaRecipeType.StandardPizza,
                [new StockDto(StockType.Dough, 1)], 10);

            var recipeService = new Mock<IRecipeService>(MockBehavior.Strict);
            recipeService.Setup(x => x.AddRecipe(recipe))
                .ReturnsAsync(recipe);

            var controller = GetController(recipeService);

            // Act
            var actual = await controller.AddRecipe(recipe);

            // Assert
            Assert.IsInstanceOfType<OkObjectResult>(actual);
            recipeService.VerifyAll();
        }

        [TestMethod]
        public async Task UpdateRecipe()
        {
            // Arrange
            var recipe = new PizzaRecipeDto(PizzaRecipeType.StandardPizza,
                [new StockDto(StockType.Dough, 2)], 15);

            var recipeService = new Mock<IRecipeService>(MockBehavior.Strict);
            recipeService.Setup(x => x.UpdateRecipe(recipe))
                .ReturnsAsync(recipe);

            var controller = GetController(recipeService);

            // Act
            var actual = await controller.UpdateRecipe(recipe);

            // Assert
            Assert.IsInstanceOfType<OkObjectResult>(actual);
            recipeService.VerifyAll();
        }
    }
}
