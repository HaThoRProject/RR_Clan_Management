using System.Collections.Generic;

namespace RR_Clan_Management.ViewModels
{
    public class WarTourViewModel
    {
        public List<WarTourRow> Rows { get; set; } = new List<WarTourRow>();
        public List<string> ColumnHeaders { get; set; } = new List<string>(); // Oszlopok nevei
    }

    public class WarTourRow
    {
        public string PlayerName { get; set; } = string.Empty;
        public Dictionary<string, string> Columns { get; set; } = new Dictionary<string, string>();
    }
}
