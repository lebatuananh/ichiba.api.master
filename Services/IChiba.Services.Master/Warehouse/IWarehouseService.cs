using IChiba.Core;
using IChiba.Core.Domain.Master;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IChiba.Services.Master
{
    public interface IWarehouseService
    {
        Task<int> InsertAsync(Warehouse entity);

        Task<int> UpdateAsync(Warehouse entity);

        Task<int> DeletesAsync(IEnumerable<string> ids);

        IList<Warehouse> GetAll(bool showHidden = false);

        IPagedList<Warehouse> Get(WarehouseSearchContext ctx);

        Task<Warehouse> GetByIdAsync(string id);

        Task<int> ActivatesAsync(IEnumerable<string> ids, bool active);

        Task<bool> ExistsAsync(string code);

        Task<bool> ExistsAsync(string oldCode, string newCode);
    }
}
