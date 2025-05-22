using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using PCGameRate.Controllers;
using PCGameRate.Data;
using PCGameRate.Interfaces;
using PCGameRate.Models;
using PCGameRate.Services;
using PCGameRate.ViewModels.Review;
using System.Security.Claims;


namespace PCGameRate.Tests
{
    public class ReviewControllerTests
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<IGrammarCheckService> _grammarCheckServiceMock;
        private readonly Mock<IProfanityCheckService> _profanityCheckServiceMock;
        private readonly ReviewController _controller;

        public ReviewControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"ReviewControllerTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new ApplicationDbContext(options);

            var store = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
            _grammarCheckServiceMock = new Mock<IGrammarCheckService>();
            _profanityCheckServiceMock = new Mock<IProfanityCheckService>();

            _controller = new ReviewController(_context, _userManagerMock.Object,
                _grammarCheckServiceMock.Object, _profanityCheckServiceMock.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user1"),
                new Claim(ClaimTypes.Role, "user")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            _controller.TempData = new TempDataDictionary(
                _controller.ControllerContext.HttpContext,
                Mock.Of<ITempDataProvider>());
        }

        [Fact]
        public async Task Create_Post_ReturnsError_WhenGrammarErrorsExist()
        {
            // Arrange
            var model = new CreateReviewViewModel { Content = "Test content", GameId = 1 };
            _grammarCheckServiceMock.Setup(s => s.CheckGrammarAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<string> { "Error1" });

            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.True(_controller.TempData.ContainsKey("ErrorMessage"));
        }

        [Fact]
        public async Task Create_Post_ReturnsError_WhenProfanityExists()
        {
            // Arrange
            var model = new CreateReviewViewModel { Content = "bad word", GameId = 1 };
            _grammarCheckServiceMock.Setup(s => s.CheckGrammarAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<string>());
            _profanityCheckServiceMock.Setup(s => s.ContainsProfanity(model.Content))
                .Returns(true);

            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.True(_controller.TempData.ContainsKey("ErrorMessage"));
        }

        [Fact]
        public async Task Create_Post_Limits10ReviewsPerDay()
        {
            // Arrange
            var userId = "user1";
            for (int i = 0; i < 10; i++)
            {
                _context.Reviews.Add(new Review
                {
                    Id = userId,
                    GameId = 1,
                    ReviewText = "Some text",
                    DatePosted = DateTime.UtcNow.Date
                });
            }
            _context.SaveChanges();

            var model = new CreateReviewViewModel { Content = "Valid content", GameId = 1 };
            _grammarCheckServiceMock.Setup(s => s.CheckGrammarAsync(It.IsAny<string>())).ReturnsAsync(new List<string>());
            _userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);

            // Act
            var result = await _controller.Create(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Detail", redirect.ActionName);
        }

        [Fact]
        public void Delete_RemovesReview_WhenExists()
        {
            // Arrange
            var review = new Review { ReviewId = 1, ReviewText = "Some text", Id = "user1" };
            _context.Reviews.Add(review);
            _context.SaveChanges();

            // Act
            var result = _controller.Delete(1);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ReviewList", redirect.ActionName);
        }

        [Fact]
        public void Delete_ReturnsNotFound_WhenNotExists()
        {
            // Act
            var result = _controller.Delete(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
