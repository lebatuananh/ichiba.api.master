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
    public partial class CurrencyService : ICurrencyService
    {
        #region Constants



        #endregion

        #region Fields

        private readonly IRepository<Currency> _currencyRepository;
        private readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
        private readonly IIChibaCacheManager _cacheManager;

        #endregion

        #region Ctor

        public CurrencyService(
            IIChibaCacheManager cacheManager)
        {
            _currencyRepository = EngineContext.Current.Resolve<IRepository<Currency>>(DataConnectionHelper.ConnectionStringNames.Master);
            _localizedPropertyRepository = EngineContext.Current.Resolve<IRepository<LocalizedProperty>>(DataConnectionHelper.ConnectionStringNames.Master);
            _cacheManager = cacheManager;
        }

        #endregion

        #region Methods

        public virtual async Task<int> InsertAsync(Currency entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.CreatedOnUtc = DateTime.Now;
            entity.UpdatedOnUtc = DateTime.Now;

            var result = await _currencyRepository.InsertAsync(entity);

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.Currencies.PrefixCacheKey);

            return result;
        }

        public virtual async Task<int> UpdateAsync(Currency entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.UpdatedOnUtc = DateTime.Now;

            var result = await _currencyRepository.UpdateAsync(entity);

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.Currencies.PrefixCacheKey);

            return result;
        }

        public virtual async Task<int> DeletesAsync(IEnumerable<string> ids)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));

            var result = await _currencyRepository.DeleteAsync(ids);

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.Currencies.PrefixCacheKey);

            return result;
        }

        public virtual IList<Currency> GetAll()
        {
            var key = MasterCacheKeys.Currencies.AllCacheKey.FormatWith("all");
            var entities = _cacheManager.GetToDb(key, () =>
            {
                var query = from p in _currencyRepository.Table select p;

                query =
                    from p in query
                    orderby p.DisplayOrder
                    select p;

                return query.ToList();
            }, CachingDefaults.MonthCacheTime);

            return entities;
        }

        public virtual IPagedList<Currency> Get(CurrencySearchContext ctx)
        {
            ctx.Keywords = ctx.Keywords?.Trim();

            var query = from p in _currencyRepository.Table select p;

            if (ctx.Keywords.HasValue())
            {
                query = query.LeftJoin(_localizedPropertyRepository.Table,
                        (e, l) => e.Id == l.EntityId,
                        (e, l) => new { e, l })
                    .Where(
                        el =>
                            el.e.CurrencyCode.Contains(ctx.Keywords) ||
                            el.e.Name.Contains(ctx.Keywords) ||
                            el.e.DisplayLocale.Contains(ctx.Keywords) ||
                            (el.l.LanguageId == ctx.LanguageId &&
                             el.l.LocaleKeyGroup == nameof(Currency) &&
                             el.l.LocaleKey == nameof(Currency.Name) &&
                             el.l.LocaleValue.Contains(ctx.Keywords)))
                    .Select(el => el.e).Distinct();
            }

            query =
                from p in query
                orderby p.DisplayOrder
                select p;

            return new PagedList<Currency>(query, ctx.PageIndex, ctx.PageSize);
        }

        public virtual async Task<Currency> GetByIdAsync(string id)
        {
            if (id.IsEmpty())
                return null;

            return await _currencyRepository.GetByIdAsync(id);
        }

        //public virtual async Task<int> ActivatesAsync(IEnumerable<string> ids, bool active)
        //{
        //    if (ids == null)
        //        throw new ArgumentNullException(nameof(ids));

        //    var result = await _currencyRepository.Table
        //        .Where(w => ids.Contains(w.Id))
        //        .Set(x => x.Active, active)
        //        .UpdateAsync();

        //    await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.Currencies.PrefixCacheKey);

        //    return result;
        //}

        public virtual async Task<bool> ExistsAsync(string code)
        {
            return await _currencyRepository.Table
                .AnyAsync(
                    a =>
                        !string.IsNullOrEmpty(a.CurrencyCode)
                        && a.CurrencyCode.Equals(code));
        }

        public virtual async Task<bool> ExistsAsync(string oldCode, string newCode)
        {
            return await _currencyRepository.Table
                .AnyAsync(
                    a =>
                        !string.IsNullOrEmpty(a.CurrencyCode)
                        && a.CurrencyCode.Equals(newCode)
                        && !a.CurrencyCode.Equals(oldCode));
        }

        #endregion
    }
}
