using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

public class FirebaseTestController : Controller
{
    [HttpGet]
    public async Task<IActionResult> TestConnection()
    {
        try
        {
            // Próbáljunk meg kapcsolódni a Firebase-hez egy egyszerű token ellenőrzéssel.
            var auth = FirebaseAuth.DefaultInstance;
            var userRecord = await auth.GetUserAsync("0iXCB6V8H7QrFTiZF3VsQlvcw1s2"); // Például egy létező felhasználó UID-ját keresheted

            return Ok(new { message = "Firebase kapcsolat sikeres!", user = userRecord });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Hiba történt a Firebase kapcsolódáskor.", error = ex.Message });
        }
    }
}
