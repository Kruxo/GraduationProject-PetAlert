using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using GraduationProject.Controllers;
using GraduationProject.Models;
using GraduationProject.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class PetTypeControllerTests
{
    private readonly Mock<ApplicationDbContext> _mockDbContext;
    private readonly PetTypeController _controller;

    public PetTypeControllerTests()
    {
        // Create a mock ApplicationDbContext
        _mockDbContext = new Mock<ApplicationDbContext>(
            new DbContextOptions<ApplicationDbContext>()
        );

        // Initialize the controller with the mocked DbContext
        _controller = new PetTypeController(_mockDbContext.Object);
    }

    [Fact]
    public async Task Create_ValidModel_ReturnsRedirectToActionResult()
    {
        // Arrange
        var createVm = new PetTypeCreateVm
        {
            Type = "Dog",
            Image = "dog_image_url"
        };

        // Set up mock context to mimic a successful SaveChangesAsync call
        _mockDbContext.Setup(db => db.Add(It.IsAny<PetType>()));
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _controller.Create(createVm);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult.ActionName.Should().Be("Index");
    }

    [Fact]
    public async Task Create_InvalidModel_ReturnsViewResult()
    {
        // Arrange
        var createVm = new PetTypeCreateVm
        {
            Type = "", // Invalid data, Type is required
            Image = "dog_image_url"
        };

        _controller.ModelState.AddModelError("Type", "Type is required");

        // Act
        var result = await _controller.Create(createVm);

        // Assert
        result.Should().BeOfType<ViewResult>();
    }
}
