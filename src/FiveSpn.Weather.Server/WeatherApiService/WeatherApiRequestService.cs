using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using FiveSpn.Weather.Library.Extensions;
using FiveSpn.Weather.Server.Classes;
using FiveSpn.Weather.Server.WeatherApiService.Classes;
using Newtonsoft.Json;

namespace FiveSpn.Weather.Server.WeatherApiService
{
    public class WeatherApiRequestService //: BaseScript
    {
        private static readonly Lazy<WeatherApiRequestService> instance = new Lazy<WeatherApiRequestService>(() => new WeatherApiRequestService());
        private WeatherApiRequestService(){}
        public static WeatherApiRequestService Instance => instance.Value;
        
        private string _apiKey = "";
        private bool _verboseLogging;

        /// <summary>
        /// Sets the api key for Open Weather Map.
        /// </summary>
        /// <param name="key"></param>
        public void SetApiKey(string key) => _apiKey = key;

        /// <summary>
        /// Enables or disables more verbose logging of events sent to FiveSPN-Logger
        /// </summary>
        /// <param name="value">bool</param>
        public void SetVerboseLogging(bool value) => _verboseLogging = value;
        
        /// <summary>
        /// Requests the weather for a given location, either in zip or city name.
        /// </summary>
        /// <param name="callString">call string for the owm api</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<string> GetGtaWeatherForLocation(string callString)
        {
            string weatherResult;
            if (_apiKey == "" || _apiKey == "NONE")
            {
                BaseScript.TriggerEvent("FiveSPN-LogToServer", API.GetCurrentResourceName(),0,"The WeatherAPI key not set, unable to get weather! Set the WeatherAPI key in the resource manifest!");
                return "CLEAR";
            }
            if (_verboseLogging) BaseScript.TriggerEvent("FiveSPN-LogToServer", API.GetCurrentResourceName(),5,$"Requesting weather for {callString}!");
            try
            {
                var request = new Request();
                var requestString = "http://api.openweathermap.org/data/2.5/weather?" + callString + "&appid=" + _apiKey;
                var response = await request.Http(requestString);
                var json = response.content;
                var responseData = JsonConvert.DeserializeObject<WeatherResponseData>(json);
                if (responseData == null) throw new Exception("Weather result is NULL");
                if (responseData.weather.Count != 0)
                {
                    weatherResult =  GtaWeatherExtensions. GetGtaWeatherFromApiId(responseData.weather[0].id);
                    if (_verboseLogging) BaseScript.TriggerEvent("FiveSPN-LogToServer", API.GetCurrentResourceName(),5,$"Weather received for {callString} - {weatherResult}");
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