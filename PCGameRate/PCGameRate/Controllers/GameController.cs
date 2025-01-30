using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> Index()
        {
            var games = await _gameRepo.GetAll();
            
            return View(games);
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
                Developer = game.Developer
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
                Developer = gameVM.Developer
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
            var game = await _gameRepo.GetWithReviewByIdAsync(id);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Пошук рейтингу для поточного користувача та конкретної гри
            var userRating = await _context.Ratings
                .Where(r => r.GameId == id && r.Id == userId)
                .Select(r => r.RatingValue)
                .FirstOrDefaultAsync();
            // Передаємо рейтинг користувача у ViewBag
            ViewBag.UserRating = userRating;

            return game == null ? NotFound() : View(game);
        }
    }
}
