using IChiba.Core;
using IChiba.Core.Domain.Master;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IChiba.Services.Master
{
    public interface IMeasureWeightService
    {
        Task<int> InsertAsync(MeasureWeight entity);

        Task<int> UpdateAsync(MeasureWeight entity);

        Task<int> DeletesAsync(IEnumerable<string> ids);

        IList<MeasureWeight> GetAll(bool showHidden = false);

        IPagedList<MeasureWeight> Get(MeasureWeightSearchContext ctx);

        Task<MeasureWeight> GetByIdAsync(string id);

        Task<int> ActivatesAsync(IEnumerable<string> ids, bool active);

        Task<bool> ExistsAsync(string code);

        Task<bool> ExistsAsync(string oldCode, string newCode);
    }
}
