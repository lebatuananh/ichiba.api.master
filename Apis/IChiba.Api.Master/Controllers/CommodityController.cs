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
using Microsoft.AspNetCore.Mvc;

namespace IChiba.Api.Master.Controllers
{
    [ApiController]
    [Route("commodity")]
    public class CommodityController : AdminControllerBase
    {
        #region Fields

        private readonly ICommodityService _commodityService;
        private readonly ICommodityGroupService _commodityGroupService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;

        #endregion

        #region Ctor

        public CommodityController(
            ICommodityService commodityService,
            ICommodityGroupService commodityGroupService,
            ILanguageService languageService,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService)
        {
            _commodityService = commodityService;
            _commodityGroupService = commodityGroupService;
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
            var model = await Task.FromResult(new CommodityModel());

            // Locales
            AddLocales(_languageService, model.Locales, model.LocaleLabels,
                (labels, languageId) =>
                {
                    var label = _localizationService.GetResource("Admin.Commodities.Fields.Name", languageId);
                    var description = _localizationService.GetResource("Common.Fields.Description", languageId);
                    labels.Add(nameof(Commodity.Name), label);
                    labels.Add(nameof(Commodity.Description), description);
                });

            PrepareModel(model);

            return Ok(new IChibaResult
            {
                data = model
            });
        }

        [Route("create")]
        [HttpPost]
        public async Task<IActionResult> Create(CommodityModel model)
        {
            if (!ModelState.IsValid)
                return InvalidModelResult();

            if (await _commodityService.ExistsAsync(model.Code))
                return Ok(new IChibaResult
                {
                    success = false,
                    message = string.Format(
                        _localizationService.GetResource("Common.Notify.AlreadyExist"),
                        _localizationService.GetResource("Admin.Commodities.Fields.Code"))
                });

            var entity = model.ToEntity();
            entity.Code = model.Code;

            await _commodityService.InsertAsync(entity);

            // Locales
            UpdateLocales(entity, model);

            return Ok(new IChibaResult
            {
                message = string.Format(
                    _localizationService.GetResource("Common.Notify.Added"),
                    _localizationService.GetResource("Common.Commodity"))
            });
        }

        [Route("edit")]
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var entity = await _commodityService.GetByIdAsync(id);
            if (entity == null)
                return Ok(new IChibaResult
                {
                    success = false,
                    message = string.Format(
                        _localizationService.GetResource("Common.Notify.DoesNotExist"),
                        _localizationService.GetResource("Common.Commodity"))
                });

            var model = entity.ToModel();

            PrepareModel(model);

            // Locales
            AddLocales(_languageService, model.Locales, model.LocaleLabels,
                (labels, languageId) =>
                {
                    var label = _localizationService.GetResource("Admin.Commodities.Fields.Name", languageId);
                    var description = _localizationService.GetResource("Common.Fields.Description", languageId);
                    labels.Add(nameof(Commodity.Name), label);
                    labels.Add(nameof(Commodity.Description), description);
                },
                (locale, languageId) =>
                {
                    locale.Name = entity.GetLocalized(x => x.Name, languageId, false, false);
                    locale.Description = entity.GetLocalized(x => x.Description, languageId, false, false);
                });

            return Ok(new IChibaResult
            {
                data = model
            });
        }

        [Route("edit")]
        [HttpPost]
        public async Task<IActionResult> Edit(CommodityModel model)
        {
            ModelState.Remove("Code");
            if (!ModelState.IsValid)
                return InvalidModelResult();

            var entity = await _commodityService.GetByIdAsync(model.Id);
            if (entity == null)
                return Ok(new IChibaResult
                {
                    success = false,
                    message = string.Format(
                        _localizationService.GetResource("Common.Notify.DoesNotExist"),
                        _localizationService.GetResource("Common.Commodity"))
                });

            entity = model.ToEntity(entity);

            await _commodityService.UpdateAsync(entity);

            // Locales
            UpdateLocales(entity, model);

            return Ok(new IChibaResult
            {
                message = string.Format(
                    _localizationService.GetResource("Common.Notify.Updated"),
                    _localizationService.GetResource("Common.Commodity"))
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

            await _commodityService.DeletesAsync(ids);

            return Ok(new IChibaResult
            {
                message = string.Format(
                    _localizationService.GetResource("Common.Notify.Deleted"),
                    _localizationService.GetResource("Common.Commodity"))
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

            await _commodityService.ActivatesAsync(model.Ids, model.Active);

            return Ok(new IChibaResult
            {
                message = model.Active
                    ? string.Format(_localizationService.GetResource("Common.Notify.Activated"), _localizationService.GetResource("Common.Commodity"))
                    : string.Format(_localizationService.GetResource("Common.Notify.Deactivated"), _localizationService.GetResource("Common.Commodity"))
            });
        }

        #endregion

        #region Lists

        [Route("get")]
        [HttpGet]
        public IActionResult Get([FromQuery] DataSourceLoadOptions loadOptions, [FromQuery] CommoditySearchModel searchModel)
        {
            var searchContext = new CommoditySearchContext
            {
                Keywords = searchModel.Keywords,
                Status = (int)searchModel.Status,
                PageIndex = loadOptions.Skip / loadOptions.Take,
                PageSize = loadOptions.Take,
                LanguageId = searchModel.LanguageId
            };

            var entities = _commodityService.Get(searchContext);
            var models = entities.Select(s =>
            {
                var m = s.ToModel();
                m.Name = s.GetLocalized(x => x.Name, searchContext.LanguageId);
                m.CommodityGroup = s.CommodityGroup == null ? null : new CommodityGroupModel
                {
                    Id = s.CommodityGroup.Id,
                    Code = s.CommodityGroup.Code,
                    Name = s.CommodityGroup.GetLocalized(x => x.Name, searchModel.LanguageId)
                };
                m.Description = s.GetLocalized(x => x.Description, searchModel.LanguageId);
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

        private void UpdateLocales(Commodity entity, CommodityModel model)
        {
            model.Locales.Each(localized =>
            {
                _localizedEntityService.SaveLocalizedValue(entity, x => x.Name, localized.Name, localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(entity, x => x.Description, localized.Description, localized.LanguageId);
            });
        }

        private void PrepareModel(CommodityModel model)
        {
            model.SelectCommodityGroups = _commodityGroupService.GetAll()
                .Where(w => w.Active || w.Id == model.CommodityGroupId)
                .Select(p => new CommodityGroupModel
                {
                    Id = p.Id,
                    Code = p.Code,
                    Name = p.GetLocalized(x => x.Name)
                }).ToList();
        }

        #endregion
    }
}
