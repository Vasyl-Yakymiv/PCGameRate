using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PCGameRate.Data;
using PCGameRate.Models;
using System.Security.Claims;

namespace PCGameRate.Controllers
{
    public class GameListController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public GameListController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        [HttpPost]
        public async Task<IActionResult> ToggleGameList(int gameId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var gameListItem = await _context.GameList.FirstOrDefaultAsync(gl => gl.Id == user.Id && gl.GameId == gameId);
            bool isInGameList;

            if (gameListItem != null)
            {
                _context.GameList.Remove(gameListItem);
                isInGameList = false;
            }
            else
            {
                _context.GameList.Add(new GameListItem { Id = user.Id, GameId = gameId });
                isInGameList = true;
            }

            await _context.SaveChangesAsync();
            return Json(new { isInGameList });
        }

        [HttpGet]
        public async Task<IActionResult> Index(string sortOrder)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var games = await _context.GameList
                .Where(gl => gl.Id == user.Id)
                .Include(gl => gl.Game)
                .ThenInclude(g => g.Genre)
                .Include(g => g.Game)
                .ThenInclude(g => g.Developer)
                .ToListAsync();
            switch (sortOrder)
            {
                case "releaseDate_asc":
                    games = games.OrderBy(r => r.Game.ReleaseDate).ToList();
                    break;
                case "releaseDate_desc":
                    games = games.OrderByDescending(r => r.Game.ReleaseDate).ToList();
                    break;
                case "rating_asc":
                    games = games.OrderBy(r => r.Game.RatingAverage).ToList();
                    break;
                case "rating_desc":
                    games = games.OrderByDescending(r => r.Game.RatingAverage).ToList();
                    break;
               
                default:
                    games = games.OrderByDescending(r => r.AddedAt).ToList();
                    break;
            }
            return View(games);
        }
    }
}
