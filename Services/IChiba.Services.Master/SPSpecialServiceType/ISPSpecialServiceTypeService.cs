using IChiba.Core;
using IChiba.Core.Domain.Master;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IChiba.Services.Master
{
    public partial interface ISPSpecialServiceTypeService
    {
        Task<int> InsertAsync(SPSpecialServiceType entity);

        Task<int> UpdateAsync(SPSpecialServiceType entity);

        Task<int> DeletesAsync(IEnumerable<string> ids);

        IList<SPSpecialServiceType> GetAll(bool showHidden = false);

        IPagedList<SPSpecialServiceType> Get(SPSpecialServiceTypeSearchContext ctx);

        Task<SPSpecialServiceType> GetByIdAsync(string id);

        Task<int> ActivatesAsync(IEnumerable<string> ids, bool active);

        Task<bool> ExistsAsync(string code);

        Task<bool> ExistsAsync(string oldCode, string newCode);
    }
}
