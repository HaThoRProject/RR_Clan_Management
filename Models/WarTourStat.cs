using Google.Cloud.Firestore;

namespace RR_Clan_Management.Models
{
    [FirestoreData]
    public class WarTourStat
    {
        [FirestoreProperty]
        public string? PlayerId { get; set; }

        [FirestoreProperty]
        public string? Participation { get; set; } // pl.: "Részt vett", "Nem vett részt", stb.

        // Egyéb mezők, ha szükséges
    }
}
