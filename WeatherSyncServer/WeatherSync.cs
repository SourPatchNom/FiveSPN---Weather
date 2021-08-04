using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using FiveSpnLoggerServerLibrary;
using FiveSpnLoggerServerLibrary.Classes;
using FiveSpnLoggerServerLibrary.Enums;
using WeatherSyncServer.Classes;

namespace WeatherSyncServer
{
    public class WeatherSync : BaseScript
    {
        DateTime _lastUpdateTime = DateTime.Now;
        private bool _currentlyUpdating = false;
        private bool _forceWeather = false;
        private string _forceWeatherSource = "";
        private object _weatherLock = new object();
        private string _serverWeather = "CLEAR";
        private string _weatherLocation = "NONE";
        private string _weatherAPIKey = "NONE";
        
        public WeatherSync()
        {
            //TODO add resource variables
            Tick += OnTick;
        }

        private async Task OnTick()
        {
            if ((DateTime.Now - _lastUpdateTime).Minutes > 1 && !_currentlyUpdating)
            {
                lock (_weatherLock)
                {
                    _currentlyUpdating = true;
                    _lastUpdateTime = DateTime.Now;
                    ServerLogger.SendServerLogMessage(new LogMessage("WeatherSync",LogMessageSeverity.Verbose,"Updating the weather."));
                }
                if (!_forceWeather)
                {
                    _serverWeather = WeatherApiRequest.Instance.GetGtaWeatherForLocation(_weatherLocation,_weatherAPIKey).Result;
                    ServerLogger.SendServerLogMessage(new LogMessage("WeatherSync",LogMessageSeverity.Verbose,$"Weather is now set to {_serverWeather}."));
                }
                else
                {
                    ServerLogger.SendServerLogMessage(new LogMessage("WeatherSync",LogMessageSeverity.Verbose,$"Weather is overriden by {_forceWeatherSource} and set to {_serverWeather}."));
                }
                TriggerClientEvent("WeatherSyncUpdate", _serverWeather);
                lock (_weatherLock)
                {
                    _currentlyUpdating = false;
                }
            }
            await Task.FromResult(0);
        }
    }
}