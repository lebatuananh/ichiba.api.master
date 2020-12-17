using IChiba.Core;
using IChiba.Core.Domain.Master;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IChiba.Services.Master
{
    public interface ISPCustomerService
    {
        Task<int> InsertAsync(SPCustomer entity);

        Task<int> UpdateAsync(SPCustomer entity);

        Task<int> DeletesAsync(IEnumerable<string> ids);

        IList<SPCustomer> GetAll(bool showHidden = false);

        IPagedList<SPCustomer> Get(SPCustomerSearchContext ctx);

        Task<SPCustomer> GetByIdAsync(string id);

        Task<int> ActivatesAsync(IEnumerable<string> ids, bool active);

        Task<bool> ExistsAsync(string code);

        Task<bool> ExistsAsync(string oldCode, string newCode);

        IList<SPCustomer> GetByPaymentTermId(string paymentTermId, bool showHidden = false);

        Task<IList<SPCustomer>> GetTop_SelectAsync(SPCustomerSearchContext ctx);
    }
}
