using DevExtreme.AspNet.Data.ResponseModel;
using DevExtreme.AspNet.Mvc;
using IChiba.Core.Domain;
using IChiba.Core.Domain.Master;
using IChiba.Services.Common;
using IChiba.Services.Localization;
using IChiba.Services.Master;
using IChiba.SharedMvc;
using IChiba.SharedMvc.Models.Master;
using IChiba.Web.Framework.Controllers;
using IChiba.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IChiba.Api.Master.Controllers
{
    [Route("shipper")]
    [ApiController]
    public class ShipperController : AdminControllerBase
    {
        #region Fields

        private readonly IShipperService _shipperService;
        private readonly IPaymentTermService _paymentTermService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ISPAddressService _addressService;
        private readonly GetDataSettings _getDataSettings;

        #endregion

        #region Ctor

        public ShipperController(
            IShipperService shipperService,
            IPaymentTermService paymentTermService,
            ISPAddressService addressService,
            ILanguageService languageService,
            GetDataSettings getDataSettings,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService)
        {
            _shipperService = shipperService;
            _paymentTermService = paymentTermService;
            _languageService = languageService;
            _localizationService = localizationService;
            _localizedEntityService = localizedEntityService;
            _addressService = addressService;
            _getDataSettings = getDataSettings;
        }

        #endregion

        #region Methods

        [Route("create")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = await Task.FromResult(new ShipperModel());

            // Locales
            AddLocales(_languageService, model.Locales, model.LocaleLabels,
                (labels, languageId) =>
                {
                    var label = _localizationService.GetResource("Admin.Shippers.Fields.Name", languageId);
                    labels.Add(nameof(ShipperLocalizedModel.Name), label);
                });

            PrepareModel(model);

            return Ok(new IChibaResult
            {
                data = model
            });
        }

        [Route("create")]
        [HttpPost]
        public async Task<IActionResult> Create(ShipperModel model)
        {
            if (!ModelState.IsValid)
                return InvalidModelResult();

            if (await _shipperService.ExistsAsync(model.Code))
            {
                return Ok(new IChibaResult
                {
                    success = false,
                    message = string.Format(
                        _localizationService.GetResource("Common.Notify.AlreadyExist"),
                        _localizationService.GetResource("Admin.Shippers.Fields.Code"))
                });
            }

            var entity = model.ToEntity();
            entity.Code = model.Code;

            await _shipperService.InsertAsync(entity);

            // Locales
            UpdateLocales(entity, model);

            return Ok(new IChibaResult
            {
                message = string.Format(
                    _localizationService.GetResource("Common.Notify.Added"),
                    _localizationService.GetResource("Common.Shipper"))
            });
        }

        [Route("edit")]
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var entity = await _shipperService.GetByIdAsync(id);
            if (entity == null)
                return Ok(new IChibaResult
                {
                    success = false,
                    message = string.Format(
                        _localizationService.GetResource("Common.Notify.DoesNotExist"),
                        _localizationService.GetResource("Common.Shipper"))
                });

            var model = entity.ToModel();

            // Locales
            AddLocales(_languageService, model.Locales, model.LocaleLabels,
                (labels, languageId) =>
                {
                    var label = _localizationService.GetResource("Admin.Shippers.Fields.Name", languageId);
                    labels.Add(nameof(Shipper.Name), label);
                },
                (locale, languageId) =>
                {
                    locale.Name = entity.GetLocalized(x => x.Name, languageId, false, false);
                });

            PrepareModel(model);

            return Ok(new IChibaResult
            {
                data = model
            });
        }

        [Route("edit")]
        [HttpPost]
        public async Task<IActionResult> Edit(ShipperModel model)
        {
            ModelState.Remove("Code");
            if (!ModelState.IsValid)
                return InvalidModelResult();

            var entity = await _shipperService.GetByIdAsync(model.Id);
            if (entity == null)
                return Ok(new IChibaResult
                {
                    success = false,
                    message = string.Format(
                        _localizationService.GetResource("Common.Notify.DoesNotExist"),
                        _localizationService.GetResource("Common.Shipper"))
                });

            entity = model.ToEntity(entity);

            await _shipperService.UpdateAsync(entity);

            // Locales
            UpdateLocales(entity, model);

            return Ok(new IChibaResult
            {
                message = string.Format(
                    _localizationService.GetResource("Common.Notify.Updated"),
                    _localizationService.GetResource("Common.Shipper"))
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

            await _shipperService.DeletesAsync(ids);

            return Ok(new IChibaResult
            {
                message = string.Format(
                    _localizationService.GetResource("Common.Notify.Deleted"),
                    _localizationService.GetResource("Common.Shipper"))
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

            await _shipperService.ActivatesAsync(model.Ids, model.Active);

            return Ok(new IChibaResult
            {
                message = model.Active
                    ? string.Format(_localizationService.GetResource("Common.Notify.Activated"), _localizationService.GetResource("Common.Shipper"))
                    : string.Format(_localizationService.GetResource("Common.Notify.Deactivated"), _localizationService.GetResource("Common.Shipper"))
            });
        }

        #endregion

        #region Lists

        [Route("get")]
        [HttpGet]
        public IActionResult Get([FromQuery] DataSourceLoadOptions loadOptions, [FromQuery] ShipperSearchModel searchModel)
        {
            var searchContext = new ShipperSearchContext
            {
                Keywords = searchModel.Keywords,
                Status = (int)searchModel.Status,
                PageIndex = loadOptions.Skip / loadOptions.Take,
                PageSize = loadOptions.Take,
                LanguageId = searchModel.LanguageId,
                //PaymentTermId = searchModel.pa
            };

            var entities = _shipperService.Get(searchContext);
            var models = entities.Select(s =>
            {
                var m = s.ToModel();
                m.Name = s.GetLocalized(x => x.Name, searchContext.LanguageId);
                m.PaymentTerm = s.PaymentTerm == null ? null : new PaymentTermModel
                {
                    Id = s.PaymentTerm.Id,
                    Name = s.PaymentTerm.GetLocalized(x => x.Name, searchContext.LanguageId),
                    Code = s.PaymentTerm.Code
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
            var shippers = _shipperService.GetAll(showHidden);

            var models = shippers.Select(p =>
            {
                var m = p.ToModel();
                m.Name = p.GetLocalized(x => x.Name);
                return m;
            });
            return Ok(new IChibaResult
            {
                data = models
            });
        }

        [Route("get-by-payment-term")]
        [HttpGet]
        public IActionResult GetByPaymentTermId(string paymentTermId, bool showHidden = false)
        {
            var shippers = _shipperService.GetByPaymentTermId(paymentTermId, showHidden);

            var models = shippers.Select(p =>
            {
                var m = p.ToModel();
                m.Name = p.GetLocalized(x => x.Name);
                return m;
            });
            return Ok(new IChibaResult
            {
                data = models
            });
        }

        #endregion

        #region Selector

        [Route("get-top-select")]
        [HttpGet]
        public async Task<IActionResult> GetTop_Select([FromQuery] ShipperSearchModel searchModel)
        {
            var searchContext = new ShipperSearchContext
            {
                Keywords = searchModel.Keywords,
                Status = (int)searchModel.Status,
                PageSize = searchModel.PageSize
            };

            var entities = await _shipperService.GetTop_SelectAsync(searchContext);
            var models = entities.Select(s =>
            {
                var m = s.ToModel();
                return m;
            });

            return Ok(new IChibaResult
            {
                data = models
            });
        }

        [Route("get-addresses-top-select")]
        [HttpGet]
        public async Task<IActionResult> GetAddresses_TopSelect(string shipperId)
        {
            var entities = await _addressService.GetByEntity_SelectAsync(new Services.Common.SPAddressSearchModel
            {
                EntityType = EntityType.Shipper,
                EntityId = shipperId,
                Keywords = null,
                Take = _getDataSettings.MaxTopSize
            });
            var models = entities
                .Select(s =>
                {
                    var m = s.ToModel();
                    return m;
                });

            return Ok(new IChibaResult
            {
                data = models
            });
        }

        [Route("get-addresses-select")]
        [HttpGet]
        public async Task<IActionResult> GetAddresses_Select([FromQuery] Services.Common.SPAddressSearchModel searchModel)
        {
            searchModel.Take = _getDataSettings.MaxTopSize;
            searchModel.EntityType = EntityType.Shipper;
            var entities = await _addressService.GetByEntity_SelectAsync(searchModel);
            var models = entities.Select(s =>
            {
                var m = s.ToModel();
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

        private void UpdateLocales(Shipper entity, ShipperModel model)
        {
            model.Locales.Each(localized =>
            {
                _localizedEntityService.SaveLocalizedValue(entity, x => x.Name, localized.Name, localized.LanguageId);
            });
        }

        private void PrepareModel(ShipperModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.SelectPaymentTerms = _paymentTermService.GetAll(true)
                .Where(w => w.Active || w.Id == model.PaymentTermId)
                .Select(s => new PaymentTermModel
                {
                    Id = s.Id,
                    Code = s.Code,
                    Name = s.GetLocalized(x => x.Name)
                }).ToList();
        }

        #endregion
    }
}
