using System.Collections.Generic;

namespace RR_Clan_Management.Models
{
    public class WarTourStatsViewModel
    {
        // A táblázat sorai, azaz játékosonkénti statisztikák
        public List<WarTourStatsRow> Rows { get; set; } = new();

        public List<PlayerStat> PlayerStats { get; set; } = new List<PlayerStat>();
    }

    public class WarTourStatsRow
    {
        public string PlayerName { get; set; } = "";

        // Összes esemény száma, amelyen a játékos szerepelt vagy kellett volna szerepelnie
        public int TotalEvents { get; set; }

        // Részt vett események száma
        public int Participated { get; set; }

        // Részben vett részt események száma
        public int Partial { get; set; }

        // Nem vett részt események száma
        public int Missed { get; set; }

        // Felmentve volt események száma
        public int Excused { get; set; }

        // A részvételi arány százalékban (pl. 85.5%)
        public double ParticipationPercent =>
            TotalEvents > 0 ? (Participated + 0.5 * Partial) / TotalEvents * 100 : 0;

        // Jelzi, hogy a játékos már kilépett-e (nem aktív)
        public bool IsLeft { get; set; }

        // Esetleges megjegyzés (pl. ha nem volt részvétele)
        public string Note { get; set; } = "";

        // Az utolsó 3 esemény részvételi státuszai dátum szerint (pl. "Részt vett", "Nem vett részt" stb.)
        public List<string> Last3Entries { get; set; } = new();
    }
}
