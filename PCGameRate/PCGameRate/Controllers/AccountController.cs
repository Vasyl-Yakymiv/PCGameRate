using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PCGameRate.Data;
using PCGameRate.Interfaces;
using PCGameRate.Models;
using PCGameRate.ViewModels.Account;
using PCGameRate.ViewModels.Rating;
using System.Security.Claims;
using reCAPTCHA.AspNetCore;
using Microsoft.Extensions.Configuration;
using PCGameRate.Services;

namespace PCGameRate.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ApplicationDbContext _context;
        private readonly IAccountRepository _accountRepo;
        private readonly CaptchaService _captchaService;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, IAccountRepository accountRepo,CaptchaService captchaService)
        {
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _accountRepo = accountRepo;
            _captchaService = captchaService;

        }

        [HttpGet]
        public IActionResult Login()
        {
            var response = new LoginViewModel();
            return View(response);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            if (!ModelState.IsValid) return View(loginViewModel);

            var captchaResponse = Request.Form["g-recaptcha-response"];
            if (!await _captchaService.IsCaptchaValid(captchaResponse))
            {
                TempData["Error"] = "reCAPTCHA не пройдено";
                return View(loginViewModel);
            }
            var user = await _userManager.FindByEmailAsync(loginViewModel.EmailAddress);

            if (user != null)
            {
                
                var passwordCheck = await _userManager.CheckPasswordAsync(user, loginViewModel.Password);
                if (passwordCheck)
                {
                  
                    var result = await _signInManager.PasswordSignInAsync(user, loginViewModel.Password, false, false);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Game");
                    }
                }
              
                TempData["Помилка"] = "Неправильні облікові дані. Спробуйте ще раз";
                return View(loginViewModel);
            }
            
            TempData["Помилка"] = "Неправильні облікові дані. Спробуйте ще раз";
            return View(loginViewModel);
        }

        [HttpGet]
        public IActionResult Register()
        {
            var response = new RegisterViewModel();

            return View(response);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            if (!ModelState.IsValid) return View(registerViewModel);

            var captchaResponse = Request.Form["g-recaptcha-response"];
            if (!await _captchaService.IsCaptchaValid(captchaResponse))
            {
                TempData["Error"] = "reCAPTCHA не пройдено";
                return View(registerViewModel);
            }

            var user = await _userManager.FindByEmailAsync(registerViewModel.EmailAddress);
            if (user != null)
            {
                TempData["Error"] = "Ця пошта вже використовується іншим корисутвачем";
                return View(registerViewModel);
            }

            var newUser = new User()
            {
                Email = registerViewModel.EmailAddress,
                UserName = registerViewModel.EmailAddress,
                FullName = registerViewModel.EmailAddress
            };
            var newUserResponse = await _userManager.CreateAsync(newUser, registerViewModel.Password);

            if (newUserResponse.Succeeded)
            {
                await _userManager.AddToRoleAsync(newUser, UserRoles.User);
            }
            else
            {
                TempData["Error"] = "Пароль повинен містити щонайменше одну цифру, одну малу літеру, одну велику літеру та один спеціальний символ.";
                return View(registerViewModel);
            }
                
            return RedirectToAction("Index", "Game");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Route("Account/Welcome")]
        public async Task<IActionResult> Welcome(int page = 0)
        {
            if (page == 0)
            {
                return View();
            }
            return View();

        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new ProfileViewModel
            {
                FullName = user.FullName,
                ProfileImageUrl = user.ProfileImageUrl ?? ""  
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            user.FullName = model.FullName;

           
            if (model.ProfileImage != null && model.ProfileImage.Length > 0)
            {
      
                if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                {
                    var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfileImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath); 
                    }
                }

               
                var fileName = $"{user.Id}_{Path.GetFileName(model.ProfileImage.FileName)}";
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/avatars", fileName);

                
                using (var stream = new FileStream(uploadPath, FileMode.Create))
                {
                    await model.ProfileImage.CopyToAsync(stream);
                }

               
                user.ProfileImageUrl = $"/images/avatars/{fileName}";
            }

            
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                return RedirectToAction("Profile");
            }

            
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> Recommendations()
        {
            var userId = User.Identity.Name;
            var userRatings = await _accountRepo.GetUserRatings(userId);
            if (userRatings.Count == 0)
            {

                var popularGames = await _accountRepo.GetPopularGames();
                return View(popularGames); 
            }

            var recommendedGames = _accountRepo.GetRecommendedGames(userRatings);

            return View(recommendedGames);
        }
    }
}
