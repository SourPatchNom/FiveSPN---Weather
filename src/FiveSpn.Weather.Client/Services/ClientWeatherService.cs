using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using FiveSpn.Weather.Library.Extensions;
using GtaWeatherExtensions = FiveSpn.Weather.Client.Extensions.GtaWeatherExtensions;

namespace FiveSpn.Weather.Client.Services
{
    public sealed class ClientWeatherService
    {
        private static readonly Lazy<ClientWeatherService> instance = new Lazy<ClientWeatherService>(() => new ClientWeatherService());
        private ClientWeatherService(){}
        public static ClientWeatherService Instance => instance.Value;
        
        private DateTime _lastUpdate = DateTime.Now;
        private CitizenFX.Core.Weather _desiredWeather = CitizenFX.Core.Weather.Clear;
        private bool _forceUpdate = false;

        public void ForceUpdate() => _forceUpdate = true;
        
        /// <summary>
        /// Monitors and updates player weather on the server based on world weather data provided by the server.
        /// </summary>
        public async Task WeatherTick()
        {
            if (_lastUpdate.AddMinutes(1) < DateTime.Now || _forceUpdate)
            {
                _forceUpdate = false;
                _lastUpdate = DateTime.Now;
                _desiredWeather = GtaWeatherExtensions.GetWeatherFromString(WorldWeatherService.Instance.GetWorldDesiredWeather(API.GetEntityCoords(API.GetPlayerPed(-1), false)));
                if (_desiredWeather != World.Weather)
                {
                    World.TransitionToWeather(_desiredWeather, 45f);
                }
            }

            if (_debugMode)
            {
                Screen.DisplayHelpTextThisFrame("FiveSPN-Weather DEBUG INFO\nCurrent Weather:"+World.Weather+"\nDesired Weather:"+_desiredWeather+"\nWeather Points:"+WorldWeatherService.Instance.GetWorldDebugDisplay());
            }
            
            await Task.FromResult("Complete");
        }

        
    }
}