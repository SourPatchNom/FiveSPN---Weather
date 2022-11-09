using System;
using System.Collections.Generic;
using CitizenFX.Core;
using Newtonsoft.Json;

namespace WeatherSyncClient.Services
{
    public static class WorldWeatherService
    {
        private static readonly List<Tuple<string, Vector3>> WorldWeatherStates = new List<Tuple<string, Vector3>>();
        private static readonly object UpdateLock = new object();
        
        /// <summary>
        /// Update the world weather point information with a json string from the server.
        /// </summary>
        /// <param name="json">World weather position data.</param>
        public static void UpdateWorldWeatherStates(string json)
        {
            lock (UpdateLock)
            {
                var data = JsonConvert.DeserializeObject<List<Tuple<string, List<float>>>>(json);
                if (data == null) return;
                WorldWeatherStates.Clear();
                data.ForEach(x => WorldWeatherStates.Add(new Tuple<string, Vector3>(x.Item1, new Vector3(x.Item2[0], x.Item2[1], x.Item2[2]))));
            }
        }

        /// <summary>
        /// Gets the weather given a supplied position, against an array of positions.
        /// </summary>
        /// <param name="position">Position of desired weather.</param>
        /// <returns>Weather string for position.</returns>
        public static string GetWorldDesiredWeather(Vector3 position)
        {
            lock (UpdateLock)
            {
                var shortestString = "CLEAR";
                var shortestDistance = float.MaxValue;
                foreach (var weatherState in WorldWeatherStates)
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