namespace FiveSpn.Weather.Server.WeatherApiService.Classes
{
    public class WeatherIdResult
    {
        public bool WasSuccessful { get; private set; }
        public string WeatherRequestResult { get; private set; }

        public int WeatherApiTypeId { get; private set; }

        public WeatherIdResult(bool wasSuccessful, string weatherRequestResult, int weatherApiTypeId)
        {
            WasSuccessful = wasSuccessful;
            WeatherRequestResult = weatherRequestResult;
            WeatherApiTypeId = weatherApiTypeId;
        }
    }
}