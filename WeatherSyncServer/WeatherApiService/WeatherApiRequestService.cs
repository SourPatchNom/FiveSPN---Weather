using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using WeatherSyncServer.Classes;
using WeatherSyncServer.WeatherApiService.Classes;

namespace WeatherSyncServer.WeatherApiService
{
    public static class WeatherApiRequestService //: BaseScript
    {
        private static string ApiKey = "";

        /// <summary>
        /// Sets the api key for Open Weather Map.
        /// </summary>
        /// <param name="key"></param>
        public static void SetApiKey(string key) => ApiKey = key;

        /// <summary>
        /// Requests the weather for a given location, either in zip or city name.
        /// </summary>
        /// <param name="callString">call string for the owm api</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<string> GetGtaWeatherForLocation(string callString)
        {
            string weatherResult;
            if (ApiKey == "" || ApiKey == "NONE")
            {
                BaseScript.TriggerEvent("FiveSPN-LogToServer", API.GetCurrentResourceName(),0,"The WeatherAPI key not set, unable to get weather! Set the WeatherAPI key in the resource manifest!");
                return "CLEAR";
            }
            BaseScript.TriggerEvent("FiveSPN-LogToServer", API.GetCurrentResourceName(),5,$"Requesting weather for {callString}!");
            try
            {
                var request = new Request();
                var requestString = "http://api.openweathermap.org/data/2.5/weather?" + callString + "&appid=" + ApiKey;
                var response = await request.Http(requestString);
                var json = response.content;
                var responseData = JsonConvert.DeserializeObject<WeatherResponseData>(json);
                if (responseData == null) throw new Exception("Weather result is NULL");
                if (responseData.weather.Count != 0)
                {
                    weatherResult = Extensions.GtaWeatherExtensions.GetGtaWeatherFromId(responseData.weather[0].id);
                    BaseScript.TriggerEvent("FiveSPN-LogToServer", API.GetCurrentResourceName(),5,$"Weather received for {callString} - {weatherResult}");
                } else throw new Exception("Weather result contains no weather data");
            }
            catch (Exception e)
            {
                BaseScript.TriggerEvent("FiveSPN-LogToServer", API.GetCurrentResourceName(),1,$"Unable to process WeatherAPI request!");
                BaseScript.TriggerEvent("FiveSPN-LogToServer", API.GetCurrentResourceName(),1,e.Message);
                weatherResult = "CLEAR";
            }
            return weatherResult;
        }
        
        
    }
}