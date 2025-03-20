using Google.Cloud.Firestore;
using System;

namespace RR_Clan_Management.Models
{
    [FirestoreData]
    public class Player
    {
        [FirestoreProperty]
        public int PlayerIndex { get; set; }
        [FirestoreProperty]
        public string? Id { get; set; }

        [FirestoreProperty]
        public string? Name { get; set; }

        [FirestoreProperty]
        public string? PlayerName { get; set; }

        [FirestoreProperty]
        public string? JoinDate { get; set; }
        public string FormattedJoinDate
        => DateTime.TryParse(JoinDate, out var dt) ? dt.ToString("yyyy-MM-dd") : "N/A";

        [FirestoreProperty]
        public string? LeaveDate { get; set; }
        public string FormattedLeaveDate
        => DateTime.TryParse(LeaveDate, out var dt) ? dt.ToString("yyyy-MM-dd") : string.Empty;

        [FirestoreProperty]
        public string? Notes { get; set; }
    }
}
