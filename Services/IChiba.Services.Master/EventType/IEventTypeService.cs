using System.Collections.Generic;
using System.Threading.Tasks;
using IChiba.Core;
using IChiba.Core.Domain.Master;

namespace IChiba.Services.Master
{
    public partial interface IEventTypeService
    {
        Task<int> InsertAsync(EventType entity);

        Task<int> UpdateAsync(EventType entity);

        Task<int> DeletesAsync(IEnumerable<string> ids);

        IList<EventType> GetAll(bool showHidden = false);

        IPagedList<EventType> Get(EventTypeSearchContext ctx);

        Task<EventType> GetByIdAsync(string id);

        Task<int> ActivatesAsync(IEnumerable<string> ids, bool active);

        Task<bool> ExistsAsync(string code);

        Task<bool> ExistsAsync(string oldCode, string newCode);
    }
}
