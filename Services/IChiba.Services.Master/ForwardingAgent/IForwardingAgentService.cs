using IChiba.Core;
using IChiba.Core.Domain.Master;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IChiba.Services.Master
{
    public interface IForwardingAgentService
    {
        Task<int> InsertAsync(ForwardingAgent entity);

        Task<int> UpdateAsync(ForwardingAgent entity);

        Task<int> DeletesAsync(IEnumerable<string> ids);

        IList<ForwardingAgent> GetAll(bool showHidden = false);

        IPagedList<ForwardingAgent> Get(ForwardingAgentSearchContext ctx);

        Task<ForwardingAgent> GetByIdAsync(string id);

        Task<int> ActivatesAsync(IEnumerable<string> ids, bool active);

        Task<bool> ExistsAsync(string code);

        Task<bool> ExistsAsync(string oldCode, string newCode);
    }
}
