using System.Collections.Generic;

namespace FiveSpn.Weather.Server.LocalServices.Classes
{
    public class LocationsJson
    {
        public List<List<object>> Cities { get; set; }
        public List<List<double>> Zips { get; set; }
        public List<List<double>> Ids { get; set; }
        public List<List<object>> Forced { get; set; }
    }
}