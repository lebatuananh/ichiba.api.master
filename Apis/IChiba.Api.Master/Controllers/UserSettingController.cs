using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IChiba.SharedMvc.Models.Master;
using IChiba.Web.Framework.Controllers;
using IChiba.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc;

namespace IChiba.Api.Master.Controllers
{
    [ApiController]
    [Route("user-setting")]
    public class UserSettingController : AdminControllerBase
    {
        #region Fields



        #endregion
        #region Ctor

        public UserSettingController()
        {

        }

        #endregion

        #region Methods

        [Route("get-language")]
        [HttpGet]
        public async Task<IActionResult> GetLanguage([FromQuery] string userId, [FromQuery] string appId)
        {
            return Ok(new IChibaResult
            {
                data = "e17f91cb-b023-483f-aa45-b1caca395ff3"
            });
        }

        [Route("update-language")]
        [HttpPost]
        public async Task<IActionResult> UpdateLanguage(UpdateLanguageModel model)
        {
            return Ok(new IChibaResult());
        }

        #endregion

        #region Lists



        #endregion

        #region Helpers



        #endregion

        #region Utilities



        #endregion
    }
}
