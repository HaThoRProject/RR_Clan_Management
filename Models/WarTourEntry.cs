using Google.Cloud.Firestore;
using System.Collections.Generic;

namespace RR_Clan_Management.Models
{
    [FirestoreData]
    public class WarTourEntry
    {
        [FirestoreProperty]
        public string PlayerName { get; set; } = "";

        [FirestoreProperty]
        public Dictionary<string, string> Columns { get; set; } = new Dictionary<string, string>(); // Oszlopok és értékek
    }
}


