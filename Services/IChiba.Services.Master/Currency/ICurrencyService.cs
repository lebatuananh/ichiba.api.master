using System.Collections.Generic;
using System.Threading.Tasks;
using IChiba.Core;
using IChiba.Core.Domain.Master;

namespace IChiba.Services.Master
{
    public partial interface ICurrencyService
    {
        Task<int> InsertAsync(Currency entity);

        Task<int> UpdateAsync(Currency entity);

        Task<int> DeletesAsync(IEnumerable<string> ids);

        IList<Currency> GetAll();

        IPagedList<Currency> Get(CurrencySearchContext ctx);

        Task<Currency> GetByIdAsync(string id);

        //Task<int> ActivatesAsync(IEnumerable<string> ids, bool active);

        Task<bool> ExistsAsync(string code);

        Task<bool> ExistsAsync(string oldCode, string newCode);
    }
}
