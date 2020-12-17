using System.Collections.Generic;
using System.Threading.Tasks;
using IChiba.Core;
using IChiba.Core.Domain.Master;

namespace IChiba.Services.Master
{
    public partial interface ICountryService
    {
        Task<int> InsertAsync(Country entity);

        Task<int> UpdateAsync(Country entity);

        Task<int> DeletesAsync(IEnumerable<string> ids);

        IList<Country> GetAll(bool showHidden = false);

        IPagedList<Country> Get(CountrySearchContext ctx);

        Task<Country> GetByIdAsync(string id);

        Task<int> ActivatesAsync(IEnumerable<string> ids, bool active);

        Task<bool> ExistsAsync(string code);

        Task<bool> ExistsAsync(string oldCode, string newCode);
    }
}
