﻿using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using RR_Clan_Management.Models;
using RR_Clan_Management.Services;
using RR_Clan_Management.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RR_Clan_Management.Controllers
{
    public class WarTourController : Controller
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly LogService _logService;

        public WarTourController(FirestoreService firestoreService, LogService logService)
        {
            _firestoreDb = FirestoreDb.Create("rr-clan-management"); // Itt add meg a Firestore projekt azonosítóját
            _logService = new LogService();
        }
        public async Task<IActionResult> Index(bool showAll = false)
        {
            // 🔸 Logoljuk, ki nyitotta meg az oldalt
            await _logService.LogEventAsync(
                User.Identity?.Name ?? "Ismeretlen",
                "WarTourView",
                "War/Tour oldal megnyitva"
            );

            var players = await GetPlayersAsync(showAll);
            players = players.OrderBy(p => p.PlayerName).ToList();
            var warTourEntries = await GetWarTourEntriesAsync();

            var allColumns = warTourEntries
                .Where(e => e.Columns != null)
                .SelectMany(e => e.Columns.Keys)
                .Distinct()
                .ToList();

            var model = new WarTourViewModel
            {
                Rows = players.Select(p =>
                {
                    var entry = warTourEntries.FirstOrDefault(e => e.PlayerName == p.PlayerName);
                    return new WarTourRow
                    {
                        PlayerName = p.PlayerName ?? "N/A",
                        Columns = entry?.Columns ?? new Dictionary<string, string>()
                    };
                }).ToList(),
                ColumnHeaders = allColumns
            };

            ViewBag.ShowAll = showAll; // 🔹 Eltároljuk az aktuális állapotot a nézethez

            return View(model);
        }
        private async Task<List<WarTourEntry>> GetWarTourEntriesAsync()
        {
            var tourCollection = _firestoreDb.Collection("WarTour");
            var snapshot = await tourCollection.GetSnapshotAsync();

            var warTourEntries = new List<WarTourEntry>();

            foreach (var doc in snapshot.Documents)
            {
                var entry = doc.ConvertTo<WarTourEntry>();

                if (entry.Columns == null)
                {
                    entry.Columns = new Dictionary<string, string>(); // Ha nincs adat, üres dictionary
                }

                warTourEntries.Add(entry);
            }

            return warTourEntries;
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] List<WarTourEntry> entries)
        {
            var tourCollection = _firestoreDb.Collection("WarTour");

            foreach (var entry in entries)
            {
                var docRef = tourCollection.Document(entry.PlayerName);
                var docSnapshot = await docRef.GetSnapshotAsync();

                Dictionary<string, string> updatedColumns = entry.Columns ?? new Dictionary<string, string>();

                if (docSnapshot.Exists)
                {
                    var existingData = docSnapshot.ConvertTo<WarTourEntry>();

                    // 🔁 Itt a korábbi összevonás helyett felülírjuk a Columns-t
                    existingData.Columns = updatedColumns;

                    await docRef.SetAsync(existingData);
                }
                else
                {
                    await docRef.SetAsync(entry);
                }
            }

            return Json(new { success = true });
        }
        private async Task<List<Player>> GetPlayersAsync(bool showAll)
        {
            var playerCollection = _firestoreDb.Collection("players");
            var snapshot = await playerCollection.GetSnapshotAsync();
            var players = snapshot.Documents
                .Select(doc => doc.ConvertTo<Player>())
                .Where(p => showAll || string.IsNullOrEmpty(p.LeaveDate)) // Ha showAll igaz, akkor nincs szűrés
                .ToList();

            System.Diagnostics.Debug.WriteLine($"[DEBUG] Szűrt játékosok száma: {players.Count}");

            return players;
        }

        [HttpPost]
        public async Task<IActionResult> LogClientEvent([FromBody] LogEntry entry)
        {
            await _logService.LogEventAsync(
                User.Identity?.Name ?? "Anonymous",
                entry.Message,
                entry.Type ?? "ClientEvent"
                );
            return Ok();
        }


        public class LogEntry
        {
            public string Message { get; set; }
            public string Type { get; set; } // Új mező
        }

    }
}
