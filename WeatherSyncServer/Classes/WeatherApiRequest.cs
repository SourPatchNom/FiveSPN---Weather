using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using WeatherSyncServer.Enums;

namespace WeatherSyncServer.Classes
{
    public class WeatherApiRequest : BaseScript
    {
        public static WeatherApiRequest Instance { get; } = new WeatherApiRequest();

        static WeatherApiRequest()
        {

        }
        private WeatherApiRequest()
        {
            TriggerEvent("FiveSPN-ServerLogToServer", API.GetCurrentResourceName(),4,"WeatherAPI request system initialized.");
            //ServerLogger.SendServerLogMessage(new LogMessage("WeatherSync",LogMessageSeverity.Info,"WeatherAPI request system initialized."));
        }
        
        /// <summary>
        /// Requests the weather for a given location, either in zip or city name.
        /// </summary>
        /// <param name="location">The value for the location requested.</param>
        /// <param name="apiKey">The api key for WeatherAPI</param>
        /// <param name="weatherApiCallType"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<string> GetGtaWeatherForLocation(string location, string apiKey, WeatherApiCallType weatherApiCallType)
        {
            string weatherResult;
            if (apiKey == "NONE")
            {
                TriggerEvent("FiveSPN-ServerLogToServer", API.GetCurrentResourceName(),0,"The WeatherAPI key not set, unable to get weather! Set the WeatherAPI key in the resource manifest!");
                return "CLEAR";
            }
            await Delay(1000);
            TriggerEvent("FiveSPN-ServerLogToServer", API.GetCurrentResourceName(),4,$"Requesting weather for {location}!");
            try
            {
                var request = new Request();
                string callType;
                switch (weatherApiCallType)
                {
                    case WeatherApiCallType.CityName:
                        callType = "q=";
                        break;
                    case WeatherApiCallType.CityId:
                        callType = "id=";
                        break;
                    case WeatherApiCallType.ZipCode:
                        callType = "zip=";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(weatherApiCallType), weatherApiCallType, null);
                }
                var requestString = "http://api.openweathermap.org/data/2.5/weather?" + callType + location + "&appid=" + apiKey;
                var response = await request.Http(requestString);
                var json = response.content;
                //dynamic latest = JObject.Parse(json);
                var responseData = JsonConvert.DeserializeObject<WeatherApi.WeatherResponseData>(json);
                if (responseData == null) throw new Exception("Weather result is NULL");
                if (responseData.weather.Count != 0)
                {
                    weatherResult = WeatherApi.GetGtaWeatherFromId(responseData.weather[0].id);
                    TriggerEvent("FiveSPN-ServerLogToServer", API.GetCurrentResourceName(),4,$"Weather received for {location}! Setting server weather to {weatherResult}");
                } else throw new Exception("Weather result contains no weather data");
            }
            catch (Exception e)
            {
                TriggerEvent("FiveSPN-ServerLogToServer", API.GetCurrentResourceName(),1,$"Unable to process WeatherAPI request!");
                TriggerEvent("FiveSPN-ServerLogToServer", API.GetCurrentResourceName(),1,e.Message);
                weatherResult = "CLEAR";
            }
            return weatherResult;
        }
    }
}