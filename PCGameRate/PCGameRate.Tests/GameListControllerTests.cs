using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using PCGameRate.Controllers;
using PCGameRate.Data;
using PCGameRate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PCGameRate.Tests
{
    public class GameListControllerTests
    {
        private readonly GameListController _controller;
        private readonly ApplicationDbContext _context;
        private readonly Mock<UserManager<User>> _userManagerMock;

        public GameListControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "GameListTestDb_" + System.Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            var store = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);

            _controller = new GameListController(_context, _userManagerMock.Object);
        }

        [Fact]
        public async Task ToggleGameList_AddsGame_WhenNotInList()
        {
            // Arrange
            var user = new User { Id = "user1" };
            _userManagerMock.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                            .ReturnsAsync(user);

            var game = new Game
            {
                GameId = 1,
                Title = "TestGame",
                Description = "Some description",
                Image = "test.jpg",
                ReleaseDate = "2023"
            };

            _context.Games.Add(game);
            await _context.SaveChangesAsync();
            // Act
            var result = await _controller.ToggleGameList(1);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var data = JsonSerializer.Deserialize<Dictionary<string, bool>>(JsonSerializer.Serialize(jsonResult.Value));
            // Assert
            Assert.True(data["isInGameList"]);
            Assert.True(_context.GameList.Any(gl => gl.GameId == 1 && gl.Id == "user1"));
        }

        [Fact]
        public async Task ToggleGameList_RemovesGame_WhenAlreadyInList()
        {
            // Arrange
            var user = new User { Id = "user1" };
            _userManagerMock.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                            .ReturnsAsync(user);

            var game = new Game
            {
                GameId = 1,
                Title = "TestGame",
                Description = "Some description",
                Image = "test.jpg",
                ReleaseDate = "2023"
            };

            _context.Games.Add(game);
            _context.GameList.Add(new GameListItem
            {
                Id = "user1",
                GameId = 1
            });

            await _context.SaveChangesAsync();
            // Act
            var result = await _controller.ToggleGameList(1);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var data = JsonSerializer.Deserialize<Dictionary<string, bool>>(JsonSerializer.Serialize(jsonResult.Value));
            // Assert
            Assert.False(data["isInGameList"]);
            Assert.False(_context.GameList.Any(gl => gl.GameId == 1 && gl.Id == "user1"));
        }

        [Fact]
        public async Task ToggleGameList_ReturnsUnauthorized_WhenUserIsNull()
        {
            // Arrange
            _userManagerMock.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                            .ReturnsAsync((User)null);
            // Act
            var result = await _controller.ToggleGameList(1);
            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

       
    }
}
