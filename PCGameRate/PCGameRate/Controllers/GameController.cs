using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PCGameRate.Data;
using PCGameRate.Interfaces;
using PCGameRate.Models;
using PCGameRate.ViewModels.Game;
using System;
using System.Net;
using System.Security.Claims;

namespace PCGameRate.Controllers
{
    public class GameController : Controller
    {
        IGameRepository _gameRepo;
        ApplicationDbContext _context;
        public GameController(IGameRepository gameRepo, ApplicationDbContext context)
        {
            _gameRepo = gameRepo;
            _context = context;
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
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
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
                RatingAverage = gameVM.RatingAverage   
            };
            _gameRepo.Add(game);
            return RedirectToAction("Index");

        }
        [HttpGet]
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
                IsPopular = (bool)game.IsPopular
            };
            return View(gameVM);
        }
        [HttpPost]
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
                RatingCount= gameVM.RatingCount,
                IsPopular = gameVM.IsPopular
                
            };

            _gameRepo.Update(game);
            return RedirectToAction("Index");
        }
        
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var game = await _gameRepo.GetByIdAsync(id);
            if (game == null) return View("Error");
            return View(game);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteGame(int id)
        {
            var game = await _gameRepo.GetByIdAsync(id);

            if (game== null)
            {
                return View("Error");
            }

            _gameRepo.Delete(game);
            return RedirectToAction("Index","Home");
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var game = await _gameRepo.GetWithReviewAndScreenshotsByIdAsync(id);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
          
            var userRating = await _context.Ratings
                .Where(r => r.GameId == id && r.Id == userId)
                .Select(r => r.RatingValue)
                .FirstOrDefaultAsync();
            
            ViewBag.UserRating = userRating;

            return game == null ? NotFound() : View(game);
        }

        [HttpGet]
        public async  Task<IActionResult> Top100()
        {
            var topGames = await _gameRepo.GetTop100();

            if (topGames == null)
            {
                return View("Error");
            }

            return View(topGames);
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
        public async Task<IActionResult> SearchResults(string query)
        {
            var results = await _context.Games
                .Where(g => g.Title.Contains(query))
                .ToListAsync();

            return View(results);
        }

        [HttpGet]
        public async Task<IActionResult> GetPopularGames()
        {
            var popularGames = await _context.Games
                .Where(g => (bool)g.IsPopular)
                .Select(g => new
                {
                    g.GameId,
                    g.Title,
                    g.Image,
                    RatingAverage = g.RatingAverage.HasValue ? g.RatingAverage.Value.ToString("0.0") : "0.0",
                    ReleaseYear = g.ReleaseDate
                })
                .ToListAsync();

            return Json(popularGames);
        }

        [HttpGet]
        public async Task<IActionResult> GetExpectedGames()
        {
            var popularGames = await _context.Games
                .Where(g => (bool)g.IsExpected)
                .Select(g => new
                {
                    g.GameId,
                    g.Title,
                    g.Image,
                    RatingAverage = g.RatingAverage.HasValue ? g.RatingAverage.Value.ToString("0.0") : "0.0",
                    ReleaseYear = g.ReleaseDate
                })
                .ToListAsync();

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
                    r.User.UserName, 
                    r.ReviewText
                })
                .ToListAsync();

            return Json(latestReviews);
        }


    }

}

