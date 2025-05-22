using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using PCGameRate.Controllers;
using PCGameRate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCGameRate.Tests
{
    public class UserManagerControllerTests
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
        private readonly UserManagerController _controller;

        public UserManagerControllerTests()
        {
            var userStoreMock = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(userStoreMock.Object, null, null, null, null, null, null, null, null);

            var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
            _roleManagerMock = new Mock<RoleManager<IdentityRole>>(roleStoreMock.Object, null, null, null, null);

            _controller = new UserManagerController(_userManagerMock.Object, _roleManagerMock.Object);

            var tempData = new Mock<ITempDataDictionary>();
            _controller.TempData = tempData.Object;
        }

        [Fact]
        public async Task Search_ReturnsViewWithFilteredUsers()
        {
            // Arrange
            var users = new List<User>
        {
            new User { Id = "1", FullName = "John Doe" },
            new User { Id = "2", FullName = "Jane Smith" },
            new User { Id = "3", FullName = "Johnny Appleseed" }
        }.AsQueryable();

            _userManagerMock.Setup(u => u.Users).Returns(users);

            var searchName = "John";

            // Act
            var result = await _controller.Search(searchName);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<User>>(viewResult.Model);
            Assert.All(model, u => Assert.Contains(searchName, u.FullName));
        }

        [Fact]
        public async Task ChangeRole_ReturnsNotFound_WhenUserNotFound()
        {
            // Arrange
            _userManagerMock.Setup(u => u.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((User)null);

            // Act
            var result = await _controller.ChangeRole("nonexistent-id", "admin");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ChangeRole_ChangesRoleAndRedirects_WhenUserExists()
        {
            // Arrange
            var user = new User { Id = "1", FullName = "John Doe" };
            var currentRoles = new List<string> { "user" };
            var newRole = "admin";

            _userManagerMock.Setup(u => u.FindByIdAsync(user.Id)).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.GetRolesAsync(user)).ReturnsAsync(currentRoles);
            _userManagerMock.Setup(u => u.RemoveFromRolesAsync(user, currentRoles)).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(u => u.AddToRoleAsync(user, newRole)).ReturnsAsync(IdentityResult.Success);

            var tempData = new Mock<ITempDataDictionary>();
            _controller.TempData = tempData.Object;

            // Act
            var result = await _controller.ChangeRole(user.Id, newRole);

            // Assert
            _userManagerMock.Verify(u => u.RemoveFromRolesAsync(user, currentRoles), Times.Once);
            _userManagerMock.Verify(u => u.AddToRoleAsync(user, newRole), Times.Once);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);

            tempData.VerifySet(t => t["SuccessMessage"] = $"Роль змінено на '{newRole}' для {user.FullName}", Times.Once);
        }
    }
}
