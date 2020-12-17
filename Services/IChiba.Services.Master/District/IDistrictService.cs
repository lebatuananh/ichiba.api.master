using IChiba.Core;
using IChiba.Core.Domain.Master;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IChiba.Services.Master
{
    public interface IDistrictService
    {
        Task<int> InsertAsync(District entity);

        Task<int> UpdateAsync(District entity);

        Task<int> DeletesAsync(IEnumerable<string> ids);

        IList<District> GetByStateProvinceId(string stateProvinceId, bool showHidden = false);

        IPagedList<District> Get(DistrictSearchContext ctx);

        Task<District> GetByIdAsync(string id);

        Task<int> ActivatesAsync(IEnumerable<string> ids, bool active);

        Task<bool> ExistsAsync(string code);

        Task<bool> ExistsAsync(string oldCode, string newCode);
    }
}
