using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PCGameRate.Data;
using PCGameRate.Interfaces;
using PCGameRate.Models;
using PCGameRate.Services;
using PCGameRate.ViewModels.Rating;
using PCGameRate.ViewModels.Review;
using System.Security.Claims;

namespace PCGameRate.Controllers
{
    public class ReviewController : Controller
    {
         private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IGrammarCheckService _grammarCheckService;
        private readonly IProfanityCheckService _profanityCheckService;
        public ReviewController(ApplicationDbContext context, UserManager<User> userManager, IGrammarCheckService grammarCheckService, IProfanityCheckService profanityCheckService)
        {
            _context = context;
            _userManager = userManager;
            _grammarCheckService = grammarCheckService;
            _profanityCheckService = profanityCheckService;
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
        [Authorize(Roles = "user")]
        public async Task<IActionResult> Create(CreateReviewViewModel model)
        {
            if (ModelState.IsValid)
            {
                var grammarErrors = await _grammarCheckService.CheckGrammarAsync(model.Content);
                if (grammarErrors.Any())
                {
                    TempData["ErrorMessage"] = "У вашій рецензії знайдено помилки:\n" + string.Join("\n", grammarErrors);
                    return View(model);
                }
                if (_profanityCheckService.ContainsProfanity(model.Content))
                {
                    TempData["ErrorMessage"] = "Ваша рецензія містить нецензурні вирази. Будь ласка, відредагуйте її.";
                    return View(model);
                }
                var userId = _userManager.GetUserId(User);
                var today = DateTime.UtcNow.Date;

                int reviewsToday = await _context.Reviews
                    .Where(r => r.Id == userId && r.DatePosted >= today)
                    .CountAsync();

                if (reviewsToday >= 10)
                {
                    TempData["ErrorMessage"] = "Ви вже залишили 10 рецензій сьогодні. Повторіть спробу завтра.";
                    return RedirectToAction("Detail","Game", new { id = model.GameId }); 
                }

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
        [Authorize(Roles = "user")]
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
                .OrderByDescending(r => r.DatePosted)
                .Select(r => new ReviewListViewModel
                {
                    ReviewId = r.ReviewId,
                    GameId = r.GameId,
                    GameTitle = r.Game.Title,
                    GameImage = r.Game.Image,
                    ReviewText = r.ReviewText,
                    DatePosted = r.DatePosted
                })
                .ToListAsync();

            return View(ratedGames);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var review = _context.Reviews.Find(id);
            if (review == null)
            {
                return NotFound();
            }

            _context.Reviews.Remove(review);
            _context.SaveChanges();
            if (User.IsInRole("user"))
            {
                return RedirectToAction("ReviewList", "Review");
            }
            else
            {
                return RedirectToAction("Index", "Game");
            }
           
        }

        [HttpPost]
        [Authorize(Roles = "user")]
        public async Task<IActionResult> VoteReview(int reviewId, bool isLike)
        {
            var review = await _context.Reviews
                .Include(r => r.Votes)
                .FirstOrDefaultAsync(r => r.ReviewId == reviewId);

            if (review == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var existingVote = review.Votes.FirstOrDefault(v => v.Id == userId);

            if (existingVote != null)
            {
                if (existingVote.IsLike == isLike)
                {
                   
                    _context.Votes.Remove(existingVote);
                    if (isLike) review.Likes--;
                    else review.Dislikes--;
                }
                else
                {
                   
                    existingVote.IsLike = isLike;
                    if (isLike)
                    {
                        review.Likes++;
                        review.Dislikes--;
                    }
                    else
                    {
                        review.Likes--;
                        review.Dislikes++;
                    }
                }
            }
            else
            {
                
                var vote = new Vote
                {
                    ReviewId = reviewId,
                    Id = userId,
                    IsLike = isLike
                };
                _context.Votes.Add(vote);

                if (isLike) review.Likes++;
                else review.Dislikes++;
            }

            await _context.SaveChangesAsync();

            return Json(new { likes = review.Likes, dislikes = review.Dislikes });
        }

    }
}
