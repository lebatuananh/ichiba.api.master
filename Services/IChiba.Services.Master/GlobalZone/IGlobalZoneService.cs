using IChiba.Core;
using IChiba.Core.Domain.Master;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IChiba.Services.Master
{
    public partial interface IGlobalZoneService
    {
        Task<int> InsertAsync(GlobalZone entity);

        Task<int> UpdateAsync(GlobalZone entity);

        Task<int> DeletesAsync(IEnumerable<string> ids);

        IList<GlobalZone> GetAll(bool showHidden = false);

        IPagedList<GlobalZone> Get(GlobalZoneSearchContext ctx);

        Task<GlobalZone> GetByIdAsync(string id);

        Task<int> ActivatesAsync(IEnumerable<string> ids, bool active);

        Task<bool> ExistsAsync(string code);

        Task<bool> ExistsAsync(string oldCode, string newCode);
    }
}
