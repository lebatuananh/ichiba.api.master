using IChiba.Core;
using IChiba.Core.Domain.Master;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IChiba.Services.Master
{
    public interface IShippingAgentService
    {
        Task<int> InsertAsync(ShippingAgent entity);

        Task<int> UpdateAsync(ShippingAgent entity);

        Task<int> DeletesAsync(IEnumerable<string> ids);

        IList<ShippingAgent> GetAll(bool showHidden = false);

        IPagedList<ShippingAgent> Get(ShippingAgentSearchContext ctx);

        Task<ShippingAgent> GetByIdAsync(string id);

        Task<int> ActivatesAsync(IEnumerable<string> ids, bool active);

        Task<bool> ExistsAsync(string code);

        Task<bool> ExistsAsync(string oldCode, string newCode);
    }
}
