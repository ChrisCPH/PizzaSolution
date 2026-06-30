using Microsoft.AspNetCore.Mvc;
using PizzaPlace.Controllers;
using PizzaPlace.Models;
using PizzaPlace.Services;

namespace PizzaPlace.Test.Controllers;

[TestClass]
public class OrderingControllerTests
{
    private static OrderingController GetController(Mock<IOrderingService> orderingService) =>
        new(orderingService.Object);

    [TestMethod]
    public async Task PlacePizzaOrder()
    {
        // Arrange
        var request = new PizzaOrderRequest([new MenuOrderAmount(10, 1)]);
        var result = new PizzaOrderResult(Guid.NewGuid(), 5, 80, [new OrderedItemResult("Mystery Pizza", 1, 80)]);

        var orderingService = new Mock<IOrderingService>(MockBehavior.Strict);
        orderingService.Setup(x => x.HandlePizzaOrder(request))
            .ReturnsAsync(result);

        var controller = GetController(orderingService);

        // Act
        var actual = await controller.PlacePizzaOrder(request);

        // Assert
        Assert.IsInstanceOfType<OkObjectResult>(actual);
        orderingService.VerifyAll();
    }
}