﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LunchAgentService.Helpers;
using LunchAgentService.Helpers.Entities;
using Microsoft.AspNetCore.Mvc;

namespace LunchAgentService.Controllers
{
    [Route("api")]
    public class ConfigurationController : Controller
    {
        private SlackHelper _slackHelper;
        private RestaurantHelper _restaurantHelper;

        public ConfigurationController(RestaurantHelper restaurantHelper, SlackHelper slackHelper)
        {
            _restaurantHelper = restaurantHelper;
            _slackHelper = slackHelper;
        }

        [HttpGet]
        [Route("getslacksetting")]
        public IActionResult GetSlackSetting()
        {
            return new EmptyResult();

            //var usernamePassword = GetPasswordFromRequest();

            //if (usernamePassword != "user:password")
            //{
            //    return new UnauthorizedResult();
            //}

            //return new JsonResult(_slackHelper.SlackConfiguration);
        }


        [HttpGet]
        [Route("getretaurantsetting")]
        public IActionResult GetRestaurantsSetting()
        {
            return new EmptyResult();

            //var usernamePassword = GetPasswordFromRequest();

            //if (usernamePassword != "user:password")
            //{
            //    return new UnauthorizedResult();
            //}

            //return new JsonResult(_restaurantHelper.RestaurantSettingses);
        }

        [HttpPost]
        [Route("setslacksetting")]
        public IActionResult SetSlackSettings([FromBody]SlackSetting slackSetting)
        {
            return new EmptyResult();

            //var usernamePassword = GetPasswordFromRequest();

            //if (usernamePassword != "user:password")
            //{
            //    return new UnauthorizedResult();
            //}

            //return new JsonResult(slackSetting);
        }

        [HttpPost]
        [Route("setrestaurantsetting")]
        public IActionResult SetRestaurantSettings([FromBody]RestaurantSettings[] restaurantSettingses)
        {
            return new EmptyResult();

            //var usernamePassword = GetPasswordFromRequest();

            //if (usernamePassword != "user:password")
            //{
            //    return new UnauthorizedResult();
            //}

            //return new JsonResult(restaurantSettingses);
        }

        [HttpPost]
        [Route("setrestaurantsetting")]
        public IActionResult ForceUpdate()
        {
            var menus = _restaurantHelper.GetMenus();

            _slackHelper.UpdateMenu(menus);

            return Ok();
        }

        private string GetPasswordFromRequest()
        {
            var authHeader = Request.Headers["Authorization"].First();

            var encodedUsernamePassword = authHeader.Substring("Basic ".Length).Trim();

            var encoding = Encoding.GetEncoding("iso-8859-1");
            var usernamePassword = encoding.GetString(Convert.FromBase64String(encodedUsernamePassword));
            return usernamePassword;
        }
    }
}
