using System;
using System.IO;
using System.Threading.Tasks;
using AttestationProject.Models;
using AttestationProject.Models.ViewModels;
using AttestationProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AttestationProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _email;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _email = emailSender;
            _logger = logger;
        }

        /* ------------ РЕГИСТРАЦИЯ ------------ */

        [HttpGet, AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid)
                return View(model);

            if (await _userManager.FindByEmailAsync(model.Email) != null)
            {
                ModelState.AddModelError("Email", "Пользователь с такой почтой уже существует");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true    // сразу помечаем подтверждённым
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError("", e.Description);
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

        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid)
                return View(model);

            // на всякий случай проверяем подтверждение, но у нас отключено RequireConfirmedEmail
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && !await _userManager.IsEmailConfirmedAsync(user))
            {
                ModelState.AddModelError("", "Подтвердите почту перед входом");
                return View(model);
            }

            var res = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (res.Succeeded)
                return RedirectToLocal(returnUrl);

            if (res.IsLockedOut)
                ModelState.AddModelError("", "Учетная запись заблокирована");
            else if (res.RequiresTwoFactor)
                ModelState.AddModelError("", "Требуется двухфакторная аутентификация");
            else
                ModelState.AddModelError("", "Неверный логин или пароль");

            return View(model);
        }

        /* --------------- ВЫХОД --------------- */

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        /* ---------- ЗАБЫЛ ПАРОЛЬ ---------- */

        [HttpGet, AllowAnonymous]
        public IActionResult ForgotPassword() => View();

        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && await _userManager.IsEmailConfirmedAsync(user))
            {
                try
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var link = Url.Action("ResetPassword", "Account",
                                 new { userId = user.Id, token }, Request.Scheme)!;

                    await _email.SendAsync(user.Email, "Сброс пароля",
                        $"<p>Для сброса пароля перейдите по <a href='{link}'>ссылке</a>.</p>");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Send reset-password mail failed");
                }
            }
            return View("ForgotPasswordConfirmation");
        }

        /* ---------- СБРОС ПАРОЛЯ ---------- */

        [HttpGet, AllowAnonymous]
        public IActionResult ResetPassword(string userId, string token)
        {
            if (userId == null || token == null)
                return BadRequest();
            return View(new ResetPasswordViewModel { UserId = userId, Token = token });
        }

        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
                return View("ResetPasswordSuccess");

            var res = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (res.Succeeded)
                return View("ResetPasswordSuccess");

            foreach (var e in res.Errors)
                ModelState.AddModelError("", e.Description);
            return View(model);
        }

        /* -------------- ПРОФИЛЬ -------------- */

        [Authorize]
        [HttpGet]
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

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Profile(ProfileViewModel model, IFormFile profileImage)
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

            if (profileImage != null && profileImage.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(profileImage.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(),
                                            "wwwroot/images/profiles", fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                await using var stream = new FileStream(path, FileMode.Create);
                await profileImage.CopyToAsync(stream);
                user.ProfileImage = "/images/profiles/" + fileName;
            }

            var res = await _userManager.UpdateAsync(user);
            if (res.Succeeded)
                return RedirectToAction("Index", "Home");

            foreach (var e in res.Errors)
                ModelState.AddModelError("", e.Description);

            model.ExistingImagePath = user.ProfileImage;
            return View(model);
        }

        /* ---------- ACCESS DENIED ---------- */

        [HttpGet]
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
