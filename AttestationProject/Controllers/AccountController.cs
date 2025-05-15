using System;
using System.IO;
using System.Threading.Tasks;
using AttestationProject.Models;
using AttestationProject.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AttestationProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        /* ------------ РЕГИСТРАЦИЯ ------------ */

        [HttpGet, AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid)
                return View(model);

            if (await _userManager.FindByEmailAsync(model.Email) != null)
            {
                ModelState.AddModelError(nameof(model.Email), "Пользователь с такой почтой уже существует");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);
                return View(model);
            }

            await _userManager.AddToRoleAsync(user, "User");
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToLocal(returnUrl);
        }

        /* --------------- ВХОД --------------- */

        [HttpGet, AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && !await _userManager.IsEmailConfirmedAsync(user))
            {
                ModelState.AddModelError(string.Empty, "Пожалуйста, подтвердите электронную почту перед входом.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
                return RedirectToLocal(returnUrl);

            if (result.IsLockedOut)
                ModelState.AddModelError(string.Empty, "Учетная запись заблокирована.");
            else if (result.RequiresTwoFactor)
                ModelState.AddModelError(string.Empty, "Требуется двухфакторная аутентификация.");
            else
                ModelState.AddModelError(string.Empty, "Неверный логин или пароль.");

            return View(model);
        }

        /* --------------- ВЫХОД --------------- */

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        /* -------------- ПРОФИЛЬ -------------- */

        [HttpGet, Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            return View(new ProfileViewModel
            {
                FullName = user.FullName,
                Address = user.Address,
                PhoneNumber = user.PhoneNumber,
                ExistingImagePath = user.ProfileImage
            });
        }

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (!ModelState.IsValid)
            {
                model.ExistingImagePath = user.ProfileImage;
                return View(model);
            }

            user.FullName = model.FullName;
            user.Address = model.Address;
            user.PhoneNumber = model.PhoneNumber;

            // Работаем с файлом из модели
            if (model.ProfileImage != null && model.ProfileImage.Length > 0)
            {
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profiles");
                Directory.CreateDirectory(uploads);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.ProfileImage.FileName)}";
                var filePath = Path.Combine(uploads, fileName);

                await using var stream = new FileStream(filePath, FileMode.Create);
                await model.ProfileImage.CopyToAsync(stream);

                user.ProfileImage = $"/images/profiles/{fileName}";
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (updateResult.Succeeded)
                return RedirectToAction("Index", "Home");

            foreach (var e in updateResult.Errors)
                ModelState.AddModelError(string.Empty, e.Description);

            model.ExistingImagePath = user.ProfileImage;
            return View(model);
        }

        /* ---------- ACCESS DENIED ---------- */

        [HttpGet, AllowAnonymous]
        public IActionResult AccessDenied() => View("AccessDenied");

        #region Helpers
        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }
        #endregion
    }
}
