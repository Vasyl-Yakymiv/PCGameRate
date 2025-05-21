using Xunit;
using Moq;
using PCGameRate.Controllers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PCGameRate.Models;
using PCGameRate.Data;
using PCGameRate.Services;
using PCGameRate.ViewModels;
using Microsoft.AspNetCore.Hosting;
using PCGameRate.Interfaces;
using PCGameRate.ViewModels.Account;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using reCAPTCHA.AspNetCore;

namespace PCGameRate.Tests
{
    public class AccountControllerTests
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<SignInManager<User>> _signInManagerMock;
        private readonly Mock<IWebHostEnvironment> _webHostEnvironmentMock;
        private readonly Mock<IAccountRepository> _accountRepoMock;
        private readonly Mock<ICaptchaService> _captchaServiceMock;
        private readonly ApplicationDbContext _context;
        private readonly AccountController _controller;

        public AccountControllerTests()
        {
            // UserManager
            var userStoreMock = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null
            );

            // SignInManager
            var contextAccessorMock = new Mock<IHttpContextAccessor>();
            var userPrincipalFactoryMock = new Mock<IUserClaimsPrincipalFactory<User>>();
            _signInManagerMock = new Mock<SignInManager<User>>(
                _userManagerMock.Object,
                contextAccessorMock.Object,
                userPrincipalFactoryMock.Object,
                null, null, null, null
            );

            _webHostEnvironmentMock = new Mock<IWebHostEnvironment>();
            _accountRepoMock = new Mock<IAccountRepository>();

            // Мок для ICaptchaService
            _captchaServiceMock = new Mock<ICaptchaService>();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid())
                .Options;
            _context = new ApplicationDbContext(options);

            _controller = new AccountController(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _context,
                _webHostEnvironmentMock.Object,
                _accountRepoMock.Object,
                _captchaServiceMock.Object
            );
            var tempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>()
            );
            _controller.TempData = tempData;
        }

        [Fact]
        public async Task Login_Post_InvalidCaptcha_ReturnsViewWithError()
        {
            // Arrange
            var model = new LoginViewModel
            {
                EmailAddress = "user@example.com",
                Password = "Test123!"
            };
            var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { "g-recaptcha-response", "invalid-captcha-token" }
        });

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Form = formCollection;
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Налаштовуємо мок: captcha вважаємо недійсною
            _captchaServiceMock
                .Setup(c => c.IsCaptchaValid(It.IsAny<string>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.Login(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(model, viewResult.Model);
            Assert.True(_controller.TempData.ContainsKey("Error"));
            Assert.Equal("reCAPTCHA не пройдено", _controller.TempData["Error"]);
        }

        [Fact]
        public async Task Login_Post_InvalidPassword_ReturnsViewWithError()
        {
            // Arrange
            var model = new LoginViewModel
            {
                EmailAddress = "user@example.com",
                Password = "WrongPassword!"
            };

            var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
    {
        { "g-recaptcha-response", "valid-captcha-token" }
    });

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Form = formCollection;
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var user = new User { Email = model.EmailAddress };

            // Імітуємо пошук користувача за email
            _userManagerMock
                .Setup(um => um.FindByEmailAsync(model.EmailAddress))
                .ReturnsAsync(user);

            // Імітуємо неуспішний вхід (пароль неправильний)
            _signInManagerMock
                .Setup(sm => sm.PasswordSignInAsync(
                    user,
                    model.Password,
                    false,
                    false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            // Капча проходить (true)
            _captchaServiceMock
                .Setup(c => c.IsCaptchaValid(It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Login(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(model, viewResult.Model);
            Assert.True(_controller.TempData.ContainsKey("Error"));
            Assert.Equal("Неправильні облікові дані. Спробуйте ще раз", _controller.TempData["Error"]);
        }

        [Fact]
        public async Task Login_Post_ValidCredentials_RedirectsToGameIndex()
        {
            // Arrange
            var model = new LoginViewModel
            {
                EmailAddress = "user@example.com",
                Password = "CorrectPassword123!"
            };

            var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
    {
        { "g-recaptcha-response", "valid-captcha-token" }
    });

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Form = formCollection;
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var user = new User { Email = model.EmailAddress };

            // Mock капчі
            var captchaServiceMock = new Mock<ICaptchaService>();
            captchaServiceMock.Setup(c => c.IsCaptchaValid(It.IsAny<string>())).ReturnsAsync(true);

            // Замінимо у контролері капча сервіс на мок
            typeof(AccountController)
                .GetField("_captchaService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_controller, captchaServiceMock.Object);

            // Mock UserManager і SignInManager
            _userManagerMock.Setup(um => um.FindByEmailAsync(model.EmailAddress)).ReturnsAsync(user);
            _userManagerMock.Setup(um => um.CheckPasswordAsync(user, model.Password)).ReturnsAsync(true);
            _signInManagerMock.Setup(sm => sm.PasswordSignInAsync(user, model.Password, false, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            // Act
            var result = await _controller.Login(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Game", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Logout_ShouldSignOutAndRedirectToLogin()
        {
            // Act
            var result = await _controller.Logout();

            // Assert
            _signInManagerMock.Verify(s => s.SignOutAsync(), Times.Once);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Register_Post_ValidModel_RegistersUserAndRedirects()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                EmailAddress = "test@example.com",
                Password = "Test123!",
                ConfirmPassword = "Test123!"
            };

            var user = new User
            {
                Email = model.EmailAddress,
                UserName = model.EmailAddress,
                FullName = model.EmailAddress
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(model.EmailAddress))
                .ReturnsAsync((User)null);

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), model.Password))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), UserRoles.User))
                .ReturnsAsync(IdentityResult.Success);

            _captchaServiceMock.Setup(x => x.IsCaptchaValid(It.IsAny<string>()))
                .ReturnsAsync(true);

            // створюємо підроблений HTTP контекст з g-recaptcha-response
            var httpContext = new DefaultHttpContext();
            var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
    {
        { "g-recaptcha-response", "dummy-token" }
    });
            httpContext.Request.Form = formCollection;

            var controller = new AccountController(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _context,
                _webHostEnvironmentMock.Object,
                _accountRepoMock.Object,
                _captchaServiceMock.Object
            )
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                },
                TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>())
            };

            // Act
            var result = await controller.Register(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Game", redirectResult.ControllerName);
        }
    }
}
