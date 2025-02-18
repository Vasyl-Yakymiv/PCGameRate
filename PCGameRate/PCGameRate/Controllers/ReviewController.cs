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

            return RedirectToAction("ReviewList","Review"); 
        }

        [HttpPost]
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
