using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using PCGameRate.Controllers;
using PCGameRate.Data;
using PCGameRate.Models;
using PCGameRate.ViewModels.Rating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PCGameRate.Tests
{
    public class RatingControllerTests
    {
        private readonly RatingController _controller;
        private readonly ApplicationDbContext _context;
        private readonly Mock<UserManager<User>> _userManagerMock;

        public RatingControllerTests()
        {
            
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"RatingControllerTestDb_{Guid.NewGuid()}")
            .Options;

            _context = new ApplicationDbContext(options);

           
            var store = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(
                store.Object, null, null, null, null, null, null, null, null
            );

          
            SeedDatabase();

           
            _controller = new RatingController(_context, _userManagerMock.Object);

           
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.NameIdentifier, "user1"),
            new Claim(ClaimTypes.Role, "user")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            var tempData = new TempDataDictionary(_controller.ControllerContext.HttpContext, Mock.Of<ITempDataProvider>());
            _controller.TempData = tempData;
        }

        private void SeedDatabase()
        {
            var game = new Game
            {
                GameId = 1,
                Title = "Game 1",
                RatingAverage = 4,
                RatingCount = 1,
                Description = "Some info",
                Developer = new Developer { DeveloperName = "Dev1" },
                Genre = new Genre { GameGenre = "Action" },
                Image = "image.png",
                ReleaseDate = "2020"
            };

            var user = new User
            {
                Id = "user1",
                UserName = "testuser"
            };

            var rating = new Rating
            {
                Id = "user1",
                GameId = 1,
                RatingValue = 4,
                RatingDate = System.DateTime.UtcNow
            };

            _context.Games.Add(game);
            _context.Users.Add(user);
            _context.Ratings.Add(rating);
            _context.SaveChanges();

            _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user1");
        }

        [Fact]
        public async Task RateGame_Get_ReturnsViewWithGame_WhenGameExists()
        {
            // Act
            var result = await _controller.RateGame(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Game>(viewResult.ViewData.Model);
            Assert.Equal("Game 1", model.Title);
        }

        [Fact]
        public async Task RateGame_Get_ReturnsNotFound_WhenGameDoesNotExist()
        {
            var result = await _controller.RateGame(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task RateGame_Post_UpdatesExistingRating()
        {
            // Act
            var result = await _controller.RateGame(1, 5);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Detail", redirect.ActionName);

            var updatedRating = await _context.Ratings.FirstOrDefaultAsync(r => r.GameId == 1 && r.Id == "user1");
            Assert.Equal(5, updatedRating.RatingValue);
        }

        [Fact]
        public async Task DeleteRating_RemovesRating_WhenExists()
        {
            // Act
            var result = await _controller.DeleteRating(1);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Detail", redirect.ActionName);

            var deletedRating = await _context.Ratings.FirstOrDefaultAsync(r => r.GameId == 1 && r.Id == "user1");
            Assert.Null(deletedRating);
        }

        [Fact]
        public async Task RatingList_ReturnsViewWithRatings()
        {
            var result = await _controller.RatingList(null);

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<RatedGameViewModel>>(view.Model);
            Assert.Single(model);
        }
    }
}
