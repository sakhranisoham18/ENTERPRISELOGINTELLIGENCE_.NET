using System.Security.Claims;
using EnterpriseLogIntelligence.Services;
using EnterpriseLogIntelligence.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseLogIntelligence.Controllers;

public sealed class AccountController : Controller
{
    private readonly IAuthService _auth;
    private readonly IAuditService _audit;
    public AccountController(IAuthService auth, IAuditService audit) { _auth = auth; _audit = audit; }

    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null) => View(new LoginViewModel { ReturnUrl = returnUrl, Username = "admin" });

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await _auth.AuthenticateAsync(model.Username, model.Password, ct);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password, or the account is inactive.");
            return View(model);
        }

        var claims = new[] { new Claim(ClaimTypes.Name, user.Username), new Claim(ClaimTypes.Role, user.Role) };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        await _audit.RecordAsync(user.Username, "Auth.Login", "Successful sign-in.", ct);
        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl)) return LocalRedirect(model.ReturnUrl);
        return RedirectToAction("Index", "Dashboard");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var username = User.Identity?.Name ?? "system";
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await _audit.RecordAsync(username, "Auth.Logout", "Signed out.", ct);
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    public IActionResult AccessDenied() => View();
}
