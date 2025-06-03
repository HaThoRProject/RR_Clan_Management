using Google.Cloud.Firestore;
using RR_Clan_Management.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace RR_Clan_Management.Services
{
    public class StatService : IStatService
    {
        private readonly FirestoreDb _firestoreDb;

        public StatService(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb ?? throw new ArgumentNullException(nameof(firestoreDb));
        }

        private string NormalizeName(string? name)
        {
            return string.IsNullOrWhiteSpace(name) ? string.Empty : name.Trim().ToLowerInvariant();
        }

        public async Task<WarTourStatsViewModel> GetWarTourStatsAsync(bool onlyLast3 = false)
        {
           // Debug.WriteLine("⚠️ GetWarTourStatsAsync elindult!");

            var viewModel = new WarTourStatsViewModel();

            // 1. WarTour adatok lekérése
            var warTourSnapshot = await _firestoreDb.Collection("WarTour").GetSnapshotAsync();

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
             //   Console.WriteLine("Nincsenek WarTour objektumok!");
                return viewModel;
            }

            // 2. Összes esemény meghatározása
            var allEvents = warTours
                .SelectMany(wt => wt.Columns.Keys)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            var filterEvents = onlyLast3 && allEvents.Count > 3
                ? allEvents.Skip(allEvents.Count - 3).ToList()
                : allEvents;

            // 3. Statisztikák összeállítása
            var playerStats = new List<PlayerStat>();

            foreach (var player in warTours)
            {
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
                        }
                    }
                }

                int totalEvents = participated + partially + notParticipated + excused;

                playerStats.Add(new PlayerStat
                {
                    PlayerName = player.PlayerName ?? "N/A",
                    ParticipationCount = participated,
                    PartialParticipationCount = partially,
                    NotParticipatedCount = notParticipated,
                    ExcusedCount = excused,
                    TotalEvents = totalEvents
                });
            }

            viewModel.Rows = playerStats
                .Select(ps => new WarTourStatsRow
                {
                    PlayerName = ps.PlayerName,
                    Participated = ps.ParticipationCount,
                    Partial = ps.PartialParticipationCount,
                    Missed = ps.NotParticipatedCount,
                    Excused = ps.ExcusedCount,
                    TotalEvents = ps.TotalEvents,
                    IsLeft = false
                })
                .ToList();

            viewModel.PlayerStats = playerStats;

            return viewModel;
        }
    }
}
