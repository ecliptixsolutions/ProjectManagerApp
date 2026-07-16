using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagerApp.Data;
using ProjectManagerApp.Models;
using ProjectManagerApp.ViewModels;

namespace ProjectManagerApp.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;

    public AccountController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _context.Users.SingleOrDefaultAsync(u => u.UserName == model.UserName);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View(model);
        }

        var hasher = new PasswordHasher<AppUser>();
        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
        if (result == PasswordVerificationResult.Success)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            await LogAudit("Login", "Account", user.Id, $"User {user.UserName} logged in", user);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, "Invalid username or password.");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var userId = GetCurrentUserId();
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
            await LogAudit("Logout", "Account", userId, $"User {user.UserName} logged out");

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login", "Account");
    }

    public IActionResult AccessDenied()
    {
        return View();
    }

    private int GetCurrentUserId()
    {
        var idValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idValue, out var id) ? id : 0;
    }

    private async Task LogAudit(string action, string entityType, int entityId, string? metadata, AppUser? user = null)
    {
        if (user == null)
            user = await _context.Users.FindAsync(GetCurrentUserId());

        _context.AuditLogs.Add(new AuditLog
        {
            UserId = user?.Id ?? 0,
            UserName = user?.UserName,
            UserRole = user?.Role,
            ActionType = action,
            EntityType = entityType,
            EntityId = entityId,
            ModuleName = "Account",
            Metadata = metadata,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            Timestamp = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
    }
}
