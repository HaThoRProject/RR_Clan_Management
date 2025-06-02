using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

public class HomeController : Controller
{
    private readonly FirebaseAuthService _authService;
    private readonly LogService _logService;

    public HomeController(FirebaseAuthService authService, LogService logService)
    {
        _authService = authService;
        _logService = logService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string name, string password)
    {
        if (await _authService.AuthenticateUserAsync(name, password))
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var identity = new ClaimsIdentity(claims, "FirebaseAuth");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(principal);

            // Naplózás sikeres bejelentkezésről
            await _logService.LogEventAsync(name, "Login", "Sikeres bejelentkezés");

            return RedirectToAction("Index", "Home");
        }

        // Naplózás sikertelen bejelentkezésről
        await _logService.LogEventAsync(name, "Login", "Sikertelen bejelentkezés");

        ViewData["Error"] = "Hibás felhasználónév vagy jelszó!";
        return View("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        string userName = User.Identity?.Name ?? "Ismeretlen";
        await HttpContext.SignOutAsync();

        // Naplózás kijelentkezésről
        await _logService.LogEventAsync(userName, "Logout", "Felhasználó kijelentkezett");

        return RedirectToAction("Index", "Home");
    }
}
