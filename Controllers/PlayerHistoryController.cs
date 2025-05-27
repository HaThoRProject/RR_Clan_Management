using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using RR_Clan_Management.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RR_Clan_Management.Controllers
{
    public class PlayerHistoryController : Controller
    {
        private readonly FirestoreDb _firestoreDb;

        public PlayerHistoryController()
        {
            _firestoreDb = FirestoreDb.Create("rr-clan-management");
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            DocumentReference docRef = _firestoreDb.Collection("players").Document(id);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists)
                return NotFound();

            var player = snapshot.ConvertTo<Models.Player>();

            if (player.MembershipHistory == null)
                player.MembershipHistory = new System.Collections.Generic.List<Models.MembershipPeriod>();

            return View(player);
        }
    }
}
