using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using WeatherSyncClient.Extensions;

namespace WeatherSyncClient.Services
{
    public static class ClientWeatherService
    {
        private static DateTime _lastUpdate = DateTime.Now;
        private static Weather _desiredWeather = Weather.Clear;
        private static bool _forceUpdate = false;

        public static void ForceUpdate() => _forceUpdate = true;
        
        /// <summary>
        /// Monitors and updates player weather on the server based on world weather data provided by the server.
        /// </summary>
        public static async Task WeatherTick()
        {
            if (_lastUpdate.AddMinutes(1) < DateTime.Now || _forceUpdate)
            {
                _forceUpdate = false;
                _lastUpdate = DateTime.Now;
                _desiredWeather = GtaWeatherExtensions.GetWeatherFromString(WorldWeatherService.GetWorldDesiredWeather(API.GetEntityCoords(API.GetPlayerPed(-1), false)));
                if (_desiredWeather != World.Weather)
                {
                    World.TransitionToWeather(_desiredWeather, 45f);
                }
            }
            await Task.FromResult("Complete");
        }
    }
}