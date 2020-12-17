using System.Collections.Generic;
using System.Threading.Tasks;
using IChiba.Core;
using IChiba.Core.Domain.Master;

namespace IChiba.Services.Master
{
    public partial interface IChargesTypeService
    {
        Task<int> InsertAsync(ChargesType entity);

        Task<int> UpdateAsync(ChargesType entity);

        Task<int> DeletesAsync(IEnumerable<string> ids);

        IList<ChargesType> GetAll(bool showHidden = false);

        IPagedList<ChargesType> Get(ChargesTypeSearchContext ctx);

        Task<ChargesType> GetByIdAsync(string id);

        Task<int> ActivatesAsync(IEnumerable<string> ids, bool active);

        Task<bool> ExistsAsync(string code);

        Task<bool> ExistsAsync(string oldCode, string newCode);
    }
}
