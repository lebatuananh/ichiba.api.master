using IChiba.Core;
using IChiba.Core.Domain.Master;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IChiba.Services.Master
{
    public interface IDeliveryTimeService
    {
        Task<int> InsertAsync(DeliveryTime entity);

        Task<int> UpdateAsync(DeliveryTime entity);

        Task<int> DeletesAsync(IEnumerable<string> ids);

        IList<DeliveryTime> GetAll(bool showHidden = false);

        IPagedList<DeliveryTime> Get(DeliveryTimeSearchContext ctx);

        Task<DeliveryTime> GetByIdAsync(string id);

        Task<int> ActivatesAsync(IEnumerable<string> ids, bool active);

        Task<bool> ExistsAsync(string code);

        Task<bool> ExistsAsync(string oldCode, string newCode);
    }
}
