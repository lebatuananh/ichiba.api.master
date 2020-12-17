using System.Collections.Generic;
using System.Threading.Tasks;
using IChiba.Core;
using IChiba.Core.Domain.Master;

namespace IChiba.Services.Master
{
    public partial interface IBankAccountService
    {
        Task<int> InsertAsync(BankAccount entity);

        Task<int> UpdateAsync(BankAccount entity);

        Task<int> DeletesAsync(IEnumerable<string> ids);

        IList<BankAccount> GetAll(bool showHidden = false);

        IPagedList<BankAccount> Get(BankAccountSearchContext ctx);

        Task<BankAccount> GetByIdAsync(string id);

        Task<int> ActivatesAsync(IEnumerable<string> ids, bool active);
    }
}
