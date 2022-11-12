using System;
using System.Threading.Tasks;
using CitizenFX.Core;

namespace FiveSpn.Weather.Server.LocalServices
{
    public class ClientUpdateService
    {
        private static readonly Lazy<ClientUpdateService> instance = new Lazy<ClientUpdateService>(() => new ClientUpdateService());
        private ClientUpdateService(){}
        public static ClientUpdateService Instance => instance.Value;
        
        private int _clientRefreshRate = 5;
        private bool _updateRequested;
        private DateTime _clientLastUpdate = DateTime.MinValue;

        /// <summary>
        /// Requests a forced update of the clients on the server.
        /// </summary>
        public void RequestUpdate() => _updateRequested = true;
        
        /// <summary>
        /// Sets the client refresh rate.
        /// </summary>
        /// <param name="rate">Minutes between refresh.</param>
        public void SetRefreshRate(int rate) => _clientRefreshRate = rate;
        
        /// <summary>
        /// Updates the clients of weather states on refresh rate interval.
        /// </summary>
        public async Task ClientUpdateTick()
        {
            if (_clientLastUpdate.AddMinutes(_clientRefreshRate) < DateTime.Now || _updateRequested)
            {
                _clientLastUpdate = DateTime.Now;
                BaseScript.TriggerClientEvent("FiveSPN-UpdateClientWx",WeatherStateService.Instance.GetClientJsonWeatherPoints());
                _updateRequested = false;
            }

            await Task.FromResult("Complete");
        }
    }
}