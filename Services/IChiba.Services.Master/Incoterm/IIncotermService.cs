using System.Collections.Generic;
using System.Threading.Tasks;
using IChiba.Core;
using IChiba.Core.Domain.Master;

namespace IChiba.Services.Master
{
    public partial interface IIncotermService
    {
        Task<int> InsertAsync(Incoterm entity);

        Task<int> UpdateAsync(Incoterm entity);

        Task<int> DeletesAsync(IEnumerable<string> ids);

        IList<Incoterm> GetAll(bool showHidden = false);

        IPagedList<Incoterm> Get(IncotermSearchContext ctx);

        Task<Incoterm> GetByIdAsync(string id);

        Task<int> ActivatesAsync(IEnumerable<string> ids, bool active);

        Task<bool> ExistsAsync(string code);

        Task<bool> ExistsAsync(string oldCode, string newCode);
    }
}
