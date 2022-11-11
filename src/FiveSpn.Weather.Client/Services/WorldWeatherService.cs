using System;
using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using FiveSpn.Weather.Library.Classes.WeatherPoint;
using Newtonsoft.Json;

namespace FiveSpn.Weather.Client.Services
{
    public sealed class WorldWeatherService
    {
        private static readonly Lazy<WorldWeatherService> instance = new Lazy<WorldWeatherService>(() => new WorldWeatherService());
        private WorldWeatherService(){}
        public static WorldWeatherService Instance => instance.Value;

        private readonly List<WeatherPoint> _worldWeatherStates = new List<WeatherPoint>();
        private readonly object _updateLock = new object();
        
        /// <summary>
        /// Update the world weather point information with a json string from the server.
        /// </summary>
        /// <param name="json">World weather position data.</param>
        public void UpdateWorldWeatherStates(string json)
        {
            lock (_updateLock)
            {
                var data = JsonConvert.DeserializeObject<List<WeatherPoint>>(json);
                if (data == null) return;
                _worldWeatherStates.Clear();
                data.ForEach(x => _worldWeatherStates.Add(x));
            }
        }

        /// <summary>
        /// Gets the weather given a supplied position, against an array of positions.
        /// </summary>
        /// <param name="position">Position of desired weather.</param>
        /// <returns>Weather string for position.</returns>
        public string GetWorldDesiredWeather(Vector3 position)
        {
            lock (_updateLock)
            {
                if (!_worldWeatherStates.Any()) return "CLEAR";
                var weatherString = "CLEAR";
                var shortestDistance = float.MaxValue;
                foreach (var weatherPoint in _worldWeatherStates)
                {
                    var distance = Vector3.DistanceSquared(position, new Vector3(weatherPoint.Position[0],weatherPoint.Position[1],weatherPoint.Position[2]));
                    if (weatherPoint.Range != 0)
                    {
                        if (distance < weatherPoint.Range)
                        {
                            return weatherPoint.Weather;
                        }
                    }
                    if (distance > shortestDistance) continue;
                    shortestDistance = distance;
                    weatherString = weatherPoint.Weather;
                }
                return weatherString;
            }
        }
    }
}