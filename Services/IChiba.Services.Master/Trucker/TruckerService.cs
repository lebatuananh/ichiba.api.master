﻿using System;
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
    public partial class TruckerService : ITruckerService
    {
        #region Constants



        #endregion

        #region Fields

        private readonly IRepository<Trucker> _truckerRepository;
        private readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
        private readonly IIChibaCacheManager _cacheManager;

        #endregion

        #region Ctor

        public TruckerService(
            IIChibaCacheManager cacheManager)
        {
            _truckerRepository = EngineContext.Current.Resolve<IRepository<Trucker>>(DataConnectionHelper.ConnectionStringNames.Master);
            _localizedPropertyRepository = EngineContext.Current.Resolve<IRepository<LocalizedProperty>>(DataConnectionHelper.ConnectionStringNames.Master);
            _cacheManager = cacheManager;
        }

        #endregion

        #region Methods

        public virtual async Task<int> InsertAsync(Trucker entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var result = await _truckerRepository.InsertAsync(entity);

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.Truckers.PrefixCacheKey);

            return result;
        }

        public virtual async Task<int> UpdateAsync(Trucker entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var result = await _truckerRepository.UpdateAsync(entity);

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.Truckers.PrefixCacheKey);

            return result;
        }

        public virtual async Task<int> DeletesAsync(IEnumerable<string> ids)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));

            var result = await _truckerRepository.DeleteAsync(ids);

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.Truckers.PrefixCacheKey);

            return result;
        }

        public virtual IList<Trucker> GetAll(bool showHidden = false)
        {
            var key = MasterCacheKeys.Truckers.AllCacheKey.FormatWith(showHidden);
            var entities = _cacheManager.GetToDb(key, () =>
            {
                var query = from p in _truckerRepository.Table select p;
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

        public virtual IPagedList<Trucker> Get(TruckerSearchContext ctx)
        {
            ctx.Keywords = ctx.Keywords?.Trim();

            var query = from p in _truckerRepository.Table select p;

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
                            el.e.Website.Contains(ctx.Keywords) ||
                            (el.l.LanguageId == ctx.LanguageId &&
                             el.l.LocaleKeyGroup == nameof(Trucker) &&
                             el.l.LocaleKey == nameof(Trucker.Name) &&
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

            return new PagedList<Trucker>(query, ctx.PageIndex, ctx.PageSize);
        }

        public virtual async Task<Trucker> GetByIdAsync(string id)
        {
            if (id.IsEmpty())
                return null;

            return await _truckerRepository.GetByIdAsync(id);
        }

        public virtual async Task<int> ActivatesAsync(IEnumerable<string> ids, bool active)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));

            var result = await _truckerRepository.Table
                .Where(w => ids.Contains(w.Id))
                .Set(x => x.Active, active)
                .UpdateAsync();

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.Truckers.PrefixCacheKey);

            return result;
        }

        public virtual async Task<bool> ExistsAsync(string code)
        {
            return await _truckerRepository.Table
                .AnyAsync(
                    a =>
                        !string.IsNullOrEmpty(a.Code)
                        && a.Code.Equals(code));
        }

        public virtual async Task<bool> ExistsAsync(string oldCode, string newCode)
        {
            return await _truckerRepository.Table
                .AnyAsync(
                    a =>
                        !string.IsNullOrEmpty(a.Code)
                        && a.Code.Equals(newCode)
                        && !a.Code.Equals(oldCode));
        }

        #endregion
    }
}
