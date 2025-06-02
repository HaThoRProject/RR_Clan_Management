using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RR_Clan_Management.Models;
using RR_Clan_Management.Services;

namespace RR_Clan_Management.Controllers
{
    public class PlayerController : Controller
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly LogService _logService;
        public PlayerController()
        {
            _firestoreDb = FirestoreDb.Create("rr-clan-management"); // Firestore inicializálás
            _logService = new LogService();
        }
        public async Task<IActionResult> Index()
        {
            await _logService.LogEventAsync(User.Identity?.Name ?? "Ismeretlen", "Player_menu", "Player menu opened");

            Query playersQuery = _firestoreDb.Collection("players");
            QuerySnapshot playersSnapshot = await playersQuery.GetSnapshotAsync();
            List<Player> players = new List<Player>();

            foreach (DocumentSnapshot doc in playersSnapshot.Documents)
            {
                if (doc.Exists)
                {
                    Player player = doc.ConvertTo<Player>();

                    // Stringből DateTime konverzió
                    DateTime? JoinDate = DateTime.TryParse(player.JoinDate, out DateTime tempBel) ? tempBel : (DateTime?)null;
                    DateTime? LeaveDate = DateTime.TryParse(player.LeaveDate, out DateTime tempKil) ? tempKil : (DateTime?)null;
                    players.Add(player);
                }
            }

            return View(players);
        }
        

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await _logService.LogEventAsync(User.Identity?.Name ?? "Ismeretlen", "Player felvitel", "Create player page opened");
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Create(Player player)
        {
           if (ModelState.IsValid)
            {
                try
                {
                    DocumentReference docRef = _firestoreDb.Collection("players").Document();
                    player.Id = docRef.Id;

                    // MembershipHistory inicializálása
                    player.MembershipHistory = new List<MembershipPeriod>
            {
                new MembershipPeriod
                {
                    JoinDate = DateTime.TryParse(player.JoinDate, out var parsedJoinDate)
                        ? parsedJoinDate.ToString("yyyy-MM-dd")
                        : DateTime.Now.ToString("yyyy-MM-dd"),

                    LeaveDate = null
                }
            };

                    await docRef.SetAsync(player);

                    await _logService.LogEventAsync(User.Identity?.Name ?? "Ismeretlen", "Player felvitel", $"Player felvéve: {player.Name} ({player.PlayerName})");

                    Console.WriteLine("Player successfully added to Firestore");

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error while adding player: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "An error occurred while adding the player.");
                }
            }

            return View(player);
        }

        // GET: Edit Player - Megjeleníti a módosítási űrlapot
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {

            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            DocumentReference docRef = _firestoreDb.Collection("players").Document(id);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists)
            {
                return NotFound();
            }

            Player player = snapshot.ConvertTo<Player>();

            await _logService.LogEventAsync(User.Identity?.Name ?? "Ismeretlen", "Player_módosítás", $"Player módosítás: {player.Name} ({player.PlayerName})");

            return View(player); // A View betölti az edit űrlapot az adatokkal
        }

        // POST: Edit Player - Mentés gombra kattintás után módosítja az adatokat
        [HttpPost]
        public async Task<IActionResult> Edit(Player player)
        {
            if (ModelState.IsValid)
            {
                DocumentReference docRef = _firestoreDb.Collection("players").Document(player.Id);
                DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    return NotFound();
                }

                Player existingPlayer = snapshot.ConvertTo<Player>();

                // Biztonság kedvéért inicializáljuk
                if (existingPlayer.MembershipHistory == null)
                {
                    existingPlayer.MembershipHistory = new List<MembershipPeriod>();
                }

                // Ha van előző időszak (mindkét dátum nem üres) és még nincs történet mentve, akkor elmentjük
                if (string.IsNullOrEmpty(existingPlayer.LeaveDate) == false && string.IsNullOrEmpty(existingPlayer.JoinDate) == false && existingPlayer.MembershipHistory.Count == 0)
                {
                    existingPlayer.MembershipHistory.Add(new MembershipPeriod
                    {
                        JoinDate = existingPlayer.JoinDate,
                        LeaveDate = existingPlayer.LeaveDate
                    });
                }

                // Dátumparszolás
                DateTime? newLeaveDate = DateTime.TryParse(player.LeaveDate, out var parsedLeaveDate) ? parsedLeaveDate : (DateTime?)null;
                DateTime? oldLeaveDate = DateTime.TryParse(existingPlayer.LeaveDate, out var parsedOldLeaveDate) ? parsedOldLeaveDate : (DateTime?)null;

                bool nowLeft = string.IsNullOrEmpty(existingPlayer.LeaveDate) && newLeaveDate != null;
                bool nowReturned = !string.IsNullOrEmpty(existingPlayer.LeaveDate) && string.IsNullOrEmpty(player.LeaveDate);

                // Távozás: frissítjük az utolsó tagsági időszak LeaveDate-jét
                if (nowLeft)
                {
                    var lastPeriod = existingPlayer.MembershipHistory.FindLast(p => p.LeaveDate == null);
                    if (lastPeriod != null)
                    {
                        lastPeriod.LeaveDate = newLeaveDate.HasValue
                            ? newLeaveDate.Value.ToString("yyyy-MM-dd")
                            : null;
                    }
                }
                // Visszatérés: új tagsági időszak
                else if (nowReturned)
                {
                    existingPlayer.MembershipHistory.Add(new MembershipPeriod
                    {
                        JoinDate = DateTime.TryParse(player.JoinDate, out var newJoinDate)
                            ? newJoinDate.ToString("yyyy-MM-dd")
                            : DateTime.Now.ToString("yyyy-MM-dd"),

                        LeaveDate = null
                    });
                }

                // Egyéb adatok frissítése
                existingPlayer.Name = player.Name;
                existingPlayer.PlayerName = player.PlayerName;
                existingPlayer.JoinDate = player.JoinDate;
                existingPlayer.LeaveDate = player.LeaveDate;
                existingPlayer.Notes = player.Notes;

                await docRef.SetAsync(existingPlayer, SetOptions.MergeAll);
                await _logService.LogEventAsync(User.Identity?.Name ?? "Ismeretlen", "Player módosítás", $"Player updated: {existingPlayer.Name} ({existingPlayer.PlayerName})");

                return RedirectToAction("Index");
            }

            return View(player);
        }
        public async Task<IActionResult> Delete(string id)
        {
            DocumentReference docRef = _firestoreDb.Collection("players").Document(id);
            await docRef.DeleteAsync();
            await _logService.LogEventAsync(User.Identity?.Name ?? "Ismeretlen", "Player törlés", $"Player deleted with ID: {id}");
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> CancelEdit(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                DocumentReference docRef = _firestoreDb.Collection("players").Document(id);
                DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

                if (snapshot.Exists)
                {
                    Player player = snapshot.ConvertTo<Player>();
                    await _logService.LogEventAsync(User.Identity?.Name ?? "Ismeretlen", "Player módosítás visszavonva", $"Edit canceled for: {player.Name} ({player.PlayerName})");
                }
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> CancelCreate()
        {
            await _logService.LogEventAsync(User.Identity?.Name ?? "Ismeretlen", "Player felvitel visszavonva", "Player creation was canceled.");
            return RedirectToAction("Index");
        }
    }
}
