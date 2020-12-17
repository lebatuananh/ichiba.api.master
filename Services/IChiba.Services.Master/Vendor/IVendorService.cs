using System.Collections.Generic;
using System.Threading.Tasks;
using IChiba.Core;
using IChiba.Core.Domain.Master;

namespace IChiba.Services.Master
{
    public partial interface IVendorService
    {
        Task<int> InsertAsync(Vendor entity);

        Task<int> UpdateAsync(Vendor entity);

        Task<int> DeletesAsync(IEnumerable<string> ids);

        IList<Vendor> GetAll(bool showHidden = false);

        IPagedList<Vendor> Get(VendorSearchContext ctx);

        Task<Vendor> GetByIdAsync(string id);

        Task<int> ActivatesAsync(IEnumerable<string> ids, bool active);

        Task<bool> ExistsAsync(string code);

        Task<bool> ExistsAsync(string oldCode, string newCode);
    }
}
