using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PCGameRate.Data;
using PCGameRate.Models;

namespace PCGameRate.Controllers
{
    public class ScreenshotController : Controller
    {
        ApplicationDbContext _context;
        public ScreenshotController(ApplicationDbContext context)
        {
               _context = context;
        }
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AddScreenshot(int gameId, string screenshotUrl)
        {
            if (string.IsNullOrEmpty(screenshotUrl))
            {
                return BadRequest("URL не може бути порожнім.");
            }

            var screenshot = new Screenshot
            {
                GameId = gameId,
                ScreenshotUrl = screenshotUrl
            };

            _context.Screenshots.Add(screenshot);
            await _context.SaveChangesAsync();

            return RedirectToAction("Detail", "Game", new { id = gameId });
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteScreenshot(int photoId)
        {
            var screenshot = await _context.Screenshots.FindAsync(photoId);
            if (screenshot == null)
            {
                return NotFound();
            }

            _context.Screenshots.Remove(screenshot);
            await _context.SaveChangesAsync();

            return RedirectToAction("Detail", "Game", new { id = screenshot.GameId });
        }
    }
}
