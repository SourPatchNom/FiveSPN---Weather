using CitizenFX.Core;

namespace WeatherSyncClient.Extensions
{
    public static class GtaWeatherExtensions
    {
        /// <summary>
        /// Returns the GTA Weather type for a given string.
        /// </summary>
        /// <param name="inputString">Desired weather.</param>
        /// <returns>Weather result</returns>
        public static Weather GetWeatherFromString(string inputString)
        {
            switch (inputString)
            {
                case "THUNDER":
                    return Weather.ThunderStorm;
                case "RAIN":
                    return Weather.Raining;
                case "SNOWLIGHT":
                    return Weather.Snowlight;
                case "SNOW":
                    return Weather.Christmas;
                case "BLIZZARD":
                    return Weather.Blizzard;
                case "FOGGY":
                    return Weather.Foggy;
                case "EXTRASUNNY":
                    return Weather.ExtraSunny;
                case "CLOUDS":
                    return Weather.Clouds;
                case "OVERCAST":
                    return Weather.Overcast;
                case "SMOG":
                    return Weather.Smog;
                default:
                    return Weather.Clear;
            }
        }
    }
}