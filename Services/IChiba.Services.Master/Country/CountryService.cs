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
    public partial class CountryService : ICountryService
    {
        #region Constants



        #endregion

        #region Fields

        private readonly IRepository<Country> _countryRepository;
        private readonly IRepository<GlobalZone> _zoneRepository;
        private readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
        private readonly IIChibaCacheManager _cacheManager;

        #endregion

        #region Ctor

        public CountryService(
            IIChibaCacheManager cacheManager)
        {
            _countryRepository = EngineContext.Current.Resolve<IRepository<Country>>(DataConnectionHelper.ConnectionStringNames.Master);
            _zoneRepository = EngineContext.Current.Resolve<IRepository<GlobalZone>>(DataConnectionHelper.ConnectionStringNames.Master);
            _localizedPropertyRepository = EngineContext.Current.Resolve<IRepository<LocalizedProperty>>(DataConnectionHelper.ConnectionStringNames.Master);
            _cacheManager = cacheManager;
        }

        #endregion

        #region Methods

        public virtual async Task<int> InsertAsync(Country entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var result = await _countryRepository.InsertAsync(entity);

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.Countries.PrefixCacheKey);

            return result;
        }

        public virtual async Task<int> UpdateAsync(Country entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var result = await _countryRepository.UpdateAsync(entity);

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.Countries.PrefixCacheKey);

            return result;
        }

        public virtual async Task<int> DeletesAsync(IEnumerable<string> ids)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));

            var result = await _countryRepository.DeleteAsync(ids);

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.Countries.PrefixCacheKey);

            return result;
        }

        public virtual IList<Country> GetAll(bool showHidden = false)
        {
            var key = MasterCacheKeys.Countries.AllCacheKey.FormatWith(showHidden);
            var entities = _cacheManager.GetToDb(key, () =>
            {
                var query = from p in _countryRepository.Table select p;
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

        public virtual IPagedList<Country> Get(CountrySearchContext ctx)
        {
            ctx.Keywords = ctx.Keywords?.Trim();

            var query =
                from c in _countryRepository.Table
                join z in _zoneRepository.Table on c.GlobalZoneId equals z.Id
                select new Country
                {
                    Id = c.Id,
                    Code = c.Code,
                    Name = c.Name,
                    ShortName = c.ShortName,
                    LocalName = c.LocalName,
                    Active = c.Active,
                    DisplayOrder = c.DisplayOrder,
                    GlobalZoneId = c.GlobalZoneId,
                    GlobalZone = new GlobalZone
                    {
                        Id = z.Id,
                        Code = z.Code,
                        Name = z.Name
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
                            el.e.ShortName.Contains(ctx.Keywords) ||
                            el.e.LocalName.Contains(ctx.Keywords) ||
                            (el.l.LanguageId == ctx.LanguageId &&
                             el.l.LocaleKeyGroup == nameof(Country) &&
                             el.l.LocaleKey == nameof(Country.Name) &&
                             el.l.LocaleValue.Contains(ctx.Keywords)))
                    .Select(el => el.e).Distinct();
            }

            if (ctx.GlobalZoneId.HasValue())
            {
                query = from c in query
                        where c.GlobalZoneId == ctx.GlobalZoneId
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
                orderby p.DisplayOrder
                select p;

            return new PagedList<Country>(query, ctx.PageIndex, ctx.PageSize);
        }

        public virtual async Task<Country> GetByIdAsync(string id)
        {
            if (id.IsEmpty())
                return null;

            return await _countryRepository.GetByIdAsync(id);
        }

        public virtual async Task<int> ActivatesAsync(IEnumerable<string> ids, bool active)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));

            var result = await _countryRepository.Table
                .Where(w => ids.Contains(w.Id))
                .Set(x => x.Active, active)
                .UpdateAsync();

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.Countries.PrefixCacheKey);

            return result;
        }

        public virtual async Task<bool> ExistsAsync(string code)
        {
            return await _countryRepository.Table
                .AnyAsync(
                    a =>
                        !string.IsNullOrEmpty(a.Code)
                        && a.Code.Equals(code));
        }

        public virtual async Task<bool> ExistsAsync(string oldCode, string newCode)
        {
            return await _countryRepository.Table
                .AnyAsync(
                    a =>
                        !string.IsNullOrEmpty(a.Code)
                        && a.Code.Equals(newCode)
                        && !a.Code.Equals(oldCode));
        }

        #endregion
    }
}
