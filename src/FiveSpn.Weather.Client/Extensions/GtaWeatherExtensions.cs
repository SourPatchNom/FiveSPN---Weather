namespace FiveSpn.Weather.Client.Extensions
{
    public static class GtaWeatherExtensions
    {
        /// <summary>
        /// Returns the GTA Weather type for a given string.
        /// </summary>
        /// <param name="inputString">Desired weather.</param>
        /// <returns>Weather result</returns>
        public static CitizenFX.Core.Weather GetWeatherFromString(string inputString)
        {
            switch (inputString)
            {
                case "THUNDER":
                    return CitizenFX.Core.Weather.ThunderStorm;
                case "RAIN":
                    return CitizenFX.Core.Weather.Raining;
                case "SNOWLIGHT":
                    return CitizenFX.Core.Weather.Snowlight;
                case "SNOW":
                    return CitizenFX.Core.Weather.Christmas;
                case "BLIZZARD":
                    return CitizenFX.Core.Weather.Blizzard;
                case "FOGGY":
                    return CitizenFX.Core.Weather.Foggy;
                case "EXTRASUNNY":
                    return CitizenFX.Core.Weather.ExtraSunny;
                case "CLOUDS":
                    return CitizenFX.Core.Weather.Clouds;
                case "OVERCAST":
                    return CitizenFX.Core.Weather.Overcast;
                case "SMOG":
                    return CitizenFX.Core.Weather.Smog;
                default:
                    return CitizenFX.Core.Weather.Clear;
            }
        }

    }
}