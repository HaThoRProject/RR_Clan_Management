namespace RR_Clan_Management.Models
{
    public class PlayerStat
    {
        public string PlayerId { get; set; }
        public string PlayerName { get; set; }

        public int ParticipationCount { get; set; }
        public int PartialParticipationCount { get; set; }
        public int NotParticipatedCount { get; set; }
        public int ExcusedCount { get; set; }
        public int TotalEvents { get; set; }
        public double ParticipationPercent
        {
            get
            {
                if (TotalEvents == 0) return 0;
                return (double)ParticipationCount / TotalEvents * 100;
            }
        }
    }
}
