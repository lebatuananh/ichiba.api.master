using IChiba.Core;
using IChiba.Core.Domain.Master;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IChiba.Services.Master
{
    public interface IShippingLineService
    {
        Task<int> InsertAsync(ShippingLine entity);

        Task<int> UpdateAsync(ShippingLine entity);

        Task<int> DeletesAsync(IEnumerable<string> ids);

        IList<ShippingLine> GetAll(bool showHidden = false);

        IList<ShippingLine> GetByShippingAgentId(string shippingAgentId, bool showHidden = false);

        IPagedList<ShippingLine> Get(ShippingLineSearchContext ctx);

        Task<ShippingLine> GetByIdAsync(string id);

        Task<int> ActivatesAsync(IEnumerable<string> ids, bool active);

        Task<bool> ExistsAsync(string code);

        Task<bool> ExistsAsync(string oldCode, string newCode);
    }
}
