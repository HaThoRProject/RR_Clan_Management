using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using RR_Clan_Management.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RR_Clan_Management.Controllers
{
    public class StatisticsController : Controller
    {
        private readonly FirestoreDb _firestoreDb;

        public StatisticsController()
        {
            _firestoreDb = FirestoreDb.Create("rr-clan-management");
        }

        // GET: /Statistics/
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Statistics/PlayerStats
        public async Task<IActionResult> PlayerStats()
        {
            Query playersQuery = _firestoreDb.Collection("players");
            QuerySnapshot playersSnapshot = await playersQuery.GetSnapshotAsync();
            List<Player> players = new List<Player>();

            int activeCount = 0;
            int inactiveCount = 0;

            // Kiszámoljuk az eltöltött időt a klánban minden player esetén
            var playersWithDuration = new List<(Player player, int durationDays)>();

            foreach (DocumentSnapshot doc in playersSnapshot.Documents)
            {
                if (doc.Exists)
                {
                    Player player = doc.ConvertTo<Player>();

                    if (string.IsNullOrEmpty(player.LeaveDate))
                        activeCount++;
                    else
                        inactiveCount++;

                    DateTime joinDate = DateTime.TryParse(player.JoinDate, out var jd) ? jd : DateTime.MinValue;
                    DateTime leaveDate = DateTime.TryParse(player.LeaveDate, out var ld) ? ld : DateTime.Now;

                    int durationDays = (leaveDate - joinDate).Days;
                    playersWithDuration.Add((player, durationDays));
                }
            }

            // Rendezés az eltöltött napok alapján csökkenő sorrendben
            var sortedPlayers = playersWithDuration
                .OrderByDescending(p => p.durationDays)
                .Select(p => p.player)
                .ToList();

            ViewBag.TotalPlayers = playersWithDuration.Count;
            ViewBag.ActivePlayers = activeCount;
            ViewBag.InactivePlayers = inactiveCount;
            ViewBag.FreeSlots = 50 - activeCount;

            return View(sortedPlayers);
        }


        // GET: /Statistics/WarTourStats
        public IActionResult WarTourStats()
        {
            return View();
        }
    }
}
