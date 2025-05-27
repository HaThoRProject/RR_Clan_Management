using System.Collections.Generic;

namespace RR_Clan_Management.Models
{
    public class WarTourStatsViewModel
    {
        public List<WarTourStatsRow> Rows { get; set; } = new();
    }

    public class WarTourStatsRow
    {
        public string PlayerName { get; set; } = "";
        public int TotalEvents { get; set; }
        public int Participated { get; set; }
        public int Partial { get; set; }
        public int Missed { get; set; }
        public int Excused { get; set; }
        public bool IsLeft { get; set; }
        public string Note { get; set; } = "";
        public List<string> Last3Entries { get; set; } = new(); // új property
        public double ParticipationPercent =>
            TotalEvents > 0 ? (Participated + 0.5 * Partial) / TotalEvents * 100 : 0;
    }
}
