using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using WeatherSyncServer.Classes;
using WeatherSyncServer.Enums;

namespace WeatherSyncServer
{
    public class WeatherSyncServer : BaseScript
    {
        private DateTime _lastUpdateTime = DateTime.Now;
        private int _refreshRate;
        private bool _currentlyUpdating;
        private bool _forceWeather;
        private string _forceWeatherType = "CLEAR";
        private bool _forceUpdate = true;
        private string _forceWeatherSource = "";
        private readonly object _weatherLock = new object();
        private string _serverWeather = "CLEAR";
        private string _weatherLocation = "";
        private WeatherApiCallType _weatherApiCallType = WeatherApiCallType.CityName;
        private readonly string _weatherApiKey;
        
        public WeatherSyncServer()
        {
            TriggerEvent("FiveSPN-ServerLogToServer", API.GetCurrentResourceName(),3, "Initializing!");
            string weatherLocationCity = API.GetResourceMetadata(API.GetCurrentResourceName(), "weather_city", 0) ?? "";
            string weatherLocationId = API.GetResourceMetadata(API.GetCurrentResourceName(), "weather_id", 0) ?? "";
            string weatherLocationZip = API.GetResourceMetadata(API.GetCurrentResourceName(), "weather_zip", 0) ?? "";
            if(!int.TryParse(API.GetResourceMetadata(API.GetCurrentResourceName(), "refresh_rate", 0), out _refreshRate)) _refreshRate = 5;
            
            if (weatherLocationCity != "")
            {
                _weatherLocation = weatherLocationCity;
                _weatherApiCallType = WeatherApiCallType.CityName;
            }
            else if (weatherLocationId != "")
            {
                _weatherLocation = weatherLocationId;
                _weatherApiCallType = WeatherApiCallType.CityId;
            }
            else if (weatherLocationZip != "")
            {
                _weatherLocation = weatherLocationZip;
                _weatherApiCallType = WeatherApiCallType.ZipCode;
            }
            
            if (_weatherLocation.Length == 0)
            {
                TriggerEvent("FiveSPN-ServerLogToServer", API.GetCurrentResourceName(),1,$"Weather location is not set in the resource manifest, unable to start the resource!");
                return;
            }
            
            TriggerEvent("FiveSPN-ServerLogToServer", API.GetCurrentResourceName(),4,$"Server weather is now set to {_weatherLocation}.");
            _weatherApiKey = API.GetResourceMetadata(API.GetCurrentResourceName(), "weather_api_key", 0) ?? "";
            
            if (_weatherApiKey.Length == 0)
            {
                TriggerEvent("FiveSPN-ServerLogToServer", API.GetCurrentResourceName(),1,$"Weather API key is not set in the resource manifest, unable to start the resource!");
                return;
            }
            
            EventHandlers["WeatherSync:SetOverride"] += new Action<Player, string>(SetOverride);
            EventHandlers["WeatherSync:SetLocationZip"] += new Action<Player, string>(SetLocationZip);
            EventHandlers["WeatherSync:SetLocationId"] += new Action<Player, string>(SetLocationId);
            EventHandlers["WeatherSync:SetLocationCity"] += new Action<Player, string>(SetLocationCity);
            EventHandlers["WeatherSync:SetRefresh"] += new Action<Player, int>(SetRefresh);
            EventHandlers["WeatherSync:RequestUpdate"] += new Action<Player>(RequestUpdate);
            
            Tick += OnTick;
        }

        private void SetOverride([FromSource]Player player, string newWeather)
        {
            if (!CheckPermsNow(player))
            {
                TriggerEvent("FiveSPN-ServerLogToServer", API.GetCurrentResourceName(),4,$"Weather override requested to be {newWeather} by {player.Name} but player is not an admin.");
                return;
            }
            lock (_weatherLock)
            {
                if (newWeather == "RESET")
                {
                    _forceWeatherType = "CLEAR";
                    _forceWeather = false;
                }
                else
                {
                    _forceWeatherType = newWeather;
                    _forceWeather = true;
                }
                _forceWeatherSource = player.Name;
                _forceUpdate = true;
                TriggerEvent("FiveSPN-ServerLogToServer", API.GetCurrentResourceName(),4,$"Weather is now set to {_serverWeather} by {player.Name}.");
                TriggerClientEvent(player,"WeatherSync:ReplyToClient",$"Weather set to {newWeather}, standby for weather change!");
            }
        }
        
        private void SetLocationZip([FromSource]Player player, string newLocation)
        {
            if (!CheckPermsNow(player))
            {
                TriggerEvent("FiveSPN-ServerLogToServer", API.GetCurrentResourceName(),4,$"Weather location requested to be {newLocation} by {player.Name} but player is not an admin.");
                return;
            }
            lock (_weatherLock)
            {
                _weatherLocation = newLocation;
                _forceUpdate = true;
                _weatherApiCallType = WeatherApiCallType.ZipCode;
                TriggerEvent("FiveSPN-ServerLogToServer", API.GetCurrentResourceName(),4,$"Weather location is now set to {newLocation} by {player.Name}.");
                TriggerClientEvent(player,"WeatherSync:ReplyToClient",$"Weather location set to {newLocation}. If this is a good location the weather should update shortly.");
            }
        }
        
        private void SetLocationId([FromSource]Player player, string newLocation)
        {
            if (!CheckPermsNow(player))
            {
                TriggerEvent("FiveSPN-ServerLogToServer", API.GetCurrentResourceName(),4,$"Weather location requested to be {newLocation} by {player.Name} but player is not an admin.");
                return;
            }
            lock (_weatherLock)
            {
                _weatherLocation = newLocation;
                _forceUpdate = true;
                _weatherApiCallType = WeatherApiCallType.CityId;
                TriggerEvent("FiveSPN-ServerLogToServer", API.GetCurrentResourceName(),4,$"Weather location is now set to {newLocation} by {player.Name}.");
                TriggerClientEvent(player,"WeatherSync:ReplyToClient",$"Weather location set to {newLocation}. If this is a good location the weather should update shortly.");
            }
        }
        private void SetLocationCity([FromSource]Player player, string newLocation)
        {
            if (!CheckPermsNow(player))
            {
                TriggerEvent("FiveSPN-ServerLogToServer", API.GetCurrentResourceName(),4,$"Weather location requested to be {newLocation} by {player.Name} but player is not an admin.");
                return;
            }
            lock (_weatherLock)
            {
                _weatherLocation = newLocation;
                _forceUpdate = true;
                _weatherApiCallType = WeatherApiCallType.CityName;
                TriggerEvent("FiveSPN-ServerLogToServer", API.GetCurrentResourceName(),4,$"Weather location is now set to {newLocation} by {player.Name}.");
                TriggerClientEvent(player,"WeatherSync:ReplyToClient",$"Weather location set to {newLocation}. If this is a good location the weather should update shortly.");
            }
        }

        private static bool CheckPermsNow([FromSource]Player player)
        {
            TriggerEvent("FiveSPN-ServerLogToServer", API.GetCurrentResourceName(),4,$"Admin permission check for {player.Name} | {player.Handle}.");
            return (API.IsPlayerAceAllowed(player.Handle, "WeatherAdmin") || API.IsPlayerAceAllowed(player.Handle, "ServerAdmin"));
        }
        
        private static void CheckPerms([FromSource]Player player)
        {
            //TriggerEvent("FiveSPN-ServerLogToServer", API.GetCurrentResourceName(),4,$"Admin permission check requested by {player.Name}."));
            //TriggerClientEvent(player, "WeatherSync:ApplyClientPerms", API.IsPlayerAceAllowed(player.ToString(), "group.admin"));
        }

        private void RequestUpdate([FromSource]Player player)
        {
            TriggerEvent("FiveSPN-ServerLogToServer", API.GetCurrentResourceName(),4,$"Weather update requested by {player.Name}.");
            TriggerClientEvent("WeatherSync:UpdateClientWx", _serverWeather, _forceWeather ? _forceWeatherSource : _weatherLocation);
        }

        private void SetRefresh([FromSource]Player player, int newRefreshRate)
        {
            if (!CheckPermsNow(player))
            {
                TriggerEvent("FiveSPN-ServerLogToServer", API.GetCurrentResourceName(),4,$"Weather refresh rate update requested to be {newRefreshRate} by {player.Name} but player is not an admin.");
                return;
            }
            lock (_weatherLock)
            {
                _refreshRate = newRefreshRate > 1 ? newRefreshRate : 1;
                TriggerEvent("FiveSPN-ServerLogToServer", API.GetCurrentResourceName(),4,$"Weather refresh rate is now set to {newRefreshRate} by {player.Name}.");
                TriggerClientEvent(player,"WeatherSync:ReplyToClient",$"Weather refresh minutes set to {newRefreshRate}.");
            }
        }

        private async Task OnTick()
        {
            if (((DateTime.Now - _lastUpdateTime).Minutes > _refreshRate || _forceUpdate) && !_currentlyUpdating)
            {
                string newWeather = ""; 
                lock (_weatherLock)
                {
                    _currentlyUpdating = true;
                    _lastUpdateTime = DateTime.Now;
                    _forceUpdate = false;
                    TriggerEvent("FiveSPN-ServerLogToServer", API.GetCurrentResourceName(),4,"Updating the weather.");
                }
                if (!_forceWeather)
                {
                    newWeather = await WeatherApiRequest.Instance.GetGtaWeatherForLocation(_weatherLocation,_weatherApiKey,_weatherApiCallType);
                }
                else
                {
                    newWeather = _forceWeatherType;
                    TriggerEvent("FiveSPN-ServerLogToServer", API.GetCurrentResourceName(),4,$"Weather is overriden by {_forceWeatherSource} and set to {_forceWeatherType}.");
                }

                if (newWeather != _serverWeather)
                {
                    _serverWeather = newWeather;
                    TriggerEvent("FiveSPN-ServerLogToServer", API.GetCurrentResourceName(),4,$"Weather is now set to {_serverWeather}, which is synced from {(_forceWeather ? _forceWeatherSource : _weatherLocation)}.");
                    TriggerClientEvent("WeatherSync:UpdateClientWx", _serverWeather, _forceWeather ? _forceWeatherSource : _weatherLocation);
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