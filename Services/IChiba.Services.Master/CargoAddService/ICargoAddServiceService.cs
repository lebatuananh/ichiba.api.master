using System.Collections.Generic;
using System.Threading.Tasks;
using IChiba.Core;
using IChiba.Core.Domain.Master;

namespace IChiba.Services.Master
{
    public interface ICargoAddServiceService
    {
        Task<int> InsertAsync(CargoAddService entity);

        Task<int> UpdateAsync(CargoAddService entity);

        Task<int> DeletesAsync(IEnumerable<string> ids);

        IList<CargoAddService> GetAll(bool showHidden = false);

        IPagedList<CargoAddService> Get(CargoAddServiceSearchContext ctx);

        Task<CargoAddService> GetByIdAsync(string id);

        Task<int> ActivatesAsync(IEnumerable<string> ids, bool active);

        Task<bool> ExistsAsync(string code);

        Task<bool> ExistsAsync(string oldCode, string newCode);
    }
}
