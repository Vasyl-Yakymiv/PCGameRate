using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PCGameRate.Data;
using PCGameRate.Models;
using PCGameRate.ViewModels.Rating;
using System.Security.Claims;
using System.Diagnostics;

namespace PCGameRate.Controllers
{
    public class RatingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        public RatingController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        
        public async Task<IActionResult> RateGame(int gameId)
        {
          
            var game = await _context.Games.FindAsync(gameId);
            if (game == null)
            {
                return NotFound();
            }

            var userIdForCount = _userManager.GetUserId(User);
            var today = DateTime.UtcNow.Date;
            int ratingToday = await _context.Ratings
                         .Where(u => u.Id == userIdForCount && u.RatingDate >= today)
                         .CountAsync();
            if (ratingToday >= 50) {
                TempData["ErrorMessage"] = "Ви вже оцінили 50 ігор на сьогодні. Повторіть спробу завтра.";
                return RedirectToAction("Detail", "Game",new { id = gameId });
            }
            var ratingDeviation = await _context.Ratings
                .Where(u => u.Id == userIdForCount && u.RatingDate >= today)
                .Take(30)
                .Select(u => u.RatingValue.Value)
                .ToListAsync();
            if (ratingDeviation.Count >= 15)
            {
                var average = ratingDeviation.Average();
                var variance = ratingDeviation.Average(r => Math.Pow(r - average, 2));
                var stdDev = Math.Sqrt(variance); 

                if (stdDev < 1.0) 
                {
                    TempData["ErrorMessage"] = " За вами виявлена підозріла активність.\n" +
                        "Повторіть спробу оцінити гру завтра.";
                    return RedirectToAction("Detail", "Game", new { id = gameId });
                }
            }
            var userId = User.Identity.Name;
            var existingRating = await _context.Ratings
                .FirstOrDefaultAsync(r => r.GameId == gameId && r.User.UserName == userId);
            
            ViewBag.Game = game;
            ViewBag.ExistingRating = existingRating?.RatingValue;

            return View(game);
        }

        
        [HttpPost]
        public async Task<IActionResult> RateGame(int gameId, int ratingValue)
        {
            var stopwatch = Stopwatch.StartNew();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            var existingRating = await _context.Ratings
                .FirstOrDefaultAsync(r => r.GameId == gameId && r.Id == userId);

            var game = await _context.Games.FindAsync(gameId);
            if (game == null)
            {
                return NotFound();
            }

            if (existingRating != null)
            {
                var previousRating = existingRating.RatingValue;
                existingRating.RatingValue = ratingValue;
                existingRating.RatingDate = DateTime.Now;
                game.RatingAverage = ((game.RatingAverage * game.RatingCount) - previousRating + ratingValue) / game.RatingCount;
            }
            else
            {
                var newRating = new Rating
                {
                    GameId = gameId,
                    Id = userId,
                    RatingValue = ratingValue,
                    RatingDate = DateTime.Now
                };
                _context.Ratings.Add(newRating);
                game.RatingCount++;
                game.RatingAverage = ((game.RatingAverage * (game.RatingCount - 1)) + ratingValue) / game.RatingCount;
            }
            await _context.SaveChangesAsync();
            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;
            TempData["ErrorMessage"] = $"{elapsedMs} мс";
            return RedirectToAction("Detail", "Game", new { id = gameId });
        }
        [HttpGet]
        public async Task<IActionResult> RatingList(string sortOrder)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            IQueryable<RatedGameViewModel> ratedGamesQuery = _context.Ratings
                .Include(r => r.Game)
                .Where(r => r.Id == userId)
                .Select(r => new RatedGameViewModel
                {
                    GameId = r.GameId,
                    GameTitle = r.Game.Title,
                    GameImage = r.Game.Image,
                    RatingAverage = r.Game.RatingAverage,
                    ReleaseDate = r.Game.ReleaseDate,
                    RatingValue = r.RatingValue,
                    RatingDate = r.RatingDate,
                    Developer = r.Game.Developer.DeveloperName,
                    Genre = r.Game.Genre.GameGenre
                });

            switch (sortOrder)
            {
                case "releaseDate_asc":
                    ratedGamesQuery = ratedGamesQuery.OrderBy(r => r.ReleaseDate);
                    break;
                case "releaseDate_desc":
                    ratedGamesQuery = ratedGamesQuery.OrderByDescending(r => r.ReleaseDate);
                    break;
                case "rating_asc":
                    ratedGamesQuery = ratedGamesQuery.OrderBy(r => r.RatingAverage);
                    break;
                case "rating_desc":
                    ratedGamesQuery = ratedGamesQuery.OrderByDescending(r => r.RatingAverage);
                    break;
                case "userRating_asc":
                    ratedGamesQuery = ratedGamesQuery.OrderBy(r => r.RatingValue);
                    break;
                case "userRating_desc":
                    ratedGamesQuery = ratedGamesQuery.OrderByDescending(r => r.RatingValue);
                    break;
                default:
                    ratedGamesQuery = ratedGamesQuery.OrderByDescending(r => r.RatingDate);
                    break;
            }

            var ratedGames = await ratedGamesQuery.ToListAsync();

            return View(ratedGames);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRating(int gameId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var existingRating = await _context.Ratings
                .FirstOrDefaultAsync(r => r.GameId == gameId && r.Id == userId);

            var game = await _context.Games.FindAsync(gameId);
            if (game == null || existingRating == null)
            {
                return NotFound();
            }

            
            if (game.RatingCount > 1)
            {
                game.RatingAverage = ((game.RatingAverage * game.RatingCount) - existingRating.RatingValue) / (game.RatingCount - 1);
                game.RatingCount--;
            }
            else
            {
                game.RatingAverage = 0; 
                game.RatingCount = 0;
            }

            _context.Ratings.Remove(existingRating);
            await _context.SaveChangesAsync();

            return RedirectToAction("Detail", "Game", new { id = gameId });
        }


    }
}
