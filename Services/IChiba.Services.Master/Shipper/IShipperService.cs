using IChiba.Core;
using IChiba.Core.Domain.Master;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IChiba.Services.Master
{
    public interface IShipperService
    {
        Task<int> InsertAsync(Shipper entity);

        Task<int> UpdateAsync(Shipper entity);

        Task<int> DeletesAsync(IEnumerable<string> ids);

        IList<Shipper> GetAll(bool showHidden = false);

        IPagedList<Shipper> Get(ShipperSearchContext ctx);

        Task<Shipper> GetByIdAsync(string id);

        Task<int> ActivatesAsync(IEnumerable<string> ids, bool active);

        Task<bool> ExistsAsync(string code);

        Task<bool> ExistsAsync(string oldCode, string newCode);

        IList<Shipper> GetByPaymentTermId(string paymentTermId, bool showHidden = false);

        Task<IList<Shipper>> GetTop_SelectAsync(ShipperSearchContext ctx);
    }
}
