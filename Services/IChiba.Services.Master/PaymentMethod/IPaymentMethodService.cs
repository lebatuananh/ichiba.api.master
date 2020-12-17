using IChiba.Core;
using IChiba.Core.Domain.Master;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IChiba.Services.Master
{
    public interface IPaymentMethodService
    {
        Task<int> InsertAsync(PaymentMethod entity);

        Task<int> UpdateAsync(PaymentMethod entity);

        Task<int> DeletesAsync(IEnumerable<string> ids);

        IList<PaymentMethod> GetAll(bool showHidden = false);

        IPagedList<PaymentMethod> Get(PaymentMethodSearchContext ctx);

        Task<PaymentMethod> GetByIdAsync(string id);

        Task<int> ActivatesAsync(IEnumerable<string> ids, bool active);

        Task<bool> ExistsAsync(string code);

        Task<bool> ExistsAsync(string oldCode, string newCode);
    }
}
