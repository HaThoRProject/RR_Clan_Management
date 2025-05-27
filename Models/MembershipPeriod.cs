using Google.Cloud.Firestore;

namespace RR_Clan_Management.Models
{
    [FirestoreData]
    public class MembershipPeriod
    {
        [FirestoreProperty]
        public string? JoinDate { get; set; }

        [FirestoreProperty]
        public string? LeaveDate { get; set; }
    }
}
