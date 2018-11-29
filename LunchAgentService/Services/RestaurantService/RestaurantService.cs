﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using log4net;

namespace LunchAgentService.Services.RestaurantService
{
    public class RestaurantService
    {
        public ILog Log { get; }

        private RestaurantServiceSetting _serviceSetting;

        public RestaurantServiceSetting ServiceSetting
        {
            get
            {
                lock (_serviceSetting)
                {
                    return _serviceSetting.Clone();
                }
            }
            set
            {
                lock (_serviceSetting)
                {
                    _serviceSetting = value;
                }
            }
        }

        public RestaurantService(RestaurantServiceSetting restaurantSettings, ILog log)
        {
            Log = log;
            _serviceSetting = restaurantSettings;
        }

        public List<RestaurantMenu> GetMenus()
        {
            var result = new List<RestaurantMenu>();

            var document = new HtmlDocument();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var restaurants = ServiceSetting.Restaurants;

            foreach (var setting in restaurants)
            {
                using (var client = new WebClient())
                {
                    try
                    {
                        var data = setting.Url.Contains("makalu")
                            ? Encoding.UTF8.GetString(client.DownloadData(setting.Url))
                            : Encoding.GetEncoding(1250).GetString(client.DownloadData(setting.Url));

                        document.LoadHtml(data);
                    }
                    catch (Exception e)
                    {
                        Log.Debug(e);
                    }
                }

                try
                {
                    var parsedMenu = setting.Url.Contains("makalu")
                        ? ParseMenuFromMakalu(document.DocumentNode)
                        : ParseMenuFromMenicka(document.DocumentNode);

                    result.Add(new RestaurantMenu()
                    {
                        Restaurant = setting,
                        Items = parsedMenu
                    });
                }
                catch (Exception e)
                {
                    Log.Debug(e);
                }
            }


            return result;
        }

        private static List<RestaurantMenuItem> ParseMenuFromMenicka(HtmlNode todayMenu)
        {
            var result = new List<RestaurantMenuItem>();

            var foodMenus = todayMenu.SelectNodes(".//tr")
                .Where(node => node.GetClasses().Contains("soup") || node.GetClasses().Contains("main"));

            foreach (var food in foodMenus)
            {
                var item = new RestaurantMenuItem();

                if (food.GetClasses().Contains("soup"))
                {
                    item.FoodType = FoodType.Soup;
                    item.Description = Regex.Replace(food.SelectNodes(".//td").Single(x => x.GetClasses().Contains("food")).InnerText, "\\d+.?", string.Empty);
                    item.Price = food.SelectNodes(".//td").Single(x => x.GetClasses().Contains("prize")).InnerText;
                }
                else
                {
                    item.FoodType = FoodType.Main;
                    item.Description = Regex.Replace(food.SelectNodes(".//td").Single(x => x.GetClasses().Contains("food")).InnerText, "\\d+.?", string.Empty);
                    item.Price = food.SelectNodes(".//td").Single(x => x.GetClasses().Contains("prize")).InnerText;
                    item.Index = food.SelectNodes(".//td").Single(x => x.GetClasses().Contains("no")).InnerText;
                }
                result.Add(item);
            }

            return result;
        }

        private static List<RestaurantMenuItem> ParseMenuFromMakalu(HtmlNode todayMenu)
        {
            var result = new List<RestaurantMenuItem>();

            var todayString = GetTodayInCzech();

            var todayNode = string.Join(" ", todayMenu.SelectNodes(".//div[contains(@class,TJStrana)]").Where(x => x.GetClasses().Contains("TJStrana")).Select(x => x.InnerHtml));

            var start = todayNode.IndexOf(todayString) + 13;

            var end = todayNode.Substring(start, todayNode.Length - start).IndexOf("Mix denn");

            var body = todayNode.Substring(start, end);

            var soupString = Regex.Match(body, "Polévky:<br>.+?(?=(1.))");

            foreach (Match item in Regex.Matches(soupString.Value, "[r]>.+?(?=<[bs])"))
            {
                var newItem = new RestaurantMenuItem
                {
                    FoodType = FoodType.Soup,
                    Description = item.Value.Substring(2)
                };

                result.Add(newItem);
            }

            var matches = Regex.Matches(body, "<b>.+?<\\/b>");

            foreach (Match match in matches)
            {
                var item = new RestaurantMenuItem
                {
                    FoodType = FoodType.Main,
                    Price = Regex.Match(match.Value, "(?='>)(.*)(?=</span)").Value.Substring(2),
                    Description = Regex.Match(match.Value, "(?=<b>)(.+?)(?=<span)").Value.Substring(3) + "  "
                };

                result.Add(item);
            }

            return result;
        }

        private static string GetTodayInCzech()
        {
            switch (DateTime.Today.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    return "Pondělí";
                case DayOfWeek.Tuesday:
                    return "Úterý";
                case DayOfWeek.Wednesday:
                    return "Středa";
                case DayOfWeek.Thursday:
                    return "Čtvrtek";
                case DayOfWeek.Friday:
                    return "Pátek";
#if DEBUG
                case DayOfWeek.Saturday:
                case DayOfWeek.Sunday:
                    return "Pátek";
#endif
            }

            return string.Empty;
        }
    }
}