using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PCGameRate.Data;
using PCGameRate.Models;
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

            if (existingRating != null)
            {
                existingRating.RatingValue = ratingValue; 
                existingRating.RatingDate = DateTime.Now;
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
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Detail", "Game", new { id = gameId });
        }
    }
}
