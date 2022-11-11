using System;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using FiveSpn.Weather.Server.LocalServices;
using FiveSpn.Weather.Server.WeatherApiService;

namespace FiveSpn.Weather.Server
{
    public class WeatherSyncServer : BaseScript
    {
        public WeatherSyncServer()
        {
            TriggerEvent("FiveSPN-LogToServer", API.GetCurrentResourceName(),3, "Initializing!");

            if (!WeatherStateService.Instance.PopulatePoints(out string result))
            {
                TriggerEvent("FiveSPN-LogToServer", API.GetCurrentResourceName(),1, result);
                return;
            }
            TriggerEvent("FiveSPN-LogToServer", API.GetCurrentResourceName(),4, result);

            var weatherApiKey = API.GetResourceMetadata(API.GetCurrentResourceName(), "weather_api_key", 0) ?? "";
            
            if (weatherApiKey.Length == 0)
            {
                TriggerEvent("FiveSPN-LogToServer", API.GetCurrentResourceName(),1,$"Weather API key is not set in the resource manifest, unable to start the resource!");
                return;
            }
            
            WeatherApiRequestService.Instance.SetApiKey(weatherApiKey);
            
            var refreshRateParseSuccessful = int.TryParse(API.GetResourceMetadata(API.GetCurrentResourceName(), "refresh_rate", 0), out int refreshRate);
            
            if (!refreshRateParseSuccessful)
            {
                TriggerEvent("FiveSPN-LogToServer", API.GetCurrentResourceName(),1,$"Weather refresh_rate is not set in the resource manifest, unable to start the resource!");
                return;
            }

            if (API.GetResourceMetadata(API.GetCurrentResourceName(), "verbose_logging", 0) == "true")
            {
                WeatherApiRequestService.Instance.SetVerboseLogging(true);
            }
            
            WeatherStateService.Instance.SetRefreshRate(refreshRate);
            ClientUpdateService.Instance.SetRefreshRate(refreshRate);
            
            EventHandlers["FiveSPN-WX-SetOverride"] += new Action<Player, string>(SetOverride);
            EventHandlers["FiveSPN-WX-ClearOverride"] += new Action<Player>(ClearOverride);
            EventHandlers["FiveSPN-WX-RequestUpdate"] += new Action<Player>(RequestUpdate);
            
            Tick += WeatherStateService.Instance.PointMonitorTick;
            Tick += WeatherStateService.Instance.PointQueueTick;
            Delay(5000);
            Tick += ClientUpdateService.Instance.ClientUpdateTick;
        }

        private void SetOverride([FromSource]Player player, string newWeather)
        {
            if (!CheckPermsNow(player))
            {
                TriggerEvent("FiveSPN-LogToServer", API.GetCurrentResourceName(),4,$"Weather override requested to be {newWeather} by {player.Name} but player is not an admin.");
                return;
            }
            WeatherStateService.Instance.EnableWeatherOverride(newWeather);
            TriggerEvent("FiveSPN-LogToServer", API.GetCurrentResourceName(),4,$"Weather is now set to {newWeather} by {player.Name}.");
            TriggerClientEvent(player,"FiveSPN-ReplyToClientWx",$"Weather set to {newWeather}, standby for weather change!");
            ClientUpdateService.Instance.RequestUpdate();
        }
        
        private void ClearOverride([FromSource]Player player)
        {
            if (!CheckPermsNow(player))
            {
                TriggerEvent("FiveSPN-LogToServer", API.GetCurrentResourceName(),4,$"Weather clear override requested by {player.Name} but player is not an admin.");
                return;
            }
            WeatherStateService.Instance.DisableWeatherOverride();
            TriggerEvent("FiveSPN-LogToServer", API.GetCurrentResourceName(),4,$"Weather is now set to defaults by {player.Name}.");
            TriggerClientEvent(player,"FiveSPN-ReplyToClientWx",$"Weather set to defaults, standby for weather change!");
            ClientUpdateService.Instance.RequestUpdate();
        }

        private static bool CheckPermsNow([FromSource]Player player)
        {
            TriggerEvent("FiveSPN-LogToServer", API.GetCurrentResourceName(),4,$"Admin permission check for {player.Name} | {player.Handle}.");
            return (API.IsPlayerAceAllowed(player.Handle, "WeatherAdmin") || API.IsPlayerAceAllowed(player.Handle, "ServerAdmin"));
        }

        private void RequestUpdate([FromSource]Player player)
        {
            TriggerEvent("FiveSPN-LogToServer", API.GetCurrentResourceName(),4,$"Weather update requested by {player.Name}.");
            ClientUpdateService.Instance.RequestUpdate();
        }
    }
}