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
    public class VesselService : IVesselService
    {
        #region Constants



        #endregion

        #region Fields

        private readonly IRepository<Vessel> _vesselRepository;
        private readonly IRepository<Country> _countryRepository;
        private readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
        private readonly IIChibaCacheManager _cacheManager;

        #endregion

        #region Ctor

        public VesselService(
            IIChibaCacheManager cacheManager)
        {
            _vesselRepository = EngineContext.Current.Resolve<IRepository<Vessel>>(DataConnectionHelper.ConnectionStringNames.Master);
            _countryRepository = EngineContext.Current.Resolve<IRepository<Country>>(DataConnectionHelper.ConnectionStringNames.Master);
            _localizedPropertyRepository = EngineContext.Current.Resolve<IRepository<LocalizedProperty>>(DataConnectionHelper.ConnectionStringNames.Master);
            _cacheManager = cacheManager;
        }

        #endregion

        #region Methods

        public virtual async Task<int> InsertAsync(Vessel entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var result = await _vesselRepository.InsertAsync(entity);

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.Vessels.PrefixCacheKey);

            return result;
        }

        public virtual async Task<int> UpdateAsync(Vessel entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var result = await _vesselRepository.UpdateAsync(entity);

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.Vessels.PrefixCacheKey);

            return result;
        }

        public virtual async Task<int> DeletesAsync(IEnumerable<string> ids)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));

            var result = await _vesselRepository.DeleteAsync(ids);

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.Vessels.PrefixCacheKey);

            return result;
        }

        public virtual IList<Vessel> GetAll(bool showHidden = false)
        {
            var key = MasterCacheKeys.Vessels.AllCacheKey.FormatWith(showHidden);
            var entities = _cacheManager.GetToDb(key, () =>
            {
                var query = from p in _vesselRepository.Table select p;
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

        public virtual IList<Vessel> GetByCountryId(string countryId, bool showHidden = false)
        {
            var key = MasterCacheKeys.Vessels.AllCacheKey.FormatWith(countryId, showHidden);
            var entities = _cacheManager.GetToDb(key, () =>
            {
                var query = from p in _vesselRepository.Table
                            where p.CountryId == countryId
                            select p;
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

        public virtual IPagedList<Vessel> Get(VesselSearchContext ctx)
        {
            ctx.Keywords = ctx.Keywords?.Trim();

            var query = from p in _vesselRepository.Table
                        join c in _countryRepository.Table on p.CountryId equals c.Id into res
                        from country in res.DefaultIfEmpty()
                        select new Vessel
                        {
                            Active = p.Active,
                            CountryId = p.CountryId,
                            DisplayOrder = p.DisplayOrder,
                            Id = p.Id,
                            IMOCode = p.IMOCode,
                            LocalName = p.LocalName,
                            Name = p.Name,
                            Note = p.Note,
                            Country = country == null ? null : new Country
                            {
                                Id = country.Id,
                                Code = country.Code,
                                Name = country.Name
                            }
                        };

            if (ctx.Keywords.HasValue())
            {
                query = query.LeftJoin(_localizedPropertyRepository.Table,
                        (e, l) => e.Id == l.EntityId,
                        (e, l) => new { e, l })
                    .Where(
                        el =>
                            el.e.IMOCode.Contains(ctx.Keywords) ||
                            el.e.Name.Contains(ctx.Keywords) ||
                            el.e.LocalName.Contains(ctx.Keywords) ||
                            (el.l.LanguageId == ctx.LanguageId &&
                             el.l.LocaleKeyGroup == nameof(Vessel) &&
                             el.l.LocaleKey == nameof(Vessel.Name) &&
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
                orderby p.DisplayOrder
                select p;

            return new PagedList<Vessel>(query, ctx.PageIndex, ctx.PageSize);
        }

        public virtual async Task<Vessel> GetByIdAsync(string id)
        {
            if (id.IsEmpty())
                return null;

            return await _vesselRepository.GetByIdAsync(id);
        }

        public virtual async Task<int> ActivatesAsync(IEnumerable<string> ids, bool active)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));

            var result = await _vesselRepository.Table
                .Where(w => ids.Contains(w.Id))
                .Set(x => x.Active, active)
                .UpdateAsync();

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.Vessels.PrefixCacheKey);

            return result;
        }

        //public virtual async Task<bool> ExistsAsync(string code)
        //{
        //    return await _vesselRepository.Table
        //        .AnyAsync(
        //            a =>
        //                !string.IsNullOrEmpty(a.IMOCode)
        //                && a.IMOCode.Equals(code));
        //}

        //public virtual async Task<bool> ExistsAsync(string oldCode, string newCode)
        //{
        //    return await _vesselRepository.Table
        //        .AnyAsync(
        //            a =>
        //                !string.IsNullOrEmpty(a.Code)
        //                && a.Code.Equals(newCode)
        //                && !a.Code.Equals(oldCode));
        //}

        #endregion
    }
}
