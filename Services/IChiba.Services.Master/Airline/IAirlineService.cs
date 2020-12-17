using System.Collections.Generic;
using System.Threading.Tasks;
using IChiba.Core;
using IChiba.Core.Domain.Master;

namespace IChiba.Services.Master
{
    public partial interface IAirlineService
    {
        Task<int> InsertAsync(Airline entity);

        Task<int> UpdateAsync(Airline entity);

        Task<int> DeletesAsync(IEnumerable<string> ids);

        IList<Airline> GetAll(bool showHidden = false);

        IPagedList<Airline> Get(AirlineSearchContext ctx);

        Task<Airline> GetByIdAsync(string id);

        Task<int> ActivatesAsync(IEnumerable<string> ids, bool active);

        Task<bool> ExistsAsync(string code);

        Task<bool> ExistsAsync(string oldCode, string newCode);
    }
}
