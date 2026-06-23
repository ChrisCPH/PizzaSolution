using Microsoft.AspNetCore.Mvc;
using PizzaPlace.Controllers;
using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Repositories;
using PizzaPlace.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaPlace.Test.Controllers
{
    [TestClass]
    public class RestockingControllerTests
    {
        private static RestockingController GetController(Mock<IStockService> stockService) =>
            new(stockService.Object);

        [TestMethod]
        public async Task Restock()
        {
            // Arrange
            ComparableList<StockDto> stock =
            [
                new StockDto(StockType.Dough, 10),
                new StockDto(StockType.Tomatoes, 5),
            ];

            ComparableList<StockDto> restockedStock =
            [
                new StockDto(StockType.Dough, 110),
                new StockDto(StockType.Tomatoes, 105),
            ];

            var stockService = new Mock<IStockService>(MockBehavior.Strict);
            stockService.Setup(x => x.Restock(stock))
                .ReturnsAsync(restockedStock);

            var controller = GetController(stockService);

            // Act
            var actual = await controller.Restock(stock);

            // Assert
            Assert.IsInstanceOfType<OkObjectResult>(actual);
            stockService.VerifyAll();
        }
    }
}
