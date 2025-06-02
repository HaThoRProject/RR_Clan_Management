using RR_Clan_Management.Models;

namespace RR_Clan_Management.Models
{
    public class PlayerDetailsViewModel
    {
        public Player Player { get; set; }
        public PlayerStat StatRow { get; set; }
        public WarTourStatsRow StatRowLast3 { get; set; }
    }
}
