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

        public PlayerController()
        {
            _firestoreDb = FirestoreDb.Create("rr-clan-management"); // Firestore inicializálás
        }

        public async Task<IActionResult> Index()
        {
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

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Player player)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Firestore-ban való mentés
                    DocumentReference docRef = _firestoreDb.Collection("players").Document();
                    player.Id = docRef.Id; // Egyedi ID generálása
                    await docRef.SetAsync(player);

                    // Ha sikeres, naplózzuk
                    Console.WriteLine("Player successfully added to Firestore");

                    // Az új játékos hozzáadása után frissítjük a listát
                    Query playersQuery = _firestoreDb.Collection("players");
                    QuerySnapshot playersSnapshot = await playersQuery.GetSnapshotAsync();
                    List<Player> players = new List<Player>();

                    foreach (DocumentSnapshot doc in playersSnapshot.Documents)
                    {
                        if (doc.Exists)
                        {
                            Player p = doc.ConvertTo<Player>();
                            players.Add(p);
                        }
                    }

                    return View("Index", players); // Visszairányítjuk az Index nézetre, már frissített adatokkal
                }
                catch (Exception ex)
                {
                    // Hibakezelés
                    Console.WriteLine($"Error while adding player: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "An error occurred while adding the player.");
                }
            }
            return View(player); // Ha nem érvényes a modell, visszaadjuk a játékost a nézetnek
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
            return View(player); // A View betölti az edit űrlapot az adatokkal
        }

        // POST: Edit Player - Mentés gombra kattintás után módosítja az adatokat
        [HttpPost]
        public async Task<IActionResult> Edit(Player player)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    DocumentReference docRef = _firestoreDb.Collection("players").Document(player.Id);
                    await docRef.SetAsync(player, SetOptions.MergeAll);

                    Console.WriteLine("Player successfully updated in Firestore");

                    return RedirectToAction("Index"); // Visszairányítjuk a listára
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error while updating player: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the player.");
                }
            }
            return View(player);
        }



        public async Task<IActionResult> Delete(string id)
        {
            DocumentReference docRef = _firestoreDb.Collection("players").Document(id);
            await docRef.DeleteAsync();
            return RedirectToAction("Index");
        }
    }
}
