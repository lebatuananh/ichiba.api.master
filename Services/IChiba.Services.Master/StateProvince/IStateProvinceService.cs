using IChiba.Core;
using IChiba.Core.Domain.Master;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IChiba.Services.Master
{
    public interface IStateProvinceService
    {
        Task<int> InsertAsync(StateProvince entity);

        Task<int> UpdateAsync(StateProvince entity);

        Task<int> DeletesAsync(IEnumerable<string> ids);

        IList<StateProvince> GetByCountryId(string countryId, bool showHidden = false);

        IPagedList<StateProvince> Get(StateProvinceSearchContext ctx);

        Task<StateProvince> GetByIdAsync(string id);

        Task<int> ActivatesAsync(IEnumerable<string> ids, bool active);

        Task<bool> ExistsAsync(string code);

        Task<bool> ExistsAsync(string oldCode, string newCode);
    }
}
