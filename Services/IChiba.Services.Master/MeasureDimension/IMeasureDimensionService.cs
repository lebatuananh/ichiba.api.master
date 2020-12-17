using IChiba.Core;
using IChiba.Core.Domain.Master;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IChiba.Services.Master
{
    public interface IMeasureDimensionService
    {
        Task<int> InsertAsync(MeasureDimension entity);

        Task<int> UpdateAsync(MeasureDimension entity);

        Task<int> DeletesAsync(IEnumerable<string> ids);

        IList<MeasureDimension> GetAll(bool showHidden = false);

        IPagedList<MeasureDimension> Get(MeasureDimensionSearchContext ctx);

        Task<MeasureDimension> GetByIdAsync(string id);

        Task<int> ActivatesAsync(IEnumerable<string> ids, bool active);

        Task<bool> ExistsAsync(string code);

        Task<bool> ExistsAsync(string oldCode, string newCode);
    }
}
