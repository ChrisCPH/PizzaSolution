using Moq;
using PizzaPlace.Models;
using PizzaPlace.Repositories;
using PizzaPlace.Services;

namespace PizzaPlace.Test.Services
{
    [TestClass]
    public class MenuServiceTests
    {
        private static MenuService GetService(Mock<IMenuRepository> menuRepository) =>
            new(menuRepository.Object);

        [TestMethod]
        public async Task GetMenu_ReturnsStandardMenuOutsideLunchHours()
        {
            // Arrange
            var standardMenuTime = new DateTimeOffset(2024, 6, 1, 10, 0, 0, TimeSpan.Zero); // 10:00 UTC
            var standardMenu = new Menu { Id = 1, Title = "Standard Menu", Items = [] };
            var lunchMenu = new Menu { Id = 2, Title = "Lunch Menu", Items = [] };

            var menuRepository = new Mock<IMenuRepository>(MockBehavior.Strict);
            menuRepository.Setup(x => x.GetAllMenus())
                .ReturnsAsync([standardMenu, lunchMenu]);

            var service = GetService(menuRepository);

            // Act
            var menu = await service.GetMenu(standardMenuTime);

            // Assert
            Assert.AreEqual("Standard Menu", menu.Title);
            menuRepository.VerifyAll();
        }

        [TestMethod]
        public async Task GetMenu_ReturnsLunchMenuDuringLunchHours()
        {
            // Arrange
            var lunchTime = new DateTimeOffset(2024, 6, 1, 12, 0, 0, TimeSpan.Zero); // 12:00 UTC
            var standardMenu = new Menu { Id = 1, Title = "Standard Menu", Items = [] };
            var lunchMenu = new Menu { Id = 2, Title = "Lunch Menu", Items = [] };

            var menuRepository = new Mock<IMenuRepository>(MockBehavior.Strict);
            menuRepository.Setup(x => x.GetAllMenus())
                .ReturnsAsync([standardMenu, lunchMenu]);

            var service = GetService(menuRepository);

            // Act
            var menu = await service.GetMenu(lunchTime);

            // Assert
            Assert.AreEqual("Lunch Menu", menu.Title);
            menuRepository.VerifyAll();
        }
    }
}