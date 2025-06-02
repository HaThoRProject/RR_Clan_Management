using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using RR_Clan_Management.Models;
using RR_Clan_Management.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace RR_Clan_Management.Controllers
{
    public class StatisticsController : Controller
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly LogService _logService;

        public StatisticsController()
        {
            _firestoreDb = FirestoreDb.Create("rr-clan-management");
            _logService = new LogService();
        }

        // GET: /Statistics/
        public async Task<IActionResult> Index()
        {
            await _logService.LogEventAsync(User.Identity?.Name ?? "Ismeretlen", "Statisztika_Fő oldal", "Statisztika főoldal megnyitva");
            return View();
        }

        // GET: /Statistics/PlayerStats
        public async Task<IActionResult> PlayerStats()
        {
            await _logService.LogEventAsync(User.Identity?.Name ?? "Ismeretlen", "Statisztika_Players", "PlayerStats oldal megnyitva");

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
        public async Task<IActionResult> WarTourStats()
        {
            await _logService.LogEventAsync(User.Identity?.Name ?? "Ismeretlen", "Statisztika_WarTour", "WarTourStats oldal megnyitva");

            // Játékosok lekérése
            var playersSnapshot = await _firestoreDb.Collection("players").GetSnapshotAsync();
            var players = playersSnapshot.Documents
                .Select(doc => doc.ConvertTo<Player>())
                .Where(p => !string.IsNullOrWhiteSpace(p.PlayerName))
                .ToList();

            // WarTour bejegyzések lekérése
            var tourSnapshot = await _firestoreDb.Collection("WarTour").GetSnapshotAsync();

            var entries = new List<WarTourEntry>();
            foreach (var doc in tourSnapshot.Documents)
            {
                var entry = doc.ConvertTo<WarTourEntry>();
                if (entry?.Columns == null)
                    entry.Columns = new Dictionary<string, string>();
                entries.Add(entry);
            }

            // Modell összeállítása
            var model = new WarTourStatsViewModel
            {
                Rows = players.Select(player =>
                {
                    // Az adott játékos bejegyzései
                    var playerEntries = entries
                        .Where(e => e.PlayerName == player.PlayerName)
                        .SelectMany(e => e.Columns?.Values ?? Enumerable.Empty<string>())
                        .ToList();

                    // Csak értelmes válaszokat szűrünk ki (kivéve "-", "", null)
                    var filtered = playerEntries
                        .Where(v => !string.IsNullOrWhiteSpace(v) && v != "-")
                        .ToList();

                    int participated = filtered.Count(v => v == "Részt vett");
                    int partial = filtered.Count(v => v == "Részben vett részt");
                    int missed = filtered.Count(v => v == "Nem vett részt");
                    int excused = filtered.Count(v => v == "Felmentve");
                    int total = filtered.Count;
                    bool isLeft = player.LeaveDate != null;

                    string note = total == 0 ? "Nem vett részt vagy nem lett rögzítve adat" : "";

                    return new WarTourStatsRow
                    {
                        PlayerName = player.PlayerName ?? "N/A",
                        Participated = participated,
                        Partial = partial,
                        Missed = missed,
                        Excused = excused,
                        TotalEvents = total,
                        IsLeft = isLeft, // ← Itt állítjuk be a színezéshez
                        Note = note                        
                    };
                })
                .OrderByDescending(r => r.TotalEvents)
                .ThenByDescending(r => r.ParticipationPercent)
                .ThenBy(r => r.PlayerName)
                .ToList()
            };

            return View(model);
        }

        public async Task<IActionResult> WarTourStatsLast3()
        {
            await _logService.LogEventAsync(User.Identity?.Name ?? "Ismeretlen", "Statisztika_Utolsó 3 War", "WarTourStatsLast3 oldal megnyitva");
            
            var playersSnapshot = await _firestoreDb.Collection("players").GetSnapshotAsync();
            var tourSnapshot = await _firestoreDb.Collection("WarTour").GetSnapshotAsync();

            // Aktív játékosok lekérése
            var players = new List<Player>();
            foreach (var doc in playersSnapshot.Documents)
            {
                var player = doc.ConvertTo<Player>();
                if (player.LeaveDate == null) // Csak aktív játékos
                {
                    players.Add(player);
                }
            }

            // War/Tour bejegyzések lekérése
            var entries = new List<WarTourEntry>();
            foreach (var doc in tourSnapshot.Documents)
            {
                var entry = doc.ConvertTo<WarTourEntry>();
                if (entry.Columns == null)
                    entry.Columns = new Dictionary<string, string>();
                entries.Add(entry);
            }

            // Utolsó 3 dátum kiszűrése (formátum: yyyy.MM.dd)
            var latest3Dates = entries
                .SelectMany(e => e.Columns.Keys)
                .Where(k => DateTime.TryParseExact(k, "yyyy.MM.dd", null, System.Globalization.DateTimeStyles.None, out _))
                .Select(k => DateTime.ParseExact(k, "yyyy.MM.dd", null))
                .Distinct()
                .OrderByDescending(d => d)
                .Take(3)
                .ToList();

            var latest3Keys = latest3Dates.Select(d => d.ToString("yyyy.MM.dd")).ToList();

            // Modell összeállítása
            var model = new WarTourStatsViewModel
            {
                Rows = players.Select(player =>
                {
                    // Az adott játékos bejegyzései az utolsó 3 dátumra
                    var playerEntries = entries
                        .Where(e => e.PlayerName == player.PlayerName)
                        .SelectMany(e => e.Columns
                            .Where(c => latest3Keys.Contains(c.Key))
                            .Select(c => c.Value)
                        )
                        .ToList();

                    var filtered = playerEntries
                        .Where(v => !string.IsNullOrWhiteSpace(v) && v != "-")
                        .ToList();

                    var last3Map = latest3Keys
                        .Select(k =>
                        {
                            var entry = entries.FirstOrDefault(e => e.PlayerName == player.PlayerName);
                                return entry?.Columns?.GetValueOrDefault(k) ?? "";
                        })
                        .ToList();


                    int participated = filtered.Count(v => v == "Részt vett");
                    int partial = filtered.Count(v => v == "Részben vett részt");
                    int missed = filtered.Count(v => v == "Nem vett részt");
                    int excused = filtered.Count(v => v == "Felmentve");
                    int total = filtered.Count;

                    string note = total == 0 ? "Nem vett még részt" : "";

                    return new WarTourStatsRow
                    {
                        PlayerName = player.PlayerName ?? "N/A",
                        Participated = participated,
                        Partial = partial,
                        Missed = missed,
                        Excused = excused,
                        TotalEvents = total,
                        Last3Entries = last3Map,
                        Note = note
                    };
                })
                .OrderByDescending(r => r.TotalEvents)                   // 1. Teljes események száma
                .ThenByDescending(r => r.ParticipationPercent)          // 2. Részvételi arány
                .ThenBy(r => r.PlayerName)                             // 3. Névsor
                .ThenBy(r => r.Missed)                                // 4. Kevesebb kihagyás előrébb
                .ToList()
            };

            return View("WarTourStats", model); // Ha külön nézet kell, írd át pl. "WarTourStatsLast3"
        }


    }
}
