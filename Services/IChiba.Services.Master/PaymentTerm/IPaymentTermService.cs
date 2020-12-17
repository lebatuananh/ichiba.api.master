using IChiba.Core;
using IChiba.Core.Domain.Master;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IChiba.Services.Master
{
    public interface IPaymentTermService
    {
        Task<int> InsertAsync(PaymentTerm entity);

        Task<int> UpdateAsync(PaymentTerm entity);

        Task<int> DeletesAsync(IEnumerable<string> ids);

        IList<PaymentTerm> GetAll(bool showHidden = false);

        IPagedList<PaymentTerm> Get(PaymentTermSearchContext ctx);

        Task<PaymentTerm> GetByIdAsync(string id);

        Task<int> ActivatesAsync(IEnumerable<string> ids, bool active);

        Task<bool> ExistsAsync(string code);

        Task<bool> ExistsAsync(string oldCode, string newCode);
    }
}
