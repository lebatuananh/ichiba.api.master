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
    public partial class PortService : IPortService
    {
        #region Constants



        #endregion

        #region Fields

        private readonly IRepository<Port> _portRepository;
        private readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
        private readonly IIChibaCacheManager _cacheManager;
        private readonly IRepository<StateProvince> _stateProvinceRepository;
        private readonly IRepository<Country> _countryRepository;

        #endregion

        #region Ctor

        public PortService(
            IIChibaCacheManager cacheManager)
        {
            _portRepository = EngineContext.Current.Resolve<IRepository<Port>>(DataConnectionHelper.ConnectionStringNames.Master);
            _stateProvinceRepository = EngineContext.Current.Resolve<IRepository<StateProvince>>(DataConnectionHelper.ConnectionStringNames.Master);
            _countryRepository = EngineContext.Current.Resolve<IRepository<Country>>(DataConnectionHelper.ConnectionStringNames.Master);
            _localizedPropertyRepository = EngineContext.Current.Resolve<IRepository<LocalizedProperty>>(DataConnectionHelper.ConnectionStringNames.Master);
            _cacheManager = cacheManager;
        }

        #endregion

        #region Methods

        public virtual async Task<int> InsertAsync(Port entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var result = await _portRepository.InsertAsync(entity);

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.Ports.PrefixCacheKey);

            return result;
        }

        public virtual async Task<int> UpdateAsync(Port entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var result = await _portRepository.UpdateAsync(entity);

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.Ports.PrefixCacheKey);

            return result;
        }

        public virtual async Task<int> DeletesAsync(IEnumerable<string> ids)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));

            var result = await _portRepository.DeleteAsync(ids);

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.Ports.PrefixCacheKey);

            return result;
        }

        public virtual IList<Port> GetAll(bool showHidden = false)
        {
            var key = MasterCacheKeys.Ports.AllCacheKey.FormatWith(showHidden);
            var entities = _cacheManager.GetToDb(key, () =>
            {
                var query = from p in _portRepository.Table select p;
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

        public virtual IPagedList<Port> Get(PortSearchContext ctx)
        {
            ctx.Keywords = ctx.Keywords?.Trim();

            var query = from p in _portRepository.Table
                        join cou in _countryRepository.Table on p.CountryId equals cou.Id into res
                        from c in res.DefaultIfEmpty()
                        join s in _stateProvinceRepository.Table on p.StateProvinceId equals s.Id into state
                        from s in state.DefaultIfEmpty()
                        select new Port
                        {
                            Active = p.Active,
                            Air = p.Air,
                            Code = p.Code,
                            CountryId = p.CountryId,
                            DisplayOrder = p.DisplayOrder,
                            Id = p.Id,
                            Inland = p.Inland,
                            LocalName = p.LocalName,
                            Name = p.Name,
                            Note = p.Note,
                            Ocean = p.Ocean,
                            ShortName = p.ShortName,
                            Country = c == null ? null : new Country
                            {
                                Id = c.Id,
                                Name = c.Name,
                                Code = c.Code
                            },
                            StateProvince = s == null ? null : new StateProvince
                            {
                                Id = s.Id,
                                Name = s.Name,
                                Code = s.Code
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
                             el.l.LocaleKeyGroup == nameof(Port) &&
                             el.l.LocaleKey == nameof(Port.Name) &&
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

            return new PagedList<Port>(query, ctx.PageIndex, ctx.PageSize);
        }

        public virtual async Task<Port> GetByIdAsync(string id)
        {
            if (id.IsEmpty())
                return null;

            return await _portRepository.GetByIdAsync(id);
        }

        public virtual async Task<int> ActivatesAsync(IEnumerable<string> ids, bool active)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));

            var result = await _portRepository.Table
                .Where(w => ids.Contains(w.Id))
                .Set(x => x.Active, active)
                .UpdateAsync();

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.Ports.PrefixCacheKey);

            return result;
        }

        public virtual async Task<bool> ExistsAsync(string code)
        {
            return await _portRepository.Table
                .AnyAsync(
                    a =>
                        !string.IsNullOrEmpty(a.Code)
                        && a.Code.Equals(code));
        }

        public virtual async Task<bool> ExistsAsync(string oldCode, string newCode)
        {
            return await _portRepository.Table
                .AnyAsync(
                    a =>
                        !string.IsNullOrEmpty(a.Code)
                        && a.Code.Equals(newCode)
                        && !a.Code.Equals(oldCode));
        }

        #endregion
    }
}
