using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using PCGameRate.Controllers;
using PCGameRate.Data;
using PCGameRate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCGameRate.Tests
{
    public class ScreenshotControllerTests
    {
        private readonly ApplicationDbContext _context;
        private readonly ScreenshotController _controller;

        public ScreenshotControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "ScreenshotTestDb")
                .Options;

            _context = new ApplicationDbContext(options);
            _controller = new ScreenshotController(_context);
        }

        [Fact]
        public async Task AddScreenshot_ReturnsBadRequest_WhenUrlIsEmpty()
        {
            // Act
            var result = await _controller.AddScreenshot(1, null);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("URL не може бути порожнім.", badRequest.Value);
        }

        [Fact]
        public async Task AddScreenshot_AddsScreenshotAndRedirects_WhenValid()
        {
            // Arrange
            var gameId = 1;
            var url = "http://example.com/screenshot.jpg";

            // Act
            var result = await _controller.AddScreenshot(gameId, url);

            // Assert
            var screenshotInDb = await _context.Screenshots.FirstOrDefaultAsync(s => s.GameId == gameId && s.ScreenshotUrl == url);
            Assert.NotNull(screenshotInDb);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Detail", redirect.ActionName);
            Assert.Equal("Game", redirect.ControllerName);
            Assert.Equal(gameId, redirect.RouteValues["id"]);
        }

        [Fact]
        public async Task DeleteScreenshot_ReturnsNotFound_WhenScreenshotDoesNotExist()
        {
            // Act
            var result = await _controller.DeleteScreenshot(999); // id якого немає

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }

}
