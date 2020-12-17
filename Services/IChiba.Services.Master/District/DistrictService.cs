using IChiba.Caching;
using IChiba.Core.Domain.Master;
using IChiba.Core.Infrastructure;
using IChiba.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using IChiba.Core;
using IChiba.Core.Domain;
using LinqToDB;

namespace IChiba.Services.Master
{
    public class DistrictService : IDistrictService
    {
        #region Fields

        private readonly IRepository<StateProvince> _stateProvinceRepository;
        private readonly IRepository<Country> _countryRepository;
        private readonly IRepository<District> _districtRepository;
        private readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
        private readonly IIChibaCacheManager _cacheManager;

        #endregion

        #region Ctor

        public DistrictService(
            IIChibaCacheManager cacheManager)
        {
            _stateProvinceRepository = EngineContext.Current.Resolve<IRepository<StateProvince>>(DataConnectionHelper.ConnectionStringNames.Master);
            _localizedPropertyRepository = EngineContext.Current.Resolve<IRepository<LocalizedProperty>>(DataConnectionHelper.ConnectionStringNames.Master);
            _countryRepository = EngineContext.Current.Resolve<IRepository<Country>>(DataConnectionHelper.ConnectionStringNames.Master);
            _districtRepository = EngineContext.Current.Resolve<IRepository<District>>(DataConnectionHelper.ConnectionStringNames.Master);
            _cacheManager = cacheManager;
        }

        #endregion

        #region Methods

        public virtual async Task<int> InsertAsync(District entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var result = await _districtRepository.InsertAsync(entity);

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.Districts.PrefixCacheKey);

            return result;
        }

        public virtual async Task<int> UpdateAsync(District entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var result = await _districtRepository.UpdateAsync(entity);

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.Districts.PrefixCacheKey);

            return result;
        }

        public virtual async Task<int> DeletesAsync(IEnumerable<string> ids)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));

            var result = await _districtRepository.DeleteAsync(ids);

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.Districts.PrefixCacheKey);

            return result;
        }

        public virtual IList<District> GetByStateProvinceId(string stateProvinceId, bool showHidden = false)
        {
            var key = MasterCacheKeys.Districts.ByStateProvinceIdCacheKey.FormatWith(stateProvinceId, showHidden);
            var entities = _cacheManager.GetToDb(key, () =>
            {
                var query = from p in _districtRepository.Table
                            where p.StateProvinceId == stateProvinceId
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

        public virtual IPagedList<District> Get(DistrictSearchContext ctx)
        {
            ctx.Keywords = ctx.Keywords?.Trim();

            var query = from p in _districtRepository.Table
                        join state in _stateProvinceRepository.Table on p.StateProvinceId equals state.Id
                        join c in _countryRepository.Table on p.CountryId equals c.Id
                        select new District
                        {
                            Id = p.Id,
                            Active = p.Active,
                            Code = p.Code,
                            CountryId = p.CountryId,
                            StateProvinceId = p.StateProvinceId,
                            Name = p.Name,
                            Note = p.Note,
                            ShortName = p.ShortName,
                            Country = new Country
                            {
                                Id = c.Id,
                                Name = c.Name,
                                Code = c.Code
                            },
                            StateProvince = new StateProvince
                            {
                                Id = state.Id,
                                Name = state.Name,
                                Code = state.Name
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
            if (ctx.StateProvinceId.HasValue())
            {
                query = from p in query
                        where p.StateProvinceId == ctx.StateProvinceId
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

            return new PagedList<District>(query, ctx.PageIndex, ctx.PageSize);
        }

        public virtual async Task<District> GetByIdAsync(string id)
        {
            if (id.IsEmpty())
                return null;

            return await _districtRepository.GetByIdAsync(id);
        }

        public virtual async Task<int> ActivatesAsync(IEnumerable<string> ids, bool active)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));

            var result = await _districtRepository.Table
                .Where(w => ids.Contains(w.Id))
                .Set(x => x.Active, active)
                .UpdateAsync();


            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.Districts.PrefixCacheKey);

            return result;
        }

        public virtual async Task<bool> ExistsAsync(string code)
        {
            return await _districtRepository.Table
                .AnyAsync(
                    a =>
                        !string.IsNullOrEmpty(a.Code)
                        && a.Code.Equals(code));
        }

        public virtual async Task<bool> ExistsAsync(string oldCode, string newCode)
        {
            return await _districtRepository.Table
                .AnyAsync(
                    a =>
                        !string.IsNullOrEmpty(a.Code)
                        && a.Code.Equals(newCode)
                        && !a.Code.Equals(oldCode));
        }

        #endregion
    }
}
