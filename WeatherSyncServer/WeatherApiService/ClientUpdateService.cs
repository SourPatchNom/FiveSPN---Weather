using System;
using System.Threading.Tasks;
using CitizenFX.Core;

namespace WeatherSyncServer.WeatherApiService
{
    public static class ClientUpdateService
    {
        private static int _clientRefreshRate = 5;
        private static bool _updateRequested;
        private static DateTime _clientLastUpdate = DateTime.Now.AddYears(-1);

        /// <summary>
        /// Requests a forced update of the clients on the server.
        /// </summary>
        public static void RequestUpdate() => _updateRequested = true;
        
        /// <summary>
        /// Sets the client refresh rate.
        /// </summary>
        /// <param name="rate">Minutes between refresh.</param>
        public static void SetRefreshRate(int rate) => _clientRefreshRate = rate;
        
        /// <summary>
        /// Updates the clients of weather states on refresh rate interval.
        /// </summary>
        public static async Task ClientUpdateTick()
        {
            if (_clientLastUpdate.AddMinutes(_clientRefreshRate) < DateTime.Now || _updateRequested)
            {
                _clientLastUpdate = DateTime.Now;
                BaseScript.TriggerClientEvent("FiveSPN-UpdateClientWx",WeatherStateService.GetClientJsonWeatherPoints());
                _updateRequested = false;
            }

            await Task.FromResult("Complete");
        }
    }
}