using IChiba.Core;
using IChiba.Core.Domain.Master;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IChiba.Services.Master
{
    public interface ICustomAgentService
    {
        Task<int> InsertAsync(CustomAgent entity);

        Task<int> UpdateAsync(CustomAgent entity);

        Task<int> DeletesAsync(IEnumerable<string> ids);

        IList<CustomAgent> GetAll(bool showHidden = false);

        IPagedList<CustomAgent> Get(CustomAgentSearchContext ctx);

        Task<CustomAgent> GetByIdAsync(string id);

        Task<int> ActivatesAsync(IEnumerable<string> ids, bool active);

        Task<bool> ExistsAsync(string code);

        Task<bool> ExistsAsync(string oldCode, string newCode);
    }
}
