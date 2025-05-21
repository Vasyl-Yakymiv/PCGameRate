using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PCGameRate.Data;
using PCGameRate.Interfaces;
using PCGameRate.Models;
using PCGameRate.Services;
using PCGameRate.ViewModels.Game;
using System;
using System.Net;
using System.Security.Claims;
using System.Diagnostics;
using Microsoft.Data.SqlClient;

namespace PCGameRate.Controllers
{
    public class GameController : Controller
    {
        IGameRepository _gameRepo;
        ApplicationDbContext _context;
        private readonly IYouTubeService _youTubeService;
        public GameController(IGameRepository gameRepo, ApplicationDbContext context, IYouTubeService youTubeService)
        {
            _gameRepo = gameRepo;
            _context = context;
            _youTubeService = youTubeService;
        }
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 20;
            int totalGames = await _context.Games.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalGames / pageSize);
            if (totalGames == 0)
            {
                return View(new GameListViewModel
                {
                    Games = new List<Game>(),
                    CurrentPage = 1,
                    TotalPages = 1
                });
            }
            var games = await _context.Games
                .Include(i => i.Developer)
                .Include(x => x.Genre)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new GameListViewModel
            {
                Games = games,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(viewModel);
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create(CreateGameViewModel gameVM)
        {
            if (!ModelState.IsValid)
            {
                return View(gameVM);
            }
            var game = new Game
            {
                Title = gameVM.Title,
                Description = gameVM.Description,
                Image = gameVM.Image,
                ReleaseDate = gameVM.ReleaseDate,
                Genre = gameVM.Genre,
                Developer = gameVM.Developer,
                RatingAverage = gameVM.RatingAverage,
                IsPopular = gameVM.IsPopular,
                IsExpected = gameVM.IsExpected
            };
            _gameRepo.Add(game);
            return RedirectToAction("Index");

        }
        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var game = await _gameRepo.GetByIdAsync(id);
            if (game == null) return View("Error");
            var gameVM = new EditGameViewModel
            {
                Title = game.Title,
                Description = game.Description,
                Image = game.Image,
                ReleaseDate = game.ReleaseDate,
                Genre = game.Genre,
                RatingAverage = game.RatingAverage,
                RatingCount = game.RatingCount,
                Developer = game.Developer,
                IsPopular = (bool)game.IsPopular,
                IsExpected = (bool)game.IsExpected
            };
            return View(gameVM);
        }
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int id, EditGameViewModel gameVM)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Failed to edit club");
                return View("Edit", gameVM);
            }

            var checkgame = await _gameRepo.GetByIdAsyncNoTracking(id);

            if (checkgame == null)
            {
                return View("Error");
            }


            var game = new Game
            {
                GameId = id,
                Title = gameVM.Title,
                Description = gameVM.Description,
                Image = gameVM.Image,
                ReleaseDate = gameVM.ReleaseDate,
                Genre = gameVM.Genre,
                Developer = gameVM.Developer,
                RatingAverage = gameVM.RatingAverage,
                RatingCount = gameVM.RatingCount,
                IsPopular = gameVM.IsPopular,
                IsExpected = gameVM.IsExpected

            };

            _gameRepo.Update(game);
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var game = await _gameRepo.GetByIdAsync(id);
            if (game == null) return View("Error");
            return View(game);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteGame(int id)
        {
            var game = await _gameRepo.GetByIdAsync(id);

            if (game == null)
            {
                return View("Error");
            }

            _gameRepo.Delete(game);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var game = await _gameRepo.GetWithReviewAndScreenshotsByIdAsync(id);
            if (game == null) return NotFound();

            var embedHtml = await _youTubeService.GetTopLiveStreamEmbedAsync(game.Title);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            bool isInGameList = false;
            if (userId != null)
            {
                isInGameList = await _context.GameList.AnyAsync(gl => gl.Id == userId && gl.GameId == id);
            }

            ViewBag.IsInGameList = isInGameList;

            var userRating = await _gameRepo.GetUserRating(id, userId);
            ViewBag.UserRating = userRating;

            var viewModel = new GameDetailViewModel
            {
                Game = game,
                YouTubeEmbed = embedHtml
            };
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> SearchSuggestions(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Json(new { results = new List<object>() });
            }

            var games = await _context.Games
            .Where(g => g.Title.Contains(query))
            .Select(g => new
            {
                g.GameId,
                g.Title,
                g.Image,
                RatingAverage = g.RatingAverage.HasValue ? g.RatingAverage.Value.ToString("0.0") : "0.0"
            })
            .Take(5)
            .ToListAsync();

            return Json(new { results = games });
        }

        [HttpGet]
        public async Task<IActionResult> SearchResults(string query, string sortOrder)
        {
            var results = await _gameRepo.SearchResult(query);

            ViewBag.Query = query;

            switch (sortOrder)
            {
                case "rating_asc":
                    results = results.OrderBy(g => g.RatingAverage).ToList();
                    break;
                case "rating_desc":
                    results = results.OrderByDescending(g => g.RatingAverage).ToList();
                    break;
                case "releaseDate_asc":
                    results = results.OrderBy(g => g.ReleaseDate).ToList();
                    break;
                case "releaseDate_desc":
                    results = results.OrderByDescending(g => g.ReleaseDate).ToList();
                    break;
                case "votes_asc":
                    results = results.OrderBy(g => g.RatingCount).ToList();
                    break;
                case "votes_desc":
                    results = results.OrderByDescending(g => g.RatingCount).ToList();
                    break;
                default:
                    results = results.OrderByDescending(g => g.RatingAverage).ToList();
                    break;
            }
            return View(results);
        }

        [HttpGet]
        public async Task<IActionResult> GetPopularGames()
        {

            var popularGames = await _gameRepo.GetPopularGames();
            var games = popularGames
                .Select(g => new
                {
                    g.GameId,
                    g.Title,
                    g.Image,
                    RatingAverage = g.RatingAverage.HasValue ? g.RatingAverage.Value.ToString("0.0") : "0.0",
                    ReleaseYear = g.ReleaseDate
                }).ToList();
            return Json(games);
        }

        [HttpGet]
        public async Task<IActionResult> GetExpectedGames()
        {
            var popularGames = await _gameRepo.GetExpectedGames();

            return Json(popularGames);
        }
        [HttpGet]
        public async Task<IActionResult> GetLatestReviews()
        {
            var latestReviews = await _context.Reviews
                .OrderByDescending(r => r.DatePosted)
                .Take(20)
                .Select(r => new
                {
                    r.Game.GameId,
                    r.Game.Title,
                    r.Game.Image,
                    r.User.FullName,
                    r.ReviewText
                })
                .ToListAsync();

            return Json(latestReviews);
        }

        [HttpGet]
        public async Task<IActionResult> Top100(string sortOrder)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var games = await _gameRepo.GetTopSortedGames();


            var topGames = games
                .OrderByDescending(g => g.RatingAverage)
                .ThenByDescending(g => g.RatingCount)
                .Take(100)
                .ToList();

            var rankedGames = topGames.Select((game, index) => new
            {
                game.GameId,
                game.Title,
                game.RatingAverage,
                game.RatingCount,
                game.Image,
                game.ReleaseDate,
                Rank = index + 1,
                UserRating = _context.Ratings
                          .Where(r => r.GameId == game.GameId && r.Id == userId)
                          .Select(r => r.RatingValue)
                          .FirstOrDefault()
            }).ToList();

            switch (sortOrder)
            {
                case "rating_asc":
                    rankedGames = rankedGames.OrderBy(g => g.RatingAverage).ToList();
                    break;
                case "rating_desc":
                    rankedGames = rankedGames.OrderByDescending(g => g.RatingAverage).ToList();
                    break;
                case "releaseDate_asc":
                    rankedGames = rankedGames.OrderBy(g => g.ReleaseDate).ToList();
                    break;
                case "releaseDate_desc":
                    rankedGames = rankedGames.OrderByDescending(g => g.ReleaseDate).ToList();
                    break;
                case "votes_asc":
                    rankedGames = rankedGames.OrderBy(g => g.RatingCount).ToList();
                    break;
                case "votes_desc":
                    rankedGames = rankedGames.OrderByDescending(g => g.RatingCount).ToList();
                    break;
                default:
                    rankedGames = rankedGames.OrderByDescending(g => g.RatingAverage).ToList();
                    break;
            }

            return View(rankedGames);
        }
        [HttpGet]
        public async Task<IActionResult> GetRatingStats(int gameId)
        {
            var ratingStats = await _context.Ratings
                .Where(r => r.GameId == gameId)
                .GroupBy(r => r.RatingValue)
                .Select(g => new
                {
                    RatingValue = g.Key,
                    Count = g.Count()
                })
                .OrderBy(r => r.RatingValue)
                .ToListAsync();

            return Json(ratingStats);
        }
    }
}

