using Microsoft.AspNetCore.Mvc;
using Google.Cloud.Firestore;
using RR_Clan_Management.Models;
using System.Threading.Tasks;

namespace RR_Clan_Management.Controllers
{
    public class PlayerDetailController : Controller
    {
        private readonly FirestoreDb _firestoreDb;

        public PlayerDetailController()
        {
            // Cseréld le a "your-project-id"-t a saját Firestore projektazonosítódra
            _firestoreDb = FirestoreDb.Create("rr-clan-management");
        }

        // GET: /PlayerDetail/Index/{playerId}
        public async Task<IActionResult> Index(string playerId)
        {
            if (string.IsNullOrEmpty(playerId))
                return BadRequest("Player ID is required.");

            var playerDocRef = _firestoreDb.Collection("players").Document(playerId);
            var snapshot = await playerDocRef.GetSnapshotAsync();

            if (!snapshot.Exists)
                return NotFound();

            Player player = snapshot.ConvertTo<Player>();

            return View(player);
        }
    }
}
