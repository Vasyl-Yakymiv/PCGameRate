using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PCGameRate.Data;
using PCGameRate.Models;
using PCGameRate.ViewModels.Rating;
using System.Security.Claims;

namespace PCGameRate.Controllers
{
    public class RatingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RatingController(ApplicationDbContext context)
        {
            _context = context;
        }

        
        public async Task<IActionResult> RateGame(int gameId)
        {
            var game = await _context.Games.FindAsync(gameId);
            if (game == null)
            {
                return NotFound();
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
            return RedirectToAction("Detail", "Game", new { id = gameId });
        }
        [HttpGet]
        public async Task<IActionResult> RatingList()
        {          
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            var ratedGames = await _context.Ratings
                .Include(r => r.Game)
                .Where(r => r.Id == userId)
                .Select(r => new RatedGameViewModel
                {
                    GameId = r.GameId,
                    GameTitle = r.Game.Title,
                    GameImage = r.Game.Image,
                    RatingAverage  = r.Game.RatingAverage,
                    ReleaseDate = r.Game.ReleaseDate,
                    RatingValue = r.RatingValue,
                    RatingDate = r.RatingDate,
                    Developer = r.Game.Developer.DeveloperName,
                    Genre = r.Game.Genre.GameGenre
                })
                .ToListAsync();

            return View(ratedGames);
        }
    }
}
