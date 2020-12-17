using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevExtreme.AspNet.Data.ResponseModel;
using DevExtreme.AspNet.Mvc;
using IChiba.Services.Localization;
using IChiba.Services.Master;
using IChiba.SharedMvc;
using IChiba.SharedMvc.Models.Master;
using IChiba.Web.Framework.Controllers;
using IChiba.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc;

namespace IChiba.Api.Master.Controllers
{
    [ApiController]
    [Route("bank-account")]
    public class BankAccountController : AdminControllerBase
    {
        #region Fields

        private readonly IBankAccountService _bankAccountService;
        private readonly ICurrencyService _currencyService;
        private readonly IBankService _bankService;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor

        public BankAccountController(
            IBankAccountService bankAccountService,
            ICurrencyService currencyService,
            IBankService bankService,
            ILocalizationService localizationService)
        {
            _bankAccountService = bankAccountService;
            _currencyService = currencyService;
            _bankService = bankService;
            _localizationService = localizationService;
        }

        #endregion

        #region Methods

        [Route("create")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = await Task.FromResult(new BankAccountModel());

            PrepareModel(model);

            return Ok(new IChibaResult
            {
                data = model
            });
        }

        [Route("create")]
        [HttpPost]
        public async Task<IActionResult> Create(BankAccountModel model)
        {
            if (!ModelState.IsValid)
                return InvalidModelResult();

            var entity = model.ToEntity();

            await _bankAccountService.InsertAsync(entity);

            return Ok(new IChibaResult
            {
                message = string.Format(
                    _localizationService.GetResource("Common.Notify.Added"),
                    _localizationService.GetResource("Common.BankAccount"))
            });
        }

        [Route("edit")]
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var entity = await _bankAccountService.GetByIdAsync(id);
            if (entity == null)
                return Ok(new IChibaResult
                {
                    success = false,
                    message = string.Format(
                        _localizationService.GetResource("Common.Notify.DoesNotExist"),
                        _localizationService.GetResource("Common.BankAccount"))
                });

            var model = entity.ToModel();

            PrepareModel(model);

            return Ok(new IChibaResult
            {
                data = model
            });
        }

        [Route("edit")]
        [HttpPost]
        public async Task<IActionResult> Edit(BankAccountModel model)
        {
            ModelState.Remove("Code");
            if (!ModelState.IsValid)
                return InvalidModelResult();

            var entity = await _bankAccountService.GetByIdAsync(model.Id);
            if (entity == null)
                return Ok(new IChibaResult
                {
                    success = false,
                    message = string.Format(
                        _localizationService.GetResource("Common.Notify.DoesNotExist"),
                        _localizationService.GetResource("Common.BankAccount"))
                });

            entity = model.ToEntity(entity);

            await _bankAccountService.UpdateAsync(entity);

            return Ok(new IChibaResult
            {
                message = string.Format(
                    _localizationService.GetResource("Common.Notify.Updated"),
                    _localizationService.GetResource("Common.BankAccount"))
            });
        }

        [Route("deletes")]
        [HttpPost]
        public async Task<IActionResult> Deletes(IEnumerable<string> ids)
        {
            if (ids == null || !ids.Any())
            {
                return Ok(new IChibaResult
                {
                    success = false,
                    message = _localizationService.GetResource("Common.Notify.NoItemsSelected")
                });
            }

            await _bankAccountService.DeletesAsync(ids);

            return Ok(new IChibaResult
            {
                message = string.Format(
                    _localizationService.GetResource("Common.Notify.Deleted"),
                    _localizationService.GetResource("Common.BankAccount"))
            });
        }

        [Route("activates")]
        [HttpPost]
        public async Task<IActionResult> Activates(ActivatesModel model)
        {
            if (model?.Ids == null || !model.Ids.Any())
            {
                return Ok(new IChibaResult
                {
                    success = false,
                    message = _localizationService.GetResource("Common.Notify.NoItemsSelected")
                });
            }

            await _bankAccountService.ActivatesAsync(model.Ids, model.Active);

            return Ok(new IChibaResult
            {
                message = model.Active
                    ? string.Format(_localizationService.GetResource("Common.Notify.Activated"), _localizationService.GetResource("Common.BankAccount"))
                    : string.Format(_localizationService.GetResource("Common.Notify.Deactivated"), _localizationService.GetResource("Common.BankAccount"))
            });
        }

        #endregion

        #region Lists

        [Route("get")]
        [HttpGet]
        public IActionResult Get([FromQuery] DataSourceLoadOptions loadOptions, [FromQuery] BankAccountSearchModel searchModel)
        {
            var searchContext = new BankAccountSearchContext
            {
                Keywords = searchModel.Keywords,
                Status = (int)searchModel.Status,
                PageIndex = loadOptions.Skip / loadOptions.Take,
                PageSize = loadOptions.Take,
                LanguageId = searchModel.LanguageId,
                BankId = searchModel.BankId
            };

            var entities = _bankAccountService.Get(searchContext);
            var models = entities.Select(s =>
            {
                var m = s.ToModel();
                m.Bank = new BankModel
                {
                    Id = s.BankId,
                    Name = s.Bank.GetLocalized(o => o.Name, searchModel.LanguageId)
                };
                m.BankBranch = s.BankBranch == null ? null : new BankBranchModel
                {
                    Id = s.BankBranch.Id,
                    Name = s.BankBranch.GetLocalized(x => x.Name, searchModel.LanguageId),
                };
                m.Currency = s.Currency == null ? null : new CurrencyModel
                {
                    Id = s.Currency.Id,
                    Name = s.Currency.GetLocalized(x => x.Name, searchModel.LanguageId),
                };
                return m;
            });

            return Ok(new IChibaResult
            {
                data = new LoadResult
                {
                    data = models,
                    totalCount = entities.TotalCount
                }
            });
        }

        [Route("get-all")]
        [HttpGet]
        public IActionResult GetAll(bool showHidden = false)
        {
            var bankAccounts = _bankAccountService.GetAll(showHidden);

            var models = bankAccounts.Select(p =>
            {
                var m = p.ToModel();
                return m;
            });
            return Ok(new IChibaResult
            {
                data = models
            });
        }

        #endregion

        #region Helpers



        #endregion

        #region Utilities

        private void PrepareModel(BankAccountModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.SelectBanks = _bankService.GetAll(true)
                .Where(w => w.Active || w.Id == model.BankId)
                .Select(s => new BankModel
                {
                    Id = s.Id,
                    Code = s.Code,
                    Name = s.GetLocalized(x => x.Name)
                }).ToList();

            model.SelectCurrencies = _currencyService.GetAll().Where(p => p.Published || p.Id == model.CurrencyId)
                .Select(p => new CurrencyModel
                {
                    Id = p.Id,
                    CurrencyCode = p.CurrencyCode,
                    Name = p.GetLocalized(x => x.Name),
                    DisplayLocale = p.DisplayLocale,
                    Rate = p.Rate
                }).ToList();
        }

        #endregion
    }
}
