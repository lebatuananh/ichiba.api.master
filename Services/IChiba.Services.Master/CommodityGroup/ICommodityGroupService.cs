using IChiba.Core;
using IChiba.Core.Domain.Master;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IChiba.Services.Master
{
    public interface ICommodityGroupService
    {
        Task<int> InsertAsync(CommodityGroup entity);

        Task<int> UpdateAsync(CommodityGroup entity);

        Task<int> DeletesAsync(IEnumerable<string> ids);

        IList<CommodityGroup> GetAll(bool showHidden = false);

        IPagedList<CommodityGroup> Get(CommodityGroupSearchContext ctx);

        Task<CommodityGroup> GetByIdAsync(string id);

        Task<int> ActivatesAsync(IEnumerable<string> ids, bool active);

        Task<bool> ExistsAsync(string code);

        Task<bool> ExistsAsync(string oldCode, string newCode);
    }
}
