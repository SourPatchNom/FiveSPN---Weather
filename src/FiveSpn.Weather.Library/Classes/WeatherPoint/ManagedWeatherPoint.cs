using System;
using System.Collections.Generic;

namespace FiveSpn.Weather.Library.Classes.WeatherPoint
{
    public class ManagedWeatherPoint : WeatherPoint
    {
        public string ApiRequestString { get; }
        public DateTime LastUpdate { get; private set; }
        public bool UpdatePending { get; private set; }
        
        public ManagedWeatherPoint(List<float> position, int range, string apiRequestString) : base(position, range)
        {
            ApiRequestString = apiRequestString;
            LastUpdate = DateTime.Now.AddDays(-1);
            UpdatePending = false;
        }

        public override void SetWeather(string weather)
        {
            Weather = weather;
            LastUpdate = DateTime.Now;
            UpdatePending = false;
        }

        public void SetUpdateQueued() => UpdatePending = true;
    }
}