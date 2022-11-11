using System.Collections.Generic;

namespace FiveSpn.Weather.Server.WeatherApiService.Classes
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
}