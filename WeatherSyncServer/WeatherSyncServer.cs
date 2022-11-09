using System;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using WeatherSyncServer.WeatherApiService;

namespace WeatherSyncServer
{
    public class WeatherSyncServer : BaseScript
    {
        public WeatherSyncServer()
        {
            TriggerEvent("FiveSPN-LogToServer", API.GetCurrentResourceName(),3, "Initializing!");

            if (!WeatherStateService.PopulatePoints(out string result))
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
            
            WeatherApiRequestService.SetApiKey(weatherApiKey);
            
            var refreshRateParseSuccessful = int.TryParse(API.GetResourceMetadata(API.GetCurrentResourceName(), "refresh_rate", 0), out int refreshRate);
            
            if (!refreshRateParseSuccessful)
            {
                TriggerEvent("FiveSPN-LogToServer", API.GetCurrentResourceName(),1,$"Weather refresh_rate is not set in the resource manifest, unable to start the resource!");
                return;
            }
            
            WeatherStateService.SetRefreshRate(refreshRate);
            ClientUpdateService.SetRefreshRate(refreshRate);
            
            EventHandlers["FiveSPN-WX-SetOverride"] += new Action<Player, string>(SetOverride);
            EventHandlers["FiveSPN-WX-ClearOverride"] += new Action<Player>(ClearOverride);
            EventHandlers["FiveSPN-WX-RequestUpdate"] += new Action<Player>(RequestUpdate);
            
            Tick += WeatherStateService.PointMonitorTick;
            Tick += WeatherStateService.PointQueueTick;
            Delay(5000);
            Tick += ClientUpdateService.ClientUpdateTick;
        }

        private void SetOverride([FromSource]Player player, string newWeather)
        {
            if (!CheckPermsNow(player))
            {
                TriggerEvent("FiveSPN-LogToServer", API.GetCurrentResourceName(),4,$"Weather override requested to be {newWeather} by {player.Name} but player is not an admin.");
                return;
            }
            WeatherStateService.EnableWeatherOverride(newWeather);
            TriggerEvent("FiveSPN-LogToServer", API.GetCurrentResourceName(),4,$"Weather is now set to {newWeather} by {player.Name}.");
            TriggerClientEvent(player,"FiveSPN-ReplyToClientWx",$"Weather set to {newWeather}, standby for weather change!");
            ClientUpdateService.RequestUpdate();
        }
        
        private void ClearOverride([FromSource]Player player)
        {
            if (!CheckPermsNow(player))
            {
                TriggerEvent("FiveSPN-LogToServer", API.GetCurrentResourceName(),4,$"Weather clear override requested by {player.Name} but player is not an admin.");
                return;
            }
            WeatherStateService.DisableWeatherOverride();
            TriggerEvent("FiveSPN-LogToServer", API.GetCurrentResourceName(),4,$"Weather is now set to defaults by {player.Name}.");
            TriggerClientEvent(player,"FiveSPN-ReplyToClientWx",$"Weather set to defaults, standby for weather change!");
            ClientUpdateService.RequestUpdate();
        }

        private static bool CheckPermsNow([FromSource]Player player)
        {
            TriggerEvent("FiveSPN-LogToServer", API.GetCurrentResourceName(),4,$"Admin permission check for {player.Name} | {player.Handle}.");
            return (API.IsPlayerAceAllowed(player.Handle, "WeatherAdmin") || API.IsPlayerAceAllowed(player.Handle, "ServerAdmin"));
        }

        private void RequestUpdate([FromSource]Player player)
        {
            TriggerEvent("FiveSPN-LogToServer", API.GetCurrentResourceName(),4,$"Weather update requested by {player.Name}.");
            ClientUpdateService.RequestUpdate();
        }
    }
}