using System.Threading.Tasks;
using RR_Clan_Management.Models;

namespace RR_Clan_Management.Services
{
    public interface IStatService
    {
        Task<WarTourStatsViewModel> GetWarTourStatsAsync(bool onlyLast3 = false);
    }
}
