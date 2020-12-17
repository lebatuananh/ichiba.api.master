using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevExtreme.AspNet.Data.ResponseModel;
using DevExtreme.AspNet.Mvc;
using IChiba.Core.Domain.Master;
using IChiba.Services.Localization;
using IChiba.Services.Master;
using IChiba.SharedMvc;
using IChiba.SharedMvc.Models.Master;
using IChiba.Web.Framework.Controllers;
using IChiba.Web.Framework.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IChiba.Api.Master.Controllers
{
    [Route("district")]
    [ApiController]
    public class DistrictController : AdminControllerBase
    {

        #region Fields

        private readonly IStateProvinceService _stateProvinceService;
        private readonly ICountryService _countryService;
        private readonly IDistrictService _districtService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;

        #endregion

        #region Ctor

        public DistrictController(
            IDistrictService districtService,
            IStateProvinceService stateProvinceService,
            ILanguageService languageService,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService,
            ICountryService countryService)
        {
            _districtService = districtService;
            _languageService = languageService;
            _localizationService = localizationService;
            _localizedEntityService = localizedEntityService;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
        }

        #endregion

        #region Methods

        [Route("create")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new DistrictModel();

            // Locales
            AddLocales(_languageService, model.Locales, model.LocaleLabels,
                (labels, languageId) =>
                {
                    var label = _localizationService.GetResource("Admin.Districts.Fields.Name", languageId);
                    labels.Add(nameof(DistrictLocalizedModel.Name), label);
                });

            PrepareModel(model);

            return Ok(new IChibaResult
            {
                data = model
            });
        }

        [Route("create")]
        [HttpPost]
        public async Task<IActionResult> Create(DistrictModel model)
        {
            if (!ModelState.IsValid)
                return InvalidModelResult();

            if (await _districtService.ExistsAsync(model.Code))
                return Ok(new IChibaResult
                {
                    success = false,
                    message = string.Format(
                        _localizationService.GetResource("Common.Notify.AlreadyExist"),
                        _localizationService.GetResource("Admin.Districts.Fields.Code"))
                });

            var entity = model.ToEntity();
            entity.Code = model.Code;

            await _districtService.InsertAsync(entity);

            // Locales
            UpdateLocales(entity, model);

            return Ok(new IChibaResult
            {
                message = string.Format(
                    _localizationService.GetResource("Common.Notify.Added"),
                    _localizationService.GetResource("Common.District"))
            });
        }

        [Route("edit")]
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var entity = await _districtService.GetByIdAsync(id);
            if (entity == null)
                return Ok(new IChibaResult
                {
                    success = false,
                    message = string.Format(
                        _localizationService.GetResource("Common.Notify.DoesNotExist"),
                        _localizationService.GetResource("Common.District"))
                });

            var model = entity.ToModel();

            // Locales
            AddLocales(_languageService, model.Locales, model.LocaleLabels,
                (labels, languageId) =>
                {
                    var label = _localizationService.GetResource("Admin.Districts.Fields.Name", languageId);
                    labels.Add(nameof(District.Name), label);
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
        public async Task<IActionResult> Edit(DistrictModel model)
        {
            ModelState.Remove("Code");
            if (!ModelState.IsValid)
                return InvalidModelResult();

            var entity = await _districtService.GetByIdAsync(model.Id);
            if (entity == null)
                return Ok(new IChibaResult
                {
                    success = false,
                    message = string.Format(
                        _localizationService.GetResource("Common.Notify.DoesNotExist"),
                        _localizationService.GetResource("Common.District"))
                });

            entity = model.ToEntity(entity);

            await _districtService.UpdateAsync(entity);

            // Locales
            UpdateLocales(entity, model);

            return Ok(new IChibaResult
            {
                message = string.Format(
                    _localizationService.GetResource("Common.Notify.Updated"),
                    _localizationService.GetResource("Common.District"))
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

            await _districtService.DeletesAsync(ids);

            return Ok(new IChibaResult
            {
                message = string.Format(
                    _localizationService.GetResource("Common.Notify.Deleted"),
                    _localizationService.GetResource("Common.District"))
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

            await _districtService.ActivatesAsync(model.Ids, model.Active);

            return Ok(new IChibaResult
            {
                message = model.Active
                    ? string.Format(_localizationService.GetResource("Common.Notify.Activated"), _localizationService.GetResource("Common.District"))
                    : string.Format(_localizationService.GetResource("Common.Notify.Deactivated"), _localizationService.GetResource("Common.District"))
            });
        }

        #endregion

        #region Lists

        [Route("get-by-state-province-id")]
        [HttpGet]
        public IActionResult GetByStateProvinceId(string stateProvinceId, bool showHidden = false)
        {
            var districts = _districtService.GetByStateProvinceId(stateProvinceId, showHidden);

            var models = districts.Select(p =>
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

        [Route("get")]
        [HttpGet]
        public IActionResult Get([FromQuery] DataSourceLoadOptions loadOptions, [FromQuery] DistrictSearchModel searchModel)
        {
            var searchContext = new DistrictSearchContext
            {
                Keywords = searchModel.Keywords,
                Status = (int)searchModel.Status,
                PageIndex = loadOptions.Skip / loadOptions.Take,
                PageSize = loadOptions.Take,
                LanguageId = searchModel.LanguageId,
                CountryId = searchModel.CountryId,
                StateProvinceId = searchModel.StateProvinceId
            };

            var entities = _districtService.Get(searchContext);
            var models = entities.Select(s =>
            {
                var m = s.ToModel();
                m.Name = s.GetLocalized(x => x.Name, searchContext.LanguageId);
                m.Country = new CountryModel
                {
                    Id = s.Country.Id,
                    Name = s.Country.GetLocalized(x => x.Name, searchContext.LanguageId),
                    Code = s.Country.Code
                };
                m.StateProvince = new StateProvinceModel
                {
                    Id = s.StateProvince.Id,
                    Name = s.StateProvince.GetLocalized(x => x.Name, searchContext.LanguageId),
                    Code = s.StateProvince.Code
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

        #endregion

        #region Helpers



        #endregion

        #region Utilities

        private void UpdateLocales(District entity, DistrictModel model)
        {
            model.Locales.Each(localized =>
            {
                _localizedEntityService.SaveLocalizedValue(entity, x => x.Name, localized.Name, localized.LanguageId);
            });
        }

        private void PrepareModel(DistrictModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.SelectCountries = _countryService.GetAll(true)
                .Where(w => w.Active || w.Id == model.CountryId)
                .Select(s => new CountryModel
                {
                    Id = s.Id,
                    Code = s.Code,
                    Name = s.GetLocalized(x => x.Name)
                }).ToList();

            if (!string.IsNullOrEmpty(model.CountryId))
            {
                model.SelectStateProvinces = _stateProvinceService.GetByCountryId(model.CountryId)
                    .Select(p => new StateProvinceModel
                    {
                        Id = p.Id,
                        Name = p.GetLocalized(x => x.Name),
                        Code = p.Code
                    }).ToList();
            }
        }

        #endregion
    }
}
