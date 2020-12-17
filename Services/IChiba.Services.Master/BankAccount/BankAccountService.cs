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
    public partial class BankAccountService : IBankAccountService
    {
        #region Constants



        #endregion

        #region Fields

        private readonly IRepository<BankAccount> _bankAccountRepository;
        private readonly IRepository<BankBranch> _bankBranchRepository;
        private readonly IRepository<Currency> _currencyRepository;
        private readonly IRepository<Bank> _bankRepository;
        private readonly IIChibaCacheManager _cacheManager;

        #endregion

        #region Ctor

        public BankAccountService(
            IIChibaCacheManager cacheManager)
        {
            _bankAccountRepository = EngineContext.Current.Resolve<IRepository<BankAccount>>(DataConnectionHelper.ConnectionStringNames.Master);
            _bankBranchRepository = EngineContext.Current.Resolve<IRepository<BankBranch>>(DataConnectionHelper.ConnectionStringNames.Master);
            _currencyRepository = EngineContext.Current.Resolve<IRepository<Currency>>(DataConnectionHelper.ConnectionStringNames.Master);
            _bankRepository = EngineContext.Current.Resolve<IRepository<Bank>>(DataConnectionHelper.ConnectionStringNames.Master);
            _cacheManager = cacheManager;
        }

        #endregion

        #region Methods

        public virtual async Task<int> InsertAsync(BankAccount entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var result = await _bankAccountRepository.InsertAsync(entity);

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.BankAccounts.PrefixCacheKey);

            return result;
        }

        public virtual async Task<int> UpdateAsync(BankAccount entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var result = await _bankAccountRepository.UpdateAsync(entity);

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.BankAccounts.PrefixCacheKey);

            return result;
        }

        public virtual async Task<int> DeletesAsync(IEnumerable<string> ids)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));

            var result = await _bankAccountRepository.DeleteAsync(ids);

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.BankAccounts.PrefixCacheKey);

            return result;
        }

        public virtual IList<BankAccount> GetAll(bool showHidden = false)
        {
            var key = MasterCacheKeys.BankAccounts.AllCacheKey.FormatWith(showHidden);
            var entities = _cacheManager.GetToDb(key, () =>
            {
                var query = from p in _bankAccountRepository.Table select p;
                if (!showHidden)
                    query =
                        from p in query
                        where p.Active
                        select p;

                query =
                    from p in query
                    orderby p.BankId
                    select p;

                return query.ToList();
            }, CachingDefaults.MonthCacheTime);

            return entities;
        }

        public virtual IPagedList<BankAccount> Get(BankAccountSearchContext ctx)
        {
            ctx.Keywords = ctx.Keywords?.Trim();

            var query =
                from c in _bankAccountRepository.Table
                join bank in _bankRepository.Table on c.BankId equals bank.Id into zj
                from z in zj.DefaultIfEmpty()
                join bankBranch in _bankBranchRepository.Table on c.BankBranchId equals bankBranch.Id into res
                from bb in res.DefaultIfEmpty()
                join currency in _currencyRepository.Table on c.CurrencyId equals currency.Id into cy
                from currency in cy.DefaultIfEmpty()
                select new BankAccount
                {
                    Id = c.Id,
                    AccountNumber = c.AccountNumber,
                    Note = c.Note,
                    Active = c.Active,
                    BankId = c.BankId,
                    Bank = new Bank
                    {
                        Id = z.Id,
                        Code = z.Code,
                        Name = z.Name
                    },
                    BankBranchId = c.BankBranchId,
                    Name = c.Name,
                    AccountMasterId = c.AccountMasterId,
                    //CurrencyId = c.CurrencyId,
                    //IBAN = c.IBAN,
                    LocalName = c.LocalName,
                    //VatNo = c.VatNo,
                    BankBranch = bb == null ? null : new BankBranch
                    {
                        Id = bb.Id,
                        Name = bb.Name
                    },
                    //Currency = currency == null ? null : new Currency
                    //{
                    //    Id = currency.Id,
                    //    Name = currency.Name
                    //}
                };

            if (ctx.Keywords.HasValue())
            {
                query = query.Where(w => w.AccountNumber.Contains(ctx.Keywords));
            }

            if (ctx.BankId.HasValue())
            {
                query = from c in query
                        where c.BankId == ctx.BankId
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
                orderby p.BankId
                select p;

            return new PagedList<BankAccount>(query, ctx.PageIndex, ctx.PageSize);
        }

        public virtual async Task<BankAccount> GetByIdAsync(string id)
        {
            if (id.IsEmpty())
                return null;

            return await _bankAccountRepository.GetByIdAsync(id);
        }

        public virtual async Task<int> ActivatesAsync(IEnumerable<string> ids, bool active)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));

            var result = await _bankAccountRepository.Table
                .Where(w => ids.Contains(w.Id))
                .Set(x => x.Active, active)
                .UpdateAsync();

            await _cacheManager.HybridProvider.RemoveByPrefixAsync(MasterCacheKeys.BankAccounts.PrefixCacheKey);

            return result;
        }

        #endregion
    }
}
