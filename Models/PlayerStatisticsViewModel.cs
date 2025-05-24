namespace RR_Clan_Management.Models
{
    public class PlayerStatisticsViewModel
    {
        public string? PlayerName { get; set; }
        public DateTime? JoinDate { get; set; }
        public DateTime? LeaveDate { get; set; }
        public string? FormattedJoinDate { get; set; }
        public string? FormattedLeaveDate { get; set; }
        public string EltoltottIdoSzovegesen { get; set; } = string.Empty;
    }
}
