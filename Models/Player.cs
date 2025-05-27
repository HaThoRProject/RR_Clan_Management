using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;

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

        // Régi mezők – ideiglenesen megtartva visszafelé kompatibilitás miatt
        [FirestoreProperty]
        public string? JoinDate { get; set; }

        [FirestoreProperty]
        public string? LeaveDate { get; set; }

        public string FormattedJoinDate =>
            DateTime.TryParse(JoinDate, out var dt) ? dt.ToString("yyyy-MM-dd") : "N/A";

        public string FormattedLeaveDate =>
            DateTime.TryParse(LeaveDate, out var dt) ? dt.ToString("yyyy-MM-dd") : string.Empty;

        // Új tagsági történet-alapú nyilvántartás
        [FirestoreProperty]
        public List<MembershipPeriod>? MembershipHistory { get; set; }

        [FirestoreProperty]
        public string? Notes { get; set; }

        // Klántagság időtartama – először a tagságtörténet alapján számol, ha van
        public string TimeInClan
        {
            get
            {
                if (MembershipHistory != null && MembershipHistory.Count > 0)
                {
                    double totalDays = 0;

                    foreach (var period in MembershipHistory)
                    {
                        if (DateTime.TryParse(period.JoinDate, out var join))
                        {
                            var leave = DateTime.TryParse(period.LeaveDate, out var tempLeave)
                                ? tempLeave
                                : DateTime.Now;

                            totalDays += (leave - join).TotalDays;
                        }
                    }

                    return $"{(int)totalDays} nap";
                }

                // Ha nincs tagságtörténet, fallback a régi mezőkre
                if (DateTime.TryParse(JoinDate, out var simpleJoin))
                {
                    var simpleLeave = DateTime.TryParse(LeaveDate, out var tempSimpleLeave)
                        ? tempSimpleLeave
                        : DateTime.Now;

                    var duration = simpleLeave - simpleJoin;
                    return $"{duration.Days} nap";
                }

                return "N/A";
            }
        }
    }
}
