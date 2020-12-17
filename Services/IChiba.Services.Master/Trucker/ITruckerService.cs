using System.Collections.Generic;
using System.Threading.Tasks;
using IChiba.Core;
using IChiba.Core.Domain.Master;

namespace IChiba.Services.Master
{
    public partial interface ITruckerService
    {
        Task<int> InsertAsync(Trucker entity);

        Task<int> UpdateAsync(Trucker entity);

        Task<int> DeletesAsync(IEnumerable<string> ids);

        IList<Trucker> GetAll(bool showHidden = false);

        IPagedList<Trucker> Get(TruckerSearchContext ctx);

        Task<Trucker> GetByIdAsync(string id);

        Task<int> ActivatesAsync(IEnumerable<string> ids, bool active);

        Task<bool> ExistsAsync(string code);

        Task<bool> ExistsAsync(string oldCode, string newCode);
    }
}
