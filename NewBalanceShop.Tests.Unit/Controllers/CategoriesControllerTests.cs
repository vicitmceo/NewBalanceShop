using Microsoft.AspNetCore.Mvc;
using Moq;
using NewBalanceShop.Application.DTO;
using NewBalanceShop.Application.Interfaces;
using NewBalanceShop.Presentation.Controllers;
using Xunit;

namespace NewBalanceShop.Tests.Unit.Controllers;

public class CategoriesControllerTests
{
    [Fact]
    public async Task GetAll_ReturnsCategoriesFromService()
    {
        var service = new Mock<ICategoryService>();
        service.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<CategoryDto> { new() { Id = 1, Name = "Кросівки" } });
        var sut = new CategoriesController(service.Object);

        var result = await sut.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var categories = Assert.IsAssignableFrom<IEnumerable<CategoryDto>>(ok.Value);
        Assert.Equal("Кросівки", Assert.Single(categories).Name);
    }
}
