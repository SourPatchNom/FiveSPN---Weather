using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGHMatti.Http;
using FiveSpnLoggerServerLibrary;
using FiveSpnLoggerServerLibrary.Classes;
using FiveSpnLoggerServerLibrary.Enums;
using Newtonsoft.Json;

namespace WeatherSyncServer.Classes
{
    public class WeatherApiRequest
    {
        public static WeatherApiRequest Instance { get; } = new WeatherApiRequest();

        static WeatherApiRequest()
        {

        }
        private WeatherApiRequest()
        {
            ServerLogger.SendServerLogMessage(new LogMessage("WeatherSync",LogMessageSeverity.Info,"WeatherAPI request system initialized."));
        }
        
        /// <summary>
        /// Requests the weather for a given location, either in zip or city name.
        /// </summary>
        /// <param name="location">The value for the location requested.</param>
        /// <param name="apiKey">The api key for WeatherAPI</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<string> GetGtaWeatherForLocation(string location, string apiKey)
        {
            string weatherResult = "CLEAR";
            if (apiKey == "NONE")
            {
                ServerLogger.SendServerLogMessage(new LogMessage("WeatherSync",LogMessageSeverity.Info,"The WeatherAPI key not set, unable to get weather! Set the WeatherAPI key in the resource manifest!"));
                return "CLEAR";
            }
            await Task.Delay(1000);
            ServerLogger.SendServerLogMessage(new LogMessage("WeatherSync",LogMessageSeverity.Info,$"Requesting weather for {location}!"));
            WeatherApi.WeatherResponseData responseData = new WeatherApi.WeatherResponseData();
            try
            {
                var request = new Request();
                string requestString = "http://api.openweathermap.org/data/2.5/weather?q=" + location + "&appid=" + apiKey;
                var response = await request.Http(requestString);
                var json = response.content;
                //dynamic latest = JObject.Parse(json);
                responseData = JsonConvert.DeserializeObject<WeatherApi.WeatherResponseData>(json);
                if (responseData == null) throw new Exception("Weather result is NULL");
                if (responseData.weather.Count != 0)
                {
                    weatherResult = WeatherApi.GetGtaWeatherFromId(responseData.weather.First().id);
                    ServerLogger.SendServerLogMessage(new LogMessage("WeatherSync",LogMessageSeverity.Info,$"Weather received for {location}! Setting server weather to {weatherResult}"));
                } else throw new Exception("Weather result contains no weather data");
            }
            catch (Exception e)
            {
                ServerLogger.SendServerLogMessage(new LogMessage("WeatherSync",LogMessageSeverity.Error,$"Unable to process WeatherAPI request!"));
                ServerLogger.SendServerLogMessage(new LogMessage("WeatherSync",LogMessageSeverity.Error,e.Message));
                return "CLEAR";
            }
            return weatherResult;
        }
    }
    
    public static class WeatherApi
    {

        /// <summary>
        /// Class used to deserialize the weather from the weather api
        /// </summary>
        public class WeatherResponseData
        {
            public class Weather
            {
                public int id;
                public string main;
                public string description;
            }

            public List<Weather> weather;
        }
        
        /// <summary>
        /// Returns the GTA weather string from the weather api int
        /// </summary>
        /// <param name="id">The weather number from the weather api.</param>
        /// <returns></returns>
        public static string GetGtaWeatherFromId(int id)
        {
            switch (id)
            {
                case 200: //thunderstorm light rain
                case 201: //thunderstorm with rain
                case 202: //thunderstorm with heavy rain
                case 210: //light thunderstorm
                case 211: //thunderstorm
                case 212: //heavy thunderstorm
                case 221: //ragged thunderstorm
                case 230: //thunderstorm with light drizzle
                case 231: //thunderstorm with drizzle
                case 232: //thunderstorm with heavy drizzle
                case 960: //storm
                case 961: //violent storm
                    return "THUNDER";
                case 300: //light intensity drizzle
                case 301: //drizzle
                case 302: //heavy intensity drizzle
                case 310: //light intensity drizzle rain
                case 311: //drizzle rain
                case 312: //heavy intensity drizzle rain
                case 313: //shower rain and drizzle
                case 314: //heavy shower rain and drizzle
                case 321: //shower drizzle
                case 500: //light rain
                case 501: //moderate rain
                case 502: //heavy intensity rain
                case 503: //very heavy rain
                case 504: //extreme rain
                case 511: //freezing rain
                case 520: //light intensity shower rain
                case 521: //shower rain
                case 522: //heavy intensity shower rain
                case 531: //ragged shower rain
                    return "RAIN";
                case 600: //light snow
                case 615: //light rain and snow
                case 620: //light shower snow
                    return "SNOWLIGHT";
                case 601: //snow
                case 611: //sleet
                case 612: //shower sleet
                case 616: //rain and snow
                case 621: //shower snow
                    return "SNOW";
                case 602: //heavy snow
                case 622: //heavy shower snow
                    return "BLIZZARD";
                case 762: //volcanic ash
                    return "FOGGY"; //idk...
                case 781: //tornado
                case 900:
                    return "THUNDER";
                case 800: //clear sky
                    return "EXTRASUNNY";
                case 801: //few-
                case 802: //scattered-
                case 803: //broken-
                    return "CLOUDS";
                case 804: //overcast clouds
                    return "OVERCAST";
                case 901: //tropical storm
                case 902: //hurricane
                case 962:
                    return "THUNDER";
                case 741: //fog
                    return "FOGGY";
                case 701: //mist
                case 711: //smoke
                case 721: //haze
                    return "SMOG";
                default:
                    return "CLEAR";
            }
        }
    }
}