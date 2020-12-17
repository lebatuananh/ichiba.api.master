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
    public partial class ChargesTypeService : IChargesTypeService
    {
        #region Constants



        #endregion

        #region Fields

        private readonly IRepository<ChargesType> _chargesTypeRepository;
        private readonly IRepository<ChargesGroup> _chargesGroupRepository;
        private readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
        private readonly IIChibaCacheManager _cacheManager;

        #endregion

        #region Ctor

        public ChargesTypeService(
            IIChibaCacheManager cacheManager)
        {
            _chargesTypeRepository = EngineContext.Current.Resolve<IRepository<ChargesType>>(DataConnectionHelper.ConnectionStringNames.Master);
            _chargesGroupRepository = EngineContext.Current.Resolve<IRepository<ChargesGroup>>(DataConnectionHelper.ConnectionStringNames.Master);
            _localizedPropertyRepository = EngineContext.Current.Resolve<IRepository<LocalizedProperty>>(DataConnectionHelper.ConnectionStringNames.Master);
            _cacheManager = cacheManager;
        }

        #endregion

        #region Methods

        public virtual async Task<int> InsertAsync(ChargesType entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var result = await _chargesTypeRepository.InsertAsync(entity);

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.ChargesTypes.PrefixCacheKey);

            return result;
        }

        public virtual async Task<int> UpdateAsync(ChargesType entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var result = await _chargesTypeRepository.UpdateAsync(entity);

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.ChargesTypes.PrefixCacheKey);

            return result;
        }

        public virtual async Task<int> DeletesAsync(IEnumerable<string> ids)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));

            var result = await _chargesTypeRepository.DeleteAsync(ids);

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.ChargesTypes.PrefixCacheKey);

            return result;
        }

        public virtual IList<ChargesType> GetAll(bool showHidden = false)
        {
            var key = MasterCacheKeys.ChargesTypes.AllCacheKey.FormatWith(showHidden);
            var entities = _cacheManager.GetToDb(key, () =>
            {
                var query = from p in _chargesTypeRepository.Table select p;
                if (!showHidden)
                    query =
                        from p in query
                        where p.Active
                        select p;

                query =
                    from p in query
                    orderby p.ChargesGroupId, p.DisplayOrder
                    select p;

                return query.ToList();
            }, CachingDefaults.MonthCacheTime);

            return entities;
        }

        public virtual IPagedList<ChargesType> Get(ChargesTypeSearchContext ctx)
        {
            ctx.Keywords = ctx.Keywords?.Trim();

            var query =
                from c in _chargesTypeRepository.Table
                join chargesGroup in _chargesGroupRepository.Table on c.ChargesGroupId equals chargesGroup.Id into zj
                from z in zj.DefaultIfEmpty()
                select new ChargesType
                {
                    Id = c.Id,
                    Code = c.Code,
                    Name = c.Name,
                    LocalName = c.LocalName,
                    Active = c.Active,
                    DisplayOrder = c.DisplayOrder,
                    ChargesGroupId = c.ChargesGroupId,
                    ChargesGroup = z == null ? null : new ChargesGroup
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
                            el.e.LocalName.Contains(ctx.Keywords) ||
                            (el.l.LanguageId == ctx.LanguageId &&
                             el.l.LocaleKeyGroup == nameof(ChargesType) &&
                             el.l.LocaleKey == nameof(ChargesType.Name) &&
                             el.l.LocaleValue.Contains(ctx.Keywords)))
                    .Select(el => el.e).Distinct();
            }

            if (ctx.ChargesGroupId.HasValue())
            {
                query = from c in query
                        where c.ChargesGroupId == ctx.ChargesGroupId
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
                orderby p.ChargesGroupId
                select p;

            return new PagedList<ChargesType>(query, ctx.PageIndex, ctx.PageSize);
        }

        public virtual async Task<ChargesType> GetByIdAsync(string id)
        {
            if (id.IsEmpty())
                return null;

            return await _chargesTypeRepository.GetByIdAsync(id);
        }

        public virtual async Task<int> ActivatesAsync(IEnumerable<string> ids, bool active)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));

            var result = await _chargesTypeRepository.Table
                .Where(w => ids.Contains(w.Id))
                .Set(x => x.Active, active)
                .UpdateAsync();

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.ChargesTypes.PrefixCacheKey);

            return result;
        }

        public virtual async Task<bool> ExistsAsync(string code)
        {
            return await _chargesTypeRepository.Table
                .AnyAsync(
                    a =>
                        !string.IsNullOrEmpty(a.Code)
                        && a.Code.Equals(code));
        }

        public virtual async Task<bool> ExistsAsync(string oldCode, string newCode)
        {
            return await _chargesTypeRepository.Table
                .AnyAsync(
                    a =>
                        !string.IsNullOrEmpty(a.Code)
                        && a.Code.Equals(newCode)
                        && !a.Code.Equals(oldCode));
        }

        #endregion
    }
}
