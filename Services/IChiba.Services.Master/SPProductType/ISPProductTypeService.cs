using IChiba.Core;
using IChiba.Core.Domain.Master;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IChiba.Services.Master
{
    public interface ISPProductTypeService
    {
        Task<int> InsertAsync(SPProductType entity);

        Task<int> UpdateAsync(SPProductType entity);

        Task<int> DeletesAsync(IEnumerable<string> ids);

        IList<SPProductType> GetAll(bool showHidden = false);

        IPagedList<SPProductType> Get(SPProductTypeSearchContext ctx);

        Task<SPProductType> GetByIdAsync(string id);

        Task<int> ActivatesAsync(IEnumerable<string> ids, bool active);

        Task<bool> ExistsAsync(string code);

        Task<bool> ExistsAsync(string oldCode, string newCode);
    }
}
