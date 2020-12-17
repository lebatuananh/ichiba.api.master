using IChiba.Core;
using IChiba.Core.Domain.Master;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IChiba.Services.Master
{
    public interface ISPMeasurementService
    {
        Task<int> InsertAsync(SPMeasurement entity);

        Task<int> UpdateAsync(SPMeasurement entity);

        Task<int> DeletesAsync(IEnumerable<string> ids);

        IList<SPMeasurement> GetAll(bool showHidden = false);

        IPagedList<SPMeasurement> Get(SPMeasurementSearchContext ctx);

        Task<SPMeasurement> GetByIdAsync(string id);

        Task<int> ActivatesAsync(IEnumerable<string> ids, bool active);

        Task<bool> ExistsAsync(string code);

        Task<bool> ExistsAsync(string oldCode, string newCode);
    }
}
