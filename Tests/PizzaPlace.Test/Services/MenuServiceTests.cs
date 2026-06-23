using PizzaPlace.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaPlace.Test.Services
{
    [TestClass]
    public class MenuServiceTests
    {
        [TestMethod]
        public async Task GetMenu()
        {
            // Arrange
            var menuService = new MenuService();
            var standardMenuTime = new DateTimeOffset(2024, 6, 1, 10, 0, 0, TimeSpan.Zero); // 10:00 UTC

            // Act
            var menu = menuService.GetMenu(standardMenuTime);

            // Assert
            Assert.AreEqual("Standard Menu", menu.Title);
        }

        [TestMethod]
        public async Task GetMenu_ReturnsLunchMenuDuringLunchHours()
        {
            // Arrange
            var menuService = new MenuService();
            var lunchTime = new DateTimeOffset(2024, 6, 1, 12, 0, 0, TimeSpan.Zero); // 12:00 UTC
            // Act
            var menu = menuService.GetMenu(lunchTime);
            // Assert
            Assert.AreEqual("Lunch Menu", menu.Title);
        }
    }
}
