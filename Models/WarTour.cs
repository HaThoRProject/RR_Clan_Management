using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;

namespace RR_Clan_Management.Models
{
    [FirestoreData]
    public class WarTour
    {
        [FirestoreProperty]
        public string? Id { get; set; }

        [FirestoreProperty]
        public string? PlayerName { get; set; } // Játékos neve

        [FirestoreProperty]
        public Dictionary<string, string>? Columns { get; set; } = new Dictionary<string, string>();
        // Oszlop neve → Kiválasztott érték (pl. "Tour 1" -> "Részt vett")
    }
}
