using System.Collections.Generic;
using System.Threading.Tasks;
using IChiba.Core;
using IChiba.Core.Domain.Master;

namespace IChiba.Services.Master
{
    public partial interface IBankService
    {
        Task<int> InsertAsync(Bank entity);

        Task<int> UpdateAsync(Bank entity);

        Task<int> DeletesAsync(IEnumerable<string> ids);

        IList<Bank> GetAll(bool showHidden = false);

        IPagedList<Bank> Get(BankSearchContext ctx);

        Task<Bank> GetByIdAsync(string id);

        Task<int> ActivatesAsync(IEnumerable<string> ids, bool active);

        Task<bool> ExistsAsync(string code);

        Task<bool> ExistsAsync(string oldCode, string newCode);
    }
}
