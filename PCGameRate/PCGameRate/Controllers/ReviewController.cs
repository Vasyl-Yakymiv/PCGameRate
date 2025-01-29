using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PCGameRate.Data;
using PCGameRate.Models;
using PCGameRate.ViewModels.Rating;
using PCGameRate.ViewModels.Review;
using System.Security.Claims;

namespace PCGameRate.Controllers
{
    public class ReviewController : Controller
    {
        ApplicationDbContext _context;
        public ReviewController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult Create(int gameId)
        {
            var reviewVM = new CreateReviewViewModel
            {
                GameId = gameId
            };
            return View(reviewVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateReviewViewModel model)
        {
            if (ModelState.IsValid)
            {
                var review = new Review
                {
                    ReviewText = model.Content,
                    GameId = model.GameId,
                    Id = User.FindFirstValue(ClaimTypes.NameIdentifier)
                };

                _context.Add(review);
                await _context.SaveChangesAsync();
                return RedirectToAction("Detail", "Game", new { id = model.GameId });
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ReviewList()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            var ratedGames = await _context.Reviews
                .Include(r => r.Game)
                .Where(r => r.Id == userId)
                .Select(r => new ReviewListViewModel
                {
                    GameId = r.GameId,
                    GameTitle = r.Game.Title,
                    GameImage = r.Game.Image,
                    ReviewText = r.ReviewText,
                    DatePosted = r.DatePosted
                })
                .ToListAsync();

            return View(ratedGames);
        }
    }
}
