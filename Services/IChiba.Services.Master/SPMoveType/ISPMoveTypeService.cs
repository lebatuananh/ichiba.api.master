using IChiba.Core;
using IChiba.Core.Domain.Master;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IChiba.Services.Master
{
    public interface ISPMoveTypeService
    {
        Task<int> InsertAsync(SPMoveType entity);

        Task<int> UpdateAsync(SPMoveType entity);

        Task<int> DeletesAsync(IEnumerable<string> ids);

        IList<SPMoveType> GetAll(bool showHidden = false);

        IPagedList<SPMoveType> Get(SPMoveTypeSearchContext ctx);

        Task<SPMoveType> GetByIdAsync(string id);

        Task<int> ActivatesAsync(IEnumerable<string> ids, bool active);

        Task<bool> ExistsAsync(string code);

        Task<bool> ExistsAsync(string oldCode, string newCode);
    }
}
