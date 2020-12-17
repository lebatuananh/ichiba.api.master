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
    public class ShippingLineService : IShippingLineService
    {
        #region Constants



        #endregion

        #region Fields

        private readonly IRepository<ShippingLine> _shippingLineRepository;
        private readonly IRepository<ShippingAgent> _shippingAgentRepository;
        private readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
        private readonly IIChibaCacheManager _cacheManager;

        #endregion

        #region Ctor

        public ShippingLineService(
            IIChibaCacheManager cacheManager)
        {
            _shippingLineRepository = EngineContext.Current.Resolve<IRepository<ShippingLine>>(DataConnectionHelper.ConnectionStringNames.Master);
            _shippingAgentRepository = EngineContext.Current.Resolve<IRepository<ShippingAgent>>(DataConnectionHelper.ConnectionStringNames.Master);
            _localizedPropertyRepository = EngineContext.Current.Resolve<IRepository<LocalizedProperty>>(DataConnectionHelper.ConnectionStringNames.Master);
            _cacheManager = cacheManager;
        }

        #endregion

        #region Methods

        public virtual async Task<int> InsertAsync(ShippingLine entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var result = await _shippingLineRepository.InsertAsync(entity);

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.ShippingLines.PrefixCacheKey);

            return result;
        }

        public virtual async Task<int> UpdateAsync(ShippingLine entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var result = await _shippingLineRepository.UpdateAsync(entity);

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.ShippingLines.PrefixCacheKey);

            return result;
        }

        public virtual async Task<int> DeletesAsync(IEnumerable<string> ids)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));

            var result = await _shippingLineRepository.DeleteAsync(ids);

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.ShippingLines.PrefixCacheKey);

            return result;
        }

        public virtual IList<ShippingLine> GetAll(bool showHidden = false)
        {
            var key = MasterCacheKeys.ShippingLines.AllCacheKey.FormatWith(showHidden);
            var entities = _cacheManager.GetToDb(key, () =>
            {
                var query = from p in _shippingLineRepository.Table select p;
                if (!showHidden)
                    query =
                        from p in query
                        where p.Active
                        select p;

                query =
                    from p in query
                    select p;

                return query.ToList();
            }, CachingDefaults.MonthCacheTime);

            return entities;
        }

        public virtual IList<ShippingLine> GetByShippingAgentId(string shippingAgentId, bool showHidden = false)
        {
            var key = MasterCacheKeys.ShippingLines.AllCacheKey.FormatWith(shippingAgentId, showHidden);
            var entities = _cacheManager.GetToDb(key, () =>
            {
                var query = from p in _shippingLineRepository.Table
                            where p.ShippingAgentId == shippingAgentId
                            select p;
                if (!showHidden)
                    query =
                        from p in query
                        where p.Active
                        select p;

                query =
                    from p in query
                    orderby p.ShippingAgentId, p.Code
                    select p;

                return query.ToList();
            }, CachingDefaults.MonthCacheTime);

            return entities;
        }

        public virtual IPagedList<ShippingLine> Get(ShippingLineSearchContext ctx)
        {
            ctx.Keywords = ctx.Keywords?.Trim();

            var query =
                from c in _shippingLineRepository.Table
                join s in _shippingAgentRepository.Table on c.ShippingAgentId equals s.Id into sj
                from sp in sj.DefaultIfEmpty()
                select new ShippingLine
                {
                    Id = c.Id,
                    Code = c.Code,
                    Name = c.Name,
                    SCAC = c.SCAC,
                    LocalName = c.LocalName,
                    Website = c.Website,
                    Note = c.Note,
                    Active = c.Active,
                    ShippingAgentId = c.ShippingAgentId,
                    ShippingAgent = new ShippingAgent
                    {
                        Id = sp.Id,
                        Code = sp.Code,
                        Name = sp.Name,
                        LocalName = sp.LocalName
                    }
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
                            el.e.SCAC.Contains(ctx.Keywords) ||
                            (el.l.LanguageId == ctx.LanguageId &&
                             el.l.LocaleKeyGroup == nameof(ShippingLine) &&
                             el.l.LocaleKey == nameof(ShippingLine.Name) &&
                             el.l.LocaleValue.Contains(ctx.Keywords)))
                    .Select(el => el.e).Distinct();
            }

            if (ctx.ShippingAgentId.HasValue())
            {
                query = from c in query
                        where c.ShippingAgentId == ctx.ShippingAgentId
                        select c;
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

            return new PagedList<ShippingLine>(query, ctx.PageIndex, ctx.PageSize);
        }

        public virtual async Task<ShippingLine> GetByIdAsync(string id)
        {
            if (id.IsEmpty())
                return null;

            return await _shippingLineRepository.GetByIdAsync(id);
        }

        public virtual async Task<int> ActivatesAsync(IEnumerable<string> ids, bool active)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));

            var result = await _shippingLineRepository.Table
                .Where(w => ids.Contains(w.Id))
                .Set(x => x.Active, active)
                .UpdateAsync();

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.ShippingLines.PrefixCacheKey);

            return result;
        }

        public virtual async Task<bool> ExistsAsync(string code)
        {
            return await _shippingLineRepository.Table
                .AnyAsync(
                    a =>
                        !string.IsNullOrEmpty(a.Code)
                        && a.Code.Equals(code));
        }

        public virtual async Task<bool> ExistsAsync(string oldCode, string newCode)
        {
            return await _shippingLineRepository.Table
                .AnyAsync(
                    a =>
                        !string.IsNullOrEmpty(a.Code)
                        && a.Code.Equals(newCode)
                        && !a.Code.Equals(oldCode));
        }

        #endregion
    }
}
