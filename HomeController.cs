using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using BCrypt;

public class HomeController : Controller
{
    private readonly FirebaseAuthService _authService;

    public HomeController(FirebaseAuthService authService)
    {
        _authService = authService;
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
            return RedirectToAction("Index", "Home");
        }

        ViewData["Error"] = "Invalid login!";
        return View("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}
