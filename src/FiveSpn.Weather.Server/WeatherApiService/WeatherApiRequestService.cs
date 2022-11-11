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
        public async Task<WeatherIdResult> GetWeatherIdForLocation(string callString)
        {
            if (_apiKey == "" || _apiKey == "NONE")
            {
                return new WeatherIdResult(false, "The WeatherAPI key not set, unable to get weather! Set the WeatherAPI key in the resource manifest!",0);
            }
            if (_verboseLogging) Console.WriteLine($">>>>Requesting weather for {callString}!");
            try
            {
                var request = new Request();
                var requestString = "http://api.openweathermap.org/data/2.5/weather?" + callString + "&appid=" + _apiKey;
                var response = await request.Http(requestString);
                var json = response.content;
                var responseData = JsonConvert.DeserializeObject<WeatherResponseData>(json);
                if (responseData == null) throw new Exception("Weather result is NULL");
                if (responseData.weather.Count == 0) throw new Exception("Weather result contains no weather data");
                if (_verboseLogging) Console.WriteLine($">>>>Weather id received for {callString} - {responseData.weather[0].id}");
                return new WeatherIdResult(true, $"Weather API id data received for {callString}!", responseData.weather[0].id);
            }
            catch (Exception e)
            {
                return new WeatherIdResult(false, "Unable to process WeatherAPI request!\n"+e.Message,0);
            }
        }
    }
}