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
    public class PostOfficeService : IPostOfficeService
    {
        #region Constants



        #endregion

        #region Fields

        private readonly IRepository<PostOffice> _postOfficeRepository;
        private readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
        private readonly IRepository<Warehouse> _warehouseRepository;
        private readonly IIChibaCacheManager _cacheManager;

        #endregion

        #region Ctor

        public PostOfficeService(
            IIChibaCacheManager cacheManager)
        {
            _postOfficeRepository = EngineContext.Current.Resolve<IRepository<PostOffice>>(DataConnectionHelper.ConnectionStringNames.Master);
            _warehouseRepository = EngineContext.Current.Resolve<IRepository<Warehouse>>(DataConnectionHelper.ConnectionStringNames.Master);
            _localizedPropertyRepository = EngineContext.Current.Resolve<IRepository<LocalizedProperty>>(DataConnectionHelper.ConnectionStringNames.Master);
            _cacheManager = cacheManager;
        }

        #endregion

        #region Methods

        public virtual async Task<int> InsertAsync(PostOffice entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var result = await _postOfficeRepository.InsertAsync(entity);

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.PostOffices.PrefixCacheKey);

            return result;
        }

        public virtual async Task<int> UpdateAsync(PostOffice entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var result = await _postOfficeRepository.UpdateAsync(entity);

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.PostOffices.PrefixCacheKey);

            return result;
        }

        public virtual async Task<int> DeletesAsync(IEnumerable<string> ids)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));

            var result = await _postOfficeRepository.DeleteAsync(ids);

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.PostOffices.PrefixCacheKey);

            return result;
        }

        public virtual IList<PostOffice> GetAll(bool showHidden = false)
        {
            var key = MasterCacheKeys.PostOffices.AllCacheKey.FormatWith(showHidden);
            var entities = _cacheManager.GetToDb(key, () =>
            {
                var query = from p in _postOfficeRepository.Table
                            join w in _warehouseRepository.Table on p.WarehouseId equals w.Id into res
                            from w in res.DefaultIfEmpty()
                            select new PostOffice
                            {
                                Active = p.Active,
                                WarehouseId = p.WarehouseId,
                                Code = p.Code,
                                Id = p.Id,
                                LocalName = p.LocalName,
                                Name = p.Name,
                                Note = p.Note,
                                Warehouse = w ?? new Warehouse
                                {
                                    Id = w.Id,
                                    Name = w.Name,
                                    Code = w.Code,
                                    Active = w.Active,
                                    CurrencyCode = w.CurrencyCode,
                                    CurrencyId = w.CurrencyId,
                                    IsInternal = w.IsInternal,
                                    LocalName = w.LocalName,
                                    MeasureDimensionCode = w.MeasureDimensionCode,
                                    MeasureDimensionId = w.MeasureDimensionId,
                                    MeasureWeightCode = w.MeasureWeightCode,
                                    MeasureWeightId = w.MeasureWeightId,
                                    TerminalCode = w.TerminalCode
                                },
                            };
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

        public virtual IPagedList<PostOffice> Get(PostOfficeSearchContext ctx)
        {
            ctx.Keywords = ctx.Keywords?.Trim();

            var query = from p in _postOfficeRepository.Table
                        join w in _warehouseRepository.Table on p.WarehouseId equals w.Id into res
                        from w in res.DefaultIfEmpty()
                            //join s in _stateProvinceRepository.Table on p.StateProvinceId equals s.Id into state
                            //from s in state.DefaultIfEmpty()
                        select new PostOffice
                        {
                            Active = p.Active,
                            WarehouseId = p.WarehouseId,
                            Code = p.Code,
                            Id = p.Id,
                            LocalName = p.LocalName,
                            Name = p.Name,
                            Note = p.Note,
                            Warehouse = w == null ? null : new Warehouse
                            {
                                Id = w.Id,
                                Name = w.Name,
                                Code = w.Code
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
                            el.e.LocalName.Contains(ctx.Keywords) ||
                            (el.l.LanguageId == ctx.LanguageId &&
                             el.l.LocaleKeyGroup == nameof(PostOffice) &&
                             el.l.LocaleKey == nameof(PostOffice.Name) &&
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

            return new PagedList<PostOffice>(query, ctx.PageIndex, ctx.PageSize);
        }

        public virtual async Task<PostOffice> GetByIdAsync(string id)
        {
            if (id.IsEmpty())
                return null;

            return await _postOfficeRepository.GetByIdAsync(id);
        }

        public virtual async Task<int> ActivatesAsync(IEnumerable<string> ids, bool active)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));

            var result = await _postOfficeRepository.Table
                .Where(w => ids.Contains(w.Id))
                .Set(x => x.Active, active)
                .UpdateAsync();

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.PostOffices.PrefixCacheKey);

            return result;
        }

        public virtual async Task<bool> ExistsAsync(string code)
        {
            return await _postOfficeRepository.Table
                .AnyAsync(
                    a =>
                        !string.IsNullOrEmpty(a.Code)
                        && a.Code.Equals(code));
        }

        public virtual async Task<bool> ExistsAsync(string oldCode, string newCode)
        {
            return await _postOfficeRepository.Table
                .AnyAsync(
                    a =>
                        !string.IsNullOrEmpty(a.Code)
                        && a.Code.Equals(newCode)
                        && !a.Code.Equals(oldCode));
        }

        #endregion
    }
}
