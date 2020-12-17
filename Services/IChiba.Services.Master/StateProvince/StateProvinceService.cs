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
    public class StateProvinceService : IStateProvinceService
    {
        #region Constants



        #endregion

        #region Fields

        private readonly IRepository<StateProvince> _stateProvinceRepository;
        private readonly IRepository<Country> _countryRepository;
        private readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
        private readonly IIChibaCacheManager _cacheManager;

        #endregion

        #region Ctor

        public StateProvinceService(
            IIChibaCacheManager cacheManager)
        {
            _stateProvinceRepository = EngineContext.Current.Resolve<IRepository<StateProvince>>(DataConnectionHelper.ConnectionStringNames.Master);
            _localizedPropertyRepository = EngineContext.Current.Resolve<IRepository<LocalizedProperty>>(DataConnectionHelper.ConnectionStringNames.Master);
            _countryRepository = EngineContext.Current.Resolve<IRepository<Country>>(DataConnectionHelper.ConnectionStringNames.Master);
            _cacheManager = cacheManager;
        }

        #endregion

        #region Methods

        public virtual async Task<int> InsertAsync(StateProvince entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var result = await _stateProvinceRepository.InsertAsync(entity);

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.StateProvinces.PrefixCacheKey);

            return result;
        }

        public virtual async Task<int> UpdateAsync(StateProvince entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var result = await _stateProvinceRepository.UpdateAsync(entity);

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.StateProvinces.PrefixCacheKey);

            return result;
        }

        public virtual async Task<int> DeletesAsync(IEnumerable<string> ids)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));

            var result = await _stateProvinceRepository.DeleteAsync(ids);

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.StateProvinces.PrefixCacheKey);

            return result;
        }

        public virtual IList<StateProvince> GetByCountryId(string countryId, bool showHidden = false)
        {
            var key = MasterCacheKeys.StateProvinces.ByCountryIdCacheKey.FormatWith(countryId, showHidden);
            var entities = _cacheManager.GetToDb(key, () =>
            {
                var query = from p in _stateProvinceRepository.Table
                            where p.CountryId == countryId
                            select p;
                if (!showHidden)
                    query =
                        from p in query
                        where p.Active
                        select p;

                query =
                    from p in query
                    orderby p.Name
                    select p;

                return query.ToList();
            }, CachingDefaults.MonthCacheTime);

            return entities;
        }

        public virtual IPagedList<StateProvince> Get(StateProvinceSearchContext ctx)
        {
            ctx.Keywords = ctx.Keywords?.Trim();

            var query = from p in _stateProvinceRepository.Table
                        join c in _countryRepository.Table on p.CountryId equals c.Id
                        select new StateProvince
                        {
                            Id = p.Id,
                            Active = p.Active,
                            Code = p.Code,
                            CountryId = p.CountryId,
                            Name = p.Name,
                            Note = p.Note,
                            ShortName = p.ShortName,
                            Country = new Country
                            {
                                Id = c.Id,
                                Name = c.Name,
                                Code = c.Code
                            },
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
                            (el.l.LanguageId == ctx.LanguageId &&
                             el.l.LocaleKeyGroup == nameof(StateProvince) &&
                             el.l.LocaleKey == nameof(StateProvince.Name) &&
                             el.l.LocaleValue.Contains(ctx.Keywords)))
                    .Select(el => el.e).Distinct();
            }
            if (ctx.CountryId.HasValue())
            {
                query = from p in query
                        where p.CountryId == ctx.CountryId
                        select p;
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
                orderby p.Name, p.Code
                select p;

            return new PagedList<StateProvince>(query, ctx.PageIndex, ctx.PageSize);
        }

        public virtual async Task<StateProvince> GetByIdAsync(string id)
        {
            if (id.IsEmpty())
                return null;

            return await _stateProvinceRepository.GetByIdAsync(id);
        }

        public virtual async Task<int> ActivatesAsync(IEnumerable<string> ids, bool active)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));

            var result = await _stateProvinceRepository.Table
                .Where(w => ids.Contains(w.Id))
                .Set(x => x.Active, active)
                .UpdateAsync();


            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.StateProvinces.PrefixCacheKey);

            return result;
        }

        public virtual async Task<bool> ExistsAsync(string code)
        {
            return await _stateProvinceRepository.Table
                .AnyAsync(
                    a =>
                        !string.IsNullOrEmpty(a.Code)
                        && a.Code.Equals(code));
        }

        public virtual async Task<bool> ExistsAsync(string oldCode, string newCode)
        {
            return await _stateProvinceRepository.Table
                .AnyAsync(
                    a =>
                        !string.IsNullOrEmpty(a.Code)
                        && a.Code.Equals(newCode)
                        && !a.Code.Equals(oldCode));
        }

        #endregion
    }
}
