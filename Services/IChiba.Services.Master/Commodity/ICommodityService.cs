using System.Collections.Generic;
using System.Threading.Tasks;
using IChiba.Core;
using IChiba.Core.Domain.Master;

namespace IChiba.Services.Master
{
    public partial interface ICommodityService
    {
        Task<int> InsertAsync(Commodity entity);

        Task<int> UpdateAsync(Commodity entity);

        Task<int> DeletesAsync(IEnumerable<string> ids);

        IList<Commodity> GetAll(bool showHidden = false);

        IPagedList<Commodity> Get(CommoditySearchContext ctx);

        Task<Commodity> GetByIdAsync(string id);

        Task<int> ActivatesAsync(IEnumerable<string> ids, bool active);

        Task<bool> ExistsAsync(string code);

        Task<bool> ExistsAsync(string oldCode, string newCode);
    }
}
