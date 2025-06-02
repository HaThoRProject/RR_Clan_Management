using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using RR_Clan_Management.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using RR_Clan_Management.Services;
using System.Diagnostics;


namespace RR_Clan_Management.Controllers
{
    public class PlayerHistoryController : Controller
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly LogService _logService;
        private readonly IStatService _statService;

        public PlayerHistoryController(FirestoreDb firestoreDb,
    LogService logService,
    IStatService statService)
        {
            _firestoreDb = FirestoreDb.Create("rr-clan-management");
            _logService = new LogService();
            _statService = statService;
        }

        public async Task<IActionResult> Details(string id)
        {
            var username = User.Identity?.Name ?? "Ismeretlen";

            await _logService.LogEventAsync(
                username,
                "Játékos részletek",
                $"Játékos adatlap megnyitva: {id}"
            );

            if (string.IsNullOrEmpty(id))
                return NotFound();

            DocumentReference docRef = _firestoreDb.Collection("players").Document(id);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists)
                return NotFound();

            var player = snapshot.ConvertTo<Models.Player>();

            await _logService.LogEventAsync(
                username,
                "Játékos részletek",
                $"Játékos adatlap megnyitva: {player.PlayerName ?? "Ismeretlen játékos"}"
            );

            // Biztonság kedvéért ellenőrizzük a tagsági történetet
            if (player.MembershipHistory == null)
                player.MembershipHistory = new List<MembershipPeriod>();

            // Statisztikai adatok betöltése
            var allStats = await _statService.GetWarTourStatsAsync();            
            var last3Stats = await _statService.GetWarTourStatsAsync(onlyLast3: true);

            string playerNameNormalized = (player.PlayerName ?? "").Trim().ToLower();            

            var statRow = allStats.PlayerStats
            .FirstOrDefault(r => (r.PlayerName ?? "").Trim().ToLower() == playerNameNormalized)
            ?? new PlayerStat { PlayerName = player.PlayerName };

            var statRowLast3 = last3Stats.Rows
                .FirstOrDefault(r => (r.PlayerName ?? "").Trim().ToLower() == playerNameNormalized)
                ?? new WarTourStatsRow { PlayerName = player.PlayerName };



            var viewModel = new PlayerDetailsViewModel
            {
                Player = player,
                StatRow = statRow,
                StatRowLast3 = statRowLast3
            };

            return View(viewModel);
        }

    }
}
