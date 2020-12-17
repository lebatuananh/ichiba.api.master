using IChiba.Core;
using IChiba.Core.Domain.Master;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IChiba.Services.Master
{
    public interface IVesselService
    {
        Task<int> InsertAsync(Vessel entity);

        Task<int> UpdateAsync(Vessel entity);

        Task<int> DeletesAsync(IEnumerable<string> ids);

        IList<Vessel> GetAll(bool showHidden = false);

        IList<Vessel> GetByCountryId(string countryId, bool showHidden = false);

        IPagedList<Vessel> Get(VesselSearchContext ctx);

        Task<Vessel> GetByIdAsync(string id);

        Task<int> ActivatesAsync(IEnumerable<string> ids, bool active);

        //Task<bool> ExistsAsync(string code);

        //Task<bool> ExistsAsync(string oldCode, string newCode);
    }
}
