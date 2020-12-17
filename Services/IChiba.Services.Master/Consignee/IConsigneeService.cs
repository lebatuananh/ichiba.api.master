using IChiba.Core;
using IChiba.Core.Domain.Master;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IChiba.Services.Master
{
    public interface IConsigneeService
    {
        Task<int> InsertAsync(Consignee entity);

        Task<int> UpdateAsync(Consignee entity);

        Task<int> DeletesAsync(IEnumerable<string> ids);

        IList<Consignee> GetAll(bool showHidden = false);

        IPagedList<Consignee> Get(ConsigneeSearchContext ctx);

        Task<Consignee> GetByIdAsync(string id);

        Task<int> ActivatesAsync(IEnumerable<string> ids, bool active);

        Task<bool> ExistsAsync(string code);

        Task<bool> ExistsAsync(string oldCode, string newCode);

        IList<Consignee> GetByPaymentTermId(string paymentTermId, bool showHidden = false);

        Task<IList<Consignee>> GetTop_SelectAsync(ConsigneeSearchContext ctx);
    }
}
