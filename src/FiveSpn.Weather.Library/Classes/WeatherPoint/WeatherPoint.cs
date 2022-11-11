using System.Collections.Generic;
using Newtonsoft.Json;

namespace FiveSpn.Weather.Library.Classes.WeatherPoint
{
    public class WeatherPoint
    {
        public List<float> Position { get; }
        public string Weather { get; protected set; }
        public int Range { get; private set; }

        public WeatherPoint(List<float> position, int range)
        {
            Position = position;
            Range = range;
            Weather = "CLEAR";
        }
        
        [JsonConstructor]
        public WeatherPoint(List<float> position, int range, string weather)
        {
            Position = position;
            Range = range;
            Weather = weather;
        }

        public virtual void SetWeather(string weather)
        {
            Weather = weather;
        }
        
        public void SetRange(int range)
        {
            Range = range;
        }
    }
}