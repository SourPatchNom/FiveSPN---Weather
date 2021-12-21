using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using FiveSpnLoggerServerLibrary;
using FiveSpnLoggerServerLibrary.Classes;
using FiveSpnLoggerServerLibrary.Enums;
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
            ServerLogger.SendServerLogMessage(new LogMessage("FiveSPN - WeatherSync", LogMessageSeverity.Info, "Initializing WeatherSync!"));
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
                ServerLogger.SendServerLogMessage(new LogMessage("WeatherSync",LogMessageSeverity.Error,$"Weather location is not set in the resource manifest, unable to start the resource!"));
                return;
            }
            
            ServerLogger.SendServerLogMessage(new LogMessage("WeatherSync",LogMessageSeverity.Debug,$"Resource variable read, server weather is now set to {_weatherLocation}."));
            _weatherApiKey = API.GetResourceMetadata(API.GetCurrentResourceName(), "weather_api_key", 0) ?? "";
            
            if (_weatherApiKey.Length == 0)
            {
                ServerLogger.SendServerLogMessage(new LogMessage("WeatherSync",LogMessageSeverity.Error,$"Weather API key is not set in the resource manifest, unable to start the resource!"));
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
                ServerLogger.SendServerLogMessage(new LogMessage("WeatherSync",LogMessageSeverity.Verbose,$"Weather override requested to be {newWeather} by {player.Name} but player is not an admin."));
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
                ServerLogger.SendServerLogMessage(new LogMessage("WeatherSync",LogMessageSeverity.Verbose,$"Weather is now set to {_serverWeather} by {player.Name}."));
                TriggerClientEvent(player,"WeatherSync:ReplyToClient",$"Weather set to {newWeather}, standby for weather change!");
            }
        }
        
        private void SetLocationZip([FromSource]Player player, string newLocation)
        {
            if (!CheckPermsNow(player))
            {
                ServerLogger.SendServerLogMessage(new LogMessage("WeatherSync",LogMessageSeverity.Verbose,$"Weather location requested to be {newLocation} by {player.Name} but player is not an admin."));
                return;
            }
            lock (_weatherLock)
            {
                _weatherLocation = newLocation;
                _forceUpdate = true;
                _weatherApiCallType = WeatherApiCallType.ZipCode;
                ServerLogger.SendServerLogMessage(new LogMessage("WeatherSync",LogMessageSeverity.Verbose,$"Weather location is now set to {newLocation} by {player.Name}."));
                TriggerClientEvent(player,"WeatherSync:ReplyToClient",$"Weather location set to {newLocation}. If this is a good location the weather should update shortly.");
            }
        }
        
        private void SetLocationId([FromSource]Player player, string newLocation)
        {
            if (!CheckPermsNow(player))
            {
                ServerLogger.SendServerLogMessage(new LogMessage("WeatherSync",LogMessageSeverity.Verbose,$"Weather location requested to be {newLocation} by {player.Name} but player is not an admin."));
                return;
            }
            lock (_weatherLock)
            {
                _weatherLocation = newLocation;
                _forceUpdate = true;
                _weatherApiCallType = WeatherApiCallType.CityId;
                ServerLogger.SendServerLogMessage(new LogMessage("WeatherSync",LogMessageSeverity.Verbose,$"Weather location is now set to {newLocation} by {player.Name}."));
                TriggerClientEvent(player,"WeatherSync:ReplyToClient",$"Weather location set to {newLocation}. If this is a good location the weather should update shortly.");
            }
        }
        private void SetLocationCity([FromSource]Player player, string newLocation)
        {
            if (!CheckPermsNow(player))
            {
                ServerLogger.SendServerLogMessage(new LogMessage("WeatherSync",LogMessageSeverity.Verbose,$"Weather location requested to be {newLocation} by {player.Name} but player is not an admin."));
                return;
            }
            lock (_weatherLock)
            {
                _weatherLocation = newLocation;
                _forceUpdate = true;
                _weatherApiCallType = WeatherApiCallType.CityName;
                ServerLogger.SendServerLogMessage(new LogMessage("WeatherSync",LogMessageSeverity.Verbose,$"Weather location is now set to {newLocation} by {player.Name}."));
                TriggerClientEvent(player,"WeatherSync:ReplyToClient",$"Weather location set to {newLocation}. If this is a good location the weather should update shortly.");
            }
        }

        private static bool CheckPermsNow([FromSource]Player player)
        {
            ServerLogger.SendServerLogMessage(new LogMessage("WeatherSync",LogMessageSeverity.Verbose,$"Admin permission check for {player.Name} | {player.Handle}."));
            return API.IsPlayerAceAllowed(player.Handle, "WeatherAdmin");
        }
        
        private static void CheckPerms([FromSource]Player player)
        {
            //ServerLogger.SendServerLogMessage(new LogMessage("WeatherSync",LogMessageSeverity.Verbose,$"Admin permission check requested by {player.Name}."));
            //TriggerClientEvent(player, "WeatherSync:ApplyClientPerms", API.IsPlayerAceAllowed(player.ToString(), "group.admin"));
        }

        private void RequestUpdate([FromSource]Player player)
        {
            ServerLogger.SendServerLogMessage(new LogMessage("WeatherSync",LogMessageSeverity.Verbose,$"Weather update requested by {player.Name}."));
            TriggerClientEvent("WeatherSync:UpdateClientWx", _serverWeather, _forceWeather ? _forceWeatherSource : _weatherLocation);
        }

        private void SetRefresh([FromSource]Player player, int newRefreshRate)
        {
            if (!CheckPermsNow(player))
            {
                ServerLogger.SendServerLogMessage(new LogMessage("WeatherSync",LogMessageSeverity.Verbose,$"Weather refresh rate update requested to be {newRefreshRate} by {player.Name} but player is not an admin."));
                return;
            }
            lock (_weatherLock)
            {
                _refreshRate = newRefreshRate > 1 ? newRefreshRate : 1;
                ServerLogger.SendServerLogMessage(new LogMessage("WeatherSync",LogMessageSeverity.Verbose,$"Weather refresh rate is now set to {newRefreshRate} by {player.Name}."));
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
                    ServerLogger.SendServerLogMessage(new LogMessage("WeatherSync",LogMessageSeverity.Verbose,"Updating the weather."));
                }
                if (!_forceWeather)
                {
                    newWeather = await WeatherApiRequest.Instance.GetGtaWeatherForLocation(_weatherLocation,_weatherApiKey,_weatherApiCallType);
                }
                else
                {
                    newWeather = _forceWeatherType;
                    ServerLogger.SendServerLogMessage(new LogMessage("WeatherSync",LogMessageSeverity.Verbose,$"Weather is overriden by {_forceWeatherSource} and set to {_forceWeatherType}."));
                }

                if (newWeather != _serverWeather)
                {
                    _serverWeather = newWeather;
                    ServerLogger.SendServerLogMessage(new LogMessage("WeatherSync",LogMessageSeverity.Verbose,$"Weather is now set to {_serverWeather}, which is synced from {(_forceWeather ? _forceWeatherSource : _weatherLocation)}."));
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