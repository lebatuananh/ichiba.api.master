using IChiba.Core;
using IChiba.Core.Domain.Master;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IChiba.Services.Master
{
    public interface IVatTypeService
    {
        Task<int> InsertAsync(VatType entity);

        Task<int> UpdateAsync(VatType entity);

        Task<int> DeletesAsync(IEnumerable<string> ids);

        IList<VatType> GetAll(bool showHidden = false);

        IPagedList<VatType> Get(VatTypeSearchContext ctx);

        Task<VatType> GetByIdAsync(string id);

        Task<int> ActivatesAsync(IEnumerable<string> ids, bool active);

        Task<bool> ExistsAsync(string code);

        Task<bool> ExistsAsync(string oldCode, string newCode);
    }
}
