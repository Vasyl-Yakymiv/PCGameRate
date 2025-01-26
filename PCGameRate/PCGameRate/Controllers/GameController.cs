using Microsoft.AspNetCore.Mvc;
using PCGameRate.Interfaces;

namespace PCGameRate.Controllers
{
    public class GameController : Controller
    {
        IGameRepository _gameRepo;
        public GameController(IGameRepository gameRepo)
        {
            _gameRepo = gameRepo;
        }
        public async Task<IActionResult> Index()
        {
            var games = await _gameRepo.GetAll();
            
            return View(games);
        }


        public IActionResult Create()
        {
            return View();
        }
        public IActionResult Edit()
        {
            return View();
        }
        public IActionResult Detail()
        {
            return View();
        }
        public IActionResult Delete()
        {
            return View();
        }
    }
}
