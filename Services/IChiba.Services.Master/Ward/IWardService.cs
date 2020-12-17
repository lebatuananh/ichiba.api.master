using IChiba.Core;
using IChiba.Core.Domain.Master;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IChiba.Services.Master
{
    public interface IWardService
    {
        Task<int> InsertAsync(Ward entity);

        Task<int> UpdateAsync(Ward entity);

        Task<int> DeletesAsync(IEnumerable<string> ids);

        IList<Ward> GetByDistrictId(string districtId, bool showHidden = false);

        IPagedList<Ward> Get(WardSearchContext ctx);

        Task<Ward> GetByIdAsync(string id);

        Task<int> ActivatesAsync(IEnumerable<string> ids, bool active);

        Task<bool> ExistsAsync(string code);

        Task<bool> ExistsAsync(string oldCode, string newCode);
    }
}
