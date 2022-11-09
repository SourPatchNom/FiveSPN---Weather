using System;
using CitizenFX.Core;

namespace WeatherSyncServer.Classes
{
    public class WeatherPoint
    {
        public string ApiRequestString { get; }
        public Vector3 Position { get; }
        public DateTime LastUpdate { get; private set; }
        public string Weather { get; private set; }
        
        public bool UpdatePending { get; private set; }

        public WeatherPoint(string apiRequestString, Vector3 position)
        {
            ApiRequestString = apiRequestString;
            Position = position;
            LastUpdate = DateTime.Now;
            Weather = "CLEAR";
        }

        public void SetUpdateQueued() => UpdatePending = true;

        public void SetWeather(string weather)
        {
            Weather = weather;
            LastUpdate = DateTime.Now;
            UpdatePending = false;
        }
    }
}