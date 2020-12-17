using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IChiba.Caching;
using IChiba.Core;
using IChiba.Core.Domain;
using IChiba.Core.Domain.Master;
using IChiba.Core.Infrastructure;
using IChiba.Data;
using LinqToDB;

namespace IChiba.Services.Master
{
    public partial class CommodityService : ICommodityService
    {
        #region Constants



        #endregion

        #region Fields

        private readonly IRepository<Commodity> _commodityRepository;
        private readonly IRepository<CommodityGroup> _commodityGroupRepository;
        private readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
        private readonly IIChibaCacheManager _cacheManager;

        #endregion

        #region Ctor

        public CommodityService(
            IIChibaCacheManager cacheManager)
        {
            _commodityRepository = EngineContext.Current.Resolve<IRepository<Commodity>>(DataConnectionHelper.ConnectionStringNames.Master);
            _commodityGroupRepository = EngineContext.Current.Resolve<IRepository<CommodityGroup>>(DataConnectionHelper.ConnectionStringNames.Master);
            _localizedPropertyRepository = EngineContext.Current.Resolve<IRepository<LocalizedProperty>>(DataConnectionHelper.ConnectionStringNames.Master);
            _cacheManager = cacheManager;
        }

        #endregion

        #region Methods

        public virtual async Task<int> InsertAsync(Commodity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var result = await _commodityRepository.InsertAsync(entity);

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.Commodities.PrefixCacheKey);

            return result;
        }

        public virtual async Task<int> UpdateAsync(Commodity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var result = await _commodityRepository.UpdateAsync(entity);

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.Commodities.PrefixCacheKey);

            return result;
        }

        public virtual async Task<int> DeletesAsync(IEnumerable<string> ids)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));

            var result = await _commodityRepository.DeleteAsync(ids);

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.Commodities.PrefixCacheKey);

            return result;
        }

        public virtual IList<Commodity> GetAll(bool showHidden = false)
        {
            var key = MasterCacheKeys.Commodities.AllCacheKey.FormatWith(showHidden);
            var entities = _cacheManager.GetToDb(key, () =>
            {
                var query = from p in _commodityRepository.Table select p;
                if (!showHidden)
                    query =
                        from p in query
                        where p.Active
                        select p;

                query =
                    from p in query
                    orderby p.DisplayOrder
                    select p;

                return query.ToList();
            }, CachingDefaults.MonthCacheTime);

            return entities;
        }

        public virtual IPagedList<Commodity> Get(CommoditySearchContext ctx)
        {
            ctx.Keywords = ctx.Keywords?.Trim();

            var query = from p in _commodityRepository.Table
                        join cg in _commodityGroupRepository.Table on p.CommodityGroupId equals cg.Id into res
                        from cg in res.DefaultIfEmpty()
                        select new Commodity
                        {
                            Active = p.Active,
                            Code = p.Code,
                            CommodityGroup = cg == null ? null : new CommodityGroup
                            {
                                Id = cg.Id,
                                Code = cg.Code,
                                Name = cg.Name
                            },
                            CommodityGroupId = p.CommodityGroupId,
                            Description = p.Description,
                            DisplayOrder = p.DisplayOrder,
                            Id = p.Id,
                            LocalName = p.LocalName,
                            Name = p.Name,
                        };

            if (ctx.Keywords.HasValue())
            {
                query = query.LeftJoin(_localizedPropertyRepository.Table,
                        (e, l) => e.Id == l.EntityId,
                        (e, l) => new { e, l })
                    .Where(
                        el =>
                            el.e.Code.Contains(ctx.Keywords) ||
                            el.e.Name.Contains(ctx.Keywords) ||
                            el.e.LocalName.Contains(ctx.Keywords) ||
                            (el.l.LanguageId == ctx.LanguageId &&
                             el.l.LocaleKeyGroup == nameof(Commodity) &&
                             el.l.LocaleKey == nameof(Commodity.Name) &&
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
                orderby p.DisplayOrder, p.Code
                select p;

            return new PagedList<Commodity>(query, ctx.PageIndex, ctx.PageSize);
        }

        public virtual async Task<Commodity> GetByIdAsync(string id)
        {
            if (id.IsEmpty())
                return null;

            return await _commodityRepository.GetByIdAsync(id);
        }

        public virtual async Task<int> ActivatesAsync(IEnumerable<string> ids, bool active)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));

            var result = await _commodityRepository.Table
                .Where(w => ids.Contains(w.Id))
                .Set(x => x.Active, active)
                .UpdateAsync();

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.Commodities.PrefixCacheKey);

            return result;
        }

        public virtual async Task<bool> ExistsAsync(string code)
        {
            return await _commodityRepository.Table
                .AnyAsync(
                    a =>
                        !string.IsNullOrEmpty(a.Code)
                        && a.Code.Equals(code));
        }

        public virtual async Task<bool> ExistsAsync(string oldCode, string newCode)
        {
            return await _commodityRepository.Table
                .AnyAsync(
                    a =>
                        !string.IsNullOrEmpty(a.Code)
                        && a.Code.Equals(newCode)
                        && !a.Code.Equals(oldCode));
        }

        #endregion
    }
}
