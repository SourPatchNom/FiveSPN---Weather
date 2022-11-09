using System.Collections.Generic;

namespace WeatherSyncServer.WeatherApiService.Classes
{
    public class LocationsJson
    {
        public List<List<object>> Cities { get; set; }
        public List<List<double>> Zips { get; set; }
        public List<List<double>> Ids { get; set; }
    }
}