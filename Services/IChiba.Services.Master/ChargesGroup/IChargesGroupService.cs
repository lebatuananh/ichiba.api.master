using System.Collections.Generic;
using System.Threading.Tasks;
using IChiba.Core;
using IChiba.Core.Domain.Master;

namespace IChiba.Services.Master
{
    public partial interface IChargesGroupService
    {
        Task<int> InsertAsync(ChargesGroup entity);

        Task<int> UpdateAsync(ChargesGroup entity);

        Task<int> DeletesAsync(IEnumerable<string> ids);

        IList<ChargesGroup> GetAll(bool showHidden = false);

        IPagedList<ChargesGroup> Get(ChargesGroupSearchContext ctx);

        Task<ChargesGroup> GetByIdAsync(string id);

        Task<int> ActivatesAsync(IEnumerable<string> ids, bool active);

        Task<bool> ExistsAsync(string code);

        Task<bool> ExistsAsync(string oldCode, string newCode);
    }
}
