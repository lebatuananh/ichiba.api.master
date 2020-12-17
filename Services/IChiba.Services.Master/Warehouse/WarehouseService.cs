using IChiba.Caching;
using IChiba.Core;
using IChiba.Core.Domain;
using IChiba.Core.Domain.Master;
using IChiba.Core.Infrastructure;
using IChiba.Data;
using LinqToDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace IChiba.Services.Master
{
    public class WarehouseService : IWarehouseService
    {
        #region Constants



        #endregion

        #region Fields

        private readonly IRepository<Warehouse> _warehouseRepository;
        private readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
        private readonly IIChibaCacheManager _cacheManager;

        #endregion

        #region Ctor

        public WarehouseService(
            IIChibaCacheManager cacheManager)
        {
            _warehouseRepository = EngineContext.Current.Resolve<IRepository<Warehouse>>(DataConnectionHelper.ConnectionStringNames.Master);
            _localizedPropertyRepository = EngineContext.Current.Resolve<IRepository<LocalizedProperty>>(DataConnectionHelper.ConnectionStringNames.Master);
            _cacheManager = cacheManager;
        }

        #endregion

        #region Methods

        public virtual async Task<int> InsertAsync(Warehouse entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var result = await _warehouseRepository.InsertAsync(entity);

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.Warehouses.PrefixCacheKey);
            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.PostOffices.PrefixCacheKey);

            return result;
        }

        public virtual async Task<int> UpdateAsync(Warehouse entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var result = await _warehouseRepository.UpdateAsync(entity);

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.Warehouses.PrefixCacheKey);
            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.PostOffices.PrefixCacheKey);

            return result;
        }

        public virtual async Task<int> DeletesAsync(IEnumerable<string> ids)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));

            var result = await _warehouseRepository.DeleteAsync(ids);

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.Warehouses.PrefixCacheKey);
            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.PostOffices.PrefixCacheKey);

            return result;
        }

        public virtual IList<Warehouse> GetAll(bool showHidden = false)
        {
            var key = MasterCacheKeys.Warehouses.AllCacheKey.FormatWith(showHidden);
            var entities = _cacheManager.GetToDb(key, () =>
            {
                var query = from p in _warehouseRepository.Table select p;
                if (!showHidden)
                    query =
                        from p in query
                        where p.Active
                        select p;

                query =
                    from p in query
                    orderby p.Code
                    select p;

                return query.ToList();
            }, CachingDefaults.MonthCacheTime);

            return entities;
        }

        public virtual IPagedList<Warehouse> Get(WarehouseSearchContext ctx)
        {
            ctx.Keywords = ctx.Keywords?.Trim();

            var query = from p in _warehouseRepository.Table
                        select p;

            if (ctx.Keywords.HasValue())
            {
                query = query.LeftJoin(_localizedPropertyRepository.Table,
                        (e, l) => e.Id == l.EntityId,
                        (e, l) => new { e, l })
                    .Where(
                        el =>
                            el.e.Code.Contains(ctx.Keywords) ||
                            el.e.Name.Contains(ctx.Keywords) ||
                            el.e.TerminalCode.Contains(ctx.Keywords) ||
                            el.e.LocalName.Contains(ctx.Keywords) ||
                            (el.l.LanguageId == ctx.LanguageId &&
                             el.l.LocaleKeyGroup == nameof(Warehouse) &&
                             el.l.LocaleKey == nameof(Warehouse.Name) &&
                             el.l.LocaleValue.Contains(ctx.Keywords)))
                    .Select(el => el.e).Distinct();
            }
            if (ctx.Status == (int)ActiveStatus.Activated)
            {
                query =
                    from p in query
                    where p.Active
                    select p;
            }
            if (ctx.Status == (int)ActiveStatus.Deactivated)
            {
                query =
                    from p in query
                    where !p.Active
                    select p;
            }

            query =
                from p in query
                orderby p.Code
                select p;

            return new PagedList<Warehouse>(query, ctx.PageIndex, ctx.PageSize);
        }

        public virtual async Task<Warehouse> GetByIdAsync(string id)
        {
            if (id.IsEmpty())
                return null;

            return await _warehouseRepository.GetByIdAsync(id);
        }

        public virtual async Task<int> ActivatesAsync(IEnumerable<string> ids, bool active)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));

            var result = await _warehouseRepository.Table
                .Where(w => ids.Contains(w.Id))
                .Set(x => x.Active, active)
                .UpdateAsync();

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.Warehouses.PrefixCacheKey);
            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.PostOffices.PrefixCacheKey);

            return result;
        }

        public virtual async Task<bool> ExistsAsync(string code)
        {
            return await _warehouseRepository.Table
                .AnyAsync(
                    a =>
                        !string.IsNullOrEmpty(a.Code)
                        && a.Code.Equals(code));
        }

        public virtual async Task<bool> ExistsAsync(string oldCode, string newCode)
        {
            return await _warehouseRepository.Table
                .AnyAsync(
                    a =>
                        !string.IsNullOrEmpty(a.Code)
                        && a.Code.Equals(newCode)
                        && !a.Code.Equals(oldCode));
        }

        #endregion
    }
}
