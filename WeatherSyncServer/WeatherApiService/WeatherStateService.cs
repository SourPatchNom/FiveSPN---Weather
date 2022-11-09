using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using WeatherSyncServer.Classes;
using WeatherSyncServer.WeatherApiService.Classes;

namespace WeatherSyncServer.WeatherApiService
{
    public static class WeatherStateService
    {
        private static readonly List<WeatherPoint> WeatherPoints = new List<WeatherPoint>();
        private static readonly Queue<WeatherPoint> WeatherPointQue = new Queue<WeatherPoint>();
        private static string WeatherOverrideState { get; set; } = "CLEAR";
        private static bool WeatherOverride { get; set; }
        private static DateTime _lastQueuePop = DateTime.Now;
        private static int _refreshRate = 5;
        private static readonly string Directory = API.GetResourcePath(API.GetCurrentResourceName()) + "/locations.json"; 
        
        /// <summary>
        /// Populates the weather point list from a locations.json file.
        /// </summary>
        /// <param name="result">Success or failure message with details.</param>
        /// <returns>true if successful, false if failed or error</returns>
        public static bool PopulatePoints(out string result)
        {
            try
            {
                if (File.Exists(Directory))
                {
                    var locations = JsonConvert.DeserializeObject<LocationsJson>(File.ReadAllText(Directory));
                    if (locations == null)
                    {
                        result = "Locations file was not read properly!";
                        return false;
                    }

                    if (locations.Cities.Any())
                    {
                        foreach (var city in locations.Cities)
                        {
                            WeatherPoints.Add(new WeatherPoint("q=" + city[0],new Vector3((float)(double)city[1], (float)(double)city[2], (float)(double)city[3])));
                        }
                    }

                    if (locations.Zips.Any())
                    {
                        foreach (var zip in locations.Zips)
                        {
                            WeatherPoints.Add(new WeatherPoint("zip=" + zip[0], new Vector3((float)zip[1], (float)zip[2], (float)zip[3])));
                        }
                    }
                    
                    if (locations.Ids.Any())
                    {
                        foreach (var id in locations.Ids)
                        {
                            WeatherPoints.Add(new WeatherPoint( "id=" + id[0], new Vector3((float)id[1], (float)id[2], (float)id[3])));
                        }
                    }

                    if (WeatherPoints.GroupBy(x => x.Position).Where(g => g.Count() > 1).Select(x => x.Key).Any())
                    {
                        result = "CRITICAL ERROR IN SETUP - You have duplicate coordinates in your locations.json!";
                        return false;    
                    }

                    if (WeatherPoints.Count > 4)
                    {
                        result = "CRITICAL ERROR IN SETUP - Reduce the amount of weather locations! You have " + WeatherPoints.Count + " in the locations.json! No more than 4 please!";
                        return false;    
                    }
                    result = "Done! " + WeatherPoints.Count + " weather points read!";
                    return true;
                }
                result = $"Unable to find file {Directory}!";
                return false;
            }
            catch (Exception e)
            {
                result = "Critical Error!\n"+e.Message;
                return false;
            }
        }

        /// <summary>
        /// Sets the refresh rate. Default is 5 minutes. Cannot be set less than 1.
        /// </summary>
        /// <param name="rate">Minutes between refreshes.</param>
        public static void SetRefreshRate(int rate)
        {
            if (rate < 1) return;
            _refreshRate = rate;   
        }
        
        /// <summary>
        /// Updates point weather if last update is over the _refreshRate.
        /// 0(n²) - Restricted to max 4 so 0(16)
        /// </summary>
        public static async Task PointMonitorTick()
        {
            foreach (var point in WeatherPoints)
            {
                if (point.UpdatePending) continue;
                if (WeatherPointQue.Any(x => x == point)) continue;
                if (point.LastUpdate.AddMinutes(_refreshRate) >= DateTime.Now) continue;
                point.SetUpdateQueued();
                WeatherPointQue.Enqueue(point);
            }
            
            await Task.FromResult("Complete");
        }

        /// <summary>
        /// Processes the weather point update queue.
        /// </summary>
        public static async Task PointQueueTick()
        {
            if (WeatherPointQue.Any())
            {
                if (_lastQueuePop.AddSeconds(1) < DateTime.Now)
                {
                    _lastQueuePop = DateTime.Now;
                    var point = WeatherPointQue.Dequeue();
                    WeatherPoints.First(x => x.Position == point.Position).SetWeather(await WeatherApiRequestService.GetGtaWeatherForLocation(point.ApiRequestString));
                }    
            }

            await Task.FromResult("Complete");
        }

        /// <summary>
        /// Serializes the weather point list into a string/vector3 dictionary to send to clients.
        /// </summary>
        /// <returns>Json string for client use.</returns>
        public static string GetClientJsonWeatherPoints()
        {
            if (WeatherOverride)
            {
                var singlePoint = new List<Tuple<string, List<float>>> ();
                singlePoint.Add(new Tuple<string, List<float>>(WeatherOverrideState, new List<float>() { 0f, 0f, 0f }));
                return JsonConvert.SerializeObject(singlePoint);    
            }

            var allPoints = WeatherPoints.Select(point => new Tuple<string, List<float>>(point.Weather, new List<float>() { point.Position.X, point.Position.Y, point.Position.Z })).ToList();
            return JsonConvert.SerializeObject(allPoints);
        }

        /// <summary>
        /// Sets the server weather override.
        /// </summary>
        /// <param name="weather">GTA weather string.</param>
        public static void EnableWeatherOverride(string weather)
        {
            WeatherOverrideState = weather;
            WeatherOverride = true;
        }

        /// <summary>
        /// Disables to the weather override.
        /// </summary>
        public static void DisableWeatherOverride()
        {
            WeatherOverride = false;
        }
    }
}