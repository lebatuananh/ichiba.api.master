using System.Collections.Generic;
using System.Threading.Tasks;
using IChiba.Core;
using IChiba.Core.Domain.Master;

namespace IChiba.Services.Master
{
    public partial interface IPortService
    {
        Task<int> InsertAsync(Port entity);

        Task<int> UpdateAsync(Port entity);

        Task<int> DeletesAsync(IEnumerable<string> ids);

        IList<Port> GetAll(bool showHidden = false);

        IPagedList<Port> Get(PortSearchContext ctx);

        Task<Port> GetByIdAsync(string id);

        Task<int> ActivatesAsync(IEnumerable<string> ids, bool active);

        Task<bool> ExistsAsync(string code);

        Task<bool> ExistsAsync(string oldCode, string newCode);
    }
}
