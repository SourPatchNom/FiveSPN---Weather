using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using FiveSpn.Weather.Library.Classes.WeatherPoint;
using FiveSpn.Weather.Library.Extensions;
using FiveSpn.Weather.Server.Classes;
using FiveSpn.Weather.Server.LocalServices.Classes;
using FiveSpn.Weather.Server.WeatherApiService;
using Newtonsoft.Json;

namespace FiveSpn.Weather.Server.LocalServices
{
    public class WeatherStateService
    {
        private static readonly Lazy<WeatherStateService> instance = new Lazy<WeatherStateService>(() => new WeatherStateService());
        private WeatherStateService(){}
        public static WeatherStateService Instance => instance.Value;
        
        private readonly List<WeatherPoint> _weatherPoints = new List<WeatherPoint>();
        private readonly Queue<ManagedWeatherPoint> _weatherPointQue = new Queue<ManagedWeatherPoint>();
        private string _masterWeatherOverrideState = "CLEAR";
        private bool _masterWeatherOverride;
        private DateTime _lastQueuePop = DateTime.Now;
        private int _refreshRate = 5;
        private readonly string _directory = API.GetResourcePath(API.GetCurrentResourceName()) + "/locations.json"; 
        
        /// <summary>
        /// Populates the weather point list from a locations.json file.
        /// </summary>
        /// <param name="result">Success or failure message with details.</param>
        /// <returns>true if successful, false if failed or error</returns>
        public bool PopulatePoints(out string result)
        {
            try
            {
                if (File.Exists(_directory))
                {
                    var locations = JsonConvert.DeserializeObject<LocationsJson>(File.ReadAllText(_directory));
                    if (locations == null)
                    {
                        result = "Locations file was not read properly!";
                        return false;
                    }

                    if (locations.Cities.Any())
                    {
                        foreach (var city in locations.Cities)
                        {
                            _weatherPoints.Add(new ManagedWeatherPoint(new List<float>{(float)(double)city[1], (float)(double)city[2], (float)(double)city[3]}, Convert.ToInt32(city[4]), "q=" + city[0]));
                        }
                    }

                    if (locations.Zips.Any())
                    {
                        foreach (var zip in locations.Zips)
                        {
                            _weatherPoints.Add(new ManagedWeatherPoint(new List<float>{(float)zip[1], (float)zip[2], (float)zip[3]}, (int)zip[4],"zip=" + zip[0]));
                        }
                    }
                    
                    if (locations.Ids.Any())
                    {
                        foreach (var id in locations.Ids)
                        {
                            _weatherPoints.Add(new ManagedWeatherPoint(new List<float>{(float)id[1], (float)id[2], (float)id[3]},(int)id[4],"id=" + id[0]));
                        }
                    }
                    
                    if (locations.Forced.Any())
                    {
                        foreach (var forced in locations.Forced)
                        {
                            _weatherPoints.Add(new WeatherPoint( new List<float>{(float)(double)forced[1], (float)(double)forced[2], (float)(double)forced[3]},Convert.ToInt32(forced[4]), Convert.ToString(forced[0])));
                        }
                    }
                    
                    if (_weatherPoints.GroupBy(x => x.Position).Where(g => g.Count() > 1).Select(x => x.Key).Any())
                    {
                        result = "CRITICAL ERROR IN SETUP - You have duplicate coordinates in your locations.json!";
                        return false;    
                    }

                    if (_weatherPoints.Count > 10)
                    {
                        result = "CRITICAL ERROR IN SETUP - Reduce the amount of weather locations! You have " + _weatherPoints.Count + " in the locations.json! No more than 4 please!";
                        return false;    
                    }
                    result = "Done! " + _weatherPoints.Count + " weather points read!";
                    return true;
                }
                result = $"Unable to find file {_directory}!";
                return false;
            }
            catch (Exception e)
            {
                result = "CRITICAL ERROR IN SETUP!\n"+e.Message;
                return false;
            }
        }

        /// <summary>
        /// Sets the refresh rate. Default is 5 minutes. Cannot be set less than 1.
        /// </summary>
        /// <param name="rate">Minutes between refreshes.</param>
        public void SetRefreshRate(int rate)
        {
            if (rate < 1) return;
            _refreshRate = rate;   
        }
        
        /// <summary>
        /// Updates point weather if last update is over the _refreshRate.
        /// 0(n²) - Restricted to max 4 so 0(16)
        /// </summary>
        public async Task PointMonitorTick()
        {
            foreach (var point in _weatherPoints)
            {
                if (!(point is ManagedWeatherPoint managedWeatherPoint)) continue;
                if (managedWeatherPoint.UpdatePending) continue;
                if (_weatherPointQue.Any(x => x == point)) continue;
                if (managedWeatherPoint.LastUpdate.AddMinutes(_refreshRate) >= DateTime.Now) continue;
                managedWeatherPoint.SetUpdateQueued();
                _weatherPointQue.Enqueue(managedWeatherPoint);
            }
            
            await Task.FromResult("Complete");
        }

        /// <summary>
        /// Processes the weather point update queue.
        /// </summary>
        public async Task PointQueueTick()
        {
            if (_weatherPointQue.Any())
            {
                if (_lastQueuePop.AddSeconds(1) < DateTime.Now)
                {
                    _lastQueuePop = DateTime.Now;
                    var point = _weatherPointQue.Dequeue();
                    var result = await WeatherApiRequestService.Instance.GetWeatherIdForLocation(point.ApiRequestString);
                    if (!result.WasSuccessful)
                    {
                        BaseScript.TriggerEvent("FiveSPN-LogToServer", API.GetCurrentResourceName(),2,$"An attempt to collect weather data from the weather API service was unsuccessful. " + result.WeatherRequestResult);
                        return;
                    }
                    _weatherPoints.First(x => x.Position == point.Position).SetWeather(GtaWeatherExtensions.GetGtaWeatherFromApiId(result.WeatherApiTypeId));
                }    
            }

            await Task.FromResult("Complete");
        }

        /// <summary>
        /// Serializes the weather point list into a string/vector3 dictionary to send to clients.
        /// </summary>
        /// <returns>Json string for client use.</returns>
        public string GetClientJsonWeatherPoints()
        {
            if (_masterWeatherOverride || !_weatherPoints.Any())
            {
                var singlePoint = new List<WeatherPoint> { new WeatherPoint(new List<float>{0.0f,0.0f,0.0f},0,_masterWeatherOverrideState) };
                return JsonConvert.SerializeObject(singlePoint);
            }

            var allPoints = _weatherPoints.Select(x => new WeatherPoint(x.Position,x.Range,x.Weather)).ToList();
            return JsonConvert.SerializeObject(allPoints);
        }

        /// <summary>
        /// Sets the server weather override.
        /// </summary>
        /// <param name="weather">GTA weather string.</param>
        public void EnableWeatherOverride(string weather)
        {
            _masterWeatherOverrideState = weather;
            _masterWeatherOverride = true;
        }

        /// <summary>
        /// Disables to the weather override.
        /// </summary>
        public void DisableWeatherOverride()
        {
            _masterWeatherOverride = false;
        }
    }
}