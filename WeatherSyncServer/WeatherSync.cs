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
        private int _refreshRate = 5;
        private bool _currentlyUpdating = false;
        private bool _forceWeather = false;
        private bool _forceUpdate = false;
        private string _forceWeatherSource = "";
        private readonly object _weatherLock = new object();
        private string _serverWeather = "CLEAR";
        private string _weatherLocation;
        private readonly string _weatherApiKey;
        
        public WeatherSync()
        {
            _weatherLocation = API.GetResourceMetadata(API.GetCurrentResourceName(), "weather_location", 0);
            _weatherApiKey = API.GetResourceMetadata(API.GetCurrentResourceName(), "weather_api_key", 0);
            
            EventHandlers["WeatherSync:SetOverride"] += new Action<Player, string>(SetOverride);
            EventHandlers["WeatherSync:SetLocation"] += new Action<Player, string>(SetLocation);
            EventHandlers["WeatherSync:SetRefresh"] += new Action<Player, int>(SetRefresh);
            EventHandlers["WeatherSync:RequestUpdate"] += new Action<Player>(RequestUpdate);
            EventHandlers["WeatherSync:CheckPerms"] += new Action<Player>(CheckPerms);
            
            Tick += OnTick;
        }

        private void CheckPerms([FromSource]Player player)
        {
            if (API.IsPlayerAceAllowed(player.Handle, "group.admin"))
            {
                TriggerClientEvent(player,"WeatherSync:ClientPerms", true);
            } else TriggerClientEvent(player,"WeatherSync:ClientPerms", false);
        }

        private void RequestUpdate([FromSource]Player player)
        {
            TriggerClientEvent("WeatherSync:ClientUpdate", _serverWeather);
        }

        private void SetRefresh([FromSource]Player player, int newRefreshRate)
        {
            lock (_weatherLock)
            {
                _refreshRate = newRefreshRate > 1 ? newRefreshRate : 1;
            }
        }

        private void SetLocation([FromSource]Player player, string newLocation)
        {
            lock (_weatherLock)
            {
                _weatherLocation = newLocation;
                _forceUpdate = true;
            }
        }

        private void SetOverride([FromSource]Player player, string newWeather)
        {
            lock (_weatherLock)
            {
                _forceWeather = newWeather != "RESET";
                _forceWeatherSource = player.Name;
                _serverWeather = newWeather;
                _forceUpdate = true;
            }
        }

        private async Task OnTick()
        {
            if (((DateTime.Now - _lastUpdateTime).Minutes > _refreshRate || _forceUpdate) && !_currentlyUpdating)
            {
                string newWeather = _serverWeather; 
                lock (_weatherLock)
                {
                    _currentlyUpdating = true;
                    _lastUpdateTime = DateTime.Now;
                    _forceUpdate = false;
                    ServerLogger.SendServerLogMessage(new LogMessage("WeatherSync",LogMessageSeverity.Verbose,"Updating the weather."));
                }
                if (!_forceWeather)
                {
                    newWeather = WeatherApiRequest.Instance.GetGtaWeatherForLocation(_weatherLocation,_weatherApiKey).Result;
                    ServerLogger.SendServerLogMessage(new LogMessage("WeatherSync",LogMessageSeverity.Verbose,$"Weather is now set to {_serverWeather}."));
                }
                else
                {
                    ServerLogger.SendServerLogMessage(new LogMessage("WeatherSync",LogMessageSeverity.Verbose,$"Weather is overriden by {_forceWeatherSource} and set to {_serverWeather}."));
                }

                if (newWeather != _serverWeather)
                {
                    _serverWeather = newWeather;
                    TriggerClientEvent("WeatherSync:ClientUpdate", _serverWeather);
                }
                
                lock (_weatherLock)
                {
                    _currentlyUpdating = false;
                }
            }
            await Task.FromResult(0);
        }
    }
}