using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PCGameRate.Data;
using PCGameRate.Models;

namespace PCGameRate.Controllers
{
    public class VideoController : Controller
    {
        private readonly ApplicationDbContext _context;
        public VideoController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AddTrailer(int gameId, string url)
        {
            var game = await _context.Games.FindAsync(gameId);
            if (game == null) return NotFound();

            var trailer = new GameVideo
            {
                GameId = gameId,
                VideoUrl = url,
                Type = "trailer"
            };

            _context.GameVideos.Add(trailer);
            await _context.SaveChangesAsync();

            return RedirectToAction("Detail", "Game", new { id = gameId });
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public IActionResult Delete(int id) 
        {
            var gameVideo = _context.GameVideos.Find(id);
            if (gameVideo == null)
            {
                return NotFound();
            }

            _context.GameVideos.Remove(gameVideo);
            _context.SaveChanges();

            return RedirectToAction("Detail", "Game", new { id = gameVideo.GameId });
        }
    }
}
