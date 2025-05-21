using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using PCGameRate.Controllers;
using PCGameRate.Data;
using PCGameRate.Interfaces;
using PCGameRate.Models;
using PCGameRate.Services;
using PCGameRate.ViewModels.Game;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Security.Claims;
using System.Text.Json;
using PCGameRate.Tests.Data;

public class GameControllerTests
{
    private readonly Mock<IGameRepository> _gameRepoMock;
    private readonly Mock<IYouTubeService> _youTubeServiceMock;
    private readonly ApplicationDbContext _context;
    private readonly GameController _controller;

    public GameControllerTests()
    {
        _gameRepoMock = new Mock<IGameRepository>();
        _youTubeServiceMock = new Mock<IYouTubeService>();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid())
            .Options;
        _context = new ApplicationDbContext(options);

        _controller = new GameController(
            _gameRepoMock.Object,
            _context,
            _youTubeServiceMock.Object
        );

        var tempData = new TempDataDictionary(
            new DefaultHttpContext(),
            Mock.Of<ITempDataProvider>()
        );
        _controller.TempData = tempData;
    }

    [Fact]
    public async Task Index_ReturnsViewWithEmptyList_WhenNoGames()
    {
        // Arrange: база порожня

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<GameListViewModel>(viewResult.Model);
        Assert.Empty(model.Games);
        Assert.Equal(1, model.CurrentPage);
        Assert.Equal(1, model.TotalPages);
    }

    [Fact]
    public void Create_Get_ReturnsView()
    {
        var result = _controller.Create();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Create_Post_InvalidModel_ReturnsViewWithModel()
    {
        _controller.ModelState.AddModelError("Title", "Required");

        var gameVM = new CreateGameViewModel();

        var result = await _controller.Create(gameVM);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<CreateGameViewModel>(viewResult.Model);
        Assert.Equal(gameVM, model);
    }

    [Fact]
    public async Task Create_Post_ValidModel_AddsGameAndRedirects()
    {
        var gameVM = new CreateGameViewModel
        {
            Title = "New Game",
            Description = "Desc",
            Image = "img.png",
            ReleaseDate = "2023",
            Genre = new Genre { GenreId = 1, GameGenre = "test" },
            Developer = new Developer { DeveloperId = 1, DeveloperName = "test" },
            RatingAverage = 5,
            IsPopular = true,
            IsExpected = false
        };

        _gameRepoMock.Setup(r => r.Add(It.IsAny<Game>()));

        var result = await _controller.Create(gameVM);

        _gameRepoMock.Verify(r => r.Add(It.Is<Game>(g => g.Title == gameVM.Title)), Times.Once);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
    }

    [Fact]
    public async Task Edit_Get_GameExists_ReturnsViewWithModel()
    {
        var game = new Game
        {
            GameId = 1,
            Title = "Game1",
            Description = "Desc",
            Image = "img.png",
            ReleaseDate = "2023",
            Genre = new Genre { GenreId = 1, GameGenre = "test" },
            Developer = new Developer { DeveloperId = 1, DeveloperName = "test" },
            RatingAverage = 4,
            RatingCount = 10,
            IsPopular = true,
            IsExpected = false
        };

        _gameRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(game);

        var result = await _controller.Edit(1);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<EditGameViewModel>(viewResult.Model);
        Assert.Equal(game.Title, model.Title);
    }

    [Fact]
    public async Task Edit_Get_GameDoesNotExist_ReturnsErrorView()
    {
        _gameRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Game)null);

        var result = await _controller.Edit(99);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Error", viewResult.ViewName);
    }

    [Fact]
    public async Task Edit_Post_InvalidModel_ReturnsViewWithModelError()
    {
        _controller.ModelState.AddModelError("", "Error");

        var gameVM = new EditGameViewModel();

        var result = await _controller.Edit(1, gameVM);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Edit", viewResult.ViewName);
        Assert.False(_controller.ModelState.IsValid);
    }

    [Fact]
    public async Task Edit_Post_GameDoesNotExist_ReturnsErrorView()
    {
        _gameRepoMock.Setup(r => r.GetByIdAsyncNoTracking(It.IsAny<int>())).ReturnsAsync((Game)null);

        var gameVM = new EditGameViewModel();

        var result = await _controller.Edit(99, gameVM);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Error", viewResult.ViewName);
    }

    [Fact]
    public async Task Edit_Post_ValidModel_UpdatesGameAndRedirects()
    {
        var existingGame = new Game { GameId = 1 };
        _gameRepoMock.Setup(r => r.GetByIdAsyncNoTracking(1)).ReturnsAsync(existingGame);

        var gameVM = new EditGameViewModel
        {
            Title = "Updated",
            Description = "Desc",
            Image = "img.png",
            ReleaseDate = "2023",
            Genre = new Genre { GenreId = 1, GameGenre = "test" },
            Developer = new Developer { DeveloperId = 1, DeveloperName = "test" },
            RatingAverage = 5,
            RatingCount = 100,
            IsPopular = true,
            IsExpected = false
        };

        _gameRepoMock.Setup(r => r.Update(It.IsAny<Game>()));

        var result = await _controller.Edit(1, gameVM);

        _gameRepoMock.Verify(r => r.Update(It.Is<Game>(g => g.Title == "Updated")), Times.Once);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
    }

    [Fact]
    public async Task Delete_Get_GameExists_ReturnsView()
    {
        var game = new Game { GameId = 1, Title = "Game1" };
        _gameRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(game);

        var result = await _controller.Delete(1);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Game>(viewResult.Model);
        Assert.Equal(game.Title, model.Title);
    }

    [Fact]
    public async Task Delete_Get_GameDoesNotExist_ReturnsErrorView()
    {
        _gameRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Game)null);

        var result = await _controller.Delete(99);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Error", viewResult.ViewName);
    }

    [Fact]
    public async Task Delete_Post_GameExists_DeletesAndRedirects()
    {
        var game = new Game { GameId = 1 };
        _gameRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(game);
        _gameRepoMock.Setup(r => r.Delete(game));

        var result = await _controller.DeleteGame(1);

        _gameRepoMock.Verify(r => r.Delete(game), Times.Once);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Home", redirect.ControllerName);
    }

    [Fact]
    public async Task Delete_Post_GameDoesNotExist_ReturnsErrorView()
    {
        _gameRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Game)null);

        var result = await _controller.DeleteGame(99);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Error", viewResult.ViewName);
    }

    [Fact]
    public async Task Detail_ReturnsNotFound_WhenGameIsNull()
    {
        _gameRepoMock.Setup(repo => repo.GetWithReviewAndScreenshotsByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Game)null);

        var result = await _controller.Detail(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task SearchSuggestions_ReturnsEmptyResults_WhenQueryIsEmpty()
    {
        var result = await _controller.SearchSuggestions("");

        var jsonResult = Assert.IsType<JsonResult>(result);
        dynamic data = jsonResult.Value;

        Assert.NotNull(data.results);
        Assert.Empty(data.results);
    }

    [Fact]
    public async Task SearchSuggestions_ReturnsGamesMatchingQuery()
    {
        // Arrange 
        _context.Games.AddRange(
            new Game { GameId = 1, Title = "Test Game One", Image = "img1", Description = "Test Description", RatingAverage = 4 },
            new Game { GameId = 2, Title = "Another Game", Image = "img2", Description = "Test Description", RatingAverage = null }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.SearchSuggestions("Test");

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        var data = jsonResult.Value as dynamic;
        var results = (IEnumerable<dynamic>)data.results;

        Assert.Single(results);
        Assert.Equal(1, (int)results.First().GameId);
    }

    [Fact]
    public async Task SearchResults_ReturnsSortedResults()
    {
        var games = new List<Game>
        {
            new Game { GameId = 1, Title = "Game1", RatingAverage = 4,Description = "Test Description", ReleaseDate = "2023", RatingCount = 10 },
            new Game { GameId = 2, Title = "Game2", RatingAverage = 5,Description = "Test Description", ReleaseDate = "2023", RatingCount = 5 }
        };

        _gameRepoMock.Setup(r => r.SearchResult("query"))
            .ReturnsAsync(games);

        var result = await _controller.SearchResults("query", "rating_asc");

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<Game>>(viewResult.Model);

        Assert.Equal(2, model.Count());
        Assert.Equal(4, model.First().RatingAverage);
    }


    [Fact]
    public async Task GetPopularGames_ReturnsFromRepo()
    {
        var expectedGames = new List<Game> {
            new Game { GameId = 10, Title = "Expected" }
        };
        _gameRepoMock.Setup(r => r.GetPopularGames())
            .ReturnsAsync(expectedGames);

        var result = await _controller.GetPopularGames();

        var jsonResult = Assert.IsType<JsonResult>(result);
        var data = jsonResult.Value as IEnumerable<Game>;

        Assert.Single(data);
        Assert.Equal(10, data.First().GameId);
    }

    [Fact]
    public async Task GetExpectedGames_ReturnsFromRepo()
    {
        var expectedGames = new List<Game> {
            new Game { GameId = 10, Title = "Expected" }
        };
        _gameRepoMock.Setup(r => r.GetExpectedGames())
            .ReturnsAsync(expectedGames);

        var result = await _controller.GetExpectedGames();

        var jsonResult = Assert.IsType<JsonResult>(result);
        var data = jsonResult.Value as IEnumerable<Game>;

        Assert.Single(data);
        Assert.Equal(10, data.First().GameId);
    }

    [Fact]
    public async Task GetLatestReviews_ReturnsLatestReviews()
    {
        var user = new User { FullName = "John Doe" };
        var game = new Game { GameId = 5, Title = "Game5", Image = "img5" , Description = "Test Description", IsPopular = true, RatingAverage = 3, ReleaseDate = "2023" };
        _context.Users.Add(user);
        _context.Games.Add(game);
        _context.Reviews.Add(new Review
        {
            Game = game,
            User = user,
            ReviewText = "Good game",
            DatePosted = System.DateTime.Now
        });
        await _context.SaveChangesAsync();

        var result = await _controller.GetLatestReviews();

        var jsonResult = Assert.IsType<JsonResult>(result);
        var jsonString = JsonSerializer.Serialize(jsonResult.Value);
        var reviews = JsonSerializer.Deserialize<List<ReviewDto>>(jsonString);

        Assert.Single(reviews);
        Assert.Equal("Game5", reviews[0].Title);
        Assert.Equal("John Doe", reviews[0].FullName);
    }

    [Fact]
    public async Task Top100_ReturnsTopGamesWithRankingAndUserRating()
    {
        var userId = "user123";

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "mock"));

        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };

        var games = new List<Game>
        {
            new Game { GameId = 1, Title = "Game1", RatingAverage = 4, RatingCount = 10, Image = "img1", ReleaseDate = "2023" },
            new Game { GameId = 2, Title = "Game2", RatingAverage = 4, RatingCount = 20, Image = "img2", ReleaseDate = "2023" }
        };

        _gameRepoMock.Setup(r => r.GetTopSortedGames())
            .ReturnsAsync(games);

        _context.Ratings.Add(new Rating
        {
            GameId = 1,
            Id = userId,
            RatingValue = 5
        });
        await _context.SaveChangesAsync();

        var result = await _controller.Top100("rating_desc");

        var viewResult = Assert.IsType<ViewResult>(result);
        dynamic model = viewResult.Model;

        foreach (var item in model)
        {
            int gameId = item.GameId;
            string title = item.Title;
            Assert.True(gameId > 0);
            Assert.False(string.IsNullOrEmpty(title));
        }

        Assert.Equal(2, model.Count());
        var first = model.First();
        Assert.Equal(1, first.GameId);
        Assert.Equal(5, first.UserRating);
    }
}

