using IChiba.Core;
using IChiba.Core.Domain.Master;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IChiba.Services.Master
{
    public interface IPackageTypeService
    {
        Task<int> InsertAsync(PackageType entity);

        Task<int> UpdateAsync(PackageType entity);

        Task<int> DeletesAsync(IEnumerable<string> ids);

        IList<PackageType> GetAll(bool showHidden = false);

        IPagedList<PackageType> Get(PackageTypeSearchContext ctx);

        Task<PackageType> GetByIdAsync(string id);

        Task<int> ActivatesAsync(IEnumerable<string> ids, bool active);

        Task<bool> ExistsAsync(string code);

        Task<bool> ExistsAsync(string oldCode, string newCode);
    }
}
