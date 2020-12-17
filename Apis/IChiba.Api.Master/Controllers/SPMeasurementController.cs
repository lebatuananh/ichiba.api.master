using DevExtreme.AspNet.Data.ResponseModel;
using DevExtreme.AspNet.Mvc;
using IChiba.Core.Domain.Master;
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
    [Route("sp-measurement")]
    [ApiController]
    public class SPMeasurementController : AdminControllerBase
    {
        #region Fields

        private readonly ISPMeasurementService _customerService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;

        #endregion

        #region Ctor

        public SPMeasurementController(
            ISPMeasurementService customerService,
            IPaymentTermService paymentTermService,
            ILanguageService languageService,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService)
        {
            _customerService = customerService;
            _languageService = languageService;
            _localizationService = localizationService;
            _localizedEntityService = localizedEntityService;
        }

        #endregion

        #region Methods

        [Route("create")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = await Task.FromResult(new SPMeasurementModel());

            // Locales
            AddLocales(_languageService, model.Locales, model.LocaleLabels,
                (labels, languageId) =>
                {
                    var label = _localizationService.GetResource("Admin.SPMeasurements.Fields.Name", languageId);
                    labels.Add(nameof(SPMeasurementLocalizedModel.Name), label);
                });

            return Ok(new IChibaResult
            {
                data = model
            });
        }

        [Route("create")]
        [HttpPost]
        public async Task<IActionResult> Create(SPMeasurementModel model)
        {
            if (!ModelState.IsValid)
                return InvalidModelResult();

            if (await _customerService.ExistsAsync(model.Code))
            {
                return Ok(new IChibaResult
                {
                    success = false,
                    message = string.Format(
                        _localizationService.GetResource("Common.Notify.AlreadyExist"),
                        _localizationService.GetResource("Admin.SPMeasurements.Fields.Code"))
                });
            }

            var entity = model.ToEntity();
            entity.Code = model.Code;

            await _customerService.InsertAsync(entity);

            // Locales
            UpdateLocales(entity, model);

            return Ok(new IChibaResult
            {
                message = string.Format(
                    _localizationService.GetResource("Common.Notify.Added"),
                    _localizationService.GetResource("Common.SPMeasurement"))
            });
        }

        [Route("edit")]
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var entity = await _customerService.GetByIdAsync(id);
            if (entity == null)
                return Ok(new IChibaResult
                {
                    success = false,
                    message = string.Format(
                        _localizationService.GetResource("Common.Notify.DoesNotExist"),
                        _localizationService.GetResource("Common.SPMeasurement"))
                });

            var model = entity.ToModel();

            // Locales
            AddLocales(_languageService, model.Locales, model.LocaleLabels,
                (labels, languageId) =>
                {
                    var label = _localizationService.GetResource("Admin.SPMeasurements.Fields.Name", languageId);
                    labels.Add(nameof(SPMeasurement.Name), label);
                },
                (locale, languageId) =>
                {
                    locale.Name = entity.GetLocalized(x => x.Name, languageId, false, false);
                });

            return Ok(new IChibaResult
            {
                data = model
            });
        }

        [Route("edit")]
        [HttpPost]
        public async Task<IActionResult> Edit(SPMeasurementModel model)
        {
            ModelState.Remove("Code");
            if (!ModelState.IsValid)
                return InvalidModelResult();

            var entity = await _customerService.GetByIdAsync(model.Id);
            if (entity == null)
                return Ok(new IChibaResult
                {
                    success = false,
                    message = string.Format(
                        _localizationService.GetResource("Common.Notify.DoesNotExist"),
                        _localizationService.GetResource("Common.SPMeasurement"))
                });

            entity = model.ToEntity(entity);

            await _customerService.UpdateAsync(entity);

            // Locales
            UpdateLocales(entity, model);

            return Ok(new IChibaResult
            {
                message = string.Format(
                    _localizationService.GetResource("Common.Notify.Updated"),
                    _localizationService.GetResource("Common.SPMeasurement"))
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

            await _customerService.DeletesAsync(ids);

            return Ok(new IChibaResult
            {
                message = string.Format(
                    _localizationService.GetResource("Common.Notify.Deleted"),
                    _localizationService.GetResource("Common.SPMeasurement"))
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

            await _customerService.ActivatesAsync(model.Ids, model.Active);

            return Ok(new IChibaResult
            {
                message = model.Active
                    ? string.Format(_localizationService.GetResource("Common.Notify.Activated"), _localizationService.GetResource("Common.SPMeasurement"))
                    : string.Format(_localizationService.GetResource("Common.Notify.Deactivated"), _localizationService.GetResource("Common.SPMeasurement"))
            });
        }

        #endregion

        #region Lists

        [Route("get")]
        [HttpGet]
        public IActionResult Get([FromQuery] DataSourceLoadOptions loadOptions, [FromQuery] SPMeasurementSearchModel searchModel)
        {
            var searchContext = new SPMeasurementSearchContext
            {
                Keywords = searchModel.Keywords,
                Status = (int)searchModel.Status,
                PageIndex = loadOptions.Skip / loadOptions.Take,
                PageSize = loadOptions.Take,
                LanguageId = searchModel.LanguageId,
                //PaymentTermId = searchModel.pa
            };

            var entities = _customerService.Get(searchContext);
            var models = entities.Select(s =>
            {
                var m = s.ToModel();
                m.Name = s.GetLocalized(x => x.Name, searchContext.LanguageId);
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
            var customers = _customerService.GetAll(showHidden);

            var models = customers.Select(p =>
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

        #region Helpers



        #endregion

        #region Utilities

        private void UpdateLocales(SPMeasurement entity, SPMeasurementModel model)
        {
            model.Locales.Each(localized =>
            {
                _localizedEntityService.SaveLocalizedValue(entity, x => x.Name, localized.Name, localized.LanguageId);
            });
        }

        #endregion
    }
}
