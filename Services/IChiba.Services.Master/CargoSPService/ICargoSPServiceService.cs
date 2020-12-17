using System.Collections.Generic;
using System.Threading.Tasks;
using IChiba.Core;
using IChiba.Core.Domain.Master;

namespace IChiba.Services.Master
{
    public interface ICargoSPServiceService
    {
        Task<int> InsertAsync(CargoSPService entity);

        Task<int> UpdateAsync(CargoSPService entity);

        Task<int> DeletesAsync(IEnumerable<string> ids);

        IList<CargoSPService> GetAll(bool showHidden = false);

        IPagedList<CargoSPService> Get(CargoSPServiceSearchContext ctx);

        Task<CargoSPService> GetByIdAsync(string id);

        Task<int> ActivatesAsync(IEnumerable<string> ids, bool active);

        Task<bool> ExistsAsync(string code);

        Task<bool> ExistsAsync(string oldCode, string newCode);
    }
}
