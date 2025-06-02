using Google.Cloud.Firestore;
using RR_Clan_Management.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RR_Clan_Management.Services
{
    public class StatService : IStatService
    {
        private readonly FirestoreDb _firestoreDb;

        public StatService(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public async Task<WarTourStatsViewModel> GetWarTourStatsAsync(bool onlyLast3 = false)
        {
            var viewModel = new WarTourStatsViewModel();

            // Lekérjük az összes WarTour dokumentumot
            var warTourSnapshot = await _firestoreDb.Collection("WarTour").GetSnapshotAsync();
            Console.WriteLine($"Dokumentumok száma: {warTourSnapshot.Documents.Count}");

            var warTours = new List<WarTour>();

            foreach (var doc in warTourSnapshot.Documents)
            {
                var dict = doc.ToDictionary();

                var warTour = new WarTour
                {
                    Id = doc.Id,
                    PlayerName = dict.TryGetValue("PlayerName", out var pn) ? pn as string : null,
                    Columns = new Dictionary<string, string>()
                };

                // A Columns mező helyes konvertálása Dictionary<string, object> -> Dictionary<string, string>
                if (dict.TryGetValue("Columns", out var columnsObj) && columnsObj is Dictionary<string, object> rawColumns)
                {
                    foreach (var kvp in rawColumns)
                    {
                        warTour.Columns[kvp.Key] = kvp.Value?.ToString() ?? "-";
                    }
                }

                warTours.Add(warTour);
            }

            if (warTours.Count == 0)
            {
                Console.WriteLine("Nincsenek WarTour objektumok!");
                return viewModel;
            }

            // Esemény nevek kigyűjtése
            var allEvents = warTours
                .SelectMany(wt => wt.Columns.Keys)
                .Distinct()
                .ToList();

            allEvents.Sort(); // Lexikálisan (pl. dátum szerint)

            // Csak utolsó 3, ha szükséges
            List<string> filterEvents = allEvents;
            if (onlyLast3 && allEvents.Count > 3)
            {
                filterEvents = allEvents.Skip(allEvents.Count - 3).ToList();
            }

            var playerStats = new List<PlayerStat>();

            foreach (var player in warTours)
            {
                int totalEvents = filterEvents.Count;
                int participated = 0;
                int partially = 0;
                int notParticipated = 0;
                int excused = 0;

                foreach (var ev in filterEvents)
                {
                    if (player.Columns != null && player.Columns.TryGetValue(ev, out var status))
                    {
                        switch (status)
                        {
                            case "Részt vett":
                                participated++;
                                break;
                            case "Részben vett részt":
                                partially++;
                                break;
                            case "Nem vett részt":
                                notParticipated++;
                                break;
                            case "Felmentve":
                                excused++;
                                break;
                            default:
                                notParticipated++;
                                break;
                        }
                    }
                    else
                    {
                        notParticipated++;
                    }
                }

                playerStats.Add(new PlayerStat
                {
                    PlayerId = player.Id ?? player.PlayerName ?? "N/A",
                    PlayerName = player.PlayerName ?? "N/A",
                    ParticipationCount = participated,
                    PartialParticipationCount = partially,
                    NotParticipatedCount = notParticipated,
                    ExcusedCount = excused,
                    TotalEvents = totalEvents
                });
            }

            // Itt töltsük fel a Rows listát is a WarTourStatsRow objektumokkal
            viewModel.Rows = playerStats
                .Select(ps => new WarTourStatsRow
                {
                    PlayerName = ps.PlayerName,
                    Participated = ps.ParticipationCount,
                    Partial = ps.PartialParticipationCount,
                    Missed = ps.NotParticipatedCount,
                    Excused = ps.ExcusedCount,
                    TotalEvents = ps.TotalEvents,
                    IsLeft = false // Ha van infód a kilépésről, ide jöhet
                })
                .ToList();

            viewModel.PlayerStats = playerStats;

            return viewModel;
        }

    }
}
