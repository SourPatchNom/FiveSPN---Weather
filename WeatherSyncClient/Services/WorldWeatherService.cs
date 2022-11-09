using System;
using System.Collections.Generic;
using CitizenFX.Core;
using Newtonsoft.Json;

namespace WeatherSyncClient.Services
{
    public sealed class WorldWeatherService
    {
        private static readonly Lazy<WorldWeatherService> instance = new Lazy<WorldWeatherService>(() => new WorldWeatherService());
        private WorldWeatherService(){}
        public static WorldWeatherService Instance => instance.Value;

        private readonly List<Tuple<string, Vector3>> _worldWeatherStates = new List<Tuple<string, Vector3>>();
        private readonly object _updateLock = new object();
        
        /// <summary>
        /// Update the world weather point information with a json string from the server.
        /// </summary>
        /// <param name="json">World weather position data.</param>
        public void UpdateWorldWeatherStates(string json)
        {
            lock (_updateLock)
            {
                var data = JsonConvert.DeserializeObject<List<Tuple<string, List<float>>>>(json);
                if (data == null) return;
                _worldWeatherStates.Clear();
                data.ForEach(x => _worldWeatherStates.Add(new Tuple<string, Vector3>(x.Item1, new Vector3(x.Item2[0], x.Item2[1], x.Item2[2]))));
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
                var shortestString = "CLEAR";
                var shortestDistance = float.MaxValue;
                foreach (var weatherState in _worldWeatherStates)
                {
                    var distance = Vector3.DistanceSquared(position, weatherState.Item2);
                    if (distance > shortestDistance) continue;
                    shortestDistance = distance;
                    shortestString = weatherState.Item1;
                }
                return shortestString;
            }
        }
    }
}